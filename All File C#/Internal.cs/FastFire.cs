using System;
using System.Numerics;
using System.Threading;

namespace AotForms
{
    internal static class AutoFire
    {
        internal static void Work()
        {
            while (true)
            {
                try
                {
                    if (!Config.FastFire)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    if (Core.LocalPlayer == 0 || !Core.HaveMatrix || Core.Width <= 0 || Core.Height <= 0 || Core.Entities == null)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    // ✅ FAST FIRE PATCH
                    if (Config.FastFire)
                    {
                        if (InternalMemory.Read(Core.LocalPlayer + Offsets.Weapon, out ulong weapon) && weapon != 0 &&
                            InternalMemory.Read(weapon + Offsets.WeaponData, out ulong weaponData) && weaponData != 0)
                        {
                            InternalMemory.Write(weaponData + 0x10, 0.02f); // Fire delay
                            InternalMemory.Write(weaponData + Offsets.WeaponRecoil, 0f); // No recoil
                            InternalMemory.Write(weapon + 0x1C, 999); // Infinite ammo
                        }
                    }

                    // ✅ INTERNAL AUTO FIRE
               
                   

                    Thread.Sleep(10);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AutoFire Error] {ex.Message}");
                    Thread.Sleep(100);
                }
            }
        }
    }
}
