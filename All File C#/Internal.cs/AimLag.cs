using System;
using System.Threading;
using System.Threading.Tasks;

namespace LagTrainer
{
    class Program
    {
        static bool LagEnabled = false;
        static CancellationTokenSource cts;

        static void Main(string[] args)
        {
            Console.Title = "Lag Trainer - Safe Mode";
            Console.WriteLine("=== Lag Trainer (SAFE) ===");
            Console.WriteLine("[F1] تشغيل / إيقاف Lag");
            Console.WriteLine("[ESC] خروج");
            Console.WriteLine();

            Task.Run(KeyListener);

            while (true)
            {
                if (LagEnabled)
                {
                    ApplyRandomLag(60, 140); // Lag واقعي
                    Console.WriteLine("Lag Applied...");
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }

        static void KeyListener()
        {
            while (true)
            {
                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.F1)
                {
                    ToggleLag();
                }
                else if (key == ConsoleKey.Escape)
                {
                    Environment.Exit(0);
                }
            }
        }

        static void ToggleLag()
        {
            LagEnabled = !LagEnabled;
            Console.ForegroundColor = LagEnabled ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(LagEnabled ? "Lag ON ✅" : "Lag OFF ❌");
            Console.ResetColor();
        }

        static void ApplyRandomLag(int minMs, int maxMs)
        {
            Random rnd = new Random();
            int lag = rnd.Next(minMs, maxMs);
            Thread.Sleep(lag);
        }
    }
}
