using AotForms;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    internal static class SilentC
    {
        internal static int AimbotSpeed = 1;

        internal static void Work()
        {
            while (true)
            {
                if (!Config.SILENT)
                {
                    Thread.Sleep(1);
                    continue;
                }

                if (Core.Width == -1 || Core.Height == -1 || !Core.HaveMatrix)
                {
                    Thread.Sleep(1);
                    continue;
                }

                var screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);
                Entity target = GetTarget360();
                float minDistance = float.MaxValue;
                object lockObject = new object();

                Parallel.ForEach(Core.Entities.Values, entity =>
                {
                    if (entity.IsDead || entity.IsKnocked || !entity.IsKnown)
                        return;

                    var head2D = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                    if (head2D.X < 1 || head2D.Y < 1)
                        return;



                    float dist = Vector2.Distance(screenCenter, new Vector2(head2D.X, head2D.Y));
                    if (dist < minDistance)
                    {
                        lock (lockObject)
                        {
                            if (dist < minDistance)
                            {
                                minDistance = dist;
                                target = entity;
                            }
                        }
                    }
                });

                if (target != null)
                {
                    bool fired = InternalMemory.Read<bool>(Core.LocalPlayer + Offsets.sAim1, out var isFiring);
                    if (isFiring && fired)
                    {
                        bool gotWeapon = InternalMemory.Read<uint>(Core.LocalPlayer + Offsets.sAim2, out var weaponPtr);
                        if (gotWeapon && weaponPtr != 0)
                        {
                            InternalMemory.Read<Vector3>(weaponPtr + Offsets.sAim3, out var startPosition);
                            Vector3 direction = target.Head - startPosition;

                            InternalMemory.Write<Vector3>(weaponPtr + Offsets.sAim4, direction);
                        }
                    }
                }

                Thread.Sleep(0);
            }
        }

        static Entity GetTarget360()
        {
            Entity target = null;
            float closestDistance = float.MaxValue;

            foreach (var entity in Core.Entities.Values)
            {
                if (!entity.IsKnown || entity.IsDead || (Config.IgnoreKnocked && entity.IsKnocked)) continue;

                Vector3 targetPosition = entity.Head;
                var playerDistance = Vector3.Distance(Core.LocalMainCamera, targetPosition);
                if (playerDistance > Config.siMaxDistance) continue;

                if (playerDistance < closestDistance)
                {
                    closestDistance = playerDistance;
                    target = entity;
                }
            }

            return target;
        }

    }
}