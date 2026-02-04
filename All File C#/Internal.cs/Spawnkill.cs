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
    internal static class telepapa
    {
        private static Task upPlayerTask;
        private static CancellationTokenSource cts = new();
        private static bool isRunning = false;
        internal static void Work()
        {
            if (isRunning) return;

            upPlayerTask = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (!Config.hamba)
                    {
                        await Task.Delay(1, cts.Token);
                        continue;
                    }

                    var closest = Core.Entities.Values
                        .Where(e => e.IsKnown && !e.IsDead && (!Config.IgnoreKnocked || !e.IsKnocked))
                        .OrderBy(e => Vector3.Distance(Core.LocalMainCamera, e.Head))
                        .FirstOrDefault();

                    if (closest != null)
                    {
                        float distance = Vector3.Distance(Core.LocalMainCamera, closest.Head);
                        if (distance <= 200)
                        {
                            // 🔁 Get enemy transform
                            var enemyRootBone = InternalMemory.Read<uint>(closest.Address + (uint)Bones.Root, out var enemyRootBonePtr);
                            var enemyTransform = InternalMemory.Read<uint>(enemyRootBonePtr + 0x8, out var enemyTransformValue);
                            var enemyTransformObj = InternalMemory.Read<uint>(enemyTransformValue + 0x8, out var enemyTransformObjPtr);
                            var enemyMatrix = InternalMemory.Read<uint>(enemyTransformObjPtr + 0x20, out var enemyMatrixValue);

                            var enemyRootPosition = Transform.GetNodePosition(enemyRootBonePtr, out var enemyRootTransform);

                            // 📍 Write to local player matrix (teleport yourself)
                            var localRootBone = InternalMemory.Read<uint>(Core.LocalPlayer + (uint)Bones.Root, out var localRootBonePtr);
                            var localTransform = InternalMemory.Read<uint>(localRootBonePtr + 0x8, out var localTransformValue);
                            var localTransformObj = InternalMemory.Read<uint>(localTransformValue + 0x8, out var localTransformObjPtr);
                            var localMatrix = InternalMemory.Read<uint>(localTransformObjPtr + 0x20, out var localMatrixValue);

                            InternalMemory.Write<Vector3>(localMatrixValue + 0x80, enemyRootTransform); // 👈 Move to enemy
                        }
                    }

                    await Task.Delay(1, cts.Token);
                }
            }, cts.Token);
        }


    }
}
