namespace AotForms
{
    internal static class GameOffsets
    {
        // استخدام نفس نمط GameBones
        public static uint Il2Cpp => GameSelector.O.Il2Cpp;

        public static uint InitBase =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.InitBase,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.InitBase,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.InitBase,
                _ => 0
            };

        public static uint StaticClass =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.StaticClass,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.StaticClass,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.StaticClass,
                _ => 0
            };

        public static uint CurrentMatch =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.CurrentMatch,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.CurrentMatch,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.CurrentMatch,
                _ => 0
            };

        public static uint LocalPlayer =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.LocalPlayer,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.LocalPlayer,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.LocalPlayer,
                _ => 0
            };

        public static uint MainCameraTransform =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.MainCameraTransform,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.MainCameraTransform,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.MainCameraTransform,
                _ => 0
            };

        public static uint FollowCamera =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.FollowCamera,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.FollowCamera,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.FollowCamera,
                _ => 0
            };

        public static uint Camera =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.Camera,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.Camera,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.Camera,
                _ => 0
            };

        public static uint ViewMatrix =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.ViewMatrix,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.ViewMatrix,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.ViewMatrix,
                _ => 0
            };

        public static uint DictionaryEntities =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.DictionaryEntities,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.DictionaryEntities,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.DictionaryEntities,
                _ => 0
            };

        public static uint AvatarManager =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.AvatarManager,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.AvatarManager,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.AvatarManager,
                _ => 0
            };

        public static uint Avatar =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.Avatar,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.Avatar,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.Avatar,
                _ => 0
            };

        public static uint Avatar_IsVisible =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.Avatar_IsVisible,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.Avatar_IsVisible,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.Avatar_IsVisible,
                _ => 0
            };

        public static uint Avatar_Data =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.Avatar_Data,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.Avatar_Data,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.Avatar_Data,
                _ => 0
            };

        public static uint Avatar_Data_IsTeam =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.Avatar_Data_IsTeam,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.Avatar_Data_IsTeam,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.Avatar_Data_IsTeam,
                _ => 0
            };

        public static uint Weapon =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.Weapon,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.Weapon,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.Weapon,
                _ => 0
            };

        public static uint WeaponData =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.WeaponData,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.WeaponData,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.WeaponData,
                _ => 0
            };

        public static uint WeaponRecoil =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.WeaponRecoil,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.WeaponRecoil,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.WeaponRecoil,
                _ => 0
            };

        public static uint Player_ShadowBase =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.Player_ShadowBase,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.Player_ShadowBase,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.Player_ShadowBase,
                _ => 0
            };

        public static uint XPose =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.XPose,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.XPose,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.XPose,
                _ => 0
            };

        public static uint Player_IsDead =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.Player_IsDead,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.Player_IsDead,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.Player_IsDead,
                _ => 0
            };

        public static uint Player_Name =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.Player_Name,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.Player_Name,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.Player_Name,
                _ => 0
            };

        public static uint Player_Data =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.Player_Data,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.Player_Data,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.Player_Data,
                _ => 0
            };

        // 🔥 إضافة WeaponName للـ Weapon ESP
        public static uint WeaponName =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.WeaponName,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.WeaponName,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.WeaponName,
                _ => 0
            };

        // 🔥 إضافة AimRotation للـ Aimbot
        public static uint AimRotation =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => Offsets.AimRotation,
                GameSelector.GameType.FreeFire_Max => OffsetsMax.AimRotation,
                GameSelector.GameType.FreeFire_V7 => OffsetsV7.AimRotation,
                _ => 0
            };
    }
}