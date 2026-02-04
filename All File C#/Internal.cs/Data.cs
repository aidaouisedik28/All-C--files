using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading;

namespace AotForms
{
    internal static class Data
    {
        internal static void Work()
        {
            while (true)
            {
                try
                {
                    if (!InitializeGameState())
                    {
                        ResetCache();
                        Thread.Sleep(100);
                        continue;
                    }

                    ProcessEntities();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Data] Error: {ex.Message}");
                    ResetCache();
                    Thread.Sleep(100);
                }

                Thread.SpinWait(1);
            }
        }

        private static bool InitializeGameState()
        {
            Core.HaveMatrix = false;
            uint currentIl2Cpp = GameOffsets.Il2Cpp;

            // قراءة BaseGameFacade
            if (!InternalMemory.Read<uint>(currentIl2Cpp + GameOffsets.InitBase, out var baseGameFacade) || baseGameFacade == 0)
            {
                Console.WriteLine($"Failed to read BaseGameFacade. Il2Cpp: {currentIl2Cpp:X}");
                return false;
            }
            Console.WriteLine($"BaseGameFacade: {baseGameFacade:X}");

            // قراءة GameFacade
            if (!InternalMemory.Read<uint>(baseGameFacade, out var gameFacade) || gameFacade == 0)
            {
                Console.WriteLine("Failed to read GameFacade.");
                return false;
            }
            Console.WriteLine($"GameFacade: {gameFacade:X}");

            // قراءة StaticGameFacade
            if (!InternalMemory.Read<uint>(gameFacade + GameOffsets.StaticClass, out var staticGameFacade) || staticGameFacade == 0)
            {
                Console.WriteLine("Failed to read StaticGameFacade.");
                return false;
            }
            Console.WriteLine($"StaticGameFacade: {staticGameFacade:X}");

            // قراءة CurrentGame
            if (!InternalMemory.Read<uint>(staticGameFacade, out var currentGame) || currentGame == 0)
            {
                Console.WriteLine("Failed to read CurrentGame.");
                return false;
            }
            Console.WriteLine($"CurrentGame: {currentGame:X}");

            // قراءة CurrentMatch
            if (!InternalMemory.Read<uint>(currentGame + GameOffsets.CurrentMatch, out var currentMatch) || currentMatch == 0)
            {
                Console.WriteLine("Failed to read CurrentMatch.");
                return false;
            }
            Console.WriteLine($"CurrentMatch: {currentMatch:X}");

            // قراءة LocalPlayer
            if (!InternalMemory.Read<uint>(currentMatch + GameOffsets.LocalPlayer, out var localPlayer) || localPlayer == 0)
            {
                Console.WriteLine("Failed to read LocalPlayer.");
                return false;
            }
            Core.LocalPlayer = localPlayer;
            Console.WriteLine($"LocalPlayer: {Core.LocalPlayer:X}");

            // تهيئة الكاميرا
            if (!InitializeCamera(localPlayer))
            {
                return false;
            }

            return true;
        }

        private static bool InitializeCamera(uint localPlayer)
        {
            // قراءة MainCameraTransform
            if (!InternalMemory.Read<uint>(localPlayer + GameOffsets.MainCameraTransform, out var mainTransform) || mainTransform == 0)
            {
                Console.WriteLine("Failed to read MainCameraTransform.");
                return false;
            }
            Console.WriteLine($"MainCameraTransform: {mainTransform:X}");

            if (Transform.GetPosition(mainTransform, out var mainPos))
            {
                Core.LocalMainCamera = mainPos;
                Console.WriteLine($"Core.LocalMainCamera: {Core.LocalMainCamera}");
            }
            else
            {
                Console.WriteLine("Failed to get MainCamera position.");
            }

            // قراءة FollowCamera
            if (!InternalMemory.Read<uint>(localPlayer + GameOffsets.FollowCamera, out var followCamera) || followCamera == 0)
            {
                Console.WriteLine("Failed to read FollowCamera.");
                return false;
            }
            Console.WriteLine($"FollowCamera: {followCamera:X}");

            // قراءة Camera
            if (!InternalMemory.Read<uint>(followCamera + GameOffsets.Camera, out var camera) || camera == 0)
            {
                Console.WriteLine("Failed to read Camera.");
                return false;
            }
            Console.WriteLine($"Camera: {camera:X}");

            // قراءة CameraBase
            if (!InternalMemory.Read<uint>(camera + 0x8, out var cameraBase) || cameraBase == 0)
            {
                Console.WriteLine("Failed to read CameraBase.");
                return false;
            }
            Core.HaveMatrix = true;
            Console.WriteLine($"CameraBase: {cameraBase:X}. Core.HaveMatrix set to true.");

            // قراءة ViewMatrix
            if (!InternalMemory.Read<Matrix4x4>(cameraBase + GameOffsets.ViewMatrix, out var viewMatrix))
            {
                Console.WriteLine("Failed to read ViewMatrix.");
                return false;
            }
            Core.CameraMatrix = viewMatrix;
            Console.WriteLine("Core.CameraMatrix updated.");

            return true;
        }

        private static void ProcessEntities()
        {
            uint currentIl2Cpp = GameOffsets.Il2Cpp;

            if (!InternalMemory.Read<uint>(currentIl2Cpp + GameOffsets.InitBase, out var baseGameFacade) || baseGameFacade == 0) return;
            if (!InternalMemory.Read<uint>(baseGameFacade, out var gameFacade) || gameFacade == 0) return;
            if (!InternalMemory.Read<uint>(gameFacade + GameOffsets.StaticClass, out var staticGameFacade) || staticGameFacade == 0) return;
            if (!InternalMemory.Read<uint>(staticGameFacade, out var currentGame) || currentGame == 0) return;
            if (!InternalMemory.Read<uint>(currentGame + GameOffsets.DictionaryEntities, out var entityDictionary) || entityDictionary == 0) return;

            if (!InternalMemory.Read<uint>(entityDictionary + 0x14, out var entities) || entities == 0)
            {
                Console.WriteLine("Failed to read entities array.");
                return;
            }
            Console.WriteLine($"Entities array address: {entities:X}");

            entities += 0x10;

            if (!InternalMemory.Read<uint>(entityDictionary + 0x18, out var entitiesCount) || entitiesCount < 1) return;
            Console.WriteLine($"Entities count: {entitiesCount}");

            for (int i = 0; i < entitiesCount; i++)
            {
                if (!InternalMemory.Read<uint>((ulong)(i * 0x4 + entities), out var entity) || entity == 0) continue;
                if (entity == Core.LocalPlayer) continue;

                Console.WriteLine($"Processing entity at address: {entity:X}");
                ProcessEntity(entity);
            }
        }

        private static void ProcessEntity(uint entity)
        {
            if (Core.Entities.TryGetValue(entity, out var player))
            {
                player.Address = entity;

                if (player.IsTeam == Bool3.True) return;

                // التحقق من الفريق
                if (player.IsTeam == Bool3.Unknown)
                {
                    CheckTeamStatus(entity, player);
                }

                // NoRecoil
                if (Config.NoRecoil)
                {
                    ApplyNoRecoil();
                }

                if (!player.IsKnown) return;

                // التحقق من حالة اللاعب
                UpdatePlayerState(entity, player);

                // تحديث البيانات
                if (Config.ESPName)
                {
                    UpdatePlayerName(entity, player);
                }

                if (Config.ESPHealth)
                {
                    UpdatePlayerHealth(entity, player);
                }

                if (Config.ESPWeapon)
                {
                    UpdatePlayerWeapon(entity, player);
                }

                // تحديث العظام
                UpdatePlayerBones(entity, player);

                // 🔥 حساب المسافة
                if (player.Head != Vector3.Zero)
                {
                    player.Distance = Vector3.Distance(Core.LocalMainCamera, player.Head);
                }
            }
            else
            {
                // إنشاء كيان جديد
                Core.Entities[entity] = CreateNewEntity();
            }
        }

        private static void CheckTeamStatus(uint entity, Entity player)
        {
            if (!InternalMemory.Read<uint>(entity + GameOffsets.AvatarManager, out var avatarManager) || avatarManager == 0) return;
            if (!InternalMemory.Read<uint>(avatarManager + GameOffsets.Avatar, out var avatar) || avatar == 0) return;
            if (!InternalMemory.Read<bool>(avatar + GameOffsets.Avatar_IsVisible, out var isVisible) || !isVisible) return;
            if (!InternalMemory.Read<uint>(avatar + GameOffsets.Avatar_Data, out var avatarData) || avatarData == 0) return;
            if (!InternalMemory.Read<bool>(avatarData + GameOffsets.Avatar_Data_IsTeam, out var isTeam)) return;

            player.IsTeam = isTeam ? Bool3.True : Bool3.False;
            if (!isTeam)
            {
                player.IsKnown = true;
            }
        }

        private static void ApplyNoRecoil()
        {
            if (!InternalMemory.Read<uint>(Core.LocalPlayer + GameOffsets.Weapon, out var weapon) || weapon == 0) return;
            if (!InternalMemory.Read<uint>(weapon + GameOffsets.WeaponData, out var weaponData) || weaponData == 0) return;
            if (!InternalMemory.Read<float>(weaponData + GameOffsets.WeaponRecoil, out var recoil) || recoil == 0) return;

            InternalMemory.Write(weaponData + GameOffsets.WeaponRecoil, 0f);
        }

