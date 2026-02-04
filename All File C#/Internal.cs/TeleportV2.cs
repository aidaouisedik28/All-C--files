using AotForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal static class TeleportV2
    {
        private static Task tele;
        private static CancellationTokenSource cts = new();
        private static bool isRunning = false;

        private static Entity? targetEnemy = null;

        public static void Work()
        {
            if (isRunning) return;

            cts = new CancellationTokenSource();
            isRunning = true;

            tele = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (!Config.TeleportV2)
                    {
                        targetEnemy = null;
                        await Task.Delay(1, cts.Token);
                        continue;
                    }

                    // البحث عن هدف جديد إذا لم يكن هناك هدف أو كان ميتاً
                    if (targetEnemy == null || targetEnemy.IsDead || (Config.IgnoreKnocked && targetEnemy.IsKnocked))
                    {
                        targetEnemy = Core.Entities.Values
                            .Where(entity => !entity.IsDead
                                             && (!Config.IgnoreKnocked || !entity.IsKnocked)
                                             && Vector3.Distance(Core.LocalMainCamera, entity.Head) <= 500
                                             && entity.Head != Vector3.Zero) // تحقق من أن الموقع صحيح
                            .OrderBy(e => Vector3.Distance(Core.LocalMainCamera, e.Head))
                            .FirstOrDefault();
                    }

                    if (targetEnemy != null)
                    {
                        try
                        {
                            var localRootBone = InternalMemory.Read<uint>(Core.LocalPlayer + (uint)GameBones.Root, out var localRootBonePtr);
                            var localTransform = InternalMemory.Read<uint>(localRootBonePtr + 0x8, out var localTransformValue);
                            var localTransformObj = InternalMemory.Read<uint>(localTransformValue + 0x8, out var localTransformObjPtr);
                            var localMatrix = InternalMemory.Read<uint>(localTransformObjPtr + 0x20, out var localMatrixValue);

                            var enemyRootBone = InternalMemory.Read<uint>(targetEnemy.Address + (uint)GameBones.Root, out var enemyRootBonePtr);
                            var enemyRootPosition = Transform.GetNodePosition(enemyRootBonePtr, out var enemyRootTransform);

                            // تحقق من أن الموقع صحيح وليس (0,0,0) أو قيمة غير منطقية
                            if (enemyRootTransform != Vector3.Zero
                                && !float.IsNaN(enemyRootTransform.X)
                                && !float.IsNaN(enemyRootTransform.Y)
                                && !float.IsNaN(enemyRootTransform.Z)
                                && Math.Abs(enemyRootTransform.X) < 10000
                                && Math.Abs(enemyRootTransform.Y) < 10000
                                && Math.Abs(enemyRootTransform.Z) < 10000
                                && localMatrixValue != 0)
                            {
                                InternalMemory.Write<Vector3>(localMatrixValue + 0x80, enemyRootTransform);
                            }
                            else
                            {
                                // الموقع غير صحيح، البحث عن هدف جديد
                                targetEnemy = null;
                            }
                        }
                        catch
                        {
                            // خطأ في قراءة الذاكرة، إعادة البحث عن هدف
                            targetEnemy = null;
                        }
                    }

                    await Task.Delay(5, cts.Token);
                }
            }, cts.Token);
        }

        public static void Stop()
        {
            if (!isRunning) return;

            cts.Cancel();
            isRunning = false;
            targetEnemy = null;
        }
    }
}
