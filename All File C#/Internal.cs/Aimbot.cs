using System;
using System.Numerics;

namespace AotForms
{
    internal static class Aimbot
    {
        internal static void Work()
        {
            // Safe no-op
        }

        static Entity? GetTarget(TargetingMode mode)
        {
            if (Core.Width <= 0 || Core.Height <= 0 || !Core.HaveMatrix)
                return null;

            Entity? best = null;
            float bestScore = float.MaxValue;
            var center = new Vector2(Core.Width / 2f, Core.Height / 2f);

            foreach (var e in Core.Entities.Values)
            {
                if (!e.IsKnown || e.IsDead || (Config.IgnoreKnocked && e.IsKnocked))
                    continue;

                var head2D = W2S.WorldToScreen(Core.CameraMatrix, e.Head, Core.Width, Core.Height);
                if (head2D.X < 1 || head2D.Y < 1)
                    continue;

                float distPlayer = Vector3.Distance(Core.LocalMainCamera, e.Head);
                if (distPlayer > Config.AimBotMaxDistance)
                    continue;

                float score = mode switch
                {
                    TargetingMode.ClosestToCrosshair =>
                        Vector2.Distance(center, head2D),

                    TargetingMode.LowestHealth =>
                        e.Health,

                    TargetingMode.ClosestToPlayer =>
                        distPlayer,

                    TargetingMode.Target360 =>
                        distPlayer,

                    _ =>
                        Vector2.Distance(center, head2D)
                };

                if (score < bestScore && score <= Config.AimFov)
                {
                    bestScore = score;
                    best = e;
                }
            }

            return best;
        }

        internal static Entity? SelectTarget(TargetingMode mode)
        {
            return GetTarget(mode);
        }

        internal static void teleportenemy()
        {
            // disabled
        }
    }
}