        private static void UpdatePlayerState(uint entity, Entity player)
        {
            // التحقق من حالة الإصابة
            if (Config.IgnoreKnocked)
            {
                if (InternalMemory.Read<uint>(entity + GameOffsets.Player_ShadowBase, out var shadowBase) && shadowBase != 0)
                {
                    if (InternalMemory.Read<int>(shadowBase + GameOffsets.XPose, out var xpose))
                    {
                        player.IsKnocked = xpose == 8;
                    }
                }
            }

            // التحقق من الموت
            if (InternalMemory.Read<bool>(entity + GameOffsets.Player_IsDead, out var isDead))
            {
                player.IsDead = isDead;
            }
        }

        private static void UpdatePlayerName(uint entity, Entity player)
        {
            if (!InternalMemory.Read<uint>(entity + GameOffsets.Player_Name, out var nameAddr) || nameAddr == 0) return;
            if (!InternalMemory.Read<int>(nameAddr + 0x8, out var nameLen)) return;

            string name;
            if (nameLen > 0 && nameLen < 50)
            {
                name = InternalMemory.ReadString(nameAddr + 0xC, nameLen * 2);
            }
            else
            {
                name = InternalMemory.ReadString(nameAddr + 0xC, 32);
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                player.Name = name;
            }
        }

        private static void UpdatePlayerHealth(uint entity, Entity player)
        {
            // 🔥 المحاولة 1: الطريقة الأصلية مع offsets مختلفة
            if (InternalMemory.Read<uint>(entity + GameOffsets.Player_Data, out var dataPool) && dataPool != 0)
            {
                if (InternalMemory.Read<uint>(dataPool + 0x8, out var poolObj) && poolObj != 0)
                {
                    if (InternalMemory.Read<uint>(poolObj + 0x10, out var pool) && pool != 0)
                    {
                        // جرب كل الـ offsets الممكنة
                        uint[] offsets = { 0x4, 0x8, 0xC, 0x10, 0x14, 0x18, 0x1C, 0x20 };

                        foreach (var offset in offsets)
                        {
                            // جرب int
                            if (InternalMemory.Read<int>(pool + offset, out var healthInt))
                            {
                                if (healthInt > 0 && healthInt <= 300)
                                {
                                    player.Health = (short)healthInt;
                                    Console.WriteLine($"✅ Health (int) found at pool+0x{offset:X}: {healthInt}");
                                    return;
                                }
                            }

                            // جرب short
                            if (InternalMemory.Read<short>(pool + offset, out var healthShort))
                            {
                                if (healthShort > 0 && healthShort <= 300)
                                {
                                    player.Health = healthShort;
                                    Console.WriteLine($"✅ Health (short) found at pool+0x{offset:X}: {healthShort}");
                                    return;
                                }
                            }

                            // جرب float
                            if (InternalMemory.Read<float>(pool + offset, out var healthFloat))
                            {
                                if (healthFloat > 0 && healthFloat <= 300)
                                {
                                    player.Health = (short)healthFloat;
                                    Console.WriteLine($"✅ Health (float) found at pool+0x{offset:X}: {healthFloat}");
                                    return;
                                }
                            }
                        }
                    }

                    // 🔥 المحاولة 2: جرب مباشرة من poolObj
                    uint[] poolObjOffsets = { 0x4, 0x8, 0xC, 0x10, 0x14, 0x18 };
                    foreach (var offset in poolObjOffsets)
                    {
                        if (InternalMemory.Read<short>(poolObj + offset, out var health))
                        {
                            if (health > 0 && health <= 300)
                            {
                                player.Health = health;
                                Console.WriteLine($"✅ Health found at poolObj+0x{offset:X}: {health}");
                                return;
                            }
                        }
                    }
                }

                // 🔥 المحاولة 3: جرب مباشرة من dataPool
                uint[] dataPoolOffsets = { 0x4, 0x8, 0xC, 0x10, 0x14 };
                foreach (var offset in dataPoolOffsets)
                {
                    if (InternalMemory.Read<short>(dataPool + offset, out var health))
                    {
                        if (health > 0 && health <= 300)
                        {
                            player.Health = health;
                            Console.WriteLine($"✅ Health found at dataPool+0x{offset:X}: {health}");
                            return;
                        }
                    }
                }
            }

            // 🔥 المحاولة 4: جرب مباشرة من entity
            uint[] entityOffsets = { 0x40, 0x44, 0x48, 0x4C, 0x50, 0x54, 0x58 };
            foreach (var offset in entityOffsets)
            {
                if (InternalMemory.Read<short>(entity + offset, out var health))
                {
                    if (health > 0 && health <= 300)
                    {
                        player.Health = health;
                        Console.WriteLine($"✅ Health found at entity+0x{offset:X}: {health}");
                        return;
                    }
                }
            }

            // افتراضي - لو ما لقينا
            player.Health = 100;
            Console.WriteLine($"⚠️ Health not found, using default: 100");
        }

