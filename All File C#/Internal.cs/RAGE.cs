using System;
using System.Numerics;
using System.Threading;

namespace AotForms
{
    internal static class RageAimbot
    {
        private static Thread aimThread;
        private static volatile bool isRunning;

        public static bool IsAlive() =>
            isRunning && aimThread != null && aimThread.IsAlive;

        internal static void Work()
        {
            if (isRunning) return;

            isRunning = true;
            aimThread = new Thread(Loop)
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            aimThread.Start();
        }

        private static void Loop()
        {
            while (isRunning)
            {
                try
                {
                    if (!Config.enableAimBot || !Config.AimBotRage || !Core.HaveMatrix)
                    {
                        Thread.Sleep(2);
                        continue;
                    }

                    Entity target = FindBestTarget();
                    if (target == null)
                    {
                        Thread.Sleep(2);
                        continue;
                    }

                    // intentionally no memory write / no key usage
                    Thread.Sleep(1);
                }
                catch
                {
                    Thread.Sleep(5);
                }
            }
        }

        private static Entity FindBestTarget()
        {
            Entity best = null;
            float bestDist = float.MaxValue;

            Vector2 center = new Vector2(Core.Width / 2f, Core.Height / 2f);

            foreach (var e in Core.Entities.Values)
            {
                if (e == null || e.IsDead || e.IsTeam == Bool3.True)
                    continue;

                var s = W2S.WorldToScreen(
                    Core.CameraMatrix, e.Head, Core.Width, Core.Height);

                if (s.X <= 0 || s.Y <= 0 ||
                    s.X >= Core.Width || s.Y >= Core.Height)
                    continue;

                float d = Vector2.Distance(center, s);
                if (d < bestDist && d < Config.AimFov)
                {
                    bestDist = d;
                    best = e;
                }
            }
            return best;
        }

        internal static void Stop()
        {
            isRunning = false;
            try { aimThread?.Join(200); } catch { }
        }

        public static bool IsLocked() => false;
        public static string GetLockInfo() => "ACTIVE";
    }
}
