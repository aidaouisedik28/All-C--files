using AotForms;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    /// <summary>
    /// Teleport Mark - وضع علامة على الخريطة والانتقال إليها
    /// مثل TeleportV2 لكن تنتقل لعلامة محفوظة بدلاً من عدو
    /// </summary>
    internal static class TeleportMark
    {
        private static Task teleportTask;
        private static CancellationTokenSource cts = new();
        private static bool isRunning = false;

        // العلامة المحفوظة
        private static Vector3 savedMark = Vector3.Zero;
        private static bool hasMarkedPosition = false;

        /// <summary>
        /// تعيين العلامة على الموقع الحالي للاعب
        /// </summary>
        public static bool SetMark()
        {
            try
            {
                if (Core.LocalPlayer == 0) return false;

                // قراءة موقع اللاعب الحالي باستخدام نفس طريقة TeleportV2
                var localRootBone = InternalMemory.Read<uint>(Core.LocalPlayer + (uint)Bones.Root, out var localRootBonePtr);
                if (!localRootBone || localRootBonePtr == 0) return false;

                var rootPosition = Transform.GetNodePosition(localRootBonePtr, out var localPosition);
                if (!rootPosition || localPosition == Vector3.Zero) return false;

                // التحقق من صحة الموقع
                if (float.IsNaN(localPosition.X) || float.IsNaN(localPosition.Y) || float.IsNaN(localPosition.Z))
                    return false;

                if (Math.Abs(localPosition.X) > 10000 || Math.Abs(localPosition.Y) > 10000 || Math.Abs(localPosition.Z) > 10000)
                    return false;

                // حفظ العلامة
                savedMark = localPosition;
                hasMarkedPosition = true;

                Console.WriteLine($"[TeleportMark] Mark set at: {localPosition}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TeleportMark] SetMark error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// الحصول على موقع العلامة المحفوظة
        /// </summary>
        public static Vector3 GetMark() => savedMark;

        /// <summary>
        /// التحقق من وجود علامة محفوظة
        /// </summary>
        public static bool HasMark() => hasMarkedPosition;

        /// <summary>
        /// الحصول على المسافة بين اللاعب والعلامة
        /// </summary>
        public static float GetDistanceToMark()
        {
            if (!hasMarkedPosition || Core.LocalPlayer == 0) return -1f;

            try
            {
                var localRootBone = InternalMemory.Read<uint>(Core.LocalPlayer + (uint)Bones.Root, out var localRootBonePtr);
                if (!localRootBone || localRootBonePtr == 0) return -1f;

                var rootPosition = Transform.GetNodePosition(localRootBonePtr, out var localPosition);
                if (!rootPosition) return -1f;

                return Vector3.Distance(localPosition, savedMark);
            }
            catch
            {
                return -1f;
            }
        }

        /// <summary>
        /// بدء العمل - ينتقل للعلامة بشكل مستمر مثل TeleportV2
        /// </summary>
        public static void Work()
        {
            if (isRunning) return;
            if (!hasMarkedPosition || savedMark == Vector3.Zero)
            {
                Console.WriteLine("[TeleportMark] No mark set!");
                return;
            }

            cts = new CancellationTokenSource();
            isRunning = true;

            teleportTask = Task.Run(async () =>
            {
                Console.WriteLine("[TeleportMark] Started teleporting to mark...");
                
                while (!cts.Token.IsCancellationRequested && Config.TeleportMarkEnabled)
                {
                    try
                    {
                        if (Core.LocalPlayer == 0)
                        {
                            await Task.Delay(10, cts.Token);
                            continue;
                        }

                        // نفس طريقة TeleportV2 للانتقال
                        var localRootBone = InternalMemory.Read<uint>(Core.LocalPlayer + (uint)Bones.Root, out var localRootBonePtr);
                        var localTransform = InternalMemory.Read<uint>(localRootBonePtr + 0x8, out var localTransformValue);
                        var localTransformObj = InternalMemory.Read<uint>(localTransformValue + 0x8, out var localTransformObjPtr);
                        var localMatrix = InternalMemory.Read<uint>(localTransformObjPtr + 0x20, out var localMatrixValue);

                        if (localMatrixValue != 0 && savedMark != Vector3.Zero)
                        {
                            // الانتقال للعلامة المحفوظة
                            InternalMemory.Write<Vector3>(localMatrixValue + 0x80, savedMark);
                        }
                    }
                    catch
                    {
                        // تجاهل الأخطاء والمتابعة
                    }

                    await Task.Delay(5, cts.Token);
                }
                
                Console.WriteLine("[TeleportMark] Stopped.");
            }, cts.Token);
        }

        /// <summary>
        /// إيقاف
        /// </summary>
        public static void Stop()
        {
            if (!isRunning) return;

            try
            {
                cts.Cancel();
            }
            catch { }

            isRunning = false;
        }

        /// <summary>
        /// مسح العلامة المحفوظة
        /// </summary>
        public static void ClearMark()
        {
            savedMark = Vector3.Zero;
            hasMarkedPosition = false;
        }

        /// <summary>
        /// الانتقال للعلامة مرة واحدة (يستدعى من ESP.cs)
        /// </summary>
        public static bool TeleportToMark()
        {
            if (!hasMarkedPosition || savedMark == Vector3.Zero) return false;

            try
            {
                if (Core.LocalPlayer == 0) return false;

                var localRootBone = InternalMemory.Read<uint>(Core.LocalPlayer + (uint)Bones.Root, out var localRootBonePtr);
                var localTransform = InternalMemory.Read<uint>(localRootBonePtr + 0x8, out var localTransformValue);
                var localTransformObj = InternalMemory.Read<uint>(localTransformValue + 0x8, out var localTransformObjPtr);
                var localMatrix = InternalMemory.Read<uint>(localTransformObjPtr + 0x20, out var localMatrixValue);

                if (localMatrixValue != 0)
                {
                    InternalMemory.Write<Vector3>(localMatrixValue + 0x80, savedMark);
                    return true;
                }
            }
            catch { }

            return false;
        }
    }
}
