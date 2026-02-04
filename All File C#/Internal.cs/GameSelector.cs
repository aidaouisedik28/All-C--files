namespace AotForms
{
    internal static class GameSelector
    {
        public enum GameType
        {
            FreeFire_Normal,
            FreeFire_Max,
            FreeFire_V7
        }

        public static GameType CurrentGame { get; private set; }

        public static void SelectGame(GameType game)
        {
            CurrentGame = game;
        }

        // ===== Offsets =====
        public static class O
        {
            public static uint Il2Cpp
            {
                get
                {
                    return CurrentGame switch
                    {
                        GameType.FreeFire_Normal => Offsets.Il2Cpp,
                        GameType.FreeFire_Max => OffsetsMax.Il2Cpp,
                        GameType.FreeFire_V7 => OffsetsV7.Il2Cpp,
                        _ => 0
                    };
                }
                set
                {
                    switch (CurrentGame)
                    {
                        case GameType.FreeFire_Normal: Offsets.Il2Cpp = value; break;
                        case GameType.FreeFire_Max: OffsetsMax.Il2Cpp = value; break;
                        case GameType.FreeFire_V7: OffsetsV7.Il2Cpp = value; break;
                    }
                }
            }
        }
    }
}