        private static void UpdatePlayerWeapon(uint entity, Entity player)
        {
            if (!InternalMemory.Read<uint>(entity + GameOffsets.Weapon, out var weapon) || weapon == 0) return;
            if (!InternalMemory.Read<uint>(weapon + GameOffsets.WeaponData, out var weaponData) || weaponData == 0) return;

            // قراءة اسم السلاح
            if (!InternalMemory.Read<uint>(weaponData + Offsets.WeaponName, out var weaponNameAddr) || weaponNameAddr == 0) return;
            if (!InternalMemory.Read<int>(weaponNameAddr + 0x8, out var weaponNameLen)) return;

            string weaponName;
            if (weaponNameLen > 0 && weaponNameLen < 50)
            {
                weaponName = InternalMemory.ReadString(weaponNameAddr + 0xC, weaponNameLen * 2);
            }
            else
            {
                weaponName = InternalMemory.ReadString(weaponNameAddr + 0xC, 32);
            }

            if (!string.IsNullOrWhiteSpace(weaponName))
            {
                player.WeaponName = weaponName;
            }
        }

        private static void UpdatePlayerBones(uint entity, Entity player)
        {
            var boneData = new[]
            {
                (GameBones.Head, (Action<Vector3>)(v => player.Head = v)),
                (GameBones.LeftWrist, (Action<Vector3>)(v => player.LeftWrist = v)),
                (GameBones.Spine, (Action<Vector3>)(v => player.Spine = v)),
                (GameBones.Hip, (Action<Vector3>)(v => player.Hip = v)),
                (GameBones.Root, (Action<Vector3>)(v => player.Root = v)),
                (GameBones.RightCalf, (Action<Vector3>)(v => player.RightCalf = v)),
                (GameBones.LeftCalf, (Action<Vector3>)(v => player.LeftCalf = v)),
                (GameBones.RightFoot, (Action<Vector3>)(v => player.RightFoot = v)),
                (GameBones.LeftFoot, (Action<Vector3>)(v => player.LeftFoot = v)),
                (GameBones.RightWrist, (Action<Vector3>)(v => player.RightWrist = v)),
                (GameBones.LeftHand, (Action<Vector3>)(v => player.LeftHand = v)),
                (GameBones.RightWristJoint, (Action<Vector3>)(v => player.RightWristJoint = v)),
                (GameBones.LeftWristJoint, (Action<Vector3>)(v => player.LeftWristJoint = v)),
                (GameBones.LeftElbow, (Action<Vector3>)(v => player.LeftElbow = v)),
                (GameBones.RightElbow, (Action<Vector3>)(v => player.RightElbow = v))
            };

            foreach (var (offset, setter) in boneData)
            {
                if (InternalMemory.Read<uint>(entity + offset, out var bone) && bone != 0)
                {
                    if (Transform.GetNodePosition(bone, out var boneTransform))
                    {
                        setter(boneTransform);
                    }
                }
            }
        }

        private static Entity CreateNewEntity()
        {
            return new Entity
            {
                IsTeam = Bool3.Unknown,
                IsKnown = false,
                IsDead = false,
                Health = 0,
                IsKnocked = false,
                Head = Vector3.Zero,
                LeftWrist = Vector3.Zero,
                Spine = Vector3.Zero,
                Root = Vector3.Zero,
                Hip = Vector3.Zero,
                RightCalf = Vector3.Zero,
                LeftCalf = Vector3.Zero,
                RightFoot = Vector3.Zero,
                LeftFoot = Vector3.Zero,
                RightWrist = Vector3.Zero,
                LeftHand = Vector3.Zero,
                RightShoulder = Vector3.Zero,
                RightWristJoint = Vector3.Zero,
                LeftWristJoint = Vector3.Zero,
                RightElbow = Vector3.Zero,
                LeftElbow = Vector3.Zero,
                Name = string.Empty,
                WeaponName = string.Empty,
                Distance = 0f // 🔥 إضافة Distance
            };
        }

        private static void ResetCache()
        {
            Core.Entities.Clear();
            InternalMemory.Cache.Clear();
        }
    }
}   
