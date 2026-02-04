using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AotForms
{
    internal static class SilentAim2
    {
        internal static int AimbotSpeed = 1;

        internal static void Work()
        {
            while (true)
            {
                if (!Config.SilentAim2)
                {
                    Thread.Sleep(50); // طالما مش مفعّل يبقى نام
                    continue;
                }

                if (Core.Width == -1 || Core.Height == -1 || !Core.HaveMatrix)
                {
                    Thread.Sleep(50);
                    continue;
                }

                Entity target = null;
                float distance = float.MaxValue;
                var screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);

                foreach (var entity in Core.Entities.Values)
                {
                    if (entity.IsDead) continue;
                    if (Config.IgnoreKnocked && entity.IsKnocked) continue;

                    var head2D = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                    if (head2D.X < 1 || head2D.Y < 1) continue;

                    Vector2 headScreenPos2 = new Vector2(head2D.X, head2D.Y);
                    var playerDistance = Vector2.Distance(screenCenter, headScreenPos2);

                    if (isInsideFOV((int)head2D.X, (int)head2D.Y) && playerDistance < distance)
                    {
                        distance = playerDistance;
                        target = entity;
                    }
                }

                if (target != null)
                {
                    var LPEIEILIKGC = InternalMemory.Read<bool>(Core.LocalPlayer + Offsets.pomba, out var LPEIEILIKGC2);

                    if (LPEIEILIKGC2)
                    {
                        var MADMMIICBNN = InternalMemory.Read<uint>(Core.LocalPlayer + OffsetsV7.bisteca, out var MADMMIICBNN2);
                        if (MADMMIICBNN2 != 0)
                        {
                            InternalMemory.Read<Vector3>(MADMMIICBNN2 + OffsetsV7.arma, out var StartPosition);
                            InternalMemory.Write<Vector3>(MADMMIICBNN2 + OffsetsV7.tiro, target.Head - StartPosition);
                        }
                    }
                }

                Thread.Sleep(1); 
            }
        }

        private static Entity FindBestTarget()
        {
            var screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);
            Entity bestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (var entity in Core.Entities.Values)
            {
                if (entity.IsDead) continue; 
                if (entity.IsKnocked) continue;

                var head2D = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                if (head2D.X < 1 || head2D.Y < 1) continue; // Skip off-screen entities

                float playerDistance = Vector3.Distance(Core.LocalMainCamera, entity.Head);
                if (playerDistance > 10) continue; // Skip too far entities

                var crosshairDistance = Vector2.Distance(screenCenter, head2D);


                bestTarget = entity;

            }

            return bestTarget;
        }


        private static bool isInsideFOV(int x, int y)
        {
            if (Config.AimFovCircle <= 0)
                return true;

            int circle_x = Core.Width / 2;
            int circle_y = Core.Height / 2;
            int rad = Config.AimFovCircle;
            return (x - circle_x) * (x - circle_x) + (y - circle_y) * (y - circle_y) <= rad * rad;
        }

        internal static void teleportenemy()
        {
            while (true)
            {
                if (!Config.enmyteleport2)
                {
                    Thread.Sleep(1);
                    continue;
                }
                if (Core.Width == -1 || Core.Height == -1 || !Core.HaveMatrix)
                {
                    Thread.Sleep(1);
                    continue;
                }
                Entity target = FindBestTarget();
                if (target != null)
                {
                    if (target == null || target.Address == 0) return;
                    var EntityRootBone = InternalMemory.Read<uint>(target.Address + (uint)Bones.Root, out var EntityrootBone);

                    var transform = InternalMemory.Read<uint>(EntityrootBone + 0x8, out var transformValue);

                    var transformObj = InternalMemory.Read<uint>(transformValue + 0x8, out var rootBoneclass);

                    var matrix = InternalMemory.Read<uint>(rootBoneclass + 0x20, out var roootmatrixValuelist);
                    var rootboneresult = InternalMemory.Read<Vector3>(roootmatrixValuelist + 0x80, out var resultValuebone);


                    InternalMemory.Write<Vector3>(roootmatrixValuelist + 0x80, Core.playerpos);


                }

            }
        }
    }
}

