using AotForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Client;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal static class TeleKil
    {

        private static Task upPlayerTask;
        private static CancellationTokenSource cts = new();
        private static bool isRunning = false;

        internal static void Work()
        {
            if (isRunning) return; // Prevent multiple tasks
            isRunning = true;

            upPlayerTask = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (!Config.Tele)
                    {
                        await Task.Delay(1, cts.Token);
                        continue;
                    }

                    foreach (var entity in Core.Entities.Values)
                    {
                        if (!entity.IsKnown || entity.IsDead || (Config.IgnoreKnocked && entity.IsKnocked)) continue;


                        var playerDistance = Vector3.Distance(Core.LocalMainCamera, entity.Head);
                        if (playerDistance > 10) continue;

                        // Get the Root bone position of your character (local player)
                        var localRootBone = InternalMemory.Read<uint>(Core.LocalPlayer + (uint)Bones.Root, out var localRootBonePtr);
                        var localTransform = InternalMemory.Read<uint>(localRootBonePtr + 0x8, out var localTransformValue);
                        var localTransformObj = InternalMemory.Read<uint>(localTransformValue + 0x8, out var localTransformObjPtr);
                        var localMatrix = InternalMemory.Read<uint>(localTransformObjPtr + 0x20, out var localMatrixValue);

                        var localRootPosition = Transform.GetNodePosition(localRootBonePtr, out var localRootTransform);

                        // Now set the enemy's position to your character's root position
                        var enemyRootBone = InternalMemory.Read<uint>(entity.Address + (uint)Bones.Root, out var enemyRootBonePtr);
                        var enemyTransform = InternalMemory.Read<uint>(enemyRootBonePtr + 0x8, out var enemyTransformValue);
                        var enemyTransformObj = InternalMemory.Read<uint>(enemyTransformValue + 0x8, out var enemyTransformObjPtr);
                        var enemyMatrix = InternalMemory.Read<uint>(enemyTransformObjPtr + 0x20, out var enemyMatrixValue);

                        // Set the enemy's position to your root position
                        InternalMemory.Write<Vector3>(enemyMatrixValue + 0x80, localRootTransform);

                        // Optional: Adjust the position a little if needed (e.g., make it a bit more dynamic)

                        localRootTransform.Y += Config.test;
                        InternalMemory.Write<Vector3>(enemyMatrixValue + 0x80, localRootTransform);  // No offset, just teleport to you
                    }

                    await Task.Delay(1, cts.Token); // Slight delay to reduce CPU usage
                }
            }, cts.Token);
        }

        internal static void Stop()
        {
            if (!isRunning) return;

            cts.Cancel();
            isRunning = false;
        }
    }
}