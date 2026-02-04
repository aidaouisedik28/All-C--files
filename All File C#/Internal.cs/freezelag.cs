using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace YourNamespace
{
    public static class LagManager
    {
        // ------------------------
        // DEFINES
        // ------------------------
        const int MAX_PACKET_SIZE = 9999;
        const int MAX_ALLOWED_SIZE = 0;

        // ------------------------
        // FILTERS / CONSTANTS
        // ------------------------
        static readonly string DefaultFilter = "outbound and udp.PayloadLength >= 30 and udp.PayloadLength <= 800";
        static readonly WINDIVERT_LAYER Layer = WINDIVERT_LAYER.NETWORK;

        const string FILTER_GHOST = "outbound and udp.PayloadLength >= 50 and udp.PayloadLength <= 150";

        // ------------------------
        // GLOBALS
        // ------------------------
        static IntPtr GhostHR = IntPtr.Zero;
        static volatile bool GhostLagHR = false;
        static Thread ghostThread;

        static IntPtr FreHR2 = IntPtr.Zero;
        static volatile bool FreezeHR2 = false;
        static Thread freeze2Thread;

        static IntPtr FreHR = IntPtr.Zero;
        static volatile bool FreezeHR = false;
        static Thread freezeThread;

        static volatile bool TeleportActive = false;
        static IntPtr hTeleport = IntPtr.Zero;
        static Thread teleportThread;

        static IntPtr handle = IntPtr.Zero;
        static volatile bool runninglag = false;
        static volatile bool fakeLagV2Enabled = false;
        static volatile int v2P = 0;
        static Thread fixLagThread;

        // ------------------------
        // WinDivert P/Invoke
        // ------------------------
        [DllImport("WinDivert.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        static extern IntPtr WinDivertOpen(
            [MarshalAs(UnmanagedType.LPStr)] string filter,
            WINDIVERT_LAYER layer,
            short priority,
            ulong flags);

        [DllImport("WinDivert.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        static extern bool WinDivertRecv(
            IntPtr handle,
            byte[] pPacket,
            uint packetLen,
            ref uint readLen,
            ref WINDIVERT_ADDRESS pAddr);

        [DllImport("WinDivert.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        static extern bool WinDivertSend(
            IntPtr handle,
            byte[] pPacket,
            uint packetLen,
            ref uint sendLen,
            ref WINDIVERT_ADDRESS pAddr);

        [DllImport("WinDivert.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        static extern bool WinDivertClose(IntPtr handle);

        enum WINDIVERT_LAYER : uint
        {
            NETWORK = 0,
            NETWORK_FORWARD = 1,
            FLOW = 2,
            SOCKET = 3,
            REFLECT = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        struct WINDIVERT_ADDRESS
        {
            public ulong Timestamp;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
            public byte[] Reserved;
        }

        static WINDIVERT_ADDRESS NewAddr() =>
            new WINDIVERT_ADDRESS { Reserved = new byte[56] };

        // ------------------------
        // THREAD FUNCTIONS
        // ------------------------
        static void GhostLagLoop()
        {
            GhostHR = WinDivertOpen("outbound and udp.PayloadLength >= 50 and udp.PayloadLength <= 300",
                                    WINDIVERT_LAYER.NETWORK, 0, 0);
            if (GhostHR == IntPtr.Zero) return;

            byte[] packet = new byte[0xFFFF];
            uint packetLen = 0;
            var addr = NewAddr();

            while (GhostLagHR)
            {
                if (!WinDivertRecv(GhostHR, packet, (uint)packet.Length, ref packetLen, ref addr))
                    continue;

                uint sendLen = 0;
                WinDivertSend(GhostHR, packet, packetLen, ref sendLen, ref addr);
                Thread.Sleep(30);
                sendLen = 0;
                WinDivertSend(GhostHR, packet, packetLen, ref sendLen, ref addr);
            }

            WinDivertClose(GhostHR);
            GhostHR = IntPtr.Zero;
        }

        static void FreezeLoop()
        {
            FreHR = WinDivertOpen("inbound and udp.PayloadLength >= 48", WINDIVERT_LAYER.NETWORK, 0, 0);
            if (FreHR == IntPtr.Zero) return;

            byte[] packet = new byte[0xFFFF];
            uint packetLen = 0;
            var addr = NewAddr();

            while (FreezeHR)
            {
                WinDivertRecv(FreHR, packet, (uint)packet.Length, ref packetLen, ref addr);
                // Drop
            }

            WinDivertClose(FreHR);
            FreHR = IntPtr.Zero;
        }

        static void Freeze2Loop()
        {
            FreHR2 = WinDivertOpen("(inbound or outbound) and udp.PayloadLength >= 48",
                                   WINDIVERT_LAYER.NETWORK, 0, 0);
            if (FreHR2 == IntPtr.Zero) return;

            byte[] packet = new byte[MAX_PACKET_SIZE];
            uint packetLen = 0;
            var addr = NewAddr();

            while (FreezeHR2)
            {
                WinDivertRecv(FreHR2, packet, (uint)packet.Length, ref packetLen, ref addr);
                // Drop
            }

            WinDivertClose(FreHR2);
            FreHR2 = IntPtr.Zero;
        }

        static void TeleportLoop()
        {
            hTeleport = WinDivertOpen(FILTER_GHOST, WINDIVERT_LAYER.NETWORK, 0, 0);
            if (hTeleport == IntPtr.Zero) return;

            byte[] packet = new byte[0xFFFF];
            uint packetLen = 0;
            var addr = NewAddr();

            while (TeleportActive)
            {
                WinDivertRecv(hTeleport, packet, (uint)packet.Length, ref packetLen, ref addr);
                // Drop
            }

            WinDivertClose(hTeleport);
            hTeleport = IntPtr.Zero;
        }

        static void FixLagV2Loop()
        {
            while (runninglag)
            {
                if (fakeLagV2Enabled)
                {
                    if (v2P == 2)
                    {
                        CutInternet(handle);
                        v2P = 0;
                    }
                    else if (v2P == 1)
                    {
                        AllowInternet(handle);
                    }
                }
                else
                {
                    v2P = 0;
                }
                Thread.Sleep(1);
            }
        }

        // ------------------------
        // HELPERS
        // ------------------------
        static void CutInternet(IntPtr h)
        {
            var addr = NewAddr();
            byte[] packet = new byte[MAX_PACKET_SIZE];
            uint packetLen = 0;

            WinDivertRecv(h, packet, (uint)packet.Length, ref packetLen, ref addr);
            // Paket discard ediliyor
        }

        static void AllowInternet(IntPtr h)
        {
            var addr = NewAddr();
            byte[] packet = new byte[MAX_PACKET_SIZE];
            uint packetLen = 0;

            if (WinDivertRecv(h, packet, (uint)packet.Length, ref packetLen, ref addr))
            {
                uint sendLen = 0;
                WinDivertSend(h, packet, packetLen, ref sendLen, ref addr);
            }
        }

        // ------------------------
        // PUBLIC API (Formdan çağırılacak)
        // ------------------------
        public static void StartGhostLag()
        {
            if (GhostLagHR) return;
            GhostLagHR = true;
            ghostThread = new Thread(GhostLagLoop) { IsBackground = true };
            ghostThread.Start();
        }

        public static void StopGhostLag()
        {
            GhostLagHR = false;
            ghostThread?.Join(500);
        }

        public static void StartFreeze()
        {
            if (FreezeHR) return;
            FreezeHR = true;
            freezeThread = new Thread(FreezeLoop) { IsBackground = true };
            freezeThread.Start();
        }

        public static void StopFreeze()
        {
            FreezeHR = false;
            freezeThread?.Join(500);
        }

        public static void StartFreeze2()
        {
            if (FreezeHR2) return;
            FreezeHR2 = true;
            freeze2Thread = new Thread(Freeze2Loop) { IsBackground = true };
            freeze2Thread.Start();
        }

        public static void StopFreeze2()
        {
            FreezeHR2 = false;
            freeze2Thread?.Join(500);
        }

        public static void StartTeleport()
        {
            if (TeleportActive) return;
            TeleportActive = true;
            teleportThread = new Thread(TeleportLoop) { IsBackground = true };
            teleportThread.Start();
        }

        public static void StopTeleport()
        {
            TeleportActive = false;
            teleportThread?.Join(500);
        }

        public static void StartFakeLagV2()
        {
            if (runninglag) return;
            handle = WinDivertOpen(DefaultFilter, Layer, 0, 0);
            if (handle == IntPtr.Zero) return;

            runninglag = true;
            fakeLagV2Enabled = true;
            fixLagThread = new Thread(FixLagV2Loop) { IsBackground = true };
            fixLagThread.Start();
        }

        public static void StopFakeLagV2()
        {
            runninglag = false;
            fixLagThread?.Join(500);
            if (handle != IntPtr.Zero) { WinDivertClose(handle); handle = IntPtr.Zero; }
        }

        public static void SetV2Mode(int mode)
        {
            // mode = 1 (allow), 2 (cut)
            v2P = mode;
        }
    }
}
