using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using AotForms;

namespace AotForm
{
    internal static class TeleportMap
    {
        private static Task tele;
        private static CancellationTokenSource cts = new();
        private static bool isRunning = false;

        public static readonly string[] TeleportNames =
        {
            "Peak",
            "Clock",
            "Mill",
            "Sentosa",
            "Observatory",
            "Plantation",
            "Factory",
            "Riverside",
            "Bullseye",
            "Graveyard",
            "Bimasakti",
            "Katulistiwa",
            "Kotatua",
            "Mars Electric",
            "Capetown",
            "Keraton"
        };

      
        private static readonly Vector3[] TeleportPositions =
        {
            new(663.7f, 75.2f, 76.3f),      // Peak
            new(217.8f, 11.9f, -102.2f),    // Clock
            new(893.9f, 54.7f, 501.8f),     // Mill
            new(1181.9f, 5.1f, -404.2f),    // Sentosa
            new(-147.9f, 32.2f, 398.6f),    // Observatory
            new(417.1f, 10.8f, 506.6f),     // Plantation
            new(433.3f, 15.8f, -214.0f),    // Factory
            new(729.8f, 17.1f, 543.3f),     // Riverside
            new(65.0f, 9.8f, 656.5f),       // Bullseye
            new(652.4f, 24.9f, -272.3f),    // Graveyard
            new(-16.1f, -0.3f, 64.2f),      // Bimasakti
            new(174.9f, 1.9f, 370.1f),      // Katulistiwa
            new(949.6f, 19.9f, -42.2f),     // Kotatua
            new(516.8f, 24.7f, -535.1f),    // Mars Electric
            new(1297.6f, 0.0f, 84.8f),      // Capetown
            new(1119.7f, 17.7f, 292.0f),    // Keraton
        };
        //Discord
        // https://discord.gg/8TZsvCHDmX
        // https://discord.gg/8TZsvCHDmX
        // 
        public static void Work()
        {
            if (isRunning) return;

            cts = new CancellationTokenSource();
            isRunning = true;

            tele = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (!Config.TeleportMap || Core.LocalPlayer == 0)
                    {
                        await Task.Delay(5, cts.Token);
                        continue;
                    }

                    int index = Config.TeleportIndex;

                    if (index >= 0 && index < TeleportPositions.Length)
                        DoTP(TeleportPositions[index]);

                    await Task.Delay(100, cts.Token);
                }
            }, cts.Token);
        }

       
        private static void DoTP(Vector3 pos)
        {
            if (!InternalMemory.Read<uint>(Core.LocalPlayer + (uint)Bones.Root, out var root)) return;
            if (!InternalMemory.Read<uint>(root + 0x8, out var t1)) return;
            if (!InternalMemory.Read<uint>(t1 + 0x8, out var t2)) return;
            if (!InternalMemory.Read<uint>(t2 + 0x20, out var matrix)) return;

            InternalMemory.Write(matrix + 0x80, pos);
        }

     
        public static void Stop()
        {
            if (!isRunning) return;

            cts.Cancel();
            isRunning = false;
        }
    }
}
