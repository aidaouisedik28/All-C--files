namespace AotForms
{
    internal static class GameBones
    {
        public static uint Head =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => (uint)Bones.Head,
                GameSelector.GameType.FreeFire_Max => (uint)Bones_MAX.Head,
                GameSelector.GameType.FreeFire_V7 => (uint)Bones_V7.Head,
                _ => 0
            };

        public static uint LeftWrist =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => (uint)Bones.LeftWrist,
                GameSelector.GameType.FreeFire_Max => (uint)Bones_MAX.LeftWrist,
                GameSelector.GameType.FreeFire_V7 => (uint)Bones_V7.LeftWrist,
                _ => 0
            };

        public static uint Spine =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => (uint)Bones.Spine,
                GameSelector.GameType.FreeFire_Max => (uint)Bones_MAX.Spine,
                GameSelector.GameType.FreeFire_V7 => (uint)Bones_V7.Spine,
                _ => 0
            };

        public static uint Hip =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => (uint)Bones.Hip,
                GameSelector.GameType.FreeFire_Max => (uint)Bones_MAX.Hip,
                GameSelector.GameType.FreeFire_V7 => (uint)Bones_V7.Hip,
                _ => 0
            };

        public static uint Root =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => (uint)Bones.Root,
                GameSelector.GameType.FreeFire_Max => (uint)Bones_MAX.Root,
                GameSelector.GameType.FreeFire_V7 => (uint)Bones_V7.Root,
                _ => 0
            };

        public static uint RightCalf =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => (uint)Bones.RightCalf,
                GameSelector.GameType.FreeFire_Max => (uint)Bones_MAX.RightCalf,
                GameSelector.GameType.FreeFire_V7 => (uint)Bones_V7.RightCalf,
                _ => 0
            };

        public static uint LeftCalf =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => (uint)Bones.LeftCalf,
                GameSelector.GameType.FreeFire_Max => (uint)Bones_MAX.LeftCalf,
                GameSelector.GameType.FreeFire_V7 => (uint)Bones_V7.LeftCalf,
                _ => 0
            };

        public static uint RightFoot =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => (uint)Bones.RightFoot,
                GameSelector.GameType.FreeFire_Max => (uint)Bones_MAX.RightFoot,
                GameSelector.GameType.FreeFire_V7 => (uint)Bones_V7.RightFoot,
                _ => 0
            };

        public static uint LeftFoot =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => (uint)Bones.LeftFoot,
                GameSelector.GameType.FreeFire_Max => (uint)Bones_MAX.LeftFoot,
                GameSelector.GameType.FreeFire_V7 => (uint)Bones_V7.LeftFoot,
                _ => 0
            };

        public static uint RightWrist =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => (uint)Bones.RightWrist,
                GameSelector.GameType.FreeFire_Max => (uint)Bones_MAX.RightWrist,
                GameSelector.GameType.FreeFire_V7 => (uint)Bones_V7.RightWrist,
                _ => 0
            };

        public static uint LeftHand =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => (uint)Bones.LeftHand,
                GameSelector.GameType.FreeFire_Max => (uint)Bones_MAX.LeftHand,
                GameSelector.GameType.FreeFire_V7 => (uint)Bones_V7.LeftHand,
                _ => 0
            };

        public static uint RightWristJoint =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => (uint)Bones.RightWristJoint,
                GameSelector.GameType.FreeFire_Max => (uint)Bones_MAX.RightWristJoint,
                GameSelector.GameType.FreeFire_V7 => (uint)Bones_V7.RightWristJoint,
                _ => 0
            };

        public static uint LeftWristJoint =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => (uint)Bones.LeftWristJoint,
                GameSelector.GameType.FreeFire_Max => (uint)Bones_MAX.LeftWristJoint,
                GameSelector.GameType.FreeFire_V7 => (uint)Bones_V7.LeftWristJoint,
                _ => 0
            };

        public static uint LeftElbow =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => (uint)Bones.LeftElbow,
                GameSelector.GameType.FreeFire_Max => (uint)Bones_MAX.LeftElbow,
                GameSelector.GameType.FreeFire_V7 => (uint)Bones_V7.LeftElbow,
                _ => 0
            };

        public static uint RightElbow =>
            GameSelector.CurrentGame switch
            {
                GameSelector.GameType.FreeFire_Normal => (uint)Bones.RightElbow,
                GameSelector.GameType.FreeFire_Max => (uint)Bones_MAX.RightElbow,
                GameSelector.GameType.FreeFire_V7 => (uint)Bones_V7.RightElbow,
                _ => 0
            };
    }
}
