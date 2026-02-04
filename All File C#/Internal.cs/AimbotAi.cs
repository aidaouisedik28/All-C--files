using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImGuiNET;

namespace AotForms
{
    internal static class AimbotVisible
    {
        internal static void Work()
        {
            while (true)
            {
                if (!Config.AimbotVisible)
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
                    AimAtTarget(target);
                }
                Thread.Sleep(1);
            }
        }

        private static Entity FindBestTarget()
        {
            Entity bestTarget = null;
            float closestDistance = float.MaxValue;
            var screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);

            foreach (var entity in Core.Entities.Values)
            {
                if (entity.IsDead || (Config.IgnoreKnocked && entity.IsKnocked)) continue;
                var head2D = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                if (head2D.X < 1 || head2D.Y < 1) continue;
                float playerDistance = Vector3.Distance(Core.LocalMainCamera, entity.Head);
                if (playerDistance > Config.AimBotMaxDistance) continue;
                var crosshairDistance = Vector2.Distance(screenCenter, head2D);
                if (crosshairDistance < closestDistance && crosshairDistance <= Config.AimFov)
                {
                    closestDistance = crosshairDistance;
                    bestTarget = entity;
                }
            }
            return bestTarget;
        }

        private static void AimAtTarget(Entity target)
        {
            if (target == null || target.Address == 0) return;

            if (!InternalMemory.Read<uint>(target.Address + 0x3d8, out uint m_HeadCollider) || m_HeadCollider == 0)
                return;
            InternalMemory.Write(target.Address + 0x50, m_HeadCollider);
        }
    }
}
