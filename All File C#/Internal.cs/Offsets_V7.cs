namespace AotForms
{
    internal static class OffsetsV7
    {
        // Base
        internal static uint Il2Cpp;
        internal static uint InitBase = 0xA3F438C;

        internal static uint StaticClass = 0x5C;
        internal static uint MatchStatus = 0x3C;
        internal static uint LocalPlayer = 0x7C;
        internal static uint DictionaryEntities = 0x68;
        internal static uint CurrentMatch = 0x50;

        // Player

        internal static uint Player_IsDead = 0x4C;
        internal static uint Player_Name = 0x28C;
        internal static uint Player_Data = 0x44;
        internal static uint Player_ShadowBase = 0x15E8;
        internal static uint XPose = 0x78;
        internal static uint PlayerAttributes = 0x45C;
        internal static uint NoReload = 0x91;
        internal static uint IsBot = 0x294;


        // Avatar
        internal static uint AvatarManager = 0x460;
        internal static uint Avatar = 0x94;
        internal static uint Avatar_IsVisible = 0x7C;
        internal static uint Avatar_Data = 0x10;
        internal static uint Avatar_Data_IsTeam = 0x51;

        // Camera
        internal static uint FollowCamera = 0x3F0;
        internal static uint Camera = 0x14;
        internal static uint AimRotation = 0x3A8;
        internal static uint MainCameraTransform = 0x1FC;
        internal static uint ViewMatrix = 0x98 + 0x24;

        // Weapon (أساسي)
        internal static uint Weapon = 0x39C;
        internal static uint WeaponData = 0x58;
        internal static uint WeaponRecoil = 0x0C;

        // Weapon Extra (pomba / weaponinfo)
        internal static uint pomba = 0x490;      // uintptr_t pomba
        internal static uint bisteca = 0x85C;    // uintptr_t bisteca
        internal static uint arma = 0x38;        // uintptr_t arma
        internal static uint tiro = 0x2C;        // uintptr_t tiro

        internal static uint WeaponName = 0x38;  // ✅ موجود

        internal static uint WeaponInfo = 0x490; // نفس pomba (WeaponInfo Class)
        internal static uint weaponinfo = 0x490; // alias إذا تحتاجه

        // Silent Aim
        internal static uint sAim1 = 0x4E0;
        internal static uint sAim2 = 0x8F0;
        internal static uint sAim3 = 0x38;
        internal static uint sAim4 = 0x2C;

        // Collider
        internal static uint HeadCollider = 0x444;
    }
}
