using Guna.UI2.WinForms;
using ImGuiNET;
using Memory;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Text;
using NAudio.Wave;
using AotForms.Components;
using static AotForms.Config;
using static AotForms.WinAPI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Color = System.Drawing.Color;
using Size = System.Drawing.Size;

namespace AotForms
{
    internal partial class ESP : ClickableTransparentOverlay.Overlay
    {
        IntPtr hWnd;
        IntPtr HDPlayer;
        private CX memoryfast = new CX();
        private bool camera = false;
        private bool fly = false;
        private bool jump = false;
        private bool left = false;
        private bool vision = false;
        private bool brust = false;
        private bool speedext = false;
        private bool wallhackkk = false;

        private IEnumerable<long> visionResult;
        private IEnumerable<long> cameraResult;
        private IEnumerable<long> speedResult;
        private float previousCameraVal = Config.cameraVal;
        private float previousVisionVal = Config.visionVal;
        private bool _styleInitialized = false;
        private const short DefaultMaxHealth = 200;
        private Vector4 lineColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private Vector4 fovColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private Vector4 boxColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private Vector4 box3DColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private Vector4 skeletonColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private static Dictionary<string, IntPtr> _weaponIcons = new();
        private static IntPtr _espEditorPreviewPtr = IntPtr.Zero;
        private static uint _espEditorPreviewW;
        private static uint _espEditorPreviewH;
        private static bool _espEditorPreviewLoadAttempted;
        private static IntPtr _espEditorWeaponPreviewPtr = IntPtr.Zero;
        private static uint _espEditorWeaponPreviewW;
        private static uint _espEditorWeaponPreviewH;
        private static bool _espEditorWeaponPreviewLoadAttempted;
        private static Vector2 _mainPanelPos;
        private static Vector2 _mainPanelSize;
        private static Vector2 _menuPanelPos = new Vector2(14f, 14f);

        private enum EspEditorDragKind { None, LineStart, LineEnd, Box, Name, Distance, Health, Weapon, Hacker }
        private enum EspEditorColorTarget { None, Line, Box, Name, Health, Weapon, Hacker }

        private EspEditorDragKind _espEditorDragKind = EspEditorDragKind.None;
        private EspEditorColorTarget _espEditorColorTarget = EspEditorColorTarget.None;
        private bool _espEditorColorPopupOpen;
        private Vector2 _espEditorColorPopupPos = Vector2.Zero;
        private EspEditorColorTarget _espEditorColorClickTarget = EspEditorColorTarget.None;
        private Vector2 _espEditorColorClickStart = Vector2.Zero;

        private static float EspEditorDistSq(Vector2 a, Vector2 b)
        {
            float dx = a.X - b.X, dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }

        private static float EspEditorDistToSegmentSq(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = ab.X * ab.X + ab.Y * ab.Y;
            if (t < 1e-6f) return EspEditorDistSq(p, a);
            float u = Math.Clamp(((p.X - a.X) * ab.X + (p.Y - a.Y) * ab.Y) / t, 0f, 1f);
            float px = a.X + ab.X * u;
            float py = a.Y + ab.Y * u;
            float dx = p.X - px, dy = p.Y - py;
            return dx * dx + dy * dy;
        }

        private static bool EspEditorPointInRect(Vector2 p, Vector2 mn, Vector2 mx)
        {
            return p.X >= mn.X && p.X <= mx.X && p.Y >= mn.Y && p.Y <= mx.Y;
        }

        private static bool EspEditorPointInRectPad(Vector2 p, Vector2 mn, Vector2 mx, float pad)
        {
            return p.X >= mn.X - pad && p.X <= mx.X + pad && p.Y >= mn.Y - pad && p.Y <= mx.Y + pad;
        }

        private static readonly Random _hackerRandom = new();
        private static uint _hackerEntityAddress = 0;
        private static DateTime _lastHackerUpdate = DateTime.MinValue;
        private const double HackerUpdateIntervalSeconds = 3.0;

        // HackerDetect (manual H-key marking)
        private static int hackerMarkIndex = 0;
        private static bool hKeyPressed = false;
        internal static uint selectedHackerAddress = 0;
        // Premium Indigo/Violet accent theme
        public static Vector4 ThemeColor = new Vector4(255f / 255f, 0f / 255f, 0f / 255f, 1f);
        public static ImFontPtr HeaderFont;
        public static ImFontPtr LebelFont;
        public static ImFontPtr TextFont;
        public static ImFontPtr TabIconFont;
        public static ImFontPtr EspNameFont;
        internal static readonly TimerManager g_TimerManager = new TimerManager();

        private static float _findWindowTimer = 0f;
        private static int _espMatchEnemyStreak;
        private static bool _espMatchHudLocked;
        private static bool _uiVisible = true;
        public static float _uiAlpha = 1.0f;
        private static float _uiFadeSpeed = 15.0f;
        private static float _uiFadeOutSpeed = 48.0f;
        private static bool _insertKeyPressedLastFrame = false;
        private static bool _vkInsertDownLast = false;
        private static readonly Dictionary<int, float> _tabFade = new();
        private static readonly Dictionary<int, float> _tabAnim = new();
        private static float _tabDropLocalY = -1f;
        private static float _tabDropTargetLocalY = -1f;
        private static float _tabDropLocalX = -1f;
        private static float _tabDropTargetLocalX = -1f;
        private static float _tabDropLocalW = 0f;
        private static float _tabDropLocalH = 0f;
        private static bool _tabDropTargetValid = false;
        private static readonly Dictionary<int, IntPtr> _tabIconPtrs = new();
        private static bool _tabIconsInitialized = false;

        private static bool _faMergedIntoTextFont;
        private static byte[]? _retainedFaSolidBytes;
        private static bool _showTabPopup = false;
        private static float _tabPopupAnim = 0f;
        private static int _previousTab = 0;
        private static int _activeTab = UiMetrics.LoginTabIndex;
        private static string? _activeBindingId = null;
        internal static string? ActiveBindingId
        {
            get => _activeBindingId;
            set => _activeBindingId = value;
        }

        internal static bool FaMergedIntoTextFont => _faMergedIntoTextFont;

        private static bool _hooksFunctionsEnabled = true;
        private static bool _hooksToolsEnabled = true;
        private static bool _resetGuestEnabled = false;

        private const string BrandTitle = "KANISHK ";
        private const string BrandTitleAccent = "CHEATS";
        private const string BrandNameWhite = "KANISHK";
        private const string BrandNamePurple = "CHEAT";
        private const string BrandSubtitle = "";
        private const string BrandDiscord = "discord.gg/kanishkcheat";
        private static Vector4 BrandAccentBlue => ThemeColor;

        private static class UiMetrics
        {
            public const float WindowW = 340f;
            public const float WindowH = 440f;
            public const float LoginWindowW = 320f;
            public const float LoginWindowH = 460f;
            public const float OuterPad = 0f;
            public const float ChromeH = 64f;
            public const float ChromeBodyGap = 6f;
            public const float WindowGlassAlpha = 0.78f;
            public const float PanelGlassAlpha = 0.70f;
            public const float CardGlassAlpha = 0.55f;
            public const float PanelPad = 4f;
            public const float TabW = 36f;
            public const float TabH = 34f;
            public const float SidebarLogo = 40f;
            public const float TabUnderlineW = 20f;
            public const float TabUnderlineH = 2.5f;
            public const int LoginTabIndex = -1;
            public const int MainSidebarTabCount = 5;
        }

        private static Vector2 GetMenuWindowSize()
        {
            if (_flowPhase == FlowPhase.Login || _flowPhase == FlowPhase.PostLoginLoad)
                return new Vector2(UiMetrics.LoginWindowW, UiMetrics.LoginWindowH);
            return new Vector2(UiMetrics.WindowW, UiMetrics.WindowH);
        }

        private static bool IsEmulatorConnecting() =>
            _flowPhase == FlowPhase.Main &&
            _emulatorInitTask != null &&
            !_emulatorInitTask.IsCompleted;

        private enum FlowPhase { Login, PostLoginLoad, Main }
        private static FlowPhase _flowPhase = FlowPhase.Login;
        private static string _loginLicense = "";
        private static string _loginUsername = "";
        private static string _loginPassword = "";
        private static string _loginError = "";
        private static bool _loginBusy = false;
        private static float _loginSwipeProgress = 0f;
        private static bool _loginSwipeSuccess = false;
        private static Task<string?>? _emulatorInitTask;

        private static void TryStartBackgroundConnect()
        {
            if (!InternalMemory.IsReady)
                return;

            if (_emulatorInitTask != null && !_emulatorInitTask.IsCompleted)
                return;

            if (_initCompletionHandled && Offsets.Il2Cpp != 0)
                return;

            _initError = null;
            _initCompletionHandled = false;
            _emulatorInitTask = BootstrapRuntime.InitEmulatorAsync();
        }
        private static volatile bool _loginDone;
        private static bool _loginOk;
        private static string _loginResultMsg = "";

        private static float _loaderTime = 0f;
        private static float _connectedFade = 0f;
        private static bool _connected = false;
        private static string? _initError;
        private static bool _initCompletionHandled;
        private static bool _soundPlayed = false;
        private static float _loaderAngle = 0f;
        private static float _pulseAlpha = 0.5f;
        private static bool _pulseIncreasing = true;

        private static bool _showHookSuccessScreen;
        private static bool _cheatsRuntimeStarted;
        private static bool _pendingFeatureRuntimeApply;
        private static float _hookSuccessAnimTime;
        private const float HookSuccessAnimDuration = 2.75f;

        private struct UiTriangleParticle
        {
            public Vector2 Pos;
            public Vector2 Vel;
            public float Rot;
            public float RotSpeed;
            public float Size;
            public float Alpha;
        }

        private static UiTriangleParticle[]? _uiTriangleParticles;
        private static Vector2 _uiTriangleParticleArea;
        private static readonly Random _uiParticleRng = new(9182);

        private static readonly Vector4 PanelBg = new(6f / 255f, 6f / 255f, 8f / 255f, 1f); // #060608
        private static readonly Vector4 CardHeaderBg = new(19f / 255f, 21f / 255f, 29f / 255f, 1f); // #13151D
        private static readonly Vector4 CardBodyBg = new(11f / 255f, 12f / 255f, 16f / 255f, 1f); // #0B0C10
        private static readonly Vector4 PanelBorder = new(24f / 255f, 25f / 255f, 31f / 255f, 1f);
        private static readonly string[] TabFaGlyphs =
        {
            "\uf05b", // Aim
            "\uf06e", // ESP
            "\uf06d", // Movement
            "\uf0e7", // Kills
            "\uf013", // Settings
        };
        private static readonly string[] TabLabels = { "Aim", "ESP", "Move", "Kill", "Settings" };
        private static readonly Vector4 TabAccentRed = new(232f / 255f, 20f / 255f, 26f / 255f, 1f);
        private static readonly Vector4 HexBrandRed = new(232f / 255f, 20f / 255f, 26f / 255f, 1f);
        private static readonly Vector4 HexTabUnderlineRed = new(232f / 255f, 20f / 255f, 26f / 255f, 1f);
        private readonly string[] _comboItems2 = { "Closest To Crosshair", "Target360", "Closest To Player", "Lowest Health" };
        private readonly string[] _comboItems1 = { "Silent Aim", "Aimbot Rage" };
        private readonly string[] _comboItems = { "Top", "Center", "Bottom" };
        private readonly string[] _espPreviewItems = { "Static", "Dynamic" };
        /// <summary>Labels match <see cref="EspNameBoxAnchor"/> order.</summary>
        private readonly string[] _espEditorNameItems =
        {
            "Top of box",
            "Bottom of box",
            "Top left",
            "Top right",
            "Bottom left",
            "Bottom right",
            "Middle left",
            "Middle right",
        };
        /// <summary>Labels match <see cref="EspDistanceBoxAnchor"/> order.</summary>
        private readonly string[] _espEditorDistanceItems =
        {
            "Inside nameplate",
            "Above name (when name at top)",
            "Top of box",
            "Bottom of box",
            "Top left",
            "Top right",
            "Bottom left",
            "Bottom right",
            "Middle left",
            "Middle right",
        };
        private readonly string[] _espEditorHealthItems =
        {
            "Right side",
            "Left side",
            "Top of box",
            "Bottom of box",
        };
        private readonly string[] _espEditorWeaponItems =
        {
            "Top of box",
            "Bottom of box",
            "Top left",
            "Top right",
            "Bottom left",
            "Bottom right",
            "Middle left",
            "Middle right",
        };
        private float _espEditorHealthSectionAnim;
        private float _visLineComboAnim;
        private float _visInfoComboAnim;
        private float _visNameComboAnim;
        private float _visWeaponComboAnim;
        private readonly string[] _headerItems = { "Aim", "Esp", "Colors", "Extras", "Misc" };
        private int _selectedHeader, _comboBox, _comboBox1, _comboBox2, _espPreviewStyleCombo;
        private struct EntityRenderData
        {
            public Vector2 headScreenPos;
            public Vector2 bottomScreenPos;
            public float Distance;
            public bool IsValid;
        }
        private string CamReplacePattern => MakeAob(cameraVal);
        private string VisionReplacePattern => MakeAob(visionVal);
        private async void CameraOn()
        {
            string[] processname = { "HD-Player" };
            bool success = memoryfast.SetProcess(processname);
            if (!success)
            {
                return;
            }
            if (cameraResult == null)
            {
                cameraResult = await memoryfast.AoBScan("00 00 80 3F 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 80 7F 00 00 80 7F 00 00 80 7F 00 00 80 FF");
                Console.Beep(2000, 600);
            }
            if (cameraResult != null && cameraResult.Any())
            {
                foreach (long id in cameraResult)
                {
                    memoryfast.AobReplace(id, CamReplacePattern);
                }
            }
        }
        private async void CameraOff()
        {
            foreach (long id in cameraResult)
            {
                memoryfast.AobReplace(id, "00 00 80 3F");
            }


        }

        private async void VisionOn()
        {
            string[] processname = { "HD-Player" };
            bool success = memoryfast.SetProcess(processname);
            if (!success)
            {
                return;
            }
            if (visionResult == null)
            {
                visionResult = await memoryfast.AoBScan("DB 0F 49 40 10 2A 00 EE 00 10 80 E5 10 3A 01 EE 14 10 80 E5");
                Console.Beep(2000, 600);
            }
            if (visionResult != null && visionResult.Any())
            {
                foreach (long id in visionResult)
                {
                    memoryfast.AobReplace(id, VisionReplacePattern);
                }
            }
        }
        private async void VisionOff()
        {
            foreach (long id in visionResult)
            {
                memoryfast.AobReplace(id, "DB 0F 49 40 10 2A 00 EE 00 10 80 E5 10 3A 01 EE 14 10 80 E5");
            }
        }

        private async Task<bool> ApplyMemoryPatch(CX memory, string[] searchPatterns, string[] replacePatterns)
        {
            bool success = false;

            for (int i = 0; i < searchPatterns.Length; i++)
            {
                if (string.IsNullOrEmpty(searchPatterns[i]) || string.IsNullOrEmpty(replacePatterns[i]))
                    continue;

                var matches = await memory.AoBScan(searchPatterns[i]);
                if (matches.Any())
                {
                    foreach (long id in matches)
                    {
                        memory.AobReplace(id, replacePatterns[i]);
                    }
                    success = true;
                }
            }

            return success;
        }

        private async Task flyahackk()
        {
            string processName = "HD-Player";
            CX memoryfast = new CX();

            if (!memoryfast.SetProcess(new[] { processName }))
            {
                return;
            }

            string[] searchPatterns =
            {
                "AC C5 27 37 00 10 A0 E1"
            };

            string[] replacePatterns =
            {
                "AC C5 A9 3F 00 10 A0 E1"
            };

            try
            {
                if (Config.flyhack)
                {
                    bool success = await ApplyMemoryPatch(memoryfast, searchPatterns, replacePatterns);
                }
                else
                {
                    bool success = await ApplyMemoryPatch(memoryfast, replacePatterns, searchPatterns);
                }
            }
            catch
            {
            }
        }

        private async Task highjump()
        {
            string processName = "HD-Player";
            CX memoryfast = new CX();

            if (!memoryfast.SetProcess(new[] { processName }))
            {
                return;
            }

            string[] searchPatterns =
            {
                "00 00 C8 42 F0 48 2D E9 10 B0 8D E2 01 50 A0 E1 00 40 A0 E1 00 00 55 E3 02 00 00 0A BC 70 D5 E1"
            };

            string[] replacePatterns =
            {
                "00 00 7A 43 F0 48 2D E9 10 B0 8D E2 01 50 A0 E1 00 40 A0 E1 00 00 55 E3 02 00 00 0A BC 70 D5 E1"
            };

            try
            {
                if (Config.highjumppp)
                {
                    bool success = await ApplyMemoryPatch(memoryfast, searchPatterns, replacePatterns);
                }
                else
                {
                    bool success = await ApplyMemoryPatch(memoryfast, replacePatterns, searchPatterns);
                }
            }
            catch
            {
            }
        }

        private async Task cameraleftt()
        {
            string processName = "HD-Player";
            CX memoryfast = new CX();

            if (!memoryfast.SetProcess(new[] { processName }))
            {
                return;
            }

            string[] searchPatterns =
            {
                "00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 80 FF"
            };

            string[] replacePatterns =
            {
                "00 00 00 00 00 00 30 40 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 80 7F 00 00 80 7F 00 00 80 7F 00 00 80 FF"
            };

            try
            {
                if (Config.cameraleftt)
                {
                    bool success = await ApplyMemoryPatch(memoryfast, searchPatterns, replacePatterns);
                }
                else
                {
                    bool success = await ApplyMemoryPatch(memoryfast, replacePatterns, searchPatterns);
                }
            }
            catch
            {
            }
        }

        private async Task visionhack()
        {
            string processName = "HD-Player";
            CX memoryfast = new CX();

            if (!memoryfast.SetProcess(new[] { processName }))
            {
                return;
            }

            string[] searchPatterns =
            {
                "DB 0F 49 40 10 2A 00 EE 00"
            };

            string[] replacePatterns =
            {
                "DB 0F 99 40 10 2A 00 EE 00"
            };

            try
            {
                if (Config.visionhackkk)
                {
                    bool success = await ApplyMemoryPatch(memoryfast, searchPatterns, replacePatterns);
                }
                else
                {
                    bool success = await ApplyMemoryPatch(memoryfast, replacePatterns, searchPatterns);
                }
            }
            catch
            {
            }
        }

        private async Task wallhackrisk()
        {
            string processName = "HD-Player";
            CX memoryfast = new CX();

            if (!memoryfast.SetProcess(new[] { processName }))
            {
                return;
            }

            string[] searchPatterns =
            {
                "3F AE 47 81 3F AE 47 81 3F AE 47 81 3F 00 1A B7 EE DC 3A 9F ED 30 00 4F E2"
            };

            string[] replacePatterns =
            {
                "3F AE 47 81 3F AE 47 81 BF AE 47 81 3F AE 1A B7 EE DC 3A 9F ED 30 00 4F E2"
            };

            try
            {
                if (Config.wallhack)
                {
                    bool success = await ApplyMemoryPatch(memoryfast, searchPatterns, replacePatterns);
                }
                else
                {
                    bool success = await ApplyMemoryPatch(memoryfast, replacePatterns, searchPatterns);
                }
            }
            catch
            {
            }
        }

        private async Task brustfire()
        {
            string processName = "HD-Player";
            CX memoryfast = new CX();

            if (!memoryfast.SetProcess(new[] { processName }))
            {
                return;
            }

            string[] searchPatterns =
            {
                "00 00 80 40 33 33 93 40 3D 0A F7 3F",
                "02 2B 07 3D 02 2B 07 3D 02 2B 07 3D",
            };

            string[] replacePatterns =
            {
                "00 00 80 40 00 00 80 40 CB D2 4D 3E",
                "08 39 60 3B 08 39 60 3B 08 39 85 3B",
            };

            try
            {
                if (Config.brsutfiree)
                {
                    bool success = await ApplyMemoryPatch(memoryfast, searchPatterns, replacePatterns);
                }
                else
                {
                    bool success = await ApplyMemoryPatch(memoryfast, replacePatterns, searchPatterns);
                }
            }
            catch
            {
            }
        }

        private async Task loadspeedext()
        {
            string[] pocessname = { "HD-Player" };
            bool success = memoryfast.SetProcess(pocessname);

            if (!success)
            {
                return;
            }

            speedResult = await memoryfast.AoBScan("02 2B 07 3D 02 2B 07 3D 02 2B 07 3D");
            speedonext();
            Console.Beep(400, 200);
        }

        private void speedonext()
        {
            if (Config.speedext)
            {
                foreach (long id in speedResult)
                {
                    memoryfast.AobReplace(id, "E3 A5 9B 3C");
                }

                Console.Beep(400, 200);
            }
            else
            {
                foreach (long id in speedResult)
                {
                    memoryfast.AobReplace(id, "02 2B 07 3D 02 2B 07 3D 02 2B 07 3D");
                }

                Console.Beep(400, 200);
            }
        }

        static string MakeAob(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return BitConverter.ToString(bytes).Replace("-", " ");
        }

        Vector4 HsvToRgb(float h, float s, float v)
        {
            int i = (int)(h * 6);
            float f = h * 6 - i;
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);

            float r = 0, g = 0, b = 0;
            switch (i % 6)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                case 5: r = v; g = p; b = q; break;
            }

            return new Vector4(r, g, b, 1f);
        }
        private void RenderEntities()
        {
            if (!Core.HaveMatrix)
                return;

            int vpW = Core.EspDrawWidth;
            int vpH = Core.EspDrawHeight;
            if (vpW < 8 || vpH < 8)
                return;

            Core.GetEspDrawingSnapshot(out var viewMatrix, out _);

            var drawList = ImGui.GetBackgroundDrawList();
            var fgDrawList = ImGui.GetForegroundDrawList(); // Cache once per frame
            const float espEdgePad = 72f;
            var tmp = Core.Entities;

            // 'H' key detection logic for HackerDetect
            if (Config.HackerDetect && (WinAPI.GetAsyncKeyState(System.Windows.Forms.Keys.H) & 0x8000) != 0)
            {
                if (!hKeyPressed)
                {
                    try
                    {
                        var aliveEntities = tmp.Values.Where(e => !e.IsDead && e.IsKnown).ToList();
                        if (aliveEntities.Count > 0)
                        {
                            hackerMarkIndex++;
                            if (hackerMarkIndex >= aliveEntities.Count) hackerMarkIndex = 0;
                            selectedHackerAddress = aliveEntities[hackerMarkIndex].Address;
                        }
                    }
                    catch { }
                    hKeyPressed = true;
                }
            }
            else { hKeyPressed = false; }

            // Update random hacker entity for tag (only one enemy marked)
            if (Config.HackerTag)
            {
                if ((DateTime.Now - _lastHackerUpdate).TotalSeconds > HackerUpdateIntervalSeconds || _hackerEntityAddress == 0)
                {
                    // Zero-allocation reservoir sampling (replaces LINQ .Where().ToList())
                    int validCount = 0;
                    uint chosenAddr = 0;
                    try
                    {
                        foreach (var e in Core.Entities.Values.ToArray())
                        {
                            if (e.IsDead || !e.IsKnown) continue;
                            validCount++;
                            if (_hackerRandom.Next(validCount) == 0)
                                chosenAddr = e.Address;
                        }
                    }
                    catch { }
                    _hackerEntityAddress = chosenAddr;
                    _lastHackerUpdate = DateTime.Now;
                }
            }

            Entity[] entities;
            try
            {
                entities = Core.Entities.Values.ToArray();
            }
            catch
            {
                return;
            }

            int enemyCount = 0;

            foreach (var entity in entities)
            {
                if (entity == null || entity.Address == 0)
                    continue;
                if (entity.IsDead || !entity.IsKnown)
                    continue;

                float distW = Vector3.Distance(Core.LocalMainCamera, entity.Head);
                float espRenderMax = Math.Clamp(Config.Esprender, 0f, 600f);
                if (distW > espRenderMax)
                    continue;

                Vector2 headScreenPos = W2S.WorldToScreen(viewMatrix, entity.Head, vpW, vpH);
                Vector2 bottomScreenPos = W2S.WorldToScreen(viewMatrix, entity.Root, vpW, vpH);
                if (headScreenPos.X < 1f || headScreenPos.Y < 1f)
                    continue;
                if (bottomScreenPos.X < 1f || bottomScreenPos.Y < 1f)
                    continue;

                entity.EspSmValid = false;

                headScreenPos += _emulatorScreenOffset;
                bottomScreenPos += _emulatorScreenOffset;

                var entityData = new EntityRenderData
                {
                    headScreenPos = headScreenPos,
                    bottomScreenPos = bottomScreenPos,
                    Distance = distW,
                    IsValid = true
                };

                float CornerHeight = Math.Abs(entityData.headScreenPos.Y - entityData.bottomScreenPos.Y);
                float CornerWidth = CornerHeight * 0.65f;

                // Dynamic ESP colors (RGB mode)
                uint lineColor = ColorToUint32(Config.ESPLineColor);
                uint boxColor = ColorToUint32(Config.ESPBoxColor);
                if (Config.ESPRGB)
                {
                    Color rainbow = ColorFromHSV((float)(ImGui.GetTime() * 100 % 360), 1.0f, 1.0f);
                    lineColor = ColorToUint32(rainbow);
                    boxColor = lineColor;
                }

                Vector2 offLineStart = ScaleEspOffsetPx(Config.EspSnapLineStartOffsetPx, CornerHeight);
                Vector2 offLineEnd = ScaleEspOffsetPx(Config.EspSnapLineEndOffsetPx, CornerHeight);
                Vector2 offBox = ScaleEspOffsetPx(Config.EspBoxOffsetPx, CornerHeight);
                Vector2 offName = ScaleEspOffsetPx(Config.EspNameOffsetPx, CornerHeight);
                Vector2 offHacker = ScaleEspOffsetPx(Config.EspHackerOffsetPx, CornerHeight);
                Vector2 offWeapon = ScaleEspOffsetPx(Config.EspWeaponIconOffsetPx, CornerHeight);
                Vector2 offHealth = ScaleEspOffsetPx(Config.EspHealthBarOffsetPx, CornerHeight);
                Vector2 offDist = ScaleEspOffsetPx(Config.EspDistanceOffsetPx, CornerHeight);

                if (string.IsNullOrEmpty(entity.Name))
                    entity.Name = "Training Bot";

                float distM = MathF.Round(Vector3.Distance(Core.LocalMainCamera, entity.Head));
                string distStr = $"{distM}m";
                string nameDisplay = SanitizeEspDisplayName(entity.Name);

                float healthPercentage = entity.Health > 1000 ? 1f :
                    entity.Health < 0 ? 1f :
                    (float)entity.Health / (entity.Health > 230 ? 500 : 200);
                healthPercentage = Math.Clamp(healthPercentage, 0f, 1f);

                GetEspBoxBounds(entityData.headScreenPos, CornerWidth, CornerHeight, offBox,
                    out float boxL, out float boxT, out float boxR, out float boxB, out float boxMidX, out float boxMidY);
                float gapScaled = Math.Max(3f, EspAnchorGapPx * (CornerHeight / EspOffsetRefCornerHeight));

                float npW = 0f;
                float npH = 0f;
                Vector2 npTL = Vector2.Zero;
                bool showDist = Config.espdistance;
                EspDistanceBoxAnchor distAnchor = Config.EspDistanceAnchor;
                if (!Config.ESPInformation &&
                    (distAnchor == EspDistanceBoxAnchor.InsideNameplate || distAnchor == EspDistanceBoxAnchor.AboveNameWhenNameAtTop))
                {
                    distAnchor = EspDistanceBoxAnchor.BoxTopCenter;
                }
                bool distInNameplate = showDist && Config.ESPInformation && distAnchor == EspDistanceBoxAnchor.InsideNameplate;
                if (Config.ESPInformation)
                {
                    if (distInNameplate)
                        ComputeEspNameplateSize(nameDisplay, distStr, out npW, out npH);
                    else
                        ComputeEspNameplateSizeNameOnly(nameDisplay, out npW, out npH);
                    npTL = GetNameBoxAnchorTopLeft(Config.EspNameAnchor, boxL, boxT, boxR, boxB, boxMidX, boxMidY, gapScaled, npW, npH) + offName;
                }

                if (Config.ESPLine)
                {
                    Vector2 endPos = entityData.headScreenPos + new Vector2(0, -45f) + offLineEnd;
                    Vector2 lineStart = new Vector2(vpW / 2f, 10f);
                    switch (Config.ESPLinePosition)
                    {
                        case LinePosition.Top:
                            lineStart = new Vector2(vpW / 2f, 10f);
                            break;
                        case LinePosition.Center:
                            lineStart = new Vector2(vpW / 2f, vpH / 2f);
                            break;
                        case LinePosition.Bottom:
                            lineStart = new Vector2(vpW / 2f, vpH - 10f);
                            break;
                    }
                    lineStart += offLineStart + _emulatorScreenOffset;

                    drawList.AddLine(lineStart, endPos, lineColor, 1.2f);
                }

                if (Config.ESPBox)
                {
                    if (Config.EspBoxGlow)
                    {
                        DrawGlowCorneredBox(drawList, entityData.headScreenPos.X - (CornerWidth / 2) + offBox.X, entityData.headScreenPos.Y + offBox.Y, CornerWidth, CornerHeight, boxColor, 0.5f, 12f, 2f, false, entity.IsKnocked);
                    }
                    else
                    {
                        DrawFullBox(drawList, entityData.headScreenPos.X - (CornerWidth / 2) + offBox.X, entityData.headScreenPos.Y + offBox.Y, CornerWidth, CornerHeight, boxColor, 0.5f, entity.IsKnocked);
                    }
                }

                // Clipping check must use core coords (origin at emulator), not overlay coords.
                Vector2 headCore = entityData.headScreenPos - _emulatorScreenOffset;
                if (headCore.X >= -espEdgePad && headCore.Y >= -espEdgePad &&
                    headCore.X <= vpW + espEdgePad && headCore.Y <= vpH + espEdgePad)
                {
                    float offsetTop = 22f;
                    float offsetBottom = 5f;
                    float offsetLeftX = 5f;
                    float offsetLeftY = 0f;
                    float offsetRightX = 5f;
                    float offsetRightY = 0f;

                    if (Config.ESPHealth)
                    {
                        if (Config.EspHealthBarSide == 0) offsetLeftX += 6f;
                        else if (Config.EspHealthBarSide == 1) offsetRightX += 6f;
                        else if (Config.EspHealthBarSide == 2) offsetBottom += 6f;
                        else if (Config.EspHealthBarSide == 3) offsetTop += 6f;
                    }

                    Func<int, Vector2, Vector2> GetTextPosition = (side, textSize) =>
                    {
                        Vector2 pos = Vector2.Zero;
                        switch (side)
                        {
                            case 0: // Left
                                pos.X = entityData.headScreenPos.X - (CornerWidth / 2) - textSize.X - offsetLeftX;
                                pos.Y = entityData.headScreenPos.Y + offsetLeftY;
                                offsetLeftY += textSize.Y + 2f;
                                break;
                            case 1: // Right
                                pos.X = entityData.headScreenPos.X + (CornerWidth / 2) + offsetRightX;
                                pos.Y = entityData.headScreenPos.Y + offsetRightY;
                                offsetRightY += textSize.Y + 2f;
                                break;
                            case 2: // Top
                                pos.X = entityData.headScreenPos.X - (textSize.X / 2);
                                pos.Y = entityData.headScreenPos.Y - textSize.Y - offsetTop;
                                offsetTop += textSize.Y + 0f;
                                break;
                            case 3: // Bottom
                                pos.X = entityData.headScreenPos.X - (textSize.X / 2);
                                pos.Y = entityData.headScreenPos.Y + CornerHeight + offsetBottom;
                                offsetBottom += textSize.Y + 2f;
                                break;
                            default:
                                pos = entityData.headScreenPos;
                                break;
                        }
                        return pos;
                    };

                    if (Config.ESPHealth)
                    {
                        float scaledBoxHeight = CornerHeight * 1.15f;
                        float boxTopY = entityData.headScreenPos.Y - 1f - (scaledBoxHeight - CornerHeight) + offHealth.Y;
                        float boxLeftX = entityData.headScreenPos.X - (CornerWidth / 2) + offHealth.X;
                        float widerBoxWidth = CornerWidth * 1.1f;

                        switch (Config.EspHealthBarSide)
                        {
                            case 0: // Left
                                DrawVerticalHealthBar(drawList, entity.Health, 200, new Vector2(boxLeftX - 5, boxTopY), scaledBoxHeight, entity.IsKnocked, 10);
                                break;
                            case 1: // Right
                                DrawVerticalHealthBar(drawList, entity.Health, 200, new Vector2(boxLeftX + CornerWidth + 5, boxTopY), scaledBoxHeight, entity.IsKnocked, 10);
                                break;
                            case 2: // Bottom
                                DrawHealthBarBelowFullBox(drawList, entity.Health, 200, entityData.headScreenPos.X - (widerBoxWidth / 2) + offHealth.X, entityData.headScreenPos.Y + offHealth.Y, widerBoxWidth, CornerHeight, entity.IsKnocked);
                                break;
                            case 3: // Top
                                DrawHealthBarBelowFullBox(drawList, entity.Health, 200, entityData.headScreenPos.X - (widerBoxWidth / 2) + offHealth.X, entityData.headScreenPos.Y + offHealth.Y - scaledBoxHeight - 5, widerBoxWidth, CornerHeight, entity.IsKnocked);
                                break;
                        }
                    }

                    if (Config.ESPName)
                    {
                        Vector2 nameSz = EspCalcTextSize(nameDisplay);
                        Vector2 namePos = GetTextPosition(Config.EspNameSide, nameSz) + offName;
                        uint outlineCol = 0xFF000000;
                        const float outlineSize = 1f;
                        for (float ox = -outlineSize; ox <= outlineSize; ox += outlineSize)
                        {
                            for (float oy = -outlineSize; oy <= outlineSize; oy += outlineSize)
                            {
                                if (ox == 0f && oy == 0f) continue;
                                EspDrawText(fgDrawList, namePos + new Vector2(ox, oy), outlineCol, nameDisplay);
                            }
                        }
                        EspDrawText(fgDrawList, namePos, ColorToUint32(Config.ESPNameColor), nameDisplay);
                    }

                    if (Config.espdistance)
                    {
                        Vector2 distSz = ImGui.CalcTextSize(distStr);
                        Vector2 distPos = GetTextPosition(Config.EspDistanceSide, distSz) + offDist;
                        uint outlineCol = 0xFF000000;
                        const float outlineSize = 1f;
                        for (float ox = -outlineSize; ox <= outlineSize; ox += outlineSize)
                        {
                            for (float oy = -outlineSize; oy <= outlineSize; oy += outlineSize)
                            {
                                if (ox == 0f && oy == 0f) continue;
                                fgDrawList.AddText(distPos + new Vector2(ox, oy), outlineCol, distStr);
                            }
                        }
                        fgDrawList.AddText(distPos, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.93f, 0.2f, 1f)), distStr);
                    }

                    if (Config.HackerTag && entity.Address == _hackerEntityAddress)
                    {
                        string hackerText = "HACKER";
                        Vector2 hackerSize = ImGui.CalcTextSize(hackerText);
                        Vector2 hackerPos = GetTextPosition(Config.EspNameSide, hackerSize) + offHacker;
                        fgDrawList.AddText(hackerPos + new Vector2(1, 1), ColorToUint32(Color.Black), hackerText);
                        fgDrawList.AddText(hackerPos, ColorToUint32(Config.HackerTagColor), hackerText);
                    }

                    if (Config.HackerDetect && entity.Address == selectedHackerAddress)
                    {
                        string detectText = "HACKER";
                        Vector2 markerSize = ImGui.CalcTextSize(detectText);
                        Vector2 markerPos = GetTextPosition(2, markerSize); // Top side
                        fgDrawList.AddText(markerPos + new Vector2(1, 1), ColorToUint32(Color.Black), detectText);
                        fgDrawList.AddText(markerPos, ColorToUint32(Color.Red), detectText);
                    }

                    if (Config.espweapon)
                    {
                        string weaponName = entity.WeaponName?.Trim() ?? "";
                        if (!string.IsNullOrWhiteSpace(weaponName))
                        {
                            weaponName = weaponName.Replace(" ", "");
                            string weaponFileName = weaponName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                                ? weaponName.ToLowerInvariant()
                                : $"{weaponName.ToLowerInvariant()}.png";

                            IntPtr imagehandle = IntPtr.Zero;
                            try
                            {
                                if (!_weaponIcons.TryGetValue(weaponFileName, out imagehandle))
                                {
                                    Assembly asm = Assembly.GetExecutingAssembly();
                                    string directResourceName = $"pixel.Resources.Icons.{weaponFileName}";
                                    string wantedSuffix = $"Resources.Icons.{weaponFileName}";

                                    Stream? stream = asm.GetManifestResourceStream(directResourceName);
                                    if (stream == null)
                                    {
                                        foreach (string res in asm.GetManifestResourceNames())
                                        {
                                            if (res.EndsWith(wantedSuffix, StringComparison.OrdinalIgnoreCase))
                                            {
                                                stream = asm.GetManifestResourceStream(res);
                                                break;
                                            }
                                        }
                                    }

                                    if (stream == null && !weaponFileName.Equals("fist.png", StringComparison.OrdinalIgnoreCase))
                                    {
                                        string fallback = "fist.png";
                                        stream = asm.GetManifestResourceStream($"pixel.Resources.Icons.{fallback}");
                                        if (stream == null)
                                        {
                                            foreach (string res in asm.GetManifestResourceNames())
                                            {
                                                if (res.EndsWith($"Resources.Icons.{fallback}", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    stream = asm.GetManifestResourceStream(res);
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (stream != null)
                                    {
                                        using (stream)
                                        using (var bitmap = new Bitmap(stream))
                                        {
                                            string tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
                                            bitmap.Save(tempPath);
                                            AddOrGetImagePointer(tempPath, true, out imagehandle, out _, out _);
                                            _weaponIcons[weaponFileName] = imagehandle;
                                            try { File.Delete(tempPath); } catch { }
                                        }
                                    }
                                }
                            }
                            catch { }

                            if (imagehandle != IntPtr.Zero)
                            {
                                Vector2 iconSize = new Vector2(60 * Config.iconsize, 20 * Config.iconsize);
                                Vector2 iconPos = GetTextPosition(Config.EspWeaponSide, iconSize) + offWeapon;
                                if (!Config.rgb)
                                {
                                    fgDrawList.AddImage(imagehandle, iconPos, iconPos + iconSize, Vector2.Zero, Vector2.One, ImGui.ColorConvertFloat4ToU32(Config.ICONCOLOR));
                                }
                                else
                                {
                                    const int slices = 10;
                                    float time = (float)ImGui.GetTime();
                                    float sliceWidth = iconSize.X / slices;
                                    for (int i = 0; i < slices; i++)
                                    {
                                        float xStart = iconPos.X + i * sliceWidth;
                                        float xEnd = xStart + sliceWidth;
                                        float uvStart = (float)i / slices;
                                        float uvEnd = (float)(i + 1) / slices;
                                        float hue = ((float)i / slices + time * 0.25f) % 1.0f;
                                        Vector4 rgbColor = HsvToRgb(hue, 1f, 1f);
                                        rgbColor.W = Config.ICONCOLOR.W;
                                        fgDrawList.AddImage(imagehandle, new Vector2(xStart, iconPos.Y), new Vector2(xEnd, iconPos.Y + iconSize.Y), new Vector2(uvStart, 0), new Vector2(uvEnd, 1), ImGui.ColorConvertFloat4ToU32(rgbColor));
                                    }
                                }
                            }
                            else
                            {
                                Vector2 weaponSz = ImGui.CalcTextSize(weaponName);
                                Vector2 weaponPos = GetTextPosition(Config.EspWeaponSide, weaponSz) + offWeapon;
                                uint outlineCol = 0xFF000000;
                                const float outlineSize = 1f;
                                for (float ox = -outlineSize; ox <= outlineSize; ox += outlineSize)
                                {
                                    for (float oy = -outlineSize; oy <= outlineSize; oy += outlineSize)
                                    {
                                        if (ox == 0f && oy == 0f) continue;
                                        fgDrawList.AddText(weaponPos + new Vector2(ox, oy), outlineCol, weaponName);
                                    }
                                }
                                fgDrawList.AddText(weaponPos, ImGui.ColorConvertFloat4ToU32(Config.ICONCOLOR), weaponName);
                            }
                        }
                    }

                    if (Config.ESPHealthText && !entity.IsKnocked)
                    {
                        string ht = $"{entity.Health} HP";
                        Vector2 hts = ImGui.CalcTextSize(ht);
                        int hSide = (Config.EspHealthBarSide == 0 || Config.EspHealthBarSide == 1) ? Config.EspHealthBarSide : 2;
                        Vector2 hpTextPos = GetTextPosition(hSide, hts);
                        fgDrawList.AddText(hpTextPos + new Vector2(1, 1), ColorToUint32(Color.Black), ht);
                        fgDrawList.AddText(hpTextPos, ColorToUint32(Config.ESPHealthColor), ht);
                    }
                }



                if (Config.ESPSkeleton)
                {
                    DrawSkeleton(entity, viewMatrix, vpW, vpH);
                }

            }

        }

        private void UpdateEspMatchTimerLogic()
        {
            g_TimerManager.Update(Core.CurrentMatchAddress, Core.MatchStatusValue, Config.EspTimer);
        }

        private void DrawEspMatchTimerOverlay()
        {
            g_TimerManager.DrawTimer(Core.Width, Core.Height, Config.EspTimer, _emulatorScreenOffset);
        }

        private void UpdateEntities()
        {

            foreach (var entity in Core.Entities.Values)
            {
                if (entity.IsTeam != Bool3.False) continue;

                TreeNode entityNode = new TreeNode(entity.Name);

                entityNode.Nodes.Add(new TreeNode($"IsKnown: {entity.IsKnown}"));
                entityNode.Nodes.Add(new TreeNode($"IsTeam: {entity.IsTeam}"));
                entityNode.Nodes.Add(new TreeNode($"Head: {entity.Head}"));
                entityNode.Nodes.Add(new TreeNode($"Root: {entity.Root}"));
                entityNode.Nodes.Add(new TreeNode($"Health: {entity.Health}"));
                entityNode.Nodes.Add(new TreeNode($"IsDead: {entity.IsDead}"));
                entityNode.Nodes.Add(new TreeNode($"IsKnocked: {entity.IsKnocked}"));


            }
            Thread.Sleep(1000);
        }
        private void NoCache()
        {
            InternalMemory.ClearCache();
            Core.Entities.Clear();
            Thread.Sleep(1000);
        }

        private void HandleUIAnimation()
        {
            bool imguiInsert = ImGui.IsKeyPressed(ImGuiKey.Insert);
            bool vkDown = (GetAsyncKeyState(System.Windows.Forms.Keys.Insert) & 0x8000) != 0;
            bool vkEdge = vkDown && !_vkInsertDownLast;
            _vkInsertDownLast = vkDown;

            bool insertPressed = imguiInsert || vkEdge;
            if (_flowPhase == FlowPhase.Main && insertPressed && !_insertKeyPressedLastFrame)
            {
                _uiVisible = !_uiVisible;
                if (!_uiVisible)
                {
                    _showTabPopup = false;
                    _tabPopupAnim = 0f;
                }
            }
            _insertKeyPressedLastFrame = insertPressed;

            float targetAlpha = _uiVisible ? 1.0f : 0.0f;
            float dt = ImGui.GetIO().DeltaTime;
            float fadeRate = _uiVisible ? _uiFadeSpeed : _uiFadeOutSpeed;
            _uiAlpha += (targetAlpha - _uiAlpha) * dt * fadeRate;
            _uiAlpha = Math.Clamp(_uiAlpha, 0.0f, 1.0f);
            if (!_uiVisible && _uiAlpha < 0.02f)
                _uiAlpha = 0f;
        }

        protected override unsafe void Render()
        {
            EnsureTabIconsLoaded();
            TryStartBackgroundConnect();
            EnsureEmulatorRenderTarget();
            if (Core.Handle != IntPtr.Zero)
                CreateHandle();
            HandleUIAnimation();

            if (_flowPhase == FlowPhase.Main)
                FeatureKeybinds.ProcessHotkeys(_activeBindingId);

            if (_loginDone)
            {
                _loginDone = false;
                if (_loginOk)
                {
                    _loginError = "";
                    _initError = null;
                    _initCompletionHandled = false;
                    _showHookSuccessScreen = false;
                    _flowPhase = FlowPhase.Main;
                    _activeTab = 0;
                    _connected = true;
                    if (_emulatorInitTask == null || _emulatorInitTask.IsCompleted)
                        _emulatorInitTask = BootstrapRuntime.InitEmulatorAsync();
                    CustomNotification.Notify("Connect", "Connecting to Free Fire…", 4f);
                }
                else
                    _loginError = _loginResultMsg;
            }

            if (_uiVisible || _uiAlpha > 0.001f)
                RenderUI();

            // Poll connect completion during login + main menu.
            if (_emulatorInitTask != null && !_initCompletionHandled)
                HandleEmulatorInitCompleted();

            if (_pendingFeatureRuntimeApply)
            {
                _pendingFeatureRuntimeApply = false;
                var self = this;
                Task.Run(() => self.ApplyLoadedFeatureRuntimeState());
            }

            if (_flowPhase == FlowPhase.Main)
            {
                CustomNotification.Render();
            }

            if (!Core.HaveMatrix && !Core.EspMatrixEverValid)
                return;

            int vpW = Core.EspDrawWidth;
            int vpH = Core.EspDrawHeight;
            Core.GetEspDrawingSnapshot(out var viewMatrix, out _);

            var espClipDl = ImGui.GetBackgroundDrawList();
            PushEspClipRect(espClipDl);
            try
            {
                if (Config.FOVEnabled)
                {
                    DrawFOVCircle(Config.AimFov);
                }

                // Aim Track Line: draw from screen center to nearest enemy inside FOV
                if (Config.AimTrackLine)
                {
                    Vector2 screenCenter = new Vector2(vpW / 2f, vpH / 2f);
                    Entity nearestEnemy = null;
                    Vector2 nearestEnemyScreenPos = Vector2.Zero;
                    float nearestDistance = float.MaxValue;

                    try
                    {
                        foreach (var entity in Core.Entities.Values.ToArray())
                        {
                            if (entity.IsDead || !entity.IsKnown) continue;

                            var headPos = W2S.WorldToScreen(viewMatrix, entity.Head, vpW, vpH);
                            if (headPos.X < 1 || headPos.Y < 1) continue;

                            float distToCrosshair = Vector2.Distance(screenCenter, headPos);
                            if (distToCrosshair < nearestDistance &&
                                (!Config.FOVEnabled || distToCrosshair <= Config.AimFov))
                            {
                                nearestDistance = distToCrosshair;
                                nearestEnemy = entity;
                                nearestEnemyScreenPos = headPos;
                            }
                        }
                    }
                    catch { }

                    if (nearestEnemy != null)
                    {
                        uint trackLineColor = ColorToUint32(Config.AimTrackLineColor);
                        DrawGlowLine(screenCenter + _emulatorScreenOffset, nearestEnemyScreenPos + _emulatorScreenOffset, trackLineColor, 0.8f, 4.0f, 1.0f, 0.6f);
                    }
                }
            }
            finally
            {
                espClipDl.PopClipRect();
            }

            // Cache FindWindow result – only call once per second instead of every frame
            _findWindowTimer -= ImGui.GetIO().DeltaTime;
            if (_findWindowTimer <= 0f)
            {
                string windowName = "Overlay";
                hWnd = FindWindow(null!, windowName);
                HDPlayer = FindWindow("BlueStacksApp", null!);
                _findWindowTimer = 1.0f;
            }

            if (hWnd != IntPtr.Zero)
            {
                long extendedStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
                SetWindowLong(hWnd, GWL_EXSTYLE, (extendedStyle | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);
            }
            UpdateEspMatchTimerLogic();

            var espEntityClipDl = ImGui.GetBackgroundDrawList();
            PushEspClipRect(espEntityClipDl);
            try
            {
                RenderEntities();
            }
            finally
            {
                espEntityClipDl.PopClipRect();
            }

            DrawEspMatchTimerOverlay();
        }

        // Glowing line helper (for Aim Track Line)
        private void DrawGlowLine(Vector2 start, Vector2 end, uint color, float thickness, float glowRadius, float feather, float glowOpacityMultiplier)
        {
            var drawList = ImGui.GetBackgroundDrawList();
            Vector4 colorVec = ImGui.ColorConvertU32ToFloat4(color);

            for (float i = glowRadius; i > 0; i -= feather)
            {
                float alpha = colorVec.W * (i / glowRadius) * glowOpacityMultiplier;
                alpha = Math.Clamp(alpha, 0, 1);
                uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(colorVec.X, colorVec.Y, colorVec.Z, alpha));
                drawList.AddLine(start, end, glowColor, thickness + (glowRadius - i) * 0.5f);
            }

            drawList.AddLine(start, end, color, thickness);
        }
        private void DrawLine(ImDrawListPtr drawList, Vector2 startPos, Vector2 endPos, uint color)
        {
            if (startPos.X > 0 && startPos.Y > 0 && endPos.X > 0 && endPos.Y > 0)
            {
                drawList.AddLine(startPos, endPos, color, 1.5f); // Adjust thickness as needed
            }
        }

        private void DrawSkeletonJoint(ImDrawListPtr drawList, Vector2 pos, float radius, uint fillColor, uint outlineColor)
        {
            if (radius <= 0.01f)
                return;

            if (pos.X <= 0f || pos.Y <= 0f)
                return;

            drawList.AddCircleFilled(pos, radius, fillColor);
            if (outlineColor != 0)
                drawList.AddCircle(pos, radius + 0.45f, outlineColor, 8, 1.0f);
        }

        private void DrawCurvedSkeletonSegment(
            ImDrawListPtr drawList,
            Vector2 startPos,
            Vector2 endPos,
            uint color,
            float thickness,
            float curveStrength,
            Vector2 bodyCenter,
            float forcedCurveSign = 0f)
        {
            if (thickness <= 0.01f)
                return;

            if (startPos.X <= 0f || startPos.Y <= 0f || endPos.X <= 0f || endPos.Y <= 0f)
                return;

            Vector2 dir = endPos - startPos;
            float len = dir.Length();
            if (!float.IsFinite(len) || len <= 0.01f)
                return;

            // If tiny segment, just draw straight.
            if (len < 6f)
            {
                drawList.AddLine(startPos, endPos, color, thickness);
                return;
            }

            float sign = Math.Abs(forcedCurveSign) > 0.001f
                ? Math.Sign(forcedCurveSign)
                : (startPos.X >= bodyCenter.X ? 1f : -1f);

            Vector2 perp = new Vector2(-dir.Y / len, dir.X / len);
            Vector2 mid = (startPos + endPos) * 0.5f;
            Vector2 control = mid + perp * (len * curveStrength * sign);

            // Approximate a quadratic Bezier with a short polyline.
            const int segments = 8;
            Vector2 prev = startPos;
            for (int i = 1; i <= segments; i++)
            {
                float u = i / (float)segments;
                float omu = 1f - u;
                Vector2 p =
                    omu * omu * startPos +
                    2f * omu * u * control +
                    u * u * endPos;

                drawList.AddLine(prev, p, color, thickness);
                prev = p;
            }
        }
        private void DrawSkeleton(Entity entity, Matrix4x4 viewMatrix, int vpW, int vpH)
        {
            var drawList = ImGui.GetBackgroundDrawList();
            uint lineColor = ColorToUint32(Config.ESPSkeletonColor);

            var headScreenPos = W2S.WorldToScreen(viewMatrix, entity.Head, vpW, vpH);
            var neckScreenPos = W2S.WorldToScreen(viewMatrix, entity.Neck, vpW, vpH);
            var leftShoulderScreenPos = W2S.WorldToScreen(viewMatrix, entity.LeftShoulder, vpW, vpH);
            var rightShoulderScreenPos = W2S.WorldToScreen(viewMatrix, entity.RightShoulder, vpW, vpH);
            var leftElbowScreenPos = W2S.WorldToScreen(viewMatrix, entity.LeftElbow, vpW, vpH);
            var rightElbowScreenPos = W2S.WorldToScreen(viewMatrix, entity.RightElbow, vpW, vpH);
            var leftWristScreenPos = W2S.WorldToScreen(viewMatrix, entity.LeftWrist, vpW, vpH);
            var rightWristScreenPos = W2S.WorldToScreen(viewMatrix, entity.RightWrist, vpW, vpH);
            var hipScreenPos = W2S.WorldToScreen(viewMatrix, entity.Hip, vpW, vpH);
            var groinScreenPos = W2S.WorldToScreen(viewMatrix, entity.Groin, vpW, vpH);
            var leftAnkleScreenPos = W2S.WorldToScreen(viewMatrix, entity.LeftAnkle, vpW, vpH);
            var rightAnkleScreenPos = W2S.WorldToScreen(viewMatrix, entity.RightAnkle, vpW, vpH);
            var leftFootScreenPos = W2S.WorldToScreen(viewMatrix, entity.LeftFoot, vpW, vpH);
            var rightFootScreenPos = W2S.WorldToScreen(viewMatrix, entity.RightFoot, vpW, vpH);

            // Match C++ straight lines exactly (straight bone drawing)
            Action<Vector2, Vector2> DrawBone = (from, to) =>
            {
                if (from.X >= 1f && from.Y >= 1f && to.X >= 1f && to.Y >= 1f &&
                    from.X <= vpW && from.Y <= vpH && to.X <= vpW && to.Y <= vpH)
                {
                    drawList.AddLine(from + _emulatorScreenOffset, to + _emulatorScreenOffset, lineColor, 1.0f);
                }
            };

            DrawBone(headScreenPos, neckScreenPos);
            DrawBone(neckScreenPos, hipScreenPos);
            DrawBone(neckScreenPos, leftShoulderScreenPos);
            DrawBone(leftShoulderScreenPos, leftElbowScreenPos);
            DrawBone(leftElbowScreenPos, leftWristScreenPos);
            DrawBone(neckScreenPos, rightShoulderScreenPos);
            DrawBone(rightShoulderScreenPos, rightElbowScreenPos);
            DrawBone(rightElbowScreenPos, rightWristScreenPos);
            DrawBone(hipScreenPos, groinScreenPos);
            DrawBone(groinScreenPos, leftAnkleScreenPos);
            DrawBone(groinScreenPos, rightAnkleScreenPos);
            DrawBone(leftAnkleScreenPos, leftFootScreenPos);
            DrawBone(rightAnkleScreenPos, rightFootScreenPos);
        }

        private void DrawSkeletonEditorPreview(
            ImDrawListPtr dl,
            Vector2 headScreen,
            Vector2 bottomScreen,
            float boxMidX,
            float cornerWidth,
            float cornerHeight)
        {
            if (!Config.ESPSkeleton)
                return;

            uint lineColor = ColorToUint32(Config.ESPSkeletonColor);
            uint outlineColor = ColorToUint32(Color.Black);

            // Editor preview is drawn on top of a fixed PNG silhouette.
            // Keep it thinner/smaller so it doesn't look like a "blob".
            float thickness = Math.Clamp(cornerHeight * 0.012f, 1.8f, 4.6f);
            float jointRadius = thickness * 0.30f;

            // Approximate points around the silhouette.
            // Y anchors are relative to head->feet height (cornerHeight).
            float neckY = headScreen.Y + cornerHeight * 0.10f;
            float shoulderY = headScreen.Y + cornerHeight * 0.24f;
            float spineY = headScreen.Y + cornerHeight * 0.34f;
            float hipY = headScreen.Y + cornerHeight * 0.56f;

            float leftX = boxMidX - cornerWidth * 0.30f;
            float rightX = boxMidX + cornerWidth * 0.30f;

            Vector2 neck = new Vector2(boxMidX, neckY);
            Vector2 spine = new Vector2(boxMidX, spineY);
            Vector2 hip = new Vector2(boxMidX, hipY);

            Vector2 leftShoulder = new Vector2(leftX, shoulderY);
            Vector2 rightShoulder = new Vector2(rightX, shoulderY);

            Vector2 leftElbow = leftShoulder + new Vector2(-cornerWidth * 0.06f, cornerHeight * 0.14f);
            Vector2 rightElbow = rightShoulder + new Vector2(cornerWidth * 0.06f, cornerHeight * 0.14f);

            Vector2 leftWristJoint = leftElbow + new Vector2(-cornerWidth * 0.03f, cornerHeight * 0.13f);
            Vector2 rightWristJoint = rightElbow + new Vector2(cornerWidth * 0.03f, cornerHeight * 0.13f);

            Vector2 leftWrist = leftWristJoint + new Vector2(-cornerWidth * 0.02f, cornerHeight * 0.09f);
            Vector2 rightWrist = rightWristJoint + new Vector2(cornerWidth * 0.02f, cornerHeight * 0.09f);

            Vector2 leftHand = leftWrist + new Vector2(-cornerWidth * 0.04f, cornerHeight * 0.05f);
            Vector2 rightHand = rightWrist + new Vector2(cornerWidth * 0.04f, cornerHeight * 0.05f);

            // Knees/calf
            Vector2 leftCalf = hip + new Vector2(-cornerWidth * 0.14f, cornerHeight * 0.18f);
            Vector2 rightCalf = hip + new Vector2(cornerWidth * 0.14f, cornerHeight * 0.18f);

            // Ankles/feet
            Vector2 leftFoot = bottomScreen + new Vector2(-cornerWidth * 0.12f, 0f);
            Vector2 rightFoot = bottomScreen + new Vector2(cornerWidth * 0.12f, 0f);

            Vector2 bodyCenter = new Vector2(boxMidX, hipY);

            // Segments (curved)
            DrawCurvedSkeletonSegment(dl, headScreen, neck, lineColor, thickness, 0.03f, bodyCenter, 0f);
            DrawCurvedSkeletonSegment(dl, neck, spine, lineColor, thickness, 0.02f, bodyCenter, 0f);
            DrawCurvedSkeletonSegment(dl, spine, hip, lineColor, thickness, 0.02f, bodyCenter, 0f);

            float leftSign = -1f;
            float rightSign = 1f;
            float armCurve = 0.05f;  // reduced curve strength (prevents bowing out)
            float legCurve = 0.04f;

            DrawCurvedSkeletonSegment(dl, neck, leftShoulder, lineColor, thickness, armCurve, bodyCenter, leftSign);
            DrawCurvedSkeletonSegment(dl, leftShoulder, leftElbow, lineColor, thickness, armCurve, bodyCenter, leftSign);
            DrawCurvedSkeletonSegment(dl, leftElbow, leftWristJoint, lineColor, thickness, armCurve, bodyCenter, leftSign);
            DrawCurvedSkeletonSegment(dl, leftWristJoint, leftWrist, lineColor, thickness, armCurve, bodyCenter, leftSign);
            DrawCurvedSkeletonSegment(dl, leftWrist, leftHand, lineColor, thickness, armCurve * 0.9f, bodyCenter, leftSign);

            DrawCurvedSkeletonSegment(dl, neck, rightShoulder, lineColor, thickness, armCurve, bodyCenter, rightSign);
            DrawCurvedSkeletonSegment(dl, rightShoulder, rightElbow, lineColor, thickness, armCurve, bodyCenter, rightSign);
            DrawCurvedSkeletonSegment(dl, rightElbow, rightWristJoint, lineColor, thickness, armCurve, bodyCenter, rightSign);
            DrawCurvedSkeletonSegment(dl, rightWristJoint, rightWrist, lineColor, thickness, armCurve, bodyCenter, rightSign);
            DrawCurvedSkeletonSegment(dl, rightWrist, rightHand, lineColor, thickness, armCurve * 0.9f, bodyCenter, rightSign);

            DrawCurvedSkeletonSegment(dl, hip, leftCalf, lineColor, thickness, legCurve, bodyCenter, leftSign);
            DrawCurvedSkeletonSegment(dl, leftCalf, leftFoot, lineColor, thickness, legCurve, bodyCenter, leftSign);
            DrawCurvedSkeletonSegment(dl, hip, rightCalf, lineColor, thickness, legCurve, bodyCenter, rightSign);
            DrawCurvedSkeletonSegment(dl, rightCalf, rightFoot, lineColor, thickness, legCurve, bodyCenter, rightSign);

            // Joints (dots)
            DrawSkeletonJoint(dl, headScreen, Math.Max(jointRadius, thickness * 0.45f), lineColor, outlineColor);
            DrawSkeletonJoint(dl, neck, jointRadius, lineColor, outlineColor);
            DrawSkeletonJoint(dl, spine, jointRadius, lineColor, outlineColor);
            DrawSkeletonJoint(dl, hip, jointRadius, lineColor, outlineColor);

            DrawSkeletonJoint(dl, leftShoulder, jointRadius, lineColor, outlineColor);
            DrawSkeletonJoint(dl, leftElbow, jointRadius, lineColor, outlineColor);
            DrawSkeletonJoint(dl, leftWristJoint, jointRadius, lineColor, outlineColor);
            DrawSkeletonJoint(dl, leftWrist, jointRadius, lineColor, outlineColor);
            DrawSkeletonJoint(dl, leftHand, jointRadius * 0.9f, lineColor, outlineColor);

            DrawSkeletonJoint(dl, rightShoulder, jointRadius, lineColor, outlineColor);
            DrawSkeletonJoint(dl, rightElbow, jointRadius, lineColor, outlineColor);
            DrawSkeletonJoint(dl, rightWristJoint, jointRadius, lineColor, outlineColor);
            DrawSkeletonJoint(dl, rightWrist, jointRadius, lineColor, outlineColor);
            DrawSkeletonJoint(dl, rightHand, jointRadius * 0.9f, lineColor, outlineColor);

            DrawSkeletonJoint(dl, leftCalf, jointRadius, lineColor, outlineColor);
            DrawSkeletonJoint(dl, rightCalf, jointRadius, lineColor, outlineColor);
            DrawSkeletonJoint(dl, leftFoot, jointRadius * 0.95f, lineColor, outlineColor);
            DrawSkeletonJoint(dl, rightFoot, jointRadius * 0.95f, lineColor, outlineColor);
        }

        private static uint[] HealthColors = new uint[]
{
    ColorToUint32(Color.Green),
    ColorToUint32(Color.Yellow),
    ColorToUint32(Color.Red)
};


        public void DrawHealthText(Entity entity, int espHealthInt)
        {

            //Vector2 headScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
            //Vector2 pieDerechoScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightFoot, Core.Width, Core.Height);


            //string healthText = "Health: " + espHealthInt.ToString();


            //Vector2 textPosition = new Vector2(.X - 25, pieDerechoScreenPos.Y + 10);


            //var drawList = ImGui.GetBackgroundDrawList();


            //uint borderColor = ColorToUint32(Color.Black);


            //float offset = 1.0f;

            //drawList.AddText(new Vector2(textPosition.X - offset, textPosition.Y), borderColor, healthText);
            //drawList.AddText(new Vector2(textPosition.X + offset, textPosition.Y), borderColor, healthText);
            //drawList.AddText(new Vector2(textPosition.X, textPosition.Y - offset), borderColor, healthText);
            //drawList.AddText(new Vector2(textPosition.X, textPosition.Y + offset), borderColor, healthText);


            //drawList.AddText(textPosition, ColorToUint32(Color.White), healthText);
        }

        private void DrawHealthTextFixed(Entity entity, int espHealthInt)
        {
            if (entity == null)
                return;

            if (entity.IsKnocked)
                return;

            // Draw the health number near the health bar.
            int hp = Math.Max(0, espHealthInt);
            string ht = hp.ToString();

            Vector2 headScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
            Vector2 bottomScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Root, Core.Width, Core.Height);
            if (headScreenPos.X < 0f && headScreenPos.Y < 0f) return;
            if (bottomScreenPos.X < 0f && bottomScreenPos.Y < 0f) return;

            headScreenPos += _emulatorScreenOffset;
            bottomScreenPos += _emulatorScreenOffset;

            float cornerHeight = Math.Abs(headScreenPos.Y - bottomScreenPos.Y);
            if (cornerHeight <= 0.01f) return;
            float cornerWidth = cornerHeight * 0.65f;

            Vector2 offBox = ScaleEspOffsetPx(Config.EspBoxOffsetPx, cornerHeight);
            Vector2 offHealth = ScaleEspOffsetPx(Config.EspHealthBarOffsetPx, cornerHeight);
            float gapScaled = Math.Max(3f, EspAnchorGapPx * (cornerHeight / EspOffsetRefCornerHeight));

            GetEspBoxBounds(headScreenPos, cornerWidth, cornerHeight, offBox,
                out float boxL, out float boxT, out float boxR, out float boxB, out float boxMidX, out float boxMidY);

            // Same thickness used in the in-game health bar drawing.
            float t = 3f;
            Vector2 healthHitMin = Vector2.Zero;
            Vector2 healthHitMax = Vector2.Zero;

            switch (Config.EspHealthAnchor)
            {
                case EspHealthBoxAnchor.BoxTop:
                    {
                        float y0 = boxT - gapScaled - t;
                        Vector2 o = offHealth;
                        healthHitMin = new Vector2(boxL + o.X, y0 + o.Y);
                        healthHitMax = new Vector2(boxR + o.X, y0 + t + o.Y);
                        break;
                    }
                case EspHealthBoxAnchor.BoxBottom:
                    {
                        float y0 = boxB + gapScaled;
                        Vector2 o = offHealth;
                        healthHitMin = new Vector2(boxL + o.X, y0 + o.Y);
                        healthHitMax = new Vector2(boxR + o.X, y0 + t + o.Y);
                        break;
                    }
                case EspHealthBoxAnchor.BoxLeft:
                    {
                        Vector2 p = new Vector2(boxL - 5f - t, boxT) + offHealth;
                        healthHitMin = p;
                        healthHitMax = p + new Vector2(t, cornerHeight);
                        break;
                    }
                default:
                    {
                        Vector2 healthBarPosRight = new Vector2(boxR + 5f, boxT) + offHealth;
                        healthHitMin = healthBarPosRight;
                        healthHitMax = healthBarPosRight + new Vector2(t, cornerHeight);
                        break;
                    }
            }

            Vector2 hts = ImGui.CalcTextSize(ht);
            Vector2 center = (healthHitMin + healthHitMax) * 0.5f;

            Vector2 hpTextPos;
            switch (Config.EspHealthAnchor)
            {
                case EspHealthBoxAnchor.BoxLeft:
                    hpTextPos = new Vector2(healthHitMax.X + 6f, center.Y - hts.Y * 0.5f);
                    break;
                case EspHealthBoxAnchor.BoxTop:
                    hpTextPos = new Vector2(center.X - hts.X * 0.5f, healthHitMin.Y - hts.Y - 6f);
                    break;
                case EspHealthBoxAnchor.BoxBottom:
                    hpTextPos = new Vector2(center.X - hts.X * 0.5f, healthHitMax.Y + 6f);
                    break;
                case EspHealthBoxAnchor.BoxRight:
                default:
                    hpTextPos = new Vector2(healthHitMin.X - hts.X - 6f, center.Y - hts.Y * 0.5f);
                    break;
            }

            var fg = ImGui.GetForegroundDrawList();
            uint outlineColor = ColorToUint32(Color.Black);
            uint textColor = ColorToUint32(Config.ESPHealthColor);

            // Outline for readability.
            fg.AddText(hpTextPos + new Vector2(1, 1), outlineColor, ht);
            fg.AddText(hpTextPos, textColor, ht);
        }

        //public void DrawGlowingBall(Vector2 position, Color color, float radius)
        //{
        //    var drawList = ImGui.GetBackgroundDrawList();
        //    uint ballColor = ColorToUint32(color);


        //    for (int i = 0; i < 5; i++)
        //    {
        //        float glowRadius = radius + (i * 2); 
        //        float alpha = 1.0f - (i * 0.2f);    

        //        drawList.AddCircleFilled(
        //            position,
        //            glowRadius,
        //            ImGui.ColorConvertFloat4ToU32(new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, alpha)),
        //            50 
        //        );
        //    }


        //    drawList.AddCircleFilled(position, radius, ballColor, 50);
        //}

        public void DrawCorneredBox(float X, float Y, float W, float H, uint color, float thickness)
        {
            var vList = ImGui.GetBackgroundDrawList();
            DrawCorneredBoxDrawList(vList, X, Y, W, H, color, thickness);
        }

        private static void DrawCorneredBoxDrawList(ImDrawListPtr dl, float X, float Y, float W, float H, uint color, float thickness)
        {
            float lineW = W / 3;
            float lineH = H / 3;
            uint outlineColor = 0xFF000000;
            float outlineThickness = thickness + 2f;

            // Draw outlines first
            dl.AddLine(new Vector2(X, Y - outlineThickness / 2), new Vector2(X, Y + lineH), outlineColor, outlineThickness);
            dl.AddLine(new Vector2(X - outlineThickness / 2, Y), new Vector2(X + lineW, Y), outlineColor, outlineThickness);
            dl.AddLine(new Vector2(X + W - lineW, Y), new Vector2(X + W + outlineThickness / 2, Y), outlineColor, outlineThickness);
            dl.AddLine(new Vector2(X + W, Y - outlineThickness / 2), new Vector2(X + W, Y + lineH), outlineColor, outlineThickness);
            dl.AddLine(new Vector2(X, Y + H - lineH), new Vector2(X, Y + H + outlineThickness / 2), outlineColor, outlineThickness);
            dl.AddLine(new Vector2(X - outlineThickness / 2, Y + H), new Vector2(X + lineW, Y + H), outlineColor, outlineThickness);
            dl.AddLine(new Vector2(X + W - lineW, Y + H), new Vector2(X + W + outlineThickness / 2, Y + H), outlineColor, outlineThickness);
            dl.AddLine(new Vector2(X + W, Y + H - lineH), new Vector2(X + W, Y + H + outlineThickness / 2), outlineColor, outlineThickness);

            // Draw main colored lines
            dl.AddLine(new Vector2(X, Y - thickness / 2), new Vector2(X, Y + lineH), color, thickness);
            dl.AddLine(new Vector2(X - thickness / 2, Y), new Vector2(X + lineW, Y), color, thickness);
            dl.AddLine(new Vector2(X + W - lineW, Y), new Vector2(X + W + thickness / 2, Y), color, thickness);
            dl.AddLine(new Vector2(X + W, Y - thickness / 2), new Vector2(X + W, Y + lineH), color, thickness);
            dl.AddLine(new Vector2(X, Y + H - lineH), new Vector2(X, Y + H + thickness / 2), color, thickness);
            dl.AddLine(new Vector2(X - thickness / 2, Y + H), new Vector2(X + lineW, Y + H), color, thickness);
            dl.AddLine(new Vector2(X + W - lineW, Y + H), new Vector2(X + W + thickness / 2, Y + H), color, thickness);
            dl.AddLine(new Vector2(X + W, Y + H - lineH), new Vector2(X + W, Y + H + thickness / 2), color, thickness);
        }

        private static void DrawFullBox(ImDrawListPtr drawList, float x, float y, float w, float h, uint color, float thickness, bool isKnocked)
        {
            float newWidth = w * 0.90f;
            float newHeight = h * 1.17f;
            float newX = x + (w - newWidth) / 2.0f;
            float newY = y - (newHeight - h) + 3.0f;

            drawList.AddRect(
                new Vector2(newX, newY),
                new Vector2(newX + newWidth, newY + newHeight),
                color,
                0.9f,
                ImDrawFlags.None,
                thickness * 0.5f
            );
        }

        private static void DrawGlowCorneredBox(ImDrawListPtr drawList, float x, float y, float w, float h, uint color, float thickness, float glowRadius, float feather, bool fillEnabled, bool isKnocked)
        {
            float newWidth = w;
            float newHeight = h * 1.15f;
            float newY = y - (newHeight - h);

            Vector4 baseColorVec = ImGui.ColorConvertU32ToFloat4(color);

            for (int i = (int)glowRadius; i > 0; --i)
            {
                float alphaFactor = 25.0f * (1.0f - ((float)i / glowRadius));
                uint glowColor = isKnocked
                    ? ImGui.GetColorU32(new Vector4(1f, 0f, 0f, alphaFactor / 255.0f))
                    : ImGui.GetColorU32(new Vector4(baseColorVec.X, baseColorVec.Y, baseColorVec.Z, alphaFactor / 255.0f));

                float offset = i * 0.5f;
                float sixthW = newWidth / 6.0f;
                float sixthH = newHeight / 6.0f;

                // Top-left
                drawList.AddLine(new Vector2(x - offset, newY - offset), new Vector2(x + sixthW + offset, newY - offset), glowColor, thickness);
                drawList.AddLine(new Vector2(x - offset, newY - offset), new Vector2(x - offset, newY + sixthH + offset), glowColor, thickness);

                // Top-right
                drawList.AddLine(new Vector2(x + newWidth + offset, newY - offset), new Vector2(x + newWidth - sixthW - offset, newY - offset), glowColor, thickness);
                drawList.AddLine(new Vector2(x + newWidth + offset, newY - offset), new Vector2(x + newWidth + offset, newY + sixthH + offset), glowColor, thickness);

                // Bottom-left
                drawList.AddLine(new Vector2(x - offset, newY + newHeight + offset), new Vector2(x + sixthW + offset, newY + newHeight + offset), glowColor, thickness);
                drawList.AddLine(new Vector2(x - offset, newY + newHeight + offset), new Vector2(x - offset, newY + newHeight - sixthH - offset), glowColor, thickness);

                // Bottom-right
                drawList.AddLine(new Vector2(x + newWidth + offset, newY + newHeight + offset), new Vector2(x + newWidth - sixthW - offset, newY + newHeight + offset), glowColor, thickness);
                drawList.AddLine(new Vector2(x + newWidth + offset, newY + newHeight + offset), new Vector2(x + newWidth + offset, newY + newHeight - sixthH - offset), glowColor, thickness);
            }

            float lineW = newWidth / 6.0f;
            float lineH = newHeight / 6.0f;

            // Top-left
            drawList.AddLine(new Vector2(x, newY), new Vector2(x + lineW, newY), color, thickness);
            drawList.AddLine(new Vector2(x, newY), new Vector2(x, newY + lineH), color, thickness);

            // Top-right
            drawList.AddLine(new Vector2(x + newWidth, newY), new Vector2(x + newWidth - lineW, newY), color, thickness);
            drawList.AddLine(new Vector2(x + newWidth, newY), new Vector2(x + newWidth, newY + lineH), color, thickness);

            // Bottom-left
            drawList.AddLine(new Vector2(x, newY + newHeight), new Vector2(x + lineW, newY + newHeight), color, thickness);
            drawList.AddLine(new Vector2(x, newY + newHeight), new Vector2(x, newY + newHeight - lineH), color, thickness);

            // Bottom-right
            drawList.AddLine(new Vector2(x + newWidth, newY + newHeight), new Vector2(x + newWidth - lineW, newY + newHeight), color, thickness);
            drawList.AddLine(new Vector2(x + newWidth, newY + newHeight), new Vector2(x + newWidth, newY + newHeight - lineH), color, thickness);
        }

        private static void DrawVerticalHealthBar(ImDrawListPtr drawList, short currentHealth, short maxHealth, Vector2 position, float totalHeight, bool isKnocked, int glowRadius)
        {
            float healthPercentage = Math.Clamp((float)currentHealth / maxHealth, 0f, 1f);
            float filledBarHeight = totalHeight * healthPercentage;
            float barWidth = 2.5f;

            Vector4 healthBarColor;
            Vector4 glowColor;
            if (isKnocked)
            {
                healthBarColor = new Vector4(1f, 0f, 0f, 1f);
                glowColor = healthBarColor;
            }
            else
            {
                Vector4 green = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
                Vector4 yellow = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
                Vector4 red = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);

                if (healthPercentage > 0.5f)
                    healthBarColor = Vector4.Lerp(yellow, green, (healthPercentage - 0.5f) * 2f);
                else
                    healthBarColor = Vector4.Lerp(red, yellow, healthPercentage * 2f);

                glowColor = healthPercentage > 0.5f ? green : (healthPercentage > 0.25f ? yellow : red);
            }

            uint barColorU32 = ImGui.GetColorU32(healthBarColor);

            drawList.AddRectFilled(
                position,
                position + new Vector2(barWidth, totalHeight),
                isKnocked ? ImGui.GetColorU32(new Vector4(80f/255f, 0f, 0f, 180f/255f)) : ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 128f/255f))
            );

            for (int i = glowRadius; i > 0; --i)
            {
                float alpha = 15.0f * (1.0f - ((float)i / glowRadius));
                drawList.AddRectFilled(
                    new Vector2(position.X - i, position.Y + (totalHeight - filledBarHeight) - i),
                    new Vector2(position.X + barWidth + i, position.Y + totalHeight + i),
                    ImGui.GetColorU32(new Vector4(glowColor.X, glowColor.Y, glowColor.Z, alpha / 255f))
                );
            }

            float fillTop = isKnocked ? position.Y : (position.Y + (totalHeight - filledBarHeight));
            float fillH = isKnocked ? totalHeight : filledBarHeight;
            drawList.AddRectFilled(
                new Vector2(position.X, fillTop),
                new Vector2(position.X + barWidth, fillTop + fillH),
                barColorU32
            );
        }

        private static void DrawHealthBarBelowFullBox(ImDrawListPtr drawList, short currentHealth, short maxHealth, float x, float y, float w, float h, bool isKnocked)
        {
            float healthPercentage = Math.Clamp((float)currentHealth / maxHealth, 0f, 1f);
            float filledBarWidth = isKnocked ? w : (w * healthPercentage);
            float barHeight = 3.5f;
            float offsetY = 3.5f;
            float rounding = 0.8f;
            float outlineSize = 1.5f;
            int glowRadius = 8;

            Vector4 healthBarColor;
            Vector4 glowColor;
            if (isKnocked)
            {
                healthBarColor = new Vector4(1f, 0f, 0f, 1f);
                glowColor = healthBarColor;
            }
            else
            {
                Vector4 green = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
                Vector4 yellow = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
                Vector4 red = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);

                if (healthPercentage > 0.5f)
                    healthBarColor = Vector4.Lerp(yellow, green, (healthPercentage - 0.5f) * 2f);
                else
                    healthBarColor = Vector4.Lerp(red, yellow, healthPercentage * 2f);

                glowColor = healthPercentage > 0.5f ? green : (healthPercentage > 0.25f ? yellow : red);
            }

            uint barColorU32 = ImGui.GetColorU32(healthBarColor);
            uint outlineColor = ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 200f/255f));

            for (int i = glowRadius; i > 0; --i)
            {
                float alpha = 15.0f * (1.0f - ((float)i / glowRadius));
                drawList.AddRectFilled(
                    new Vector2(x - i, y + h + offsetY - i),
                    new Vector2(x + filledBarWidth + i, y + h + offsetY + barHeight + i),
                    ImGui.GetColorU32(new Vector4(glowColor.X, glowColor.Y, glowColor.Z, alpha / 255f)),
                    rounding
                );
            }

            drawList.AddRect(
                new Vector2(x, y + h + offsetY),
                new Vector2(x + w, y + h + offsetY + barHeight),
                outlineColor,
                rounding,
                ImDrawFlags.RoundCornersAll,
                outlineSize
            );

            if (filledBarWidth > 0)
            {
                float actualRounding = (filledBarWidth < rounding * 2) ? filledBarWidth / 2 : rounding;
                drawList.AddRectFilled(
                    new Vector2(x, y + h + offsetY),
                    new Vector2(x + filledBarWidth, y + h + offsetY + barHeight),
                    barColorU32,
                    actualRounding,
                    ImDrawFlags.RoundCornersAll
                );

                if (filledBarWidth < w)
                {
                    drawList.AddRectFilled(
                        new Vector2(x + filledBarWidth - 0.5f, y + h + offsetY),
                        new Vector2(x + filledBarWidth + 0.5f, y + h + offsetY + barHeight),
                        outlineColor
                    );
                }
            }
        }


        private const float EspOffsetRefCornerHeight = 200f;

        private static Vector2 ScaleEspOffsetPx(Vector2 editorPx, float cornerHeight)
        {
            float refH = Math.Max(1f, EspOffsetRefCornerHeight);
            float s = cornerHeight / refH;
            return editorPx * s;
        }

        private const float EspAnchorGapPx = 5f;

        private static bool IsNameAnchorTopRow(EspNameBoxAnchor a) =>
            a == EspNameBoxAnchor.BoxTopCenter || a == EspNameBoxAnchor.BoxTopLeft || a == EspNameBoxAnchor.BoxTopRight;

        private static void GetEspBoxBounds(Vector2 headScreen, float cornerWidth, float cornerHeight, Vector2 offBox,
            out float bl, out float bt, out float br, out float bb, out float midX, out float midY)
        {
            bl = headScreen.X - cornerWidth * 0.5f + offBox.X;
            bt = headScreen.Y + offBox.Y;
            br = bl + cornerWidth;
            bb = bt + cornerHeight;
            midX = bl + cornerWidth * 0.5f;
            midY = bt + cornerHeight * 0.5f;
        }

        private static Vector2 GetNameBoxAnchorTopLeft(EspNameBoxAnchor anchor, float bl, float bt, float br, float bb, float midX, float midY, float gap, float w, float h)
        {
            return anchor switch
            {
                EspNameBoxAnchor.BoxTopCenter => new Vector2(midX - w * 0.5f, bt - gap - h),
                EspNameBoxAnchor.BoxBottomCenter => new Vector2(midX - w * 0.5f, bb + gap),
                EspNameBoxAnchor.BoxTopLeft => new Vector2(bl - gap - w, bt),
                EspNameBoxAnchor.BoxTopRight => new Vector2(br + gap, bt),
                EspNameBoxAnchor.BoxBottomLeft => new Vector2(bl - gap - w, bb - h),
                EspNameBoxAnchor.BoxBottomRight => new Vector2(br + gap, bb - h),
                EspNameBoxAnchor.BoxMiddleLeft => new Vector2(bl - gap - w, midY - h * 0.5f),
                EspNameBoxAnchor.BoxMiddleRight => new Vector2(br + gap, midY - h * 0.5f),
                _ => new Vector2(midX - w * 0.5f, bt - gap - h),
            };
        }

        private static readonly EspNameBoxAnchor[] EspEditorNameAnchorOrder =
        {
            EspNameBoxAnchor.BoxTopCenter,
            EspNameBoxAnchor.BoxBottomCenter,
            EspNameBoxAnchor.BoxTopLeft,
            EspNameBoxAnchor.BoxTopRight,
            EspNameBoxAnchor.BoxBottomLeft,
            EspNameBoxAnchor.BoxBottomRight,
            EspNameBoxAnchor.BoxMiddleLeft,
            EspNameBoxAnchor.BoxMiddleRight,
        };

        private static Vector2 GetEspNameAnchorAttachPoint(EspNameBoxAnchor a, float bl, float bt, float br, float bb, float midX, float midY)
        {
            return a switch
            {
                EspNameBoxAnchor.BoxTopCenter => new Vector2(midX, bt),
                EspNameBoxAnchor.BoxBottomCenter => new Vector2(midX, bb),
                EspNameBoxAnchor.BoxTopLeft => new Vector2(bl, bt),
                EspNameBoxAnchor.BoxTopRight => new Vector2(br, bt),
                EspNameBoxAnchor.BoxBottomLeft => new Vector2(bl, bb),
                EspNameBoxAnchor.BoxBottomRight => new Vector2(br, bb),
                EspNameBoxAnchor.BoxMiddleLeft => new Vector2(bl, midY),
                EspNameBoxAnchor.BoxMiddleRight => new Vector2(br, midY),
                _ => new Vector2(midX, bt),
            };
        }

        private static Vector2 GetEspDistanceAnchorAttachPoint(EspDistanceBoxAnchor d, EspNameBoxAnchor nameA,
            Vector2 npTL, float npW, float npH, float bl, float bt, float br, float bb, float midX, float midY, float gap)
        {
            if (d == EspDistanceBoxAnchor.InsideNameplate)
                return npTL + new Vector2(npW * 0.5f, npH * 0.5f);
            if (d == EspDistanceBoxAnchor.AboveNameWhenNameAtTop)
            {
                if (IsNameAnchorTopRow(nameA))
                    return new Vector2(npTL.X + npW * 0.5f, npTL.Y);
                return new Vector2(midX, bt);
            }
            EspNameBoxAnchor map = d switch
            {
                EspDistanceBoxAnchor.BoxTopCenter => EspNameBoxAnchor.BoxTopCenter,
                EspDistanceBoxAnchor.BoxBottomCenter => EspNameBoxAnchor.BoxBottomCenter,
                EspDistanceBoxAnchor.BoxTopLeft => EspNameBoxAnchor.BoxTopLeft,
                EspDistanceBoxAnchor.BoxTopRight => EspNameBoxAnchor.BoxTopRight,
                EspDistanceBoxAnchor.BoxBottomLeft => EspNameBoxAnchor.BoxBottomLeft,
                EspDistanceBoxAnchor.BoxBottomRight => EspNameBoxAnchor.BoxBottomRight,
                EspDistanceBoxAnchor.BoxMiddleLeft => EspNameBoxAnchor.BoxMiddleLeft,
                EspDistanceBoxAnchor.BoxMiddleRight => EspNameBoxAnchor.BoxMiddleRight,
                _ => EspNameBoxAnchor.BoxTopCenter,
            };
            return GetEspNameAnchorAttachPoint(map, bl, bt, br, bb, midX, midY);
        }

        private static Vector2 GetEspHealthAnchorAttachPoint(EspHealthBoxAnchor a, float bl, float bt, float br, float bb, float midX, float midY, float cornerH)
        {
            float my = bt + cornerH * 0.5f;
            return a switch
            {
                EspHealthBoxAnchor.BoxTop => new Vector2(midX, bt),
                EspHealthBoxAnchor.BoxBottom => new Vector2(midX, bb),
                EspHealthBoxAnchor.BoxLeft => new Vector2(bl, my),
                _ => new Vector2(br, my),
            };
        }

        private static void DrawEspEditorNameSnapSlots(ImDrawListPtr dl, float boxL, float boxT, float boxR, float boxB, float boxMidX, float boxMidY, float gap, float slotW, float slotH, EspNameBoxAnchor currentAnchor, EspNameBoxAnchor? highlightAnchor)
        {
            const float rounding = 2.5f;
            uint fillBase = 0x20FFFFFF;
            uint fillCurrent = 0x40B0B0FF;
            uint fillHi = 0x55C8E0FF;
            uint borderCol = 0x5AFFFFFF;
            foreach (EspNameBoxAnchor a in EspEditorNameAnchorOrder)
            {
                Vector2 tl = GetNameBoxAnchorTopLeft(a, boxL, boxT, boxR, boxB, boxMidX, boxMidY, gap, slotW, slotH);
                Vector2 brc = tl + new Vector2(slotW, slotH);
                bool isCur = a == currentAnchor;
                bool isHi = highlightAnchor.HasValue && a == highlightAnchor.Value;
                uint fill = isHi ? fillHi : (isCur ? fillCurrent : fillBase);
                dl.AddRectFilled(tl, brc, fill, rounding, ImDrawFlags.RoundCornersAll);
                dl.AddRect(tl, brc, borderCol, rounding, ImDrawFlags.RoundCornersAll, 1f);
            }
        }

        private static EspNameBoxAnchor? EspEditorNameSlotUnderMouse(Vector2 mouse, float boxL, float boxT, float boxR, float boxB, float boxMidX, float boxMidY, float gap, float slotW, float slotH)
        {
            const float padIn = 12f;
            foreach (EspNameBoxAnchor a in EspEditorNameAnchorOrder)
            {
                Vector2 tl = GetNameBoxAnchorTopLeft(a, boxL, boxT, boxR, boxB, boxMidX, boxMidY, gap, slotW, slotH);
                Vector2 brc = tl + new Vector2(slotW, slotH);
                if (EspEditorPointInRectPad(mouse, tl, brc, padIn))
                    return a;
            }
            return null;
        }

        private static bool TrySnapEspNameAnchorAtMouse(Vector2 mouse, float boxL, float boxT, float boxR, float boxB, float boxMidX, float boxMidY, float gap, float slotW, float slotH, out EspNameBoxAnchor chosen)
        {
            const float padIn = 12f;
            chosen = EspNameBoxAnchor.BoxTopCenter;
            foreach (EspNameBoxAnchor a in EspEditorNameAnchorOrder)
            {
                Vector2 tl = GetNameBoxAnchorTopLeft(a, boxL, boxT, boxR, boxB, boxMidX, boxMidY, gap, slotW, slotH);
                Vector2 brc = tl + new Vector2(slotW, slotH);
                if (EspEditorPointInRectPad(mouse, tl, brc, padIn))
                {
                    chosen = a;
                    return true;
                }
            }
            return false;
        }

        private static readonly EspDistanceBoxAnchor[] EspEditorDistanceAnchorOrder =
        {
            EspDistanceBoxAnchor.InsideNameplate,
            EspDistanceBoxAnchor.AboveNameWhenNameAtTop,
            EspDistanceBoxAnchor.BoxTopCenter,
            EspDistanceBoxAnchor.BoxBottomCenter,
            EspDistanceBoxAnchor.BoxTopLeft,
            EspDistanceBoxAnchor.BoxTopRight,
            EspDistanceBoxAnchor.BoxBottomLeft,
            EspDistanceBoxAnchor.BoxBottomRight,
            EspDistanceBoxAnchor.BoxMiddleLeft,
            EspDistanceBoxAnchor.BoxMiddleRight,
        };

        private static readonly EspHealthBoxAnchor[] EspEditorHealthAnchorOrder =
        {
            EspHealthBoxAnchor.BoxRight,
            EspHealthBoxAnchor.BoxLeft,
            EspHealthBoxAnchor.BoxTop,
            EspHealthBoxAnchor.BoxBottom,
        };

        private static void EspEditorGetDistanceSlotRect(EspDistanceBoxAnchor d, EspNameBoxAnchor nameA, Vector2 distSz,
            Vector2 npTL, float npW, float npH,
            float bl, float bt, float br, float bb, float midX, float midY, float gap,
            out Vector2 mn, out Vector2 mx)
        {
            if (d == EspDistanceBoxAnchor.InsideNameplate)
            {
                mn = npTL;
                mx = npTL + new Vector2(npW, npH);
                return;
            }
            Vector2 tl = GetDistanceBoxAnchorTopLeft(d, nameA, distSz, npTL, npW, npH, bl, bt, br, bb, midX, midY, gap);
            mn = tl;
            mx = tl + distSz;
        }

        private static void EspEditorGetHealthBarSlotRect(EspHealthBoxAnchor a, float bl, float bt, float br, float bb, float cornerW, float cornerH, float gap, float t,
            out Vector2 mn, out Vector2 mx)
        {
            switch (a)
            {
                case EspHealthBoxAnchor.BoxTop:
                    {
                        float y0 = bt - gap - t;
                        mn = new Vector2(bl, y0);
                        mx = new Vector2(br, y0 + t);
                        break;
                    }
                case EspHealthBoxAnchor.BoxBottom:
                    {
                        float y0 = bb + gap;
                        mn = new Vector2(bl, y0);
                        mx = new Vector2(br, y0 + t);
                        break;
                    }
                case EspHealthBoxAnchor.BoxLeft:
                    {
                        Vector2 p = new Vector2(bl - 5f - t, bt);
                        mn = p;
                        mx = p + new Vector2(t, cornerH);
                        break;
                    }
                default:
                    {
                        Vector2 p = new Vector2(br + 5f, bt);
                        mn = p;
                        mx = p + new Vector2(t, cornerH);
                        break;
                    }
            }
        }

        /// <summary>Actual drawn health bar rect (same math as editor preview draw) for hit-testing before draw.</summary>
        private static void EspEditorGetHealthBarDrawHitBounds(
            EspHealthBoxAnchor anchor,
            float boxL, float boxT, float boxR, float boxB,
            float cornerWidth, float cornerHeight, float gapScaled, float t,
            Vector2 offHealth,
            out Vector2 hitMin, out Vector2 hitMax)
        {
            switch (anchor)
            {
                case EspHealthBoxAnchor.BoxTop:
                    {
                        float y0 = boxT - gapScaled - t;
                        hitMin = new Vector2(boxL + offHealth.X, y0 + offHealth.Y);
                        hitMax = new Vector2(boxR + offHealth.X, y0 + t + offHealth.Y);
                        break;
                    }
                case EspHealthBoxAnchor.BoxBottom:
                    {
                        float y0 = boxB + gapScaled;
                        hitMin = new Vector2(boxL + offHealth.X, y0 + offHealth.Y);
                        hitMax = new Vector2(boxR + offHealth.X, y0 + t + offHealth.Y);
                        break;
                    }
                case EspHealthBoxAnchor.BoxLeft:
                    {
                        Vector2 p = new Vector2(boxL - 5f - t, boxT) + offHealth;
                        hitMin = p;
                        hitMax = p + new Vector2(t, cornerHeight);
                        break;
                    }
                default:
                    {
                        Vector2 p = new Vector2(boxR + 5f, boxT) + offHealth;
                        hitMin = p;
                        hitMax = p + new Vector2(t, cornerHeight);
                        break;
                    }
            }
        }

        private static void EspEditorGetWeaponSlotRect(EspWeaponBoxAnchor a, float bl, float bt, float br, float bb, float midX, float midY, float gap, Vector2 iconSz,
            out Vector2 mn, out Vector2 mx)
        {
            EspNameBoxAnchor map = a switch
            {
                EspWeaponBoxAnchor.BoxTopCenter => EspNameBoxAnchor.BoxTopCenter,
                EspWeaponBoxAnchor.BoxBottomCenter => EspNameBoxAnchor.BoxBottomCenter,
                EspWeaponBoxAnchor.BoxTopLeft => EspNameBoxAnchor.BoxTopLeft,
                EspWeaponBoxAnchor.BoxTopRight => EspNameBoxAnchor.BoxTopRight,
                EspWeaponBoxAnchor.BoxBottomLeft => EspNameBoxAnchor.BoxBottomLeft,
                EspWeaponBoxAnchor.BoxBottomRight => EspNameBoxAnchor.BoxBottomRight,
                EspWeaponBoxAnchor.BoxMiddleLeft => EspNameBoxAnchor.BoxMiddleLeft,
                EspWeaponBoxAnchor.BoxMiddleRight => EspNameBoxAnchor.BoxMiddleRight,
                _ => EspNameBoxAnchor.BoxTopCenter,
            };
            mn = GetNameBoxAnchorTopLeft(map, bl, bt, br, bb, midX, midY, gap, iconSz.X, iconSz.Y);
            mx = mn + iconSz;
        }

        private static void DrawEspEditorDistanceSnapSlots(ImDrawListPtr dl,
            EspNameBoxAnchor nameA, Vector2 distSz, Vector2 npTL, float npW, float npH,
            float bl, float bt, float br, float bb, float midX, float midY, float gap,
            EspDistanceBoxAnchor current, EspDistanceBoxAnchor? highlight)
        {
            const float rounding = 2f;
            uint fillBase = 0x22FFE090;
            uint fillCurrent = 0x45FFCC66;
            uint fillHi = 0x58FFEEDD;
            uint borderCol = 0x66FFCC44;
            foreach (EspDistanceBoxAnchor d in EspEditorDistanceAnchorOrder)
            {
                EspEditorGetDistanceSlotRect(d, nameA, distSz, npTL, npW, npH, bl, bt, br, bb, midX, midY, gap, out Vector2 mn, out Vector2 mx);
                bool isCur = d == current;
                bool isHi = highlight.HasValue && d == highlight.Value;
                uint fill = isHi ? fillHi : (isCur ? fillCurrent : fillBase);
                dl.AddRectFilled(mn, mx, fill, rounding, ImDrawFlags.RoundCornersAll);
                dl.AddRect(mn, mx, borderCol, rounding, ImDrawFlags.RoundCornersAll, 1f);
            }
        }

        private static void DrawEspEditorHealthSnapSlots(ImDrawListPtr dl,
            float bl, float bt, float br, float bb, float cornerW, float cornerH, float gap, float t,
            EspHealthBoxAnchor current, EspHealthBoxAnchor? highlight)
        {
            const float rounding = 2f;
            uint fillBase = 0x2290FF90;
            uint fillCurrent = 0x45AAFFAA;
            uint fillHi = 0x55CCFFCC;
            uint borderCol = 0x6644FF66;
            foreach (EspHealthBoxAnchor h in EspEditorHealthAnchorOrder)
            {
                EspEditorGetHealthBarSlotRect(h, bl, bt, br, bb, cornerW, cornerH, gap, t, out Vector2 mn, out Vector2 mx);
                bool isCur = h == current;
                bool isHi = highlight.HasValue && h == highlight.Value;
                uint fill = isHi ? fillHi : (isCur ? fillCurrent : fillBase);
                dl.AddRectFilled(mn, mx, fill, rounding, ImDrawFlags.RoundCornersAll);
                dl.AddRect(mn, mx, borderCol, rounding, ImDrawFlags.RoundCornersAll, 1f);
            }
        }

        private static void DrawEspEditorWeaponSnapSlots(ImDrawListPtr dl,
            float bl, float bt, float br, float bb, float midX, float midY, float gap, Vector2 iconSz,
            EspWeaponBoxAnchor current, EspWeaponBoxAnchor? highlight)
        {
            const float rounding = 2f;
            uint fillBase = 0x2290D0FF;
            uint fillCurrent = 0x45A0E0FF;
            uint fillHi = 0x55B8F0FF;
            uint borderCol = 0x6644AAFF;
            foreach (EspWeaponBoxAnchor w in new[]
            {
                EspWeaponBoxAnchor.BoxTopCenter,
                EspWeaponBoxAnchor.BoxBottomCenter,
                EspWeaponBoxAnchor.BoxTopLeft,
                EspWeaponBoxAnchor.BoxTopRight,
                EspWeaponBoxAnchor.BoxBottomLeft,
                EspWeaponBoxAnchor.BoxBottomRight,
                EspWeaponBoxAnchor.BoxMiddleLeft,
                EspWeaponBoxAnchor.BoxMiddleRight,
            })
            {
                EspEditorGetWeaponSlotRect(w, bl, bt, br, bb, midX, midY, gap, iconSz, out Vector2 mn, out Vector2 mx);
                bool isCur = w == current;
                bool isHi = highlight.HasValue && w == highlight.Value;
                uint fill = isHi ? fillHi : (isCur ? fillCurrent : fillBase);
                dl.AddRectFilled(mn, mx, fill, rounding, ImDrawFlags.RoundCornersAll);
                dl.AddRect(mn, mx, borderCol, rounding, ImDrawFlags.RoundCornersAll, 1f);
            }
        }

        private static EspDistanceBoxAnchor? EspEditorDistanceSlotUnderMouse(Vector2 mouse, EspNameBoxAnchor nameA, Vector2 distSz,
            Vector2 npTL, float npW, float npH, float bl, float bt, float br, float bb, float midX, float midY, float gap)
        {
            const float padIn = 10f;
            foreach (EspDistanceBoxAnchor d in EspEditorDistanceAnchorOrder)
            {
                EspEditorGetDistanceSlotRect(d, nameA, distSz, npTL, npW, npH, bl, bt, br, bb, midX, midY, gap, out Vector2 mn, out Vector2 mx);
                if (EspEditorPointInRectPad(mouse, mn, mx, padIn))
                    return d;
            }
            return null;
        }

        private static EspHealthBoxAnchor? EspEditorHealthSlotUnderMouse(Vector2 mouse, float bl, float bt, float br, float bb, float cornerW, float cornerH, float gap, float t)
        {
            const float padIn = 10f;
            foreach (EspHealthBoxAnchor h in EspEditorHealthAnchorOrder)
            {
                EspEditorGetHealthBarSlotRect(h, bl, bt, br, bb, cornerW, cornerH, gap, t, out Vector2 mn, out Vector2 mx);
                if (EspEditorPointInRectPad(mouse, mn, mx, padIn))
                    return h;
            }
            return null;
        }

        private static EspWeaponBoxAnchor? EspEditorWeaponSlotUnderMouse(Vector2 mouse, float bl, float bt, float br, float bb, float midX, float midY, float gap, Vector2 iconSz)
        {
            const float padIn = 10f;
            foreach (EspWeaponBoxAnchor w in new[]
            {
                EspWeaponBoxAnchor.BoxTopCenter,
                EspWeaponBoxAnchor.BoxBottomCenter,
                EspWeaponBoxAnchor.BoxTopLeft,
                EspWeaponBoxAnchor.BoxTopRight,
                EspWeaponBoxAnchor.BoxBottomLeft,
                EspWeaponBoxAnchor.BoxBottomRight,
                EspWeaponBoxAnchor.BoxMiddleLeft,
                EspWeaponBoxAnchor.BoxMiddleRight,
            })
            {
                EspEditorGetWeaponSlotRect(w, bl, bt, br, bb, midX, midY, gap, iconSz, out Vector2 mn, out Vector2 mx);
                if (EspEditorPointInRectPad(mouse, mn, mx, padIn))
                    return w;
            }
            return null;
        }

        private static bool TrySnapEspDistanceAnchorAtMouse(Vector2 mouse, EspNameBoxAnchor nameA, Vector2 distSz,
            Vector2 npTL, float npW, float npH, float bl, float bt, float br, float bb, float midX, float midY, float gap,
            out EspDistanceBoxAnchor chosen)
        {
            const float padIn = 10f;
            chosen = EspDistanceBoxAnchor.InsideNameplate;
            foreach (EspDistanceBoxAnchor d in EspEditorDistanceAnchorOrder)
            {
                EspEditorGetDistanceSlotRect(d, nameA, distSz, npTL, npW, npH, bl, bt, br, bb, midX, midY, gap, out Vector2 mn, out Vector2 mx);
                if (EspEditorPointInRectPad(mouse, mn, mx, padIn))
                {
                    chosen = d;
                    return true;
                }
            }
            return false;
        }

        private static bool TrySnapEspHealthAnchorAtMouse(Vector2 mouse, float bl, float bt, float br, float bb, float cornerW, float cornerH, float gap, float t,
            out EspHealthBoxAnchor chosen)
        {
            const float padIn = 10f;
            chosen = EspHealthBoxAnchor.BoxRight;
            foreach (EspHealthBoxAnchor h in EspEditorHealthAnchorOrder)
            {
                EspEditorGetHealthBarSlotRect(h, bl, bt, br, bb, cornerW, cornerH, gap, t, out Vector2 mn, out Vector2 mx);
                if (EspEditorPointInRectPad(mouse, mn, mx, padIn))
                {
                    chosen = h;
                    return true;
                }
            }
            return false;
        }

        private static bool TrySnapEspWeaponAnchorAtMouse(Vector2 mouse, float bl, float bt, float br, float bb, float midX, float midY, float gap, Vector2 iconSz,
            out EspWeaponBoxAnchor chosen)
        {
            const float padIn = 10f;
            chosen = EspWeaponBoxAnchor.BoxTopCenter;
            foreach (EspWeaponBoxAnchor w in new[]
            {
                EspWeaponBoxAnchor.BoxTopCenter,
                EspWeaponBoxAnchor.BoxBottomCenter,
                EspWeaponBoxAnchor.BoxTopLeft,
                EspWeaponBoxAnchor.BoxTopRight,
                EspWeaponBoxAnchor.BoxBottomLeft,
                EspWeaponBoxAnchor.BoxBottomRight,
                EspWeaponBoxAnchor.BoxMiddleLeft,
                EspWeaponBoxAnchor.BoxMiddleRight,
            })
            {
                EspEditorGetWeaponSlotRect(w, bl, bt, br, bb, midX, midY, gap, iconSz, out Vector2 mn, out Vector2 mx);
                if (EspEditorPointInRectPad(mouse, mn, mx, padIn))
                {
                    chosen = w;
                    return true;
                }
            }
            return false;
        }

        private static Vector2 GetDistanceBoxAnchorTopLeft(EspDistanceBoxAnchor d, EspNameBoxAnchor nameA, Vector2 distSz,
            Vector2 namePlateTL, float npW, float npH, float bl, float bt, float br, float bb, float midX, float midY, float gap)
        {
            if (d == EspDistanceBoxAnchor.InsideNameplate)
                return Vector2.Zero;

            if (d == EspDistanceBoxAnchor.AboveNameWhenNameAtTop)
            {
                if (IsNameAnchorTopRow(nameA))
                    return new Vector2(namePlateTL.X + npW * 0.5f - distSz.X * 0.5f, namePlateTL.Y - gap - distSz.Y);
                return new Vector2(midX - distSz.X * 0.5f, bt - gap - distSz.Y);
            }

            EspNameBoxAnchor map = d switch
            {
                EspDistanceBoxAnchor.BoxTopCenter => EspNameBoxAnchor.BoxTopCenter,
                EspDistanceBoxAnchor.BoxBottomCenter => EspNameBoxAnchor.BoxBottomCenter,
                EspDistanceBoxAnchor.BoxTopLeft => EspNameBoxAnchor.BoxTopLeft,
                EspDistanceBoxAnchor.BoxTopRight => EspNameBoxAnchor.BoxTopRight,
                EspDistanceBoxAnchor.BoxBottomLeft => EspNameBoxAnchor.BoxBottomLeft,
                EspDistanceBoxAnchor.BoxBottomRight => EspNameBoxAnchor.BoxBottomRight,
                EspDistanceBoxAnchor.BoxMiddleLeft => EspNameBoxAnchor.BoxMiddleLeft,
                EspDistanceBoxAnchor.BoxMiddleRight => EspNameBoxAnchor.BoxMiddleRight,
                _ => EspNameBoxAnchor.BoxTopCenter,
            };
            return GetNameBoxAnchorTopLeft(map, bl, bt, br, bb, midX, midY, gap, distSz.X, distSz.Y);
        }

        private static EspNameBoxAnchor MapHealthAnchorToNameAnchor(EspHealthBoxAnchor a)
        {
            return a switch
            {
                EspHealthBoxAnchor.BoxTop => EspNameBoxAnchor.BoxTopCenter,
                EspHealthBoxAnchor.BoxBottom => EspNameBoxAnchor.BoxBottomCenter,
                EspHealthBoxAnchor.BoxLeft => EspNameBoxAnchor.BoxMiddleLeft,
                _ => EspNameBoxAnchor.BoxMiddleRight,
            };
        }

        private static EspNameBoxAnchor MapWeaponAnchorToNameAnchor(EspWeaponBoxAnchor a)
        {
            return a switch
            {
                EspWeaponBoxAnchor.BoxTopCenter => EspNameBoxAnchor.BoxTopCenter,
                EspWeaponBoxAnchor.BoxBottomCenter => EspNameBoxAnchor.BoxBottomCenter,
                EspWeaponBoxAnchor.BoxTopLeft => EspNameBoxAnchor.BoxTopLeft,
                EspWeaponBoxAnchor.BoxTopRight => EspNameBoxAnchor.BoxTopRight,
                EspWeaponBoxAnchor.BoxBottomLeft => EspNameBoxAnchor.BoxBottomLeft,
                EspWeaponBoxAnchor.BoxBottomRight => EspNameBoxAnchor.BoxBottomRight,
                EspWeaponBoxAnchor.BoxMiddleLeft => EspNameBoxAnchor.BoxMiddleLeft,
                EspWeaponBoxAnchor.BoxMiddleRight => EspNameBoxAnchor.BoxMiddleRight,
                _ => EspNameBoxAnchor.BoxTopCenter,
            };
        }

        private static bool IsOccupiedByOthers(EspNameBoxAnchor candidate, bool excludeName, bool excludeHealth, bool excludeWeapon)
        {
            if (!excludeName && Config.ESPName && Config.EspNameAnchor == candidate)
                return true;

            if (!excludeHealth && Config.ESPHealth && MapHealthAnchorToNameAnchor(Config.EspHealthAnchor) == candidate)
                return true;

            if (!excludeWeapon && Config.espweapon && MapWeaponAnchorToNameAnchor(Config.EspWeaponAnchor) == candidate)
                return true;

            return false;
        }

        private static Vector4 ColorToVec4(Color c)
        {
            return new Vector4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        }

        private static Color Vec4ToColor(Vector4 v)
        {
            v.X = Math.Clamp(v.X, 0f, 1f);
            v.Y = Math.Clamp(v.Y, 0f, 1f);
            v.Z = Math.Clamp(v.Z, 0f, 1f);
            v.W = Math.Clamp(v.W, 0f, 1f);
            return Color.FromArgb((int)(v.W * 255f), (int)(v.X * 255f), (int)(v.Y * 255f), (int)(v.Z * 255f));
        }

        private static string GetEspEditorColorTargetLabel(EspEditorColorTarget t)
        {
            return t switch
            {
                EspEditorColorTarget.Line => "Line",
                EspEditorColorTarget.Box => "Box",
                EspEditorColorTarget.Name => "Name",
                EspEditorColorTarget.Health => "Health",
                EspEditorColorTarget.Weapon => "Weapon",
                EspEditorColorTarget.Hacker => "Hacker",
                _ => "ESP",
            };
        }

        private bool TryGetEspEditorTargetColor(EspEditorColorTarget t, out Vector4 c)
        {
            c = new Vector4(1f, 1f, 1f, 1f);
            switch (t)
            {
                case EspEditorColorTarget.Line: c = ColorToVec4(Config.ESPLineColor); return true;
                case EspEditorColorTarget.Box: c = ColorToVec4(Config.ESPBoxColor); return true;
                case EspEditorColorTarget.Name: c = ColorToVec4(Config.ESPNameColor); return true;
                case EspEditorColorTarget.Health: c = ColorToVec4(Config.ESPHealthColor); return true;
                case EspEditorColorTarget.Weapon: c = Config.ICONCOLOR; return true;
                case EspEditorColorTarget.Hacker: c = ColorToVec4(Config.HackerTagColor); return true;
                default: return false;
            }
        }

        private void SetEspEditorTargetColor(EspEditorColorTarget t, Vector4 c)
        {
            switch (t)
            {
                case EspEditorColorTarget.Line: Config.ESPLineColor = Vec4ToColor(c); break;
                case EspEditorColorTarget.Box: Config.ESPBoxColor = Vec4ToColor(c); break;
                case EspEditorColorTarget.Name: Config.ESPNameColor = Vec4ToColor(c); break;
                case EspEditorColorTarget.Health: Config.ESPHealthColor = Vec4ToColor(c); break;
                case EspEditorColorTarget.Weapon: Config.ICONCOLOR = c; break;
                case EspEditorColorTarget.Hacker: Config.HackerTagColor = Vec4ToColor(c); break;
            }
        }

        private const float EspNameplateBarH = 20f;
        private const float EspNameplateHealthH = 3f;
        private const float EspNameplateAboveHeadGap = 5f;
        private const float EspNameplateIdW = 18f;
        private const float EspNameplateMinWidth = 128f;

        /// <summary>Keep full player names for ESP; only strip control chars (memory-safe).</summary>
        private static string SanitizeEspDisplayName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Training Bot";

            string cleaned = InternalMemory.CleanPlayerDisplayName(name);
            if (string.IsNullOrWhiteSpace(cleaned))
                return "Training Bot";
            if (cleaned.Length > 512)
                return cleaned.Substring(0, 509) + "...";
            return cleaned;
        }

        private static void EspDrawText(ImDrawListPtr dl, Vector2 pos, uint col, string text)
        {
            ImFontPtr font = EspNameFont.IsLoaded() ? EspNameFont : TextFont;
            if (font.IsLoaded())
                ImGui.PushFont(font);
            dl.AddText(pos, col, text);
            if (font.IsLoaded())
                ImGui.PopFont();
        }

        private static Vector2 EspCalcTextSize(string text)
        {
            if (string.IsNullOrEmpty(text))
                text = " ";
            ImFontPtr font = EspNameFont.IsLoaded() ? EspNameFont : TextFont;
            if (font.IsLoaded())
            {
                ImGui.PushFont(font);
                Vector2 s = ImGui.CalcTextSize(text);
                ImGui.PopFont();
                return s;
            }
            return ImGui.CalcTextSize(text);
        }

        private static void ComputeEspNameplateSize(string nameDisplay, string distStr, out float width, out float totalHeight)
        {
            const float padMain = 5f;
            Vector2 n = EspCalcTextSize(nameDisplay);
            Vector2 d = ImGui.CalcTextSize(distStr);
            width = EspNameplateIdW + padMain + n.X + 8f + d.X + padMain;
            width = Math.Max(EspNameplateMinWidth, width);
            totalHeight = EspNameplateBarH + EspNameplateHealthH;
        }

        private static void ComputeEspNameplateSizeNameOnly(string nameDisplay, out float width, out float totalHeight)
        {
            const float padMain = 5f;
            Vector2 n = EspCalcTextSize(nameDisplay);
            width = EspNameplateIdW + padMain + n.X + padMain;
            width = Math.Max(96f, width);
            totalHeight = EspNameplateBarH + EspNameplateHealthH;
        }

        private static void DrawEspInfoNameplate(
            ImDrawListPtr dl,
            Vector2 topLeft,
            float nameplateW,
            string nameDisplay,
            string distStr,
            int teamDigit,
            float healthFraction,
            bool knocked,
            bool drawDistanceInBar = true)
        {
            bool popFont = TextFont.IsLoaded();
            if (popFont) ImGui.PushFont(TextFont);

            const float padMain = 6f;
            float barH = EspNameplateBarH;
            float healthH = EspNameplateHealthH;
            float totalH = barH + healthH;
            teamDigit = Math.Abs(teamDigit % 10);

            Vector2 tl = topLeft;
            Vector2 br = tl + new Vector2(nameplateW, totalH);

            uint bgHealthTrack = ImGui.ColorConvertFloat4ToU32(new Vector4(0.04f, 0.04f, 0.05f, 0.95f));
            dl.AddRectFilled(new Vector2(tl.X, tl.Y + barH), br, bgHealthTrack);

            float hp = Math.Clamp(healthFraction, 0f, 1f);
            uint hpCol = knocked
                ? ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.25f, 0.25f, 1f))
                : ImGui.ColorConvertFloat4ToU32(new Vector4(0.15f, 0.95f, 0.2f, 1f));
            if (nameplateW * hp > 0.5f)
                dl.AddRectFilled(new Vector2(tl.X, tl.Y + barH), new Vector2(tl.X + nameplateW * hp, br.Y), hpCol);

            dl.AddRectFilled(tl, tl + new Vector2(EspNameplateIdW, barH), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)));
            dl.AddRect(tl, tl + new Vector2(EspNameplateIdW, barH), ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.55f)), 0f, ImDrawFlags.None, 1f);

            string idStr = teamDigit.ToString();
            Vector2 idSz = ImGui.CalcTextSize(idStr);
            dl.AddText(tl + new Vector2((EspNameplateIdW - idSz.X) * 0.5f, (barH - idSz.Y) * 0.5f),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 1f)), idStr);

            Vector2 mainMin = tl + new Vector2(EspNameplateIdW, 0f);
            Vector2 mainMax = tl + new Vector2(nameplateW, barH);
            uint cTL = ImGui.ColorConvertFloat4ToU32(new Vector4(0.07f, 0.32f, 0.38f, 0.98f));
            uint cTR = ImGui.ColorConvertFloat4ToU32(new Vector4(0.14f, 0.52f, 0.56f, 0.98f));
            uint cBL = ImGui.ColorConvertFloat4ToU32(new Vector4(0.06f, 0.26f, 0.32f, 0.98f));
            uint cBR = ImGui.ColorConvertFloat4ToU32(new Vector4(0.11f, 0.42f, 0.48f, 0.98f));
            dl.AddRectFilledMultiColor(mainMin, mainMax, cTL, cTR, cBR, cBL);

            Vector2 nameSz = EspCalcTextSize(nameDisplay);
            float nameX = mainMin.X + padMain;
            float textY = tl.Y + (barH - nameSz.Y) * 0.5f;
            EspDrawText(dl, new Vector2(nameX, textY), ColorToUint32(Config.ESPNameColor), nameDisplay);

            if (drawDistanceInBar)
            {
                Vector2 distSz = ImGui.CalcTextSize(distStr);
                float distX = mainMax.X - padMain - distSz.X;
                dl.AddText(new Vector2(distX, textY), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.93f, 0.2f, 1f)), distStr);
            }

            uint borderBlack = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.92f));
            dl.AddRect(tl, br, borderBlack, 0f, ImDrawFlags.None, 1f);

            if (popFont) ImGui.PopFont();
        }

        static uint ColorToUint32(Color color)
        {
            // Correct RGBA order: Red, Green, Blue, Alpha
            return ImGui.ColorConvertFloat4ToU32(new Vector4(
                color.R / 255.0f,
                color.G / 255.0f,
                color.B / 255.0f,
                color.A / 255.0f));
        }

        // HSV -> Color helper (for ESP RGB)
        private static Color ColorFromHSV(float hue, float saturation, float value)
        {
            int hi = System.Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            float f = hue / 60 - (float)Math.Floor(hue / 60);
            value *= 255;
            int v = System.Convert.ToInt32(value);
            int p = System.Convert.ToInt32(value * (1 - saturation));
            int q = System.Convert.ToInt32(value * (1 - f * saturation));
            int t = System.Convert.ToInt32(value * (1 - (1 - f) * saturation));

            return hi switch
            {
                0 => Color.FromArgb(255, v, t, p),
                1 => Color.FromArgb(255, q, v, p),
                2 => Color.FromArgb(255, p, v, t),
                3 => Color.FromArgb(255, p, q, v),
                4 => Color.FromArgb(255, t, p, v),
                _ => Color.FromArgb(255, v, p, q),
            };
        }

        private int lastWidth = 0, lastHeight = 0;
        // Offset between emulator render area (Core coords origin) and overlay window coords origin.
        // Used so ESP stays aligned even if overlay window is expanded beyond the emulator.
        private Vector2 _emulatorScreenOffset = Vector2.Zero;
        private Vector2 _emulatorClipMin = Vector2.Zero;
        private Vector2 _emulatorClipMax = Vector2.Zero;
        void CreateHandle()
        {
            if (Core.Handle == IntPtr.Zero)
                return;

            try
            {
                if (!WinAPI.IsWindow(Core.Handle))
                    return;
            }
            catch
            {
                return;
            }

            if (Config.StreamMode)
            {
                SetWindowDisplayAffinity(hWnd, WDA_EXCLUDEFROMCAPTURE);
            }
            else
            {
                SetWindowDisplayAffinity(hWnd, WDA_NONE);
            }

            IntPtr sizeHandle = Core.Handle;
            RECT emulatorRect;
            if (!GetWindowRect(sizeHandle, out emulatorRect))
            {
                if (BootstrapRuntime.MainWindowHandle != IntPtr.Zero)
                    sizeHandle = BootstrapRuntime.MainWindowHandle;
                if (!GetWindowRect(sizeHandle, out emulatorRect))
                    return;
            }

            int emulatorX = emulatorRect.Left;
            int emulatorY = emulatorRect.Top;
            int emulatorWidth = emulatorRect.Right - emulatorRect.Left;
            int emulatorHeight = emulatorRect.Bottom - emulatorRect.Top;
            if (emulatorWidth < 32 || emulatorHeight < 32)
            {
                if (BootstrapRuntime.MainWindowHandle != IntPtr.Zero &&
                    BootstrapRuntime.MainWindowHandle != sizeHandle &&
                    GetWindowRect(BootstrapRuntime.MainWindowHandle, out emulatorRect))
                {
                    sizeHandle = BootstrapRuntime.MainWindowHandle;
                    emulatorX = emulatorRect.Left;
                    emulatorY = emulatorRect.Top;
                    emulatorWidth = emulatorRect.Right - emulatorRect.Left;
                    emulatorHeight = emulatorRect.Bottom - emulatorRect.Top;
                }
            }

            if (emulatorWidth < 32 || emulatorHeight < 32)
                return;

            // Expand overlay to FULL virtual desktop so UI can be dragged out from every side.
            // ESP projection stays in emulator-size; drawing positions are shifted by _emulatorScreenOffset.
            int virtualLeft = int.MaxValue;
            int virtualTop = int.MaxValue;
            int virtualRight = int.MinValue;
            int virtualBottom = int.MinValue;
            foreach (var screen in System.Windows.Forms.Screen.AllScreens)
            {
                virtualLeft = Math.Min(virtualLeft, screen.Bounds.Left);
                virtualTop = Math.Min(virtualTop, screen.Bounds.Top);
                virtualRight = Math.Max(virtualRight, screen.Bounds.Right);
                virtualBottom = Math.Max(virtualBottom, screen.Bounds.Bottom);
            }

            int overlayWidth = virtualRight - virtualLeft;
            int overlayHeight = virtualBottom - virtualTop;

            if (overlayWidth != lastWidth || overlayHeight != lastHeight)
            {
                lastWidth = overlayWidth;
                lastHeight = overlayHeight;
                ImGui.SetWindowSize(new Vector2(overlayWidth, overlayHeight));
            }

            Size = new Size(overlayWidth, overlayHeight);
            Position = new Point(virtualLeft, virtualTop);

            // Core coords origin is emulator top-left, but overlay coords origin is virtual top-left.
            _emulatorScreenOffset = new Vector2(emulatorX - virtualLeft, emulatorY - virtualTop);
            _emulatorClipMin = _emulatorScreenOffset;
            _emulatorClipMax = _emulatorScreenOffset + new Vector2(emulatorWidth, emulatorHeight);

            Core.Width = emulatorWidth;
            Core.Height = emulatorHeight;
            Core.EspCachedViewportW = emulatorWidth;
            Core.EspCachedViewportH = emulatorHeight;
        }

        private void EnsureEmulatorRenderTarget()
        {
            if (Core.Handle != IntPtr.Zero || Offsets.Il2Cpp == 0)
                return;

            if (BootstrapRuntime.MainWindowHandle == IntPtr.Zero)
                BootstrapRuntime.MainWindowHandle = BootstrapRuntime.ResolveEmulatorMainWindow();

            if (BootstrapRuntime.MainWindowHandle == IntPtr.Zero)
                return;

            Core.Handle = BootstrapRuntime.FindRenderWindow(BootstrapRuntime.MainWindowHandle);
            if (Core.Handle == IntPtr.Zero)
                Core.Handle = BootstrapRuntime.MainWindowHandle;
        }

        private void PushEspClipRect(ImDrawListPtr drawList)
        {
            if (_emulatorClipMax.X > _emulatorClipMin.X + 8f && _emulatorClipMax.Y > _emulatorClipMin.Y + 8f)
            {
                drawList.PushClipRect(_emulatorClipMin, _emulatorClipMax, true);
                return;
            }

            Vector2 disp = ImGui.GetIO().DisplaySize;
            drawList.PushClipRect(Vector2.Zero, disp, true);
        }

        public void DrawFOVCircle(float radius)
        {
            var drawList = ImGui.GetBackgroundDrawList();
            var center = new Vector2(Core.Width / 2f, Core.Height / 2f) + _emulatorScreenOffset;
            uint color = ColorToUint32(Config.FOVColor);

            drawList.AddCircle(center, radius, color, 0, 1f);
        }


        Vector4 darkBg = new Vector4(18 / 255f, 18 / 255f, 22 / 255f, 1.0f);

        // Premium accent — red family (matches theme default)
        Vector4 accent1 = new Vector4(1f, 0.38f, 0.42f, 1.0f);
        Vector4 accent2 = new Vector4(0.85f, 0.2f, 0.24f, 1.0f);
        Vector4 accent3 = new Vector4(0.38f, 0.1f, 0.12f, 1.0f);

        private void ApplyCustomStyle()
        {
            var style = ImGui.GetStyle();
            var colors = style.Colors;

            Vector4 darkBg = new Vector4(18 / 255f, 18 / 255f, 22 / 255f, 1.0f);
            Vector4 accent1 = new Vector4(1f, 0.38f, 0.42f, 1.0f);
            Vector4 accent2 = new Vector4(0.85f, 0.2f, 0.24f, 1.0f);
            Vector4 accent3 = new Vector4(0.38f, 0.1f, 0.12f, 1.0f);



            colors[(int)ImGuiCol.WindowBg] = darkBg;
            colors[(int)ImGuiCol.ChildBg] = new Vector4(0.12f, 0.12f, 0.15f, 0.95f);
            colors[(int)ImGuiCol.Border] = new Vector4(0.25f, 0.25f, 0.3f, 0.5f);
            colors[(int)ImGuiCol.BorderShadow] = new Vector4(0, 0, 0, 0.5f);

            colors[(int)ImGuiCol.Text] = new Vector4(0.98f, 0.98f, 0.98f, 1.0f);
            colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.5f, 0.5f, 0.55f, 1.0f);

            colors[(int)ImGuiCol.Button] = new Vector4(0.22f, 0.22f, 0.26f, 0.8f);
            colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.28f, 0.28f, 0.32f, 1f);
            colors[(int)ImGuiCol.ButtonActive] = accent2;

            Vector4 sliderNeutral = new Vector4(0.52f, 0.54f, 0.58f, 1f);
            colors[(int)ImGuiCol.SliderGrab] = sliderNeutral;
            colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.62f, 0.64f, 0.68f, 1f);

            colors[(int)ImGuiCol.CheckMark] = accent1;
            colors[(int)ImGuiCol.FrameBg] = new Vector4(0.15f, 0.15f, 0.18f, 0.6f);
            colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.2f, 0.2f, 0.24f, 0.7f);
            colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.25f, 0.25f, 0.3f, 0.8f);

            colors[(int)ImGuiCol.Header] = accent3;
            colors[(int)ImGuiCol.HeaderHovered] = accent2;
            colors[(int)ImGuiCol.HeaderActive] = accent1;

            colors[(int)ImGuiCol.Separator] = new Vector4(42f / 255f, 42f / 255f, 42f / 255f, 0.9f);

            colors[(int)ImGuiCol.PopupBg] = new Vector4(0.12f, 0.12f, 0.15f, 0.98f);
            colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0f, 0f, 0f, 0f);
            colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0f, 0f, 0f, 0f);
            colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0f, 0f, 0f, 0f);
            colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0f, 0f, 0f, 0f);
            style.ScrollbarSize = 0f;

            // Crisp radii — compact outer/inner hierarchy
            style.WindowRounding = 4.0f;
            style.FrameRounding = 2.0f;
            style.GrabRounding = 2.0f;
            style.ScrollbarRounding = 2.0f;
            style.TabRounding = 2.0f;
            style.ChildRounding = 3.0f;
            style.PopupRounding = 3.0f;

            // Compact spacing (reduce gaps between functions)
            style.WindowPadding = new Vector2(8, 8);
            style.FramePadding = new Vector2(6, 4);
            style.ItemSpacing = new Vector2(2, 1);
            style.ItemInnerSpacing = new Vector2(3, 1);
            style.WindowTitleAlign = new Vector2(0.0f, 0.5f);
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
        }
        private Vector4 Lerp(Vector4 a, Vector4 b, float t)
        {
            return new Vector4(
                a.X + (b.X - a.X) * t,
                a.Y + (b.Y - a.Y) * t,
                a.Z + (b.Z - a.Z) * t,
                a.W + (b.W - a.W) * t
            );
        }

        private void DrawBottomCenterTextSafe()
        {
            if (_uiAlpha <= 0.01f)
                return;

            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(ImGui.GetIO().DisplaySize);

            ImGui.Begin("##bottom_text_overlay_cs",
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoInputs |
                ImGuiWindowFlags.NoBackground |
                ImGuiWindowFlags.NoBringToFrontOnFocus);

            var draw = ImGui.GetWindowDrawList();
            float time = (float)ImGui.GetTime();
            float displayW = ImGui.GetIO().DisplaySize.X;
            float displayH = ImGui.GetIO().DisplaySize.Y;

            if (LebelFont.IsLoaded())
                ImGui.PushFont(LebelFont);

            string whiteWord = BrandTitle;
            string redWord = BrandNamePurple;
            Vector2 whiteSz = ImGui.CalcTextSize(whiteWord);
            Vector2 redSz = ImGui.CalcTextSize(redWord);
            float totalW = whiteSz.X + 6f + redSz.X;
            float baseY = displayH - whiteSz.Y - 80f;

            float speed = 50f;
            float offsetX = (time * speed) % (displayW + totalW);
            float startX = displayW - offsetX;

            Vector4 whiteCol = new Vector4(1f, 1f, 1f, 0.90f * _uiAlpha);
            Vector4 redCol = new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, 0.90f * _uiAlpha);
            uint shadowCol = ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.65f * _uiAlpha));

            Vector2[] shadowOffsets = {
                new Vector2(-1,0), new Vector2(1,0), new Vector2(0,-1), new Vector2(0,1),
                new Vector2(-1,-1), new Vector2(1,-1), new Vector2(-1,1), new Vector2(1,1)
            };

            // Draw two copies for seamless looping
            for (int c = 0; c < 2; c++)
            {
                float x = startX - c * (displayW + totalW);
                float whiteX = x;
                float redX = x + whiteSz.X + 6f;

                // Zigzag peak & valley — sine wave based on X position
                float zigZagOffset = MathF.Sin((x + time * 40f) * 0.02f) * 12f;

                // Shadow for white word
                foreach (var so in shadowOffsets)
                    draw.AddText(new Vector2(whiteX + so.X, baseY + zigZagOffset + so.Y), shadowCol, whiteWord);
                // Shadow for red word
                foreach (var so in shadowOffsets)
                    draw.AddText(new Vector2(redX + so.X, baseY + zigZagOffset + so.Y), shadowCol, redWord);
                // White word
                draw.AddText(new Vector2(whiteX, baseY + zigZagOffset), ImGui.GetColorU32(whiteCol), whiteWord);
                // Red accent word
                draw.AddText(new Vector2(redX, baseY + zigZagOffset), ImGui.GetColorU32(redCol), redWord);
            }

            if (LebelFont.IsLoaded())
                ImGui.PopFont();

            ImGui.End();
        }

        private void ApplyThemeOverlay()
        {
            var style = ImGui.GetStyle();
            style.WindowRounding = 12f;
            style.FrameRounding = 6f;
            style.ChildRounding = 8f;
            style.PopupRounding = 6f;
            style.ScrollbarRounding = 0f;
            style.GrabRounding = 4f;
            style.TabRounding = 6f;
            style.WindowBorderSize = 0f;
            style.ChildBorderSize = 0f;
            style.PopupBorderSize = 0f;
            style.FrameBorderSize = 0f;
            var c = style.Colors;
            Vector4 bg = new Vector4(0.04f, 0.04f, 0.06f, _uiAlpha);
            c[(int)ImGuiCol.WindowBg] = bg;
            c[(int)ImGuiCol.FrameBg] = new Vector4(0.06f, 0.06f, 0.10f, _uiAlpha * 0.85f);
            c[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.10f, 0.10f, 0.14f, _uiAlpha * 0.90f);
            c[(int)ImGuiCol.FrameBgActive] = new Vector4(0.14f, 0.14f, 0.18f, _uiAlpha);
            c[(int)ImGuiCol.Button] = new Vector4(0.06f, 0.06f, 0.10f, _uiAlpha * 0.80f);
            c[(int)ImGuiCol.ButtonHovered] = new Vector4(0.12f, 0.12f, 0.16f, _uiAlpha * 0.85f);
            c[(int)ImGuiCol.ButtonActive] = new Vector4(0.18f, 0.18f, 0.22f, _uiAlpha);
            c[(int)ImGuiCol.PopupBg] = new Vector4(0.06f, 0.06f, 0.10f, 0.96f * _uiAlpha);
            c[(int)ImGuiCol.Header] = new Vector4(0.08f, 0.08f, 0.12f, _uiAlpha * 0.70f);
            c[(int)ImGuiCol.HeaderHovered] = new Vector4(0.12f, 0.12f, 0.16f, _uiAlpha * 0.80f);
            c[(int)ImGuiCol.HeaderActive] = new Vector4(0.16f, 0.16f, 0.20f, _uiAlpha);
            c[(int)ImGuiCol.Text] = new Vector4(1f, 1f, 1f, _uiAlpha);
            c[(int)ImGuiCol.TextDisabled] = new Vector4(0.50f, 0.50f, 0.55f, _uiAlpha * 0.70f);
            c[(int)ImGuiCol.Separator] = new Vector4(0.12f, 0.12f, 0.16f, _uiAlpha * 0.60f);
            c[(int)ImGuiCol.Border] = new Vector4(0.86f, 0.08f, 0.08f, 0.15f * _uiAlpha);
            c[(int)ImGuiCol.ScrollbarBg] = new Vector4(0f, 0f, 0f, 0f);
            c[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0f, 0f, 0f, 0f);
            c[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0f, 0f, 0f, 0f);
            c[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0f, 0f, 0f, 0f);
            style.ScrollbarSize = 0f;
            style.ItemSpacing = new Vector2(4f, 3f);
            style.WindowPadding = new Vector2(0f, 0f);
        }

        private void DrawAnimatedBrandTitle(ImDrawListPtr draw, Vector2 textOrigin, float lineH)
        {
            bool hasTitleFont = HeaderFont.IsLoaded();
            if (hasTitleFont) ImGui.PushFont(HeaderFont);

            Vector2 whiteSz = ImGui.CalcTextSize(BrandNameWhite);
            Vector4 whiteCol = new Vector4(1f, 1f, 1f, _uiAlpha);
            draw.AddText(textOrigin, ImGui.GetColorU32(whiteCol), BrandNameWhite);

            Vector2 cheatSz = ImGui.CalcTextSize(BrandNamePurple);
            Vector4 pRgb = new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, _uiAlpha);
            draw.AddText(new Vector2(textOrigin.X + whiteSz.X + 4f, textOrigin.Y), ImGui.GetColorU32(pRgb), BrandNamePurple);

            if (hasTitleFont) ImGui.PopFont();
        }

        private static Vector2 ClampMenuWindowPos(Vector2 pos, Vector2 windowSize)
        {
            Vector2 disp = ImGui.GetIO().DisplaySize;
            float maxX = MathF.Max(0f, disp.X - windowSize.X);
            float maxY = MathF.Max(0f, disp.Y - windowSize.Y);
            return new Vector2(
                Math.Clamp(pos.X, 0f, maxX),
                Math.Clamp(pos.Y, 0f, maxY));
        }

        private static void ApplyTitleBarDrag()
        {
            if (!ImGui.IsItemActive() || !ImGui.IsMouseDown(ImGuiMouseButton.Left))
                return;

            Vector2 delta = ImGui.GetIO().MouseDelta;
            if (delta.X == 0f && delta.Y == 0f)
                return;

            Vector2 newPos = ClampMenuWindowPos(
                ImGui.GetWindowPos() + delta,
                ImGui.GetWindowSize());
            ImGui.SetWindowPos(newPos, ImGuiCond.Always);
        }

        private void OnStreamModeChanged(bool enabled)
        {
            Config.StreamModeStamp++;
            if (hWnd == IntPtr.Zero)
                return;

            SetWindowDisplayAffinity(hWnd, enabled ? WDA_EXCLUDEFROMCAPTURE : WDA_NONE);
        }

        /// <summary>Re-submit title hitbox last so drag wins over overlapping widgets in the same window.</summary>
        private void SubmitTitleBarDragHitbox(Vector2 wsize)
        {
            float chromeH = UiMetrics.ChromeH;
            float innerW = wsize.X - UiMetrics.OuterPad * 2f;
            const float closeBtnSize = 20f;
            const float closeReserve = closeBtnSize + 12f;
            float dragW = MathF.Max(40f, innerW - closeReserve);

            ImGui.SetCursorPos(new Vector2(UiMetrics.OuterPad, UiMetrics.OuterPad));
            ImGui.InvisibleButton("##drag_region_top", new Vector2(dragW, chromeH));
            ApplyTitleBarDrag();
        }

        private void DrawWindowChrome(Vector2 wpos, Vector2 wsize)
        {
            var draw = ImGui.GetWindowDrawList();
            float chromeH = UiMetrics.ChromeH;
            bool isLogin = _flowPhase == FlowPhase.Login;

            float headerH = isLogin ? chromeH : 36f;
            // Dark glass header
            draw.AddRectFilled(wpos, wpos + new Vector2(wsize.X, headerH),
                ImGui.GetColorU32(new Vector4(0.04f, 0.04f, 0.07f, 0.88f * _uiAlpha)), 12f, ImDrawFlags.RoundCornersTop);

            if (isLogin)
            {
                if (HeaderFont.IsLoaded()) ImGui.PushFont(HeaderFont);
                Vector2 tSz = ImGui.CalcTextSize(BrandTitle);
                Vector2 tSz2 = ImGui.CalcTextSize(BrandTitleAccent);
                float totalW = tSz.X + tSz2.X;
                float tX = wpos.X + (wsize.X - totalW) * 0.5f;
                float tY = wpos.Y + (headerH - tSz.Y) * 0.5f;
                draw.AddText(new Vector2(tX, tY), ImGui.GetColorU32(new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, _uiAlpha)), BrandTitle);
                draw.AddText(new Vector2(tX + tSz.X, tY), ImGui.GetColorU32(new Vector4(1f, 1f, 1f, _uiAlpha)), BrandTitleAccent);
                if (HeaderFont.IsLoaded()) ImGui.PopFont();
            }
            else
            {
                float btnW = 16f, btnH = 16f;
                Vector2 closePos = wpos + new Vector2(wsize.X - btnW - 6f, (headerH - btnH) * 0.5f);
                Vector2 minPos = closePos - new Vector2(btnW + 4f, 0f);

                // Minimize button
                ImGui.SetCursorScreenPos(minPos);
                ImGui.InvisibleButton("##hdr_min", new Vector2(btnW, btnH));
                if (ImGui.IsItemHovered()) draw.AddRectFilled(minPos, minPos + new Vector2(btnW, btnH), ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.10f * _uiAlpha)), 3f);
                if (ImGui.IsItemClicked()) _uiVisible = false;
                draw.AddLine(minPos + new Vector2(4f, btnH * 0.55f), minPos + new Vector2(btnW - 4f, btnH * 0.55f),
                    ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.50f * _uiAlpha)), 1.2f);

                // Close button
                ImGui.SetCursorScreenPos(closePos);
                ImGui.InvisibleButton("##hdr_cls", new Vector2(btnW, btnH));
                if (ImGui.IsItemHovered()) draw.AddRectFilled(closePos, closePos + new Vector2(btnW, btnH), ImGui.GetColorU32(new Vector4(0.86f, 0.08f, 0.08f, 0.40f * _uiAlpha)), 3f);
                if (ImGui.IsItemClicked()) Environment.Exit(0);
                draw.AddLine(closePos + new Vector2(4f, 4f), closePos + new Vector2(btnW - 4f, btnH - 4f),
                    ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.50f * _uiAlpha)), 1.2f);
                draw.AddLine(closePos + new Vector2(btnW - 4f, 4f), closePos + new Vector2(4f, btnH - 4f),
                    ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.50f * _uiAlpha)), 1.2f);

                // Brand title
                if (HeaderFont.IsLoaded()) ImGui.PushFont(HeaderFont);
                Vector2 bSz = ImGui.CalcTextSize(BrandTitle);
                Vector2 bSz2 = ImGui.CalcTextSize(BrandTitleAccent);
                draw.AddText(wpos + new Vector2(8f, (headerH - bSz.Y) * 0.5f),
                    ImGui.GetColorU32(new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, _uiAlpha)), BrandTitle);
                draw.AddText(wpos + new Vector2(8f + bSz.X, (headerH - bSz2.Y) * 0.5f),
                    ImGui.GetColorU32(new Vector4(1f, 1f, 1f, _uiAlpha * 0.92f)), BrandTitleAccent);
                if (HeaderFont.IsLoaded()) ImGui.PopFont();

                // Centered tabs with pill style
                float tabY = wpos.Y + headerH;
                float tabSpacing = 50f;
                int tabCount = 5;
                string[] tabIcons = { Lucide.Skull, Lucide.Eye, Lucide.Crosshair, Lucide.Flame, Lucide.Settings };
                float totalTabW = (tabCount - 1) * tabSpacing;
                float startX = wpos.X + (wsize.X - totalTabW) * 0.5f;
                float pillW = 32f, pillH = 26f;

                for (int i = 0; i < tabCount; i++)
                {
                    float tX = startX + i * tabSpacing;
                    bool tActive = (_activeTab == i);

                    if (TabIconFont.IsLoaded()) ImGui.PushFont(TabIconFont);
                    Vector2 iconSz = ImGui.CalcTextSize(tabIcons[i]);

                    // Pill background for active tab
                    Vector2 pillMin = new Vector2(tX - pillW * 0.5f, tabY + 2f);
                    Vector2 pillMax = new Vector2(tX + pillW * 0.5f, tabY + pillH);

                    if (tActive)
                    {
                        draw.AddRectFilled(pillMin, pillMax,
                            ImGui.GetColorU32(new Vector4(0.86f, 0.08f, 0.08f, 0.20f * _uiAlpha)), 13f);
                        draw.AddRect(pillMin, pillMax,
                            ImGui.GetColorU32(new Vector4(0.86f, 0.08f, 0.08f, 0.40f * _uiAlpha)), 13f, ImDrawFlags.None, 1f);
                    }

                    // Hover zone
                    ImGui.SetCursorScreenPos(pillMin);
                    ImGui.InvisibleButton($"##hdr_tab_{i}", new Vector2(pillW, pillH));
                    if (ImGui.IsItemHovered() && !tActive)
                        draw.AddRectFilled(pillMin, pillMax,
                            ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.06f * _uiAlpha)), 13f);
                    if (ImGui.IsItemClicked())
                        _activeTab = i;

                    Vector4 tColor = tActive
                        ? new Vector4(0.86f, 0.08f, 0.08f, _uiAlpha)
                        : new Vector4(0.50f, 0.50f, 0.55f, _uiAlpha * 0.65f);

                    draw.AddText(new Vector2(tX - iconSz.X * 0.5f, tabY + (pillH - iconSz.Y) * 0.5f + 2f),
                        ImGui.GetColorU32(tColor), tabIcons[i]);
                    if (TabIconFont.IsLoaded()) ImGui.PopFont();
                }

                // Subtle separator below tabs
                float sepY = tabY + pillH + 4f;
                draw.AddRectFilled(new Vector2(wpos.X + 8f, sepY), new Vector2(wpos.X + wsize.X - 8f, sepY + 0.5f),
                    ImGui.GetColorU32(new Vector4(0.86f, 0.08f, 0.08f, 0.12f * _uiAlpha)), 0f);
            }

            ImGui.SetCursorPos(new Vector2(20f, 0f));
            ImGui.InvisibleButton("##drag_region", new Vector2(wsize.X - 40f, headerH));
            ApplyTitleBarDrag();
        }

        private void DrawConnectingStatusBar(Vector2 wpos, Vector2 wsize)
        {
            if (_uiAlpha <= 0.01f)
                return;

            var draw = ImGui.GetWindowDrawList();
            const float barH = 24f;
            Vector2 barMin = wpos + new Vector2(0f + 8f, UiMetrics.ChromeH + 6f);
            Vector2 barMax = barMin + new Vector2(MathF.Max(120f, wsize.X - 0f - 16f), barH);

            draw.AddRectFilled(barMin, barMax,
                ImGui.GetColorU32(new Vector4(0.08f, 0.04f, 0.05f, 0.88f * _uiAlpha)), 5f);
            draw.AddRect(barMin, barMax,
                ImGui.GetColorU32(new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, 0.35f * _uiAlpha)),
                5f, ImDrawFlags.None, 1f);

            float pulse = 0.5f + 0.5f * MathF.Sin((float)ImGui.GetTime() * 4f);
            draw.AddCircleFilled(barMin + new Vector2(12f, barH * 0.5f), 4f,
                ImGui.GetColorU32(new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, pulse * _uiAlpha)), 12);

            string msg = "Connecting to emulator...";
            if (TextFont.IsLoaded()) ImGui.PushFont(TextFont);
            draw.AddText(barMin + new Vector2(24f, (barH - ImGui.CalcTextSize(msg).Y) * 0.5f),
                ImGui.GetColorU32(new Vector4(1f, 0.92f, 0.92f, 0.95f * _uiAlpha)), msg);
            if (TextFont.IsLoaded()) ImGui.PopFont();
        }

        private static void DrawHeader()
        {
            float headerHeight = 34f;
            float extendDown = 0f;

            ImGui.BeginChild("##header", new Vector2(0, headerHeight + extendDown));
            var draw = ImGui.GetWindowDrawList();

            Vector2 pos = ImGui.GetWindowPos();
            Vector2 size = ImGui.GetWindowSize();

            Vector2 bgMin = pos;
            Vector2 bgMax = pos + new Vector2(size.X, headerHeight);

            // ---- GLASS HEADER BACKGROUND ----
            uint bgTop = ImGui.GetColorU32(new Vector4(0.04f, 0.04f, 0.08f, 0.50f * _uiAlpha));
            uint bgBot = ImGui.GetColorU32(new Vector4(0.06f, 0.06f, 0.12f, 0.70f * _uiAlpha));
            draw.AddRectFilledMultiColor(bgMin, bgMax, bgTop, bgTop, bgBot, bgBot);

            // Bottom border with neon cyan glow
            Vector2 lineStart = new Vector2(bgMin.X, bgMax.Y);
            Vector2 lineEnd = bgMax;
            uint lineCol = ImGui.GetColorU32(new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, 0.75f * _uiAlpha));
            draw.AddLine(lineStart, lineEnd, lineCol, 1.2f);

            // Subtle soft glow under the separator line
            for (int i = 1; i <= 6; i++)
            {
                float falloff = MathF.Exp(-i * i * 0.4f);
                uint glowCol = ImGui.GetColorU32(new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, 0.18f * falloff * _uiAlpha));
                draw.AddLine(lineStart + new Vector2(0, i * 0.5f), lineEnd + new Vector2(0, i * 0.5f), glowCol, 1.0f);
            }

            bool hasHeaderFont = HeaderFont.IsLoaded();

            if (hasHeaderFont) ImGui.PushFont(HeaderFont);

            string hexWord = BrandNameWhite;
            string corpWord = BrandNamePurple;

            Vector2 hexSz = ImGui.CalcTextSize(hexWord);
            Vector2 corpSz = ImGui.CalcTextSize(corpWord);

            float textY = pos.Y + (headerHeight - hexSz.Y) * 0.5f;
            float leftPad = 14f;
            float gap = 5f;

            Vector2 hexPos = new(pos.X + leftPad, textY);
            Vector2 corpPos = new(hexPos.X + hexSz.X + gap, textY);

            // Soft, dynamic professional glow behind brand text
            {
                float time = (float)ImGui.GetTime();
                float pulse = 0.5f + 0.5f * MathF.Sin(time * 2.0f);
                int layers = 12;
                float maxSpread = 18f;
                // Center glow ONLY behind HEX
                Vector2 hexGlowCenter = new Vector2(hexPos.X + hexSz.X * 0.5f, hexPos.Y + hexSz.Y * 0.5f);
                Vector2 hexGlowHalfSz = new Vector2(hexSz.X * 0.5f + 2f, hexSz.Y * 0.5f + 2f);
                // Center glow ONLY behind CORPORATION
                Vector2 corpGlowCenter = new Vector2(corpPos.X + corpSz.X * 0.5f, corpPos.Y + corpSz.Y * 0.5f);
                Vector2 corpGlowHalfSz = new Vector2(corpSz.X * 0.5f + 2f, corpSz.Y * 0.5f + 2f);

                for (int i = 0; i < layers; i++)
                {
                    float t = i / (float)layers;
                    float falloff = MathF.Exp(-t * t * 4.5f);
                    float spread = t * 24f; // softer spread
                    float alphaHex = falloff * (0.015f + 0.01f * pulse) * _uiAlpha;
                    float alphaCorp = falloff * (0.008f + 0.004f * pulse) * _uiAlpha; // Subtle white glow

                    // HEX (RED GLOW)
                    draw.AddRectFilled(
                        hexGlowCenter - hexGlowHalfSz - new Vector2(spread, spread),
                        hexGlowCenter + hexGlowHalfSz + new Vector2(spread, spread),
                        ImGui.GetColorU32(new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, alphaHex)),
                        8f + spread);

                    // CORPORATION (WHITE GLOW)
                    draw.AddRectFilled(
                        corpGlowCenter - corpGlowHalfSz - new Vector2(spread, spread),
                        corpGlowCenter + corpGlowHalfSz + new Vector2(spread, spread),
                        ImGui.GetColorU32(new Vector4(1f, 1f, 1f, alphaCorp)),
                        8f + spread);
                }
            }

            // HEX (red = ThemeColor)
            ImGui.SetCursorScreenPos(hexPos);
            ImGui.PushStyleColor(ImGuiCol.Text,
                new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, _uiAlpha));
            ImGui.TextUnformatted(hexWord);
            ImGui.PopStyleColor();

            // CORPORATION (white)
            ImGui.SetCursorScreenPos(corpPos);
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 1f, 1f, _uiAlpha));
            ImGui.TextUnformatted(corpWord);
            ImGui.PopStyleColor();

            if (hasHeaderFont) ImGui.PopFont();

            // ---- CLOSE BUTTON  ("X" text style) ----
            float closeHitW = 22f;
            float closeHitH = 22f;
            Vector2 closeHitPos = new(
                bgMax.X - closeHitW - 10f,
                pos.Y + (headerHeight - closeHitH) * 0.5f
            );

            ImGui.SetCursorScreenPos(closeHitPos);
            ImGui.InvisibleButton("##close_btn", new Vector2(closeHitW, closeHitH));
            bool closeHovered = ImGui.IsItemHovered();
            bool closeClicked = ImGui.IsItemClicked();

            if (closeClicked)
                _uiVisible = false;

            // Draw a clean X using two lines (matches reference screenshot style)
            float xPad = 5f;
            float xThick = 1.5f;
            Vector2 x0 = closeHitPos + new Vector2(xPad, xPad);
            Vector2 x1 = closeHitPos + new Vector2(closeHitW - xPad, closeHitH - xPad);
            Vector2 x2 = closeHitPos + new Vector2(closeHitW - xPad, xPad);
            Vector2 x3 = closeHitPos + new Vector2(xPad, closeHitH - xPad);

            Vector4 xColor = closeHovered
                ? new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, 0.95f * _uiAlpha)
                : new Vector4(1f, 1f, 1f, 0.60f * _uiAlpha);
            uint xCol = ImGui.GetColorU32(xColor);

            draw.AddLine(x0, x1, xCol, xThick);
            draw.AddLine(x2, x3, xCol, xThick);

            ImGui.EndChild();
        }

        // DrawHorizontalHeaderAndTabs removed — replaced by DrawWindowChrome

        private void FillPanelBackground(float rounding, ImDrawFlags roundFlags = ImDrawFlags.None)
        {
            var draw = ImGui.GetWindowDrawList();
            Vector2 p = ImGui.GetWindowPos();
            Vector2 s = ImGui.GetWindowSize();
            draw.AddRectFilled(p, p + s,
                ImGui.GetColorU32(new Vector4(CardBodyBg.X, CardBodyBg.Y, CardBodyBg.Z, UiMetrics.PanelGlassAlpha * _uiAlpha)),
                rounding,
                roundFlags);
            DrawUiTriangleParticles(draw, p, s);
        }

        private void EnsureUiTriangleParticles(Vector2 area)
        {
            if (_uiTriangleParticles != null &&
                MathF.Abs(_uiTriangleParticleArea.X - area.X) < 1f &&
                MathF.Abs(_uiTriangleParticleArea.Y - area.Y) < 1f)
                return;

            _uiTriangleParticleArea = area;
            _uiTriangleParticles = new UiTriangleParticle[56];
            for (int i = 0; i < _uiTriangleParticles.Length; i++)
            {
                _uiTriangleParticles[i] = new UiTriangleParticle
                {
                    Pos = new Vector2(
                        (float)_uiParticleRng.NextDouble() * area.X,
                        (float)_uiParticleRng.NextDouble() * area.Y),
                    Vel = new Vector2(
                        -18f - (float)_uiParticleRng.NextDouble() * 28f,
                        22f + (float)_uiParticleRng.NextDouble() * 38f),
                    Rot = (float)_uiParticleRng.NextDouble() * MathF.PI * 2f,
                    RotSpeed = ((float)_uiParticleRng.NextDouble() - 0.5f) * 1.8f,
                    Size = 5f + (float)_uiParticleRng.NextDouble() * 9f,
                    Alpha = 0.12f + (float)_uiParticleRng.NextDouble() * 0.22f,
                };
            }
        }

        private void DrawUiTriangleParticles(ImDrawListPtr draw, Vector2 origin, Vector2 size)
        {
            EnsureUiTriangleParticles(size);
            if (_uiTriangleParticles == null) return;

            float dt = ImGui.GetIO().DeltaTime;

            for (int i = 0; i < _uiTriangleParticles.Length; i++)
            {
                ref var p = ref _uiTriangleParticles[i];

                // Update position
                p.Pos.X += p.Vel.X * dt;
                p.Pos.Y += p.Vel.Y * dt;

                // Wrap around screen boundaries
                if (p.Pos.X < 0) p.Pos.X = size.X;
                if (p.Pos.X > size.X) p.Pos.X = 0;
                if (p.Pos.Y < 0) p.Pos.Y = size.Y;
                if (p.Pos.Y > size.Y) p.Pos.Y = 0;

                // Draw circles for particles
                Vector2 drawPos = origin + p.Pos;
                draw.AddCircleFilled(drawPos, p.Size * 0.25f, ImGui.GetColorU32(new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, p.Alpha * _uiAlpha)));
                
                // Removed Plexus effect to match image
            }
        }

        private static Vector2 Rotate2D(Vector2 v, float angle)
        {
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);
            return new Vector2(v.X * cos - v.Y * sin, v.X * sin + v.Y * cos);
        }

        private static void PushPanelContentPadding()
        {
            ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(UiMetrics.PanelPad, UiMetrics.PanelPad));
        }

        private void DrawPanelBackground(float rounding)
        {
            FillPanelBackground(rounding);
            PushPanelContentPadding();
        }

        private void DrawLoginGateMessage()
        {
            float a = Math.Clamp(_uiAlpha, 0f, 1f);
            Vector2 avail = ImGui.GetContentRegionAvail();
            var draw = ImGui.GetWindowDrawList();
            Vector2 winPos = ImGui.GetWindowPos();

            // -- Centered in available area --
            float lockIconSize = 28f;
            float textGap = 14f;

            // Use FA lock glyph if available, else fallback to a text symbol
            string lockGlyph = "\uF023"; // FA lock
            bool hasFa = TextFont.IsLoaded() && _faMergedIntoTextFont;

            // Measure text
            if (hasFa) ImGui.PushFont(TextFont);
            Vector2 lockSz = ImGui.CalcTextSize(lockGlyph);
            if (hasFa) ImGui.PopFont();

            if (LebelFont.IsLoaded()) ImGui.PushFont(LebelFont);
            Vector2 mainSz = ImGui.CalcTextSize("Login first to use features");
            if (LebelFont.IsLoaded()) ImGui.PopFont();

            if (TextFont.IsLoaded()) ImGui.PushFont(TextFont);
            Vector2 subSz = ImGui.CalcTextSize("Select the account tab to login");
            if (TextFont.IsLoaded()) ImGui.PopFont();

            float totalH = lockIconSize + textGap + mainSz.Y + 6f + subSz.Y;
            float startY = (avail.Y - totalH) * 0.5f;
            float centerX = avail.X * 0.5f;

            Vector2 cp = ImGui.GetCursorPos();

            // Soft glow behind lock icon
            Vector2 glowCenter = winPos + cp + new Vector2(centerX, startY + lockIconSize * 0.5f);
            for (int layer = 5; layer >= 1; layer--)
            {
                float spread = layer * 5f;
                float alpha = (0.04f / layer) * a;
                draw.AddCircleFilled(glowCenter,
                    lockIconSize * 0.6f + spread,
                    ImGui.GetColorU32(new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, alpha)));
            }

            // Lock icon
            float iconX = centerX - (hasFa ? lockSz.X : lockIconSize) * 0.5f;
            float iconY = startY + (lockIconSize - (hasFa ? lockSz.Y : lockIconSize)) * 0.5f;
            ImGui.SetCursorPos(new Vector2(cp.X + iconX, cp.Y + iconY));
            if (hasFa)
            {
                ImGui.PushFont(TextFont);
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, 0.9f * a));
                ImGui.TextUnformatted(lockGlyph);
                ImGui.PopStyleColor();
                ImGui.PopFont();
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, 0.9f * a));
                ImGui.TextUnformatted("[ ]");
                ImGui.PopStyleColor();
            }

            // Main text
            float mainY = startY + lockIconSize + textGap;
            ImGui.SetCursorPos(new Vector2(cp.X + centerX - mainSz.X * 0.5f, cp.Y + mainY));
            if (LebelFont.IsLoaded()) ImGui.PushFont(LebelFont);
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 1f, 1f, 0.92f * a));
            ImGui.TextUnformatted("Login first to use features");
            ImGui.PopStyleColor();
            if (LebelFont.IsLoaded()) ImGui.PopFont();

            float subY = mainY + mainSz.Y + 6f;
            ImGui.SetCursorPos(new Vector2(cp.X + centerX - subSz.X * 0.5f, cp.Y + subY));
            if (TextFont.IsLoaded()) ImGui.PushFont(TextFont);
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.55f, 0.55f, 0.58f, 0.88f * a));
            ImGui.TextUnformatted("Select the account tab to login");
            ImGui.PopStyleColor();
            if (TextFont.IsLoaded()) ImGui.PopFont();
        }

        // Dead sidebar methods removed (moved to top-tab layout in DrawWindowChrome)

        private void DrawContentFade(Action drawAction)
        {
            float fade = _tabFade.ContainsKey(_activeTab) ? _tabFade[_activeTab] : 1f;
            if (fade <= 0f) return;

            float combinedAlpha = fade * _uiAlpha;
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 1f, 1f, combinedAlpha));
            ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(1f, 1f, 1f, combinedAlpha));
            // Subtle dark divider under tab titles (~#2A2A2A), matches reference
            ImGui.PushStyleColor(ImGuiCol.Separator, new Vector4(42f / 255f, 42f / 255f, 42f / 255f, combinedAlpha));
            drawAction.Invoke();
            ImGui.PopStyleColor(3);
        }

        private void DrawMainContent()
        {
            if (_flowPhase == FlowPhase.Login)
            {
                float a = Math.Clamp(_uiAlpha, 0f, 1f);
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 1f, 1f, a));
                DrawEmbeddedLoginPanel();
                ImGui.PopStyleColor();
                return;
            }
            if (_flowPhase != FlowPhase.Main)
                return;

            DrawContentFade(() =>
            {
                if (TextFont.IsLoaded()) ImGui.PushFont(TextFont);
                switch (_activeTab)
                {
                    case 0:
                        RenderCombatAimTab();
                        break;
                    case 1:
                        RenderVisualsTab();
                        break;
                    case 2:
                        RenderMovementTab();
                        break;
                    case 3:
                        RenderKillTab();
                        break;
                    case 4:
                        RenderSettingsTab();
                        break;
                    default:
                        RenderCombatAimTab();
                        break;
                }
                if (TextFont.IsLoaded()) ImGui.PopFont();
            });
        }

        // === Combat / movement / kill UI sections (moved from CombatFeatureUi.cs into main ESP.cs) ===

        private void BeginFeatureScrollArea(string scrollId = "main")
        {
            float h = ImGui.GetContentRegionAvail().Y;
            ImGui.PushStyleColor(ImGuiCol.ScrollbarBg, new Vector4(0f, 0f, 0f, 0f));
            ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab, new Vector4(0f, 0f, 0f, 0f));
            ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered, new Vector4(0f, 0f, 0f, 0f));
            ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabActive, new Vector4(0f, 0f, 0f, 0f));
            ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize, 0f);
            ImGui.BeginChild($"##feature_scroll_{scrollId}", new Vector2(0f, h), ImGuiChildFlags.None,
                ImGuiWindowFlags.NoBackground);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(3f, 3f));
        }

        private void EndFeatureScrollArea()
        {
            ImGui.PopStyleVar();
            ImGui.EndChild();
            ImGui.PopStyleVar();
            ImGui.PopStyleColor(4);
        }

        private bool FeatureCheckbox(string label, ref bool value, Action<bool>? onChanged = null)
        {
            bool pressed = CustomCheckbox.CaxCustomCheckbox(label, ref value);
            if (pressed && onChanged != null)
                onChanged(value);
            return pressed;
        }

        private bool FeatureCheckboxStackRow(float anchorX, float rowWidth, string label, ref bool value, Action<bool>? onChanged = null, string? bindId = null)
        {
            const float bindW = 74f;
            const float bindH = 22f;
            const float bindGap = 6f;

            ImGui.SetCursorPosX(anchorX);
            Vector2 rowStart = ImGui.GetCursorScreenPos();
            float labelClipW = string.IsNullOrEmpty(bindId) ? rowWidth : rowWidth - bindW - bindGap;

            ImGui.PushClipRect(rowStart, rowStart + new Vector2(labelClipW, 28f), true);
            bool result = FeatureCheckbox(label, ref value, onChanged);
            ImGui.PopClipRect();

            if (!string.IsNullOrEmpty(bindId))
            {
                FeatureKeybinds.EnsureInitialized();
                Vector2 rowEnd = ImGui.GetItemRectMax();
                float bindY = rowStart.Y + MathF.Max(0f, (rowEnd.Y - rowStart.Y - bindH) * 0.5f);
                ImGui.SetCursorScreenPos(new Vector2(rowStart.X + rowWidth - bindW, bindY));
                AnimatedKeybind.Draw(bindId, new Vector2(bindW, bindH));
            }

            Vector2 screenP = ImGui.GetCursorScreenPos();
            ImGui.GetWindowDrawList().AddLine(
                screenP + new Vector2(0f, 2f),
                screenP + new Vector2(rowWidth, 2f),
                ImGui.GetColorU32(new Vector4(24f / 255f, 25f / 255f, 31f / 255f, 0.40f * _uiAlpha)),
                1f);
            ImGui.Dummy(new Vector2(rowWidth, 8f));

            return result;
        }

        private bool FeatureCheckboxKeybindRow(float rowX, float rowWidth, string label, ref bool value, Action<bool>? onChanged, string bindId)
        {
            return FeatureCheckboxStackRow(rowX, rowWidth, label, ref value, onChanged, bindId);
        }

        private void DrawAimbotHoldKeyRow(float rowWidth)
        {
            const float bindW = 74f;
            const float bindH = 22f;
            float rowX = ImGui.GetCursorPosX();

            FeatureKeybinds.EnsureInitialized();
            if (FeatureKeybinds.GetKey("AimbotKey") == Keys.None && Config.AimbotKey != Keys.None)
                FeatureKeybinds.SetKey("AimbotKey", Config.AimbotKey);

            ImGui.SetCursorPosX(rowX);
            Vector2 rowStart = ImGui.GetCursorScreenPos();
            if (TextFont.IsLoaded()) ImGui.PushFont(TextFont);
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted("Aimbot Hold Key");
            if (TextFont.IsLoaded()) ImGui.PopFont();

            Vector2 rowEnd = ImGui.GetItemRectMax();
            float bindY = rowStart.Y + MathF.Max(0f, (rowEnd.Y - rowStart.Y - bindH) * 0.5f);
            ImGui.SetCursorScreenPos(new Vector2(rowStart.X + rowWidth - bindW, bindY));
            AnimatedKeybind.Draw("AimbotKey", new Vector2(bindW, bindH));
            Config.AimbotKey = FeatureKeybinds.GetKey("AimbotKey");
            if (Config.AimbotKey == Keys.None)
                Config.AimbotKey = Keys.LButton;

            Vector2 screenP = ImGui.GetCursorScreenPos();
            ImGui.GetWindowDrawList().AddLine(
                screenP + new Vector2(0f, 2f),
                screenP + new Vector2(rowWidth, 2f),
                ImGui.GetColorU32(new Vector4(24f / 255f, 25f / 255f, 31f / 255f, 0.40f * _uiAlpha)),
                1f);
            ImGui.Dummy(new Vector2(rowWidth, 8f));
        }

        private static bool DrawKeyBind(string id, ref System.Windows.Forms.Keys key, Vector2 size)
        {
            string label = key.ToString();
            if (label == "LButton") label = "LMouse";
            else if (label == "RButton") label = "RMouse";
            else if (label == "MButton") label = "MMouse";
            else if (label == "XButton1") label = "Mouse4";
            else if (label == "XButton2") label = "Mouse5";

            bool isBinding = _activeBindingId == id;
            if (isBinding)
            {
                label = "...";
                ImGuiIOPtr io = ImGui.GetIO();
                for (int i = 1; i < 256; i++)
                {
                    if (ImGui.IsKeyDown((ImGuiKey)i))
                    {
                        key = (System.Windows.Forms.Keys)i;
                        _activeBindingId = null;
                        break;
                    }
                }
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left)) { key = System.Windows.Forms.Keys.LButton; _activeBindingId = null; }
                else if (ImGui.IsMouseClicked(ImGuiMouseButton.Right)) { key = System.Windows.Forms.Keys.RButton; _activeBindingId = null; }
                else if (ImGui.IsMouseClicked(ImGuiMouseButton.Middle)) { key = System.Windows.Forms.Keys.MButton; _activeBindingId = null; }
            }

            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(12f / 255f, 13f / 255f, 18f / 255f, _uiAlpha));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(20f / 255f, 21f / 255f, 28f / 255f, _uiAlpha));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(232f / 255f, 20f / 255f, 26f / 255f, _uiAlpha));
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 3f);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1f);
            ImGui.PushStyleColor(ImGuiCol.Border, isBinding ? new Vector4(232f / 255f, 20f / 255f, 26f / 255f, _uiAlpha) : new Vector4(30f / 255f, 32f / 255f, 40f / 255f, _uiAlpha));

            Vector2 pos = ImGui.GetCursorScreenPos();
            string text = isBinding ? "..." : FormatKeyLabel(key);
            ImGui.SetCursorScreenPos(pos);
            ImGui.InvisibleButton(text + "##btn_" + id, size);
            bool pressed = ImGui.IsItemClicked();
            if (pressed)
                _activeBindingId = isBinding ? null : id;

            var btnDraw = ImGui.GetWindowDrawList();
            Vector2 btnEnd = pos + size;
            btnDraw.AddRectFilled(pos, btnEnd, ImGui.GetColorU32(new Vector4(12f / 255f, 13f / 255f, 18f / 255f, _uiAlpha)), 3f);
            btnDraw.AddRect(pos, btnEnd,
                ImGui.GetColorU32(isBinding
                    ? new Vector4(232f / 255f, 20f / 255f, 26f / 255f, _uiAlpha)
                    : new Vector4(30f / 255f, 32f / 255f, 40f / 255f, _uiAlpha)),
                3f, ImDrawFlags.None, 1f);

            if (LebelFont.IsLoaded()) ImGui.PushFont(LebelFont);
            else if (TextFont.IsLoaded()) ImGui.PushFont(TextFont);
            Vector2 labelSz = ImGui.CalcTextSize(text);
            btnDraw.AddText(
                pos + new Vector2(8f, (size.Y - labelSz.Y) * 0.5f),
                ImGui.GetColorU32(new Vector4(0.78f, 0.78f, 0.82f, _uiAlpha)),
                text);
            if (LebelFont.IsLoaded() || TextFont.IsLoaded()) ImGui.PopFont();

            UiKeyboardIcon.Draw(btnDraw, pos + new Vector2(size.X - 14f, size.Y * 0.5f), 9f, ThemeColor, _uiAlpha);

            ImGui.PopStyleColor(4);
            ImGui.PopStyleVar(2);
            return isBinding;
        }

        private static bool FeatureKeyBindStackRow(float anchorX, float rowWidth, string label, ref System.Windows.Forms.Keys key)
        {
            ImGui.SetCursorPosX(anchorX);
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(label);
            ImGui.SameLine(0, 0);

            float btnW = 68f;
            float btnH = 20f;
            ImGui.SetCursorPosX(anchorX + rowWidth - btnW);

            bool result = DrawKeyBind(label, ref key, new Vector2(btnW, btnH));

            Vector2 screenP = ImGui.GetCursorScreenPos();
            ImGui.GetWindowDrawList().AddLine(screenP + new Vector2(0f, 2f), screenP + new Vector2(rowWidth, 2f), ImGui.GetColorU32(new Vector4(24f / 255f, 25f / 255f, 31f / 255f, 0.40f * _uiAlpha)), 1f);
            ImGui.Dummy(new Vector2(rowWidth, 8f));

            return result;
        }

        private void ApplySpinBotState(bool enabled)
        {
            if (enabled)
                RapidSpin.Activate();
            else
                RapidSpin.Deactivate();
        }

        private void ApplyFlyInvisible(bool enabled)
        {
            Config.FlyHack50x = enabled;
            Config.FlyHack50xx = enabled;
        }

        private void ApplyFlyX80Int(bool enabled)
        {
            Config.FLYHACKINTTTT = enabled;
            Config.FLYHACKINTTT = enabled;
        }

        private void OnPatchToggle(Func<Task> patchTask) => _ = patchTask();

        // === Manik editz UI Helpers ===

        /// <summary>Draws a section header row like "⚡ Aimbot" in the Manik editz style.</summary>
        private void DrawHexSectionHeader(string faIcon, string title)
        {
            var draw = ImGui.GetWindowDrawList();
            Vector2 pos = ImGui.GetCursorScreenPos();
            float availW = ImGui.GetContentRegionAvail().X;
            float headerH = 22f;

            // Background
            draw.AddRectFilled(pos, pos + new Vector2(availW, headerH),
                ImGui.GetColorU32(new Vector4(16f / 255f, 16f / 255f, 20f / 255f, _uiAlpha)), 3f);
            // Left accent bar
            draw.AddRectFilled(pos, pos + new Vector2(3f, headerH),
                ImGui.GetColorU32(new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, 0.9f * _uiAlpha)), 0f);

            float textY = pos.Y + (headerH - 13f) * 0.5f;
            float cx = pos.X + 10f;

            // Icon
            bool hasFa = TextFont.IsLoaded() && _faMergedIntoTextFont;
            if (hasFa)
            {
                ImGui.PushFont(TextFont);
                Vector2 iSz = ImGui.CalcTextSize(faIcon);
                draw.AddText(new Vector2(cx, pos.Y + (headerH - iSz.Y) * 0.5f),
                    ImGui.GetColorU32(new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, _uiAlpha)), faIcon);
                cx += iSz.X + 6f;
                ImGui.PopFont();
            }

            // Title text
            if (LebelFont.IsLoaded()) ImGui.PushFont(LebelFont);
            Vector2 tSz = ImGui.CalcTextSize(title);
            draw.AddText(new Vector2(cx, pos.Y + (headerH - tSz.Y) * 0.5f),
                ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.96f * _uiAlpha)), title);
            if (LebelFont.IsLoaded()) ImGui.PopFont();

            ImGui.SetCursorScreenPos(pos + new Vector2(0f, headerH + 2f));
            ImGui.Dummy(new Vector2(availW, 0f));
        }

        /// <summary>Draws a HEX-style inline slider with red fill and value label, matching the image.</summary>
        private bool DrawHexSlider(string label, ref float value, float min, float max, float width)
        {
            var draw = ImGui.GetWindowDrawList();

            const float height = 18f;
            const float rounding = 3f;

            // Capture screen position BEFORE dummy
            Vector2 pos = ImGui.GetCursorScreenPos();

            // Reserve space first so layout advances properly
            ImGui.Dummy(new Vector2(width, height));
            Vector2 sliderMin = pos;
            Vector2 sliderMax = sliderMin + new Vector2(width, height);

            // Hit-test using the item rect from the Dummy
            bool hovered = ImGui.IsItemHovered();
            bool active = ImGui.IsItemActive();

            // Register as an invisible button for proper mouse interaction
            ImGui.SetCursorScreenPos(pos);
            ImGui.InvisibleButton($"##hexslider_{label}", new Vector2(width, height));
            bool changed = false;
            if (ImGui.IsItemActive())
            {
                float newVal = min + Math.Clamp((ImGui.GetMousePos().X - sliderMin.X) / width, 0f, 1f) * (max - min);
                if (MathF.Abs(newVal - value) > 0.00001f) { value = newVal; changed = true; }
            }

            // Recalculate after potential value update
            float t = (max > min) ? Math.Clamp((value - min) / (max - min), 0f, 1f) : 0f;
            float fillW = width * t;

            // Track bg
            draw.AddRectFilled(sliderMin, sliderMax,
                ImGui.GetColorU32(new Vector4(20f / 255f, 20f / 255f, 24f / 255f, _uiAlpha)), rounding);
            draw.AddRect(sliderMin, sliderMax,
                ImGui.GetColorU32(new Vector4(50f / 255f, 50f / 255f, 58f / 255f, _uiAlpha)), rounding, ImDrawFlags.None, 1f);

            // Red fill
            if (fillW > 1f)
            {
                Vector2 fillMax = sliderMin + new Vector2(fillW, height);
                draw.AddRectFilled(sliderMin, fillMax,
                    ImGui.GetColorU32(new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, 0.95f * _uiAlpha)), rounding);
                // Shine highlight
                if (fillW > 6f)
                {
                    draw.AddRectFilled(
                        sliderMin + new Vector2(2f, 1f),
                        new Vector2(sliderMin.X + fillW - 2f, sliderMin.Y + height * 0.38f),
                        ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.12f * _uiAlpha)), rounding);
                }
            }

            // Format value: always show 2 decimal places like "0.00" / "90.00"
            string valStr = value.ToString("F2");
            if (LebelFont.IsLoaded()) ImGui.PushFont(LebelFont);
            Vector2 labelSz = ImGui.CalcTextSize(label);
            Vector2 valSz = ImGui.CalcTextSize(valStr);
            // Label text (left, inside slider)
            draw.AddText(
                new Vector2(sliderMin.X + 7f, sliderMin.Y + (height - labelSz.Y) * 0.5f),
                ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.92f * _uiAlpha)), label);
            // Value text (right, inside slider)
            draw.AddText(
                new Vector2(sliderMax.X - valSz.X - 6f, sliderMin.Y + (height - valSz.Y) * 0.5f),
                ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.88f * _uiAlpha)), valStr);
            if (LebelFont.IsLoaded()) ImGui.PopFont();

            // Small gap after slider
            ImGui.Dummy(new Vector2(width, 2f));
            
            return changed;
        }

        private static readonly Dictionary<string, float> _toggleRowAnim = new();

        private bool FeatureCheckboxToggleRow(float anchorX, float rowWidth, string label, ref bool value, Action<bool>? onChanged = null, string? bindId = null)
        {
            const float bindW = 68f;
            const float bindH = 20f;
            const float rowH = 24f;
            const float bottomPad = 6f;

            if (bindId == null)
                bindId = label.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", "");
            else if (bindId == "NONE")
                bindId = null;

            ImGui.SetCursorPosX(anchorX);
            Vector2 rowStart = ImGui.GetCursorScreenPos();

            bool pressed = CustomCheckbox.CaxCustomCheckbox(label, ref value, null, rowWidth);

            if (!string.IsNullOrEmpty(bindId))
            {
                FeatureKeybinds.EnsureInitialized();
                float keybindX = rowStart.X + rowWidth - bindW;
                float bindY = rowStart.Y + MathF.Max(0f, (rowH - bindH) * 0.5f);
                ImGui.SetCursorScreenPos(new Vector2(keybindX, bindY));
                AnimatedKeybind.Draw(bindId, new Vector2(bindW, bindH));
            }

            ImGui.SetCursorScreenPos(new Vector2(rowStart.X, rowStart.Y + rowH));

            if (pressed && onChanged != null)
                onChanged(value);

            Vector2 screenP = ImGui.GetCursorScreenPos();
            ImGui.GetWindowDrawList().AddLine(
                screenP + new Vector2(0f, 0f),
                screenP + new Vector2(rowWidth, 0f),
                ImGui.GetColorU32(new Vector4(24f / 255f, 25f / 255f, 31f / 255f, 0.35f * _uiAlpha)),
                1f);
            ImGui.Dummy(new Vector2(rowWidth, bottomPad));

            return pressed;
        }

        private bool FeatureCheckboxToggleRowWithColor(float anchorX, float rowWidth, string label, ref bool value, ref Vector4 color, Action<bool>? onChanged = null, string? bindId = null)
        {
            const float bindW = 74f;
            const float bindH = 22f;
            const float colorBtnSize = 18f;
            const float checkW = 17f;
            const float checkPadding = 16f;

            if (bindId == null)
            {
                bindId = label.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", "");
            }
            else if (bindId == "NONE")
            {
                bindId = null;
            }

            ImGui.SetCursorPosX(anchorX);
            Vector2 rowStart = ImGui.GetCursorScreenPos();
            
            bool pressed = CustomCheckbox.CaxCustomCheckbox(label, ref value, null, rowWidth);

            // Draw color button
            // Changed layout slightly to align correctly, ensuring space for bind
            float colorX = rowStart.X + rowWidth - colorBtnSize;
            if (bindId != null) colorX -= (bindW + checkW + checkPadding);
            
            float colorY = rowStart.Y + MathF.Max(0f, (24f - colorBtnSize) * 0.5f);
            ImGui.SetCursorScreenPos(new Vector2(colorX, colorY));
            
            if (ImGui.ColorButton($"##{label}_color_btn", color, ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoAlpha, new Vector2(colorBtnSize, colorBtnSize)))
            {
                ImGui.OpenPopup($"##{label}_picker_popup");
            }
            
            if (ImGui.BeginPopup($"##{label}_picker_popup"))
            {
                Vector2 pickerPos = ImGui.GetCursorScreenPos();
                Vector2 pickerSize = new Vector2(200, 20);
                
                var drawList = ImGui.GetWindowDrawList();
                const int steps = 360;
                float stepWidth = pickerSize.X / steps;
                for (int i = 0; i < steps; i++)
                {
                    float t = i / (float)steps;
                    float h = t * 360f;
                    float s = 1f;
                    float v = 1f;
                    int hi = (int)(h / 60f) % 6;
                    float f = (h / 60f) - hi;
                    float p_val = v * (1f - s);
                    float q_val = v * (1f - f * s);
                    float t_val = v * (1f - (1f - f) * s);
                    Vector4 c = hi switch
                    {
                        0 => new Vector4(v, t_val, p_val, 1f),
                        1 => new Vector4(q_val, v, p_val, 1f),
                        2 => new Vector4(p_val, v, t_val, 1f),
                        3 => new Vector4(p_val, q_val, v, 1f),
                        4 => new Vector4(t_val, p_val, v, 1f),
                        _ => new Vector4(v, p_val, q_val, 1f),
                    };
                    drawList.AddRectFilled(pickerPos + new Vector2(i * stepWidth, 0),
                                           pickerPos + new Vector2((i + 1) * stepWidth, pickerSize.Y),
                                           ImGui.ColorConvertFloat4ToU32(c), 0f);
                }
                
                ImGui.SetCursorScreenPos(pickerPos);
                ImGui.InvisibleButton("##picker_interaction", pickerSize);
                
                if (ImGui.IsItemActive() && ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    Vector2 mouse = ImGui.GetMousePos();
                    float relativeX = Math.Clamp((mouse.X - pickerPos.X) / pickerSize.X, 0f, 1f);
                    float h = relativeX * 360f;
                    float s = 1f;
                    float v = 1f;
                    int hi = (int)(h / 60f) % 6;
                    float f = (h / 60f) - hi;
                    float p_val = v * (1f - s);
                    float q_val = v * (1f - f * s);
                    float t_val = v * (1f - (1f - f) * s);
                    color = hi switch
                    {
                        0 => new Vector4(v, t_val, p_val, 1f),
                        1 => new Vector4(q_val, v, p_val, 1f),
                        2 => new Vector4(p_val, v, t_val, 1f),
                        3 => new Vector4(p_val, q_val, v, 1f),
                        4 => new Vector4(t_val, p_val, v, 1f),
                        _ => new Vector4(v, p_val, q_val, 1f),
                    };
                }
                
                ImGui.EndPopup();
            }

            if (!string.IsNullOrEmpty(bindId))
            {
                FeatureKeybinds.EnsureInitialized();
                float keybindX = rowStart.X + rowWidth - checkW - checkPadding - bindW;
                float bindY = rowStart.Y + MathF.Max(0f, (28f - bindH) * 0.5f);
                
                ImGui.SetCursorScreenPos(new Vector2(keybindX, bindY));
                AnimatedKeybind.Draw(bindId, new Vector2(bindW, bindH));
            }

            ImGui.SetCursorScreenPos(new Vector2(rowStart.X, rowStart.Y + 28f));

            if (pressed && onChanged != null)
                onChanged(value);

            Vector2 screenP = ImGui.GetCursorScreenPos();
            ImGui.GetWindowDrawList().AddLine(
                screenP + new Vector2(0f, 2f),
                screenP + new Vector2(rowWidth, 2f),
                ImGui.GetColorU32(new Vector4(24f / 255f, 25f / 255f, 31f / 255f, 0.40f * _uiAlpha)),
                1f);
            ImGui.Dummy(new Vector2(rowWidth, 8f));

            return pressed;
        }

        // === END HEX UI Helpers ===

        private void RenderCombatAimTab()
        {
            float avail = ImGui.GetContentRegionAvail().X;
            float colW = MathF.Max(60f, avail);
            float innerW = MathF.Max(50f, colW - 16f);

            BeginFeatureScrollArea("aim_tab");

            DrawCard("", new Vector2(colW, 0f), () =>
            {
                float rowX = ImGui.GetCursorPosX();
                if (FeatureCheckboxToggleRow(rowX, innerW, "Aimbot", ref enableAimBot, null, "enableAimBot"))
                    BootstrapRuntime.StartFeatureBg("aimbot", AimbotV2.Work, () => Config.enableAimBot);
                if (FeatureCheckboxToggleRow(rowX, innerW, "Aimbot Visible", ref AimbotVisible, null, "AimbotVisible"))
                    BootstrapRuntime.StartFeatureBg("aimvisible", AimbotAi.Work, () => Config.AimbotVisible);
                FeatureCheckboxToggleRow(rowX, innerW, "Aimbot Rage", ref AimBotRage, null, "AimBotRage");
                
                // Silent Aim (Safe) maps to socket feature BackendSafeSilentAim (mode 890)
                if (FeatureCheckboxToggleRow(rowX, innerW, "Silent Aim (Safe)", ref Config.BackendSafeSilentAim, null, "SafeSilentAim"))
                {
                    ToggleSocketFeature(890, Config.BackendSafeSilentAim);
                }

                // Silent Cover maps to socket feature BackendSilentAimCover (mode 889)
                if (FeatureCheckboxToggleRow(rowX, innerW, "Silent Cover", ref kcbrutasilnetaim, null, "kcbrutasilnetaim"))
                {
                    ToggleSocketFeature(889, Config.BackendSilentAimCover);
                    BootstrapRuntime.StartFeatureBg("brutalsilent", Client.KCBRUTALSILENT.Work, () => Config.kcbrutasilnetaim);
                }

                // Silent Aim Max (Brutal) maps to C# brutal memory hacks
                if (FeatureCheckboxToggleRow(rowX, innerW, "Silent Aim Max (Brutal)", ref kcbrutasilnetaim, null, "kcbrutasilnetaim"))
                {
                    kcbrutasilnetaimBODY = kcbrutasilnetaim; // sync body aim
                    BootstrapRuntime.StartFeatureBg("brutalsilent", Client.KCBRUTALSILENT.Work, () => Config.kcbrutasilnetaim);
                }

                if (FeatureCheckboxToggleRow(rowX, innerW, "Enemy Pull Raycast", ref Config.EnemyPullRaycast, null, "EnemyPullRaycast"))
                    BootstrapRuntime.StartFeatureBg("pullraycast", Client.EnemyPullRaycast.Work, () => Config.EnemyPullRaycast);

                FeatureCheckboxToggleRow(rowX, innerW, "Silent kill", ref SilentKillEnabled, null, "SilentKillEnabled");
                FeatureCheckboxToggleRow(rowX, innerW, "Ignore Knocked", ref IgnoreKnocked, null, "IgnoreKnocked");
                FeatureCheckboxToggleRow(rowX, innerW, "No Recoil", ref NoRecoil, null, "NoRecoil");

                // Draw FOV Circle with color picker next to it
                Vector4 fovColorVec = new Vector4(Config.FOVColor.R / 255f, Config.FOVColor.G / 255f, Config.FOVColor.B / 255f, Config.FOVColor.A / 255f);
                if (FeatureCheckboxToggleRowWithColor(rowX, innerW, "Draw FOV Circle", ref Config.FOVEnabled, ref fovColorVec, null, "FOVEnabled"))
                {
                    Config.FOVColor = System.Drawing.Color.FromArgb((int)(fovColorVec.W * 255), (int)(fovColorVec.X * 255), (int)(fovColorVec.Y * 255), (int)(fovColorVec.Z * 255));
                }

                ImGui.Dummy(new Vector2(0f, 6f));

                float fovF = AimFov;
                if (DrawPremiumSlider("Field of View", ref fovF, 0f, 1000f, innerW))
                    AimFov = fovF;

                float distF = Config.AimBotMaxDistance;
                if (DrawPremiumSlider("Aim Max Distance", ref distF, 0f, 500f, innerW))
                    Config.AimBotMaxDistance = (int)distF;

                float smoothnessF = Config.AimBotSmooth;
                if (DrawPremiumSlider("Aimbot Smoothness", ref smoothnessF, 0f, 100f, innerW))
                    Config.AimBotSmooth = smoothnessF;
            });

            EndFeatureScrollArea();
        }

        private void RenderVisualsTab()
        {
            float avail = ImGui.GetContentRegionAvail().X;
            float colW = MathF.Max(60f, avail);
            float innerW = MathF.Max(50f, colW - 16f);

            BeginFeatureScrollArea("esp_tab");

            DrawCard("", new Vector2(colW, 0f), () =>
            {
                float rowX = ImGui.GetCursorPosX();
                FeatureCheckboxToggleRow(rowX, innerW, "Esp Editor", ref Config.ESPEditor);
                FeatureCheckboxToggleRow(rowX, innerW, "Detect Hacker", ref Config.HackerTag);
                FeatureCheckboxToggleRow(rowX, innerW, "Esp RGB", ref Config.ESPRGB);
                FeatureCheckboxToggleRow(rowX, innerW, "Count Valid Match", ref Config.EspTimer);
                FeatureCheckboxToggleRow(rowX, innerW, "Aim Track Line", ref Config.AimTrackLine);
                FeatureCheckboxToggleRow(rowX, innerW, "Stream Proof", ref Config.StreamMode);
                FeatureCheckboxToggleRow(rowX, innerW, "Visuals Enabled", ref VisualsEnabled);
                FeatureCheckboxToggleRow(rowX, innerW, "FOV Enabled", ref FOVEnabled);
                FeatureCheckboxToggleRow(rowX, innerW, "ESP Information", ref ESPInformation);
                FeatureCheckboxToggleRow(rowX, innerW, "Hacker Detect", ref HackerDetect);
                FeatureCheckboxToggleRow(rowX, innerW, "Spoof Name", ref SpoofNameEnabled);
                FeatureCheckboxToggleRow(rowX, innerW, "ESP Screen Smooth", ref EspScreenSmooth);
            });
            
            ImGui.Dummy(new Vector2(0f, 8f));
            
            DrawCard("ESP Draw Settings", new Vector2(colW, 0f), () =>
            {
                float rowX2 = ImGui.GetCursorPosX();
                
                // 1. Esp Line
                FeatureCheckboxToggleRow(rowX2, innerW, "Esp Line", ref Config.ESPLine);
                RenderSubComboLocation(rowX2, innerW, "EspLine", "Line Position", ref Config.ESPLinePosition, new[] { "Top", "Center", "Bottom" });

                // 2. Esp Box
                FeatureCheckboxToggleRow(rowX2, innerW, "Esp Box", ref Config.ESPBox, null, "NONE");
                RenderSubComboLocation(rowX2, innerW, "EspBox", "Box Style", ref Config.EspBoxGlow, new[] { "Normal", "Glow" });

                // 3. Esp Name
                FeatureCheckboxToggleRow(rowX2, innerW, "Esp Name", ref Config.ESPName, null, "NONE");
                RenderSubComboLocation(rowX2, innerW, "EspName", "Name Position", ref Config.EspNameSide, new[] { "Left", "Right", "Top", "Bottom" });

                // 4. Esp Health Bar
                FeatureCheckboxToggleRow(rowX2, innerW, "Esp Health Bar", ref Config.ESPHealth, null, "NONE");
                RenderSubComboLocation(rowX2, innerW, "EspHealth", "Health Position", ref Config.EspHealthBarSide, new[] { "Left", "Right", "Top", "Bottom" });

                // 5. Esp Health Text
                FeatureCheckboxToggleRow(rowX2, innerW, "Esp Health Text", ref Config.ESPHealthText, null, "NONE");

                // 6. Esp Skeleton
                FeatureCheckboxToggleRow(rowX2, innerW, "Esp Skeleton", ref Config.ESPSkeleton, null, "NONE");

                // 7. Esp Distance
                FeatureCheckboxToggleRow(rowX2, innerW, "Esp Distance", ref Config.espdistance, null, "NONE");
                RenderSubComboLocation(rowX2, innerW, "EspDistance", "Distance Position", ref Config.EspDistanceSide, new[] { "Left", "Right", "Top", "Bottom" });

                // 8. Esp Weapon
                FeatureCheckboxToggleRow(rowX2, innerW, "Esp Weapon", ref Config.espweapon, null, "NONE");
                RenderSubComboLocation(rowX2, innerW, "EspWeapon", "Weapon Position", ref Config.EspWeaponSide, new[] { "Left", "Right", "Top", "Bottom" });
            });

            ImGui.EndChild(); // end feature scroll
        }

        private static readonly Dictionary<string, float> _subRowAnimations = new Dictionary<string, float>();

        private void RenderSubComboLocation(float anchorX, float rowWidth, string key, string label, ref LinePosition value, string[] items)
        {
            int idx = (int)value;
            RenderSubComboLocationRaw(anchorX, rowWidth, key, label, ref idx, items);
            value = (LinePosition)idx;
        }

        private void RenderSubComboLocation(float anchorX, float rowWidth, string key, string label, ref bool value, string[] items)
        {
            int idx = value ? 1 : 0;
            RenderSubComboLocationRaw(anchorX, rowWidth, key, label, ref idx, items);
            value = (idx == 1);
        }

        private void RenderSubComboLocation(float anchorX, float rowWidth, string key, string label, ref int value, string[] items)
        {
            RenderSubComboLocationRaw(anchorX, rowWidth, key, label, ref value, items);
        }

        private void RenderSubComboLocationRaw(float anchorX, float rowWidth, string key, string label, ref int selectedIndex, string[] items)
        {
            if (!_subRowAnimations.TryGetValue(key, out float anim))
                anim = 0f;

            bool active = false;
            if (key == "EspLine") active = Config.ESPLine;
            else if (key == "EspBox") active = Config.ESPBox;
            else if (key == "EspName") active = Config.ESPName;
            else if (key == "EspWeapon") active = Config.espweapon;
            else if (key == "EspDistance") active = Config.espdistance;
            else if (key == "EspHealth") active = Config.ESPHealth;

            float target = active ? 1f : 0f;
            float dt = ImGui.GetIO().DeltaTime;
            anim = anim + (target - anim) * (15f * dt);
            anim = Math.Clamp(anim, 0f, 1f);
            _subRowAnimations[key] = anim;

            if (anim <= 0.01f)
                return;

            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, anim * _uiAlpha);
            ImGui.SetCursorPosX(anchorX + 16f); // Indent slightly

            ImGui.BeginGroup();

            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(new Vector4(0.6f, 0.6f, 0.65f, 1f), label);
            ImGui.SameLine();

            float comboW = rowWidth - 160f;
            ImGui.SetNextItemWidth(comboW);
            ImGui.SetCursorPosX(anchorX + rowWidth - comboW - 16f);

            string currentItem = items[Math.Clamp(selectedIndex, 0, items.Length - 1)];
            if (ImGui.BeginCombo("##" + key, currentItem, ImGuiComboFlags.None))
            {
                for (int i = 0; i < items.Length; i++)
                {
                    bool isSelected = (i == selectedIndex);
                    if (ImGui.Selectable(items[i], isSelected))
                    {
                        selectedIndex = i;
                    }
                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }
                ImGui.EndCombo();
            }

            ImGui.EndGroup();
            ImGui.PopStyleVar();

            // Space below
            ImGui.Dummy(new Vector2(rowWidth, 4f * anim));
        }

        private void RenderCombatMovementTab()
        {
            float avail = ImGui.GetContentRegionAvail().X;
            float colW = MathF.Max(60f, (avail - 8f) * 0.5f);
            float innerW = MathF.Max(50f, colW - 16f);

            ImGui.PushStyleColor(ImGuiCol.ScrollbarBg, new Vector4(0f, 0f, 0f, 0f));
            ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab, new Vector4(0f, 0f, 0f, 0f));
            ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered, new Vector4(0f, 0f, 0f, 0f));
            ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabActive, new Vector4(0f, 0f, 0f, 0f));
            ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize, 0f);

            float bodyH = ImGui.GetContentRegionAvail().Y;

            // ---- LEFT COLUMN: Movement ----
            ImGui.BeginChild("##misc_left_col", new Vector2(colW, bodyH), ImGuiChildFlags.None,
                ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoScrollbar);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(3f, 2f));

            DrawHexSectionHeader("\uF544", "Movement");

            float rowX = ImGui.GetCursorPosX();
            if (FeatureCheckboxToggleRow(rowX, innerW, "Flight Patch", ref Config.flyhack, null, "flyhack"))
                OnPatchToggle(() => flyahackk());
            FeatureCheckboxToggleRow(rowX, innerW, "Fly X80 Altitude", ref FLYHACKINTTTT, ApplyFlyX80Int, "FLYHACKINTTTT");
            FeatureCheckboxToggleRow(rowX, innerW, "Fly X80 Matrix", ref FlyHack80xx, null, "FlyHack80xx");
            FeatureCheckboxToggleRow(rowX, innerW, "Fly X40", ref Config.FLYHACKX400, v =>
            {
                Config.FLYHACKX400 = v;
                Config.FLYHACKX40 = v;
            }, "FLYHACKX400");
            FeatureCheckboxToggleRow(rowX, innerW, "Invisible Fly", ref Config.FlyHack50x, ApplyFlyInvisible, "FlyHack50x");
            FeatureCheckboxToggleRow(rowX, innerW, "Zero-Gravity Fly", ref FlyHack, KCNOGRAVITYFLY.SetState, "FlyHack");
            FeatureCheckboxToggleRow(rowX, innerW, "Fly Rage", ref Config.FlyRage, FlyRage.SetState, "FlyRage");
            FeatureCheckboxToggleRow(rowX, innerW, "Climb Assist", ref ClimbUpEnabled, null, "ClimbUpEnabled");
            FeatureCheckboxToggleRow(rowX, innerW, "Climb Up V2", ref ClimbUpV2Enabled, null, "ClimbUpV2Enabled");
            ImGui.Dummy(new Vector2(0f, 4f));
            DrawHexSlider("Climb Height", ref ClimbUpV2Value, 0f, 80f, innerW);
            if (FeatureCheckboxToggleRow(rowX, innerW, "Speed Boost", ref speedint, null, "speedint")) { }
            if (FeatureCheckboxToggleRow(rowX, innerW, "High Jump", ref Config.highjumppp, null, "highjumppp"))
                OnPatchToggle(() => highjump());

            ImGui.PopStyleVar();
            ImGui.EndChild();

            ImGui.SameLine(0f, 8f);

            // ---- RIGHT COLUMN: Kill Functions ----
            ImGui.BeginChild("##misc_right_col", new Vector2(colW, bodyH), ImGuiChildFlags.None,
                ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoScrollbar);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(3f, 2f));

            DrawHexSectionHeader("\uF1E2", "Kill Functions");

            float rowX2 = ImGui.GetCursorPosX();
            FeatureCheckboxToggleRow(rowX2, innerW, "Teleport Kill", ref tele, null, "tele");
            FeatureCheckboxToggleRow(rowX2, innerW, "Shake Kill", ref ShakeKill, null, "ShakeKill");
            FeatureCheckboxToggleRow(rowX2, innerW, "Down Player", ref Config.DownPlayer, null, "DownPlayer");
            FeatureCheckboxToggleRow(rowX2, innerW, "Underground Kill", ref undergroundkill, null, "undergroundkill");
            FeatureCheckboxToggleRow(rowX2, innerW, "Spawn Kill", ref Kcspawnkill, null, "Kcspawnkill");
            FeatureCheckboxToggleRow(rowX2, innerW, "Quick Teleport", ref teliport, null, "teliport");
            FeatureCheckboxToggleRow(rowX2, innerW, "AI Teleport Wall", ref teliportwall, null, "teliportwall");
            FeatureCheckboxToggleRow(rowX2, innerW, "Mark Map Teleport", ref teleportmap, null, "teleportmap");
            if (FeatureCheckboxToggleRow(rowX2, innerW, "Wallhack (AoB)", ref Config.wallhack, null, "wallhack"))
                OnPatchToggle(() => wallhackrisk());
            FeatureCheckboxToggleRow(rowX2, innerW, "Spin Bot V2", ref spinbot, ApplySpinBotState, "spinbot");

            ImGui.PopStyleVar();
            ImGui.EndChild();

            ImGui.PopStyleVar();
            ImGui.PopStyleColor(4);
        }

        /// <summary>Re-applies AoB patches and thread toggles after config load or hook success.</summary>
        internal void ApplyLoadedFeatureRuntimeState()
        {
            Config.FlyHack50xx = Config.FlyHack50x;
            Config.FLYHACKINTTT = Config.FLYHACKINTTTT;
            Config.FLYHACKX40 = Config.FLYHACKX400;

            ApplySpinBotState(Config.spinbot);
            KCNOGRAVITYFLY.SetState(Config.FlyHack);
            FlyRage.SetState(Config.FlyRage);

            _ = flyahackk();
            _ = brustfire();
            _ = highjump();
            _ = cameraleftt();
            _ = visionhack();
            _ = wallhackrisk();

            if (Config.speedext)
                _ = loadspeedext();
            else if (speedResult != null)
                speedonext();

            if (Config.SpoofNameEnabled)
                SpoofName.ApplyNow();
        }

        // Legacy names kept for any external references
        private void RenderAimTab() => RenderCombatAimTab();
        private void RenderMiscTab() => RenderCombatMovementTab();

        private void RenderMovementTab()
        {
            float avail = ImGui.GetContentRegionAvail().X;
            float colW = MathF.Max(60f, avail);
            float innerW = MathF.Max(50f, colW - 16f);

            BeginFeatureScrollArea("movement_tab");

            DrawCard("", new Vector2(colW, 0f), () =>
            {
                float rowX = ImGui.GetCursorPosX();
                if (FeatureCheckboxToggleRow(rowX, innerW, "Flight Patch", ref Config.flyhack, null, "flyhack"))
                    OnPatchToggle(() => flyahackk());
                if (FeatureCheckboxToggleRow(rowX, innerW, "Fly X80 Altitude", ref FLYHACKINTTTT, ApplyFlyX80Int, "FLYHACKINTTTT"))
                    BootstrapRuntime.StartFeatureBg("flyint", Client.fkyhackint.Work, () => Config.FLYHACKINTTTT);
                if (FeatureCheckboxToggleRow(rowX, innerW, "Fly X80 Matrix", ref FlyHack80xx, null, "FlyHack80xx"))
                    BootstrapRuntime.StartFeatureBg("fly80", FlyHack80x.Start, () => Config.FlyHack80xx);
                if (FeatureCheckboxToggleRow(rowX, innerW, "Enemy Pull 360", ref EnemyPullEnabled, null, "EnemyPullEnabled"))
                    BootstrapRuntime.StartFeatureBg("enemypull360", EnemyPull360.Start, () => Config.EnemyPullEnabled);
                if (FeatureCheckboxToggleRow(rowX, innerW, "Fly X40", ref Config.FLYHACKX400, v =>
                {
                    Config.FLYHACKX400 = v;
                    Config.FLYHACKX40 = v;
                }, "FLYHACKX400"))
                    BootstrapRuntime.StartFeatureBg("flyx40", Client.FLYHACKX40.Work, () => Config.FLYHACKX400);
                if (FeatureCheckboxToggleRow(rowX, innerW, "Invisible Fly", ref Config.FlyHack50x, ApplyFlyInvisible, "FlyHack50x"))
                    BootstrapRuntime.StartFeatureBg("fly50", FlyHack50x.Start, () => Config.FlyHack50xx);
                if (FeatureCheckboxToggleRow(rowX, innerW, "Zero-Gravity Fly", ref FlyHack, KCNOGRAVITYFLY.SetState, "FlyHack")) { }
                if (FeatureCheckboxToggleRow(rowX, innerW, "Fly Rage", ref Config.FlyRage, FlyRage.SetState, "FlyRage")) { }
                if (FeatureCheckboxToggleRow(rowX, innerW, "Climb Assist", ref ClimbUpEnabled, null, "ClimbUpEnabled"))
                    BootstrapRuntime.StartFeatureBg("climbup", ClimbUp.Start, () => Config.ClimbUpEnabled);
                if (FeatureCheckboxToggleRow(rowX, innerW, "Climb Up V2", ref ClimbUpV2Enabled, null, "ClimbUpV2Enabled"))
                    BootstrapRuntime.StartFeatureBg("climbupv2", KCClimbUpV2.Start, () => Config.ClimbUpV2Enabled);
                ImGui.Dummy(new Vector2(0f, 4f));
                DrawHexSlider("Climb Height", ref ClimbUpV2Value, 0f, 80f, innerW);
                if (FeatureCheckboxToggleRow(rowX, innerW, "Speed Boost", ref speedint, null, "speedint")) { }
                if (FeatureCheckboxToggleRow(rowX, innerW, "Speed Boost (External)", ref Config.speedext, null, "speedext"))
                    loadspeedext();
                if (FeatureCheckboxToggleRow(rowX, innerW, "Burst Fire", ref Config.brsutfiree, null, "brsutfiree"))
                    brustfire();
            });

            ImGui.EndChild(); // end feature scroll
        }

        private void RenderKillTab()
        {
            float avail = ImGui.GetContentRegionAvail().X;
            float colW = MathF.Max(60f, avail);
            float innerW = MathF.Max(50f, colW - 16f);

            BeginFeatureScrollArea("kill_tab");

            DrawCard("Kill Functions", new Vector2(colW, 0f), () =>
            {
                float rowX = ImGui.GetCursorPosX();
                if (FeatureCheckboxToggleRow(rowX, innerW, "Teleport Kill", ref tele, null, "tele"))
                    BootstrapRuntime.StartFeatureBg("telekill", Tele.Work, () => Config.tele);
                if (FeatureCheckboxToggleRow(rowX, innerW, "Shake Kill", ref ShakeKill, null, "ShakeKill"))
                    BootstrapRuntime.StartFeatureBg("shakekill", Client.ShakeKill.Work, () => Config.ShakeKill);
                if (FeatureCheckboxToggleRow(rowX, innerW, "Down Player", ref Config.DownPlayer, null, "DownPlayer"))
                    BootstrapRuntime.StartFeatureBg("downplayer", DownPlayer.Work, () => Config.DownPlayer || Config.undergroundkill);
                if (FeatureCheckboxToggleRow(rowX, innerW, "Underground Kill", ref undergroundkill, null, "undergroundkill"))
                    BootstrapRuntime.StartFeatureBg("downplayer", DownPlayer.Work, () => Config.DownPlayer || Config.undergroundkill);
                if (FeatureCheckboxToggleRow(rowX, innerW, "Spawn Kill", ref Kcspawnkill, null, "Kcspawnkill"))
                    BootstrapRuntime.StartFeatureBg("spawnkill", Client.KCSPAWNKIL.Work, () => Config.Kcspawnkill);
                if (FeatureCheckboxToggleRow(rowX, innerW, "Quick Teleport", ref teliport, null, "teliport"))
                    BootstrapRuntime.StartFeatureBg("telport", Telport.Work, () => Config.teliport);
                if (FeatureCheckboxToggleRow(rowX, innerW, "AI Teleport Wall", ref teliportwall, null, "teliportwall"))
                    BootstrapRuntime.StartFeatureBg("teleportwall", Client.Teleportwall.Work, () => Config.teliportwall);
                if (FeatureCheckboxToggleRow(rowX, innerW, "Mark Map Teleport", ref teleportmap, null, "teleportmap"))
                    BootstrapRuntime.StartFeatureBg("mapteleport", KCMap_Teleport.Work, () => Config.teleportmap);
                if (FeatureCheckboxToggleRow(rowX, innerW, "Wallhack (AoB)", ref Config.wallhack, null, "wallhack"))
                    OnPatchToggle(() => wallhackrisk());
                FeatureCheckboxToggleRow(rowX, innerW, "Spin Bot V2", ref spinbot, ApplySpinBotState, "spinbot");
            });

            ImGui.EndChild(); // end feature scroll
        }

        private static string FormatKeyLabel(System.Windows.Forms.Keys key)
        {
            if (key == System.Windows.Forms.Keys.None)
                return "None";

            string label = key.ToString();
            if (label == "LButton") return "LMouse";
            if (label == "RButton") return "RMouse";
            if (label == "MButton") return "MMouse";
            if (label == "XButton1") return "Mouse4";
            if (label == "XButton2") return "Mouse5";
            return label;
        }

        private static bool DrawKeyLabelButton(string id, ref System.Windows.Forms.Keys key, Vector2 size)
        {
            bool isBinding = _activeBindingId == id;
            if (isBinding)
            {
                ImGuiIOPtr io = ImGui.GetIO();
                for (int i = 1; i < 256; i++)
                {
                    if (ImGui.IsKeyDown((ImGuiKey)i))
                    {
                        key = (System.Windows.Forms.Keys)i;
                        _activeBindingId = null;
                        break;
                    }
                }
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left)) { key = System.Windows.Forms.Keys.LButton; _activeBindingId = null; }
                else if (ImGui.IsMouseClicked(ImGuiMouseButton.Right)) { key = System.Windows.Forms.Keys.RButton; _activeBindingId = null; }
                else if (ImGui.IsMouseClicked(ImGuiMouseButton.Middle)) { key = System.Windows.Forms.Keys.MButton; _activeBindingId = null; }
            }

            string label = isBinding ? "..." : FormatKeyLabel(key);

            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(12f / 255f, 13f / 255f, 18f / 255f, _uiAlpha));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(20f / 255f, 21f / 255f, 28f / 255f, _uiAlpha));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(232f / 255f, 20f / 255f, 26f / 255f, _uiAlpha));
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 3f);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1f);
            ImGui.PushStyleColor(ImGuiCol.Border, isBinding
                ? new Vector4(232f / 255f, 20f / 255f, 26f / 255f, _uiAlpha)
                : new Vector4(30f / 255f, 32f / 255f, 40f / 255f, _uiAlpha));
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.62f, 0.62f, 0.66f, _uiAlpha));

            bool pressed = ImGui.Button(label + "##keylbl_" + id, size);
            if (pressed)
                _activeBindingId = isBinding ? null : id;

            ImGui.PopStyleColor(5);
            ImGui.PopStyleVar(2);
            return pressed;
        }

        private static bool DrawKeyBindIconButton(string id, ref System.Windows.Forms.Keys key, Vector2 size)
        {
            bool isBinding = _activeBindingId == id;
            if (isBinding)
            {
                ImGuiIOPtr io = ImGui.GetIO();
                for (int i = 1; i < 256; i++)
                {
                    if (ImGui.IsKeyDown((ImGuiKey)i))
                    {
                        key = (System.Windows.Forms.Keys)i;
                        _activeBindingId = null;
                        break;
                    }
                }
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left)) { key = System.Windows.Forms.Keys.LButton; _activeBindingId = null; }
                else if (ImGui.IsMouseClicked(ImGuiMouseButton.Right)) { key = System.Windows.Forms.Keys.RButton; _activeBindingId = null; }
                else if (ImGui.IsMouseClicked(ImGuiMouseButton.Middle)) { key = System.Windows.Forms.Keys.MButton; _activeBindingId = null; }
            }

            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(12f / 255f, 13f / 255f, 18f / 255f, _uiAlpha));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(20f / 255f, 21f / 255f, 28f / 255f, _uiAlpha));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(232f / 255f, 20f / 255f, 26f / 255f, _uiAlpha));
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 3f);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1f);
            ImGui.PushStyleColor(ImGuiCol.Border, isBinding
                ? new Vector4(232f / 255f, 20f / 255f, 26f / 255f, _uiAlpha)
                : new Vector4(30f / 255f, 32f / 255f, 40f / 255f, _uiAlpha));
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.78f, 0.78f, 0.82f, _uiAlpha));

            Vector2 pos = ImGui.GetCursorScreenPos();
            ImGui.SetCursorScreenPos(pos);
            ImGui.InvisibleButton("##keyicon_" + id, size);
            bool pressed = ImGui.IsItemClicked();
            if (pressed)
                _activeBindingId = isBinding ? null : id;

            var btnDraw = ImGui.GetWindowDrawList();
            Vector2 btnEnd = pos + size;
            btnDraw.AddRectFilled(pos, btnEnd, ImGui.GetColorU32(new Vector4(12f / 255f, 13f / 255f, 18f / 255f, _uiAlpha)), 3f);
            btnDraw.AddRect(pos, btnEnd,
                ImGui.GetColorU32(isBinding
                    ? new Vector4(232f / 255f, 20f / 255f, 26f / 255f, _uiAlpha)
                    : new Vector4(30f / 255f, 32f / 255f, 40f / 255f, _uiAlpha)),
                3f, ImDrawFlags.None, 1f);
            UiKeyboardIcon.Draw(btnDraw, pos + size * 0.5f, MathF.Min(size.X, size.Y) * 0.42f, ThemeColor, _uiAlpha);

            ImGui.PopStyleColor(5);
            ImGui.PopStyleVar(2);
            return pressed;
        }

        private void FeatureHookRow(float rowWidth, string label, ref bool value, ref System.Windows.Forms.Keys key, string bindId)
        {
            const float noneBtnW = 52f;
            const float keyBtnW = 26f;
            const float gap = 6f;

            float anchorX = ImGui.GetCursorPosX();
            ImGui.SetCursorPosX(anchorX);

            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.88f, 0.88f, 0.90f, _uiAlpha));
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(label);
            ImGui.PopStyleColor();

            float noneX = anchorX + rowWidth - noneBtnW - keyBtnW - gap;
            ImGui.SetCursorPosX(noneX);
            DrawKeyLabelButton(bindId + "_lbl", ref key, new Vector2(noneBtnW, 20f));

            ImGui.SameLine(0f, gap);
            DrawKeyBindIconButton(bindId + "_ico", ref key, new Vector2(keyBtnW, 20f));

            Vector2 screenP = ImGui.GetCursorScreenPos();
            ImGui.GetWindowDrawList().AddLine(
                screenP + new Vector2(0f, 2f),
                screenP + new Vector2(rowWidth, 2f),
                ImGui.GetColorU32(new Vector4(24f / 255f, 25f / 255f, 31f / 255f, 0.55f * _uiAlpha)),
                1f);
            ImGui.Dummy(new Vector2(rowWidth, 8f));

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                Vector2 mouse = ImGui.GetMousePos();
                Vector2 rowMin = screenP - new Vector2(0f, 22f);
                Vector2 rowMax = screenP + new Vector2(noneX - anchorX, 0f);
                if (mouse.X >= rowMin.X && mouse.X <= rowMax.X && mouse.Y >= rowMin.Y && mouse.Y <= rowMax.Y)
                    value = !value;
            }
        }

        private void DrawCrossArrowsGraphic(Vector2 center, float size)
        {
            var draw = ImGui.GetWindowDrawList();
            uint col = ImGui.GetColorU32(new Vector4(40f / 255f, 245f / 255f, 40f / 255f, 0.95f * _uiAlpha));
            float arm = size * 0.5f;
            float head = size * 0.22f;
            float thick = 2.2f;

            draw.AddLine(center + new Vector2(-arm, 0f), center + new Vector2(arm, 0f), col, thick);
            draw.AddLine(center + new Vector2(0f, -arm), center + new Vector2(0f, arm), col, thick);

            draw.AddTriangleFilled(center + new Vector2(arm + head, 0f), center + new Vector2(arm - head * 0.2f, head * 0.55f), center + new Vector2(arm - head * 0.2f, -head * 0.55f), col);
            draw.AddTriangleFilled(center + new Vector2(-arm - head, 0f), center + new Vector2(-arm + head * 0.2f, head * 0.55f), center + new Vector2(-arm + head * 0.2f, -head * 0.55f), col);
            draw.AddTriangleFilled(center + new Vector2(0f, -arm - head), center + new Vector2(head * 0.55f, -arm + head * 0.2f), center + new Vector2(-head * 0.55f, -arm + head * 0.2f), col);
            draw.AddTriangleFilled(center + new Vector2(0f, arm + head), center + new Vector2(head * 0.55f, arm - head * 0.2f), center + new Vector2(-head * 0.55f, arm - head * 0.2f), col);
        }

        private void RenderExtremeHooksTab()
        {
            float avail = ImGui.GetContentRegionAvail().X;
            float colW = MathF.Max(60f, (avail - 8f) * 0.5f);
            float innerW = MathF.Max(50f, colW - 16f);

            ImGui.PushStyleColor(ImGuiCol.ScrollbarBg, new Vector4(0f, 0f, 0f, 0f));
            ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab, new Vector4(0f, 0f, 0f, 0f));
            ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered, new Vector4(0f, 0f, 0f, 0f));
            ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabActive, new Vector4(0f, 0f, 0f, 0f));
            ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize, 0f);

            float bodyH = ImGui.GetContentRegionAvail().Y;

            // ---- LEFT COLUMN: Hooks ----
            ImGui.BeginChild("##hooks_left_col", new Vector2(colW, bodyH), ImGuiChildFlags.None,
                ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoScrollbar);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(3f, 2f));

            DrawHexSectionHeader("\uF0E7", "Hook Features");

            float rowX = ImGui.GetCursorPosX();
            FeatureCheckboxToggleRow(rowX, innerW, "Teleport Kill", ref tele);
            FeatureCheckboxToggleRow(rowX, innerW, "Shake Kill", ref ShakeKill);
            FeatureCheckboxToggleRow(rowX, innerW, "Down Player", ref Config.DownPlayer);
            FeatureCheckboxToggleRow(rowX, innerW, "Underground Kill", ref undergroundkill);
            FeatureCheckboxToggleRow(rowX, innerW, "Spawn Kill", ref Kcspawnkill);
            FeatureCheckboxToggleRow(rowX, innerW, "Enemy Pull 360", ref EnemyPullEnabled);
            if (FeatureCheckboxToggleRow(rowX, innerW, "Speed Boost (External)", ref Config.speedext, null, null))
            {
                loadspeedext();
            }
            if (FeatureCheckboxToggleRow(rowX, innerW, "Burst Fire", ref Config.brsutfiree, null, null))
            {
                brustfire();
            }
            ImGui.PopStyleVar();
            ImGui.EndChild();

            ImGui.SameLine(0f, 8f);

            // ---- RIGHT COLUMN: Teleport ----
            ImGui.BeginChild("##hooks_right_col", new Vector2(colW, bodyH), ImGuiChildFlags.None,
                ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoScrollbar);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(3f, 2f));

            DrawHexSectionHeader("\uF015", "Teleport");

            float rowX2 = ImGui.GetCursorPosX();
            FeatureCheckboxToggleRow(rowX2, innerW, "Quick Teleport", ref teliport);
            FeatureCheckboxToggleRow(rowX2, innerW, "AI Teleport Wall", ref teliportwall);
            FeatureCheckboxToggleRow(rowX2, innerW, "Mark Map Teleport", ref teleportmap);
            if (FeatureCheckboxToggleRow(rowX2, innerW, "Wallhack (AoB)", ref Config.wallhack))
                OnPatchToggle(() => wallhackrisk());
            FeatureCheckboxToggleRow(rowX2, innerW, "Spin Bot V2", ref spinbot, ApplySpinBotState);

            ImGui.PopStyleVar();
            ImGui.EndChild();

            ImGui.PopStyleVar();
            ImGui.PopStyleColor(4);
        }

        private void RenderSettingsTab()
        {
            float avail = ImGui.GetContentRegionAvail().X;
            float colW = MathF.Max(60f, avail);
            float innerW = MathF.Max(50f, colW - 16f);

            BeginFeatureScrollArea("settings_tab");

            float bodyH = ImGui.GetContentRegionAvail().Y;
            Vector2 btnSz = new Vector2(innerW, 30f);

            DrawCard("", new Vector2(colW, 0f), () =>
            {
                float rowX = ImGui.GetCursorPosX();
                FeatureCheckboxToggleRow(rowX, innerW, "Auto Load Config", ref AutoLoadConfig);
                FeatureCheckboxToggleRow(rowX, innerW, "Stream Proof", ref Config.StreamMode);

                ImGui.Dummy(new Vector2(0f, 6f));
                if (TextFont.IsLoaded()) ImGui.PushFont(TextFont);
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.45f, 0.45f, 0.50f, _uiAlpha));
                ImGui.TextWrapped("INSERT = toggle UI");
                ImGui.PopStyleColor();
                if (TextFont.IsLoaded()) ImGui.PopFont();

                ImGui.Dummy(new Vector2(0f, 6f));
                DrawAimbotHoldKeyRow(innerW);
            });
            


            DrawCard("Configuration", new Vector2(colW, 0f), () =>
            {
                ImGui.Dummy(new Vector2(0f, 4f));
                if (CustomButton.CaxCustomButton("Save Config", btnSz, new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, 0.85f)))
                    SaveSingleConfigFromUi();

                ImGui.Dummy(new Vector2(0f, 4f));
                if (CustomButton.CaxCustomButton("Load Config", btnSz, new Vector4(0.30f, 0.30f, 0.34f, 1f)))
                    LoadSingleConfigFromUi();
                    
                ImGui.Dummy(new Vector2(0f, 4f));
                if (CustomButton.CaxCustomButton("Delete Config", btnSz, new Vector4(0.55f, 0.12f, 0.12f, 0.85f)))
                    DeleteSingleConfigFromUi();
            });
            
            ImGui.Dummy(new Vector2(0f, 8f));
            
            DrawCard("ADB Port", new Vector2(colW, 0f), () =>
            {
                int port = Math.Clamp(Config.AdbPort, 1, 65535);
                ImGui.SetNextItemWidth(innerW);
                if (ImGui.InputInt("##adb_port", ref port, 1, 100))
                    Config.AdbPort = Math.Clamp(port, 1, 65535);
                ImGui.Dummy(new Vector2(0f, 4f));
                if (CustomButton.CaxCustomButton("Apply & Reconnect", btnSz, new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, 0.85f)))
                    RestartAdbConnectionFromUi();
            });

            ImGui.EndChild(); // end feature scroll
        }

        private void DrawTabPopup()
        {
            var io = ImGui.GetIO();
            float speed = 18f;
            _tabPopupAnim += ((_showTabPopup ? 1f : 0f) - _tabPopupAnim) * io.DeltaTime * speed;
            _tabPopupAnim = Math.Clamp(_tabPopupAnim, 0f, 1f);

            if (_tabPopupAnim < 0.01f) return;

            Vector2 center = ImGui.GetMainViewport().GetCenter();
            Vector2 popupSize = new Vector2(220, 100) * _tabPopupAnim;
            var draw = ImGui.GetWindowDrawList();
            Vector2 p = center - popupSize * 0.5f;

            draw.AddRectFilled(p, p + popupSize,
                ImGui.GetColorU32(new Vector4(0.1f, 0.1f, 0.15f, 0.95f * _tabPopupAnim * _uiAlpha)),
                3f);
            draw.AddRect(p, p + popupSize,
                ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.6f * _tabPopupAnim * _uiAlpha)),
                3f);

            Vector2 textSize = ImGui.CalcTextSize("Tab Changed!");
            draw.AddText(p + (popupSize - textSize) * 0.5f,
                ImGui.GetColorU32(new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, _tabPopupAnim * _uiAlpha)),
                "Tab Changed!");

            if (_tabPopupAnim >= 0.99f)
                _showTabPopup = false;
        }

        private void EnsureTabIconsLoaded()
        {
            if (_tabIconsInitialized)
                return;
            _tabIconsInitialized = true;
            // Primary: Font Awesome glyphs via TextFont (fa-solid-900 merged). PNGs only if TextFont unavailable.
        }

        private IntPtr LoadTabIcon(string fileName)
        {
            string[] candidatePaths =
            {
                Path.Combine(AppContext.BaseDirectory, "Components", fileName),
                Path.Combine(Directory.GetCurrentDirectory(), "Components", fileName),
                Path.Combine(AppContext.BaseDirectory, fileName)
            };

            for (int i = 0; i < candidatePaths.Length; i++)
            {
                if (!File.Exists(candidatePaths[i]))
                    continue;

                IntPtr texturePtr = IntPtr.Zero;
                uint width = 0;
                uint height = 0;

                AddOrGetImagePointer(candidatePaths[i], false, out texturePtr, out width, out height);
                if (texturePtr != IntPtr.Zero)
                    return texturePtr;
            }

            return IntPtr.Zero;
        }

        private static string ResolveWav(string fileName)
        {
            string p = Path.Combine(AppContext.BaseDirectory, fileName);
            return File.Exists(p) ? p : "";
        }

        private static void PlayWav(string path)
        {
            Task.Run(() =>
            {
                try
                {
                    using (var audioFile = new AudioFileReader(path))
                    using (var output = new WaveOutEvent())
                    {
                        output.Init(audioFile);
                        output.Play();
                        while (output.PlaybackState == PlaybackState.Playing)
                            Task.Delay(10).Wait();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Sound playback error: " + ex.Message);
                }
            });
        }

        private static void ResetLoaderVisualState()
        {
            _loaderTime = 0f;
            _connected = false;
            _connectedFade = 0f;
            _soundPlayed = false;
            _loaderAngle = 0f;
            _pulseAlpha = 0.5f;
            _pulseIncreasing = true;
        }

        private static void ResetFlowToLogin()
        {
            _flowPhase = FlowPhase.Login;
            _emulatorInitTask = null;
            _initError = null;
            _initCompletionHandled = false;
            _cheatsRuntimeStarted = false;
            BootstrapRuntime.ResetCheatsThreadsGate();
            _activeTab = UiMetrics.LoginTabIndex;
            _showHookSuccessScreen = false;
            _hookSuccessAnimTime = 0f;
            ResetLoaderVisualState();
        }

        private static void RestartEmulatorInit()
        {
            RestartAdbConnectionFromUi();
            _showHookSuccessScreen = false;
            _hookSuccessAnimTime = 0f;
            _loaderTime = 0f;
            _connectedFade = 0f;
        }

        private static void HandleEmulatorInitCompleted()
        {
            if (_initCompletionHandled || _emulatorInitTask == null || !_emulatorInitTask.IsCompleted)
                return;

            _initCompletionHandled = true;

            if (_emulatorInitTask.IsFaulted)
            {
                _initError = _emulatorInitTask.Exception?.GetBaseException().Message ?? "Connection failed.";
                return;
            }

            string? err = _emulatorInitTask.Result;
            if (!string.IsNullOrEmpty(err))
            {
                _initError = err;
                return;
            }

            if (Offsets.Il2Cpp == 0)
            {
                _initError = "Hook incomplete. Open Free Fire lobby, then Apply & Reconnect.";
                return;
            }

            if (Core.Handle == IntPtr.Zero)
                Core.Handle = BootstrapRuntime.ResolveGameRenderHandle(BootstrapRuntime.MainWindowHandle);

            if (Core.Handle == IntPtr.Zero && BootstrapRuntime.MainWindowHandle != IntPtr.Zero)
                Core.Handle = BootstrapRuntime.MainWindowHandle;

            if (Core.Handle == IntPtr.Zero)
            {
                _initError = "Hook incomplete. Open Free Fire lobby, then Apply & Reconnect.";
                return;
            }

            _initError = null;

            if (_flowPhase == FlowPhase.Main)
            {
                CustomNotification.Notify("ADB", $"Connected — port {Config.AdbPort}", 4f);
                if (!_cheatsRuntimeStarted)
                {
                    _cheatsRuntimeStarted = true;
                    Config.ApplyStartupFromSingleConfigFile();
                    _pendingFeatureRuntimeApply = true;
                    _ = BootstrapRuntime.StartCheatsAsync();
                }
                return;
            }

            _showHookSuccessScreen = true;
            _hookSuccessAnimTime = 0f;
        }

        private static void RestartAdbConnectionFromUi()
        {
            _initError = null;
            _initCompletionHandled = false;
            _connected = true;
            Core.HaveMatrix = false;
            Core.LocalPlayer = 0;
            Core.Entities.Clear();
            Offsets.Il2Cpp = 0;
            InternalMemory.ClearCache();
            _emulatorInitTask = BootstrapRuntime.InitEmulatorAsync();
            CustomNotification.Notify("Connect", "Reconnecting to Free Fire…", 3f);
        }

        private void RenderAdbTab()
        {
            GetSingleColumnLayout(out float colW, out float innerW);
            BeginFeatureScrollArea("adb");

            HandleEmulatorInitCompleted();

            DrawCard("ADB Port", new Vector2(colW, 1f), () =>
            {
                if (TextFont.IsLoaded()) ImGui.PushFont(TextFont);
                ImGui.TextUnformatted("Emulator ADB port");
                if (TextFont.IsLoaded()) ImGui.PopFont();

                int port = Math.Clamp(Config.AdbPort, 1, 65535);
                ImGui.SetNextItemWidth(innerW);
                if (ImGui.InputInt("##adb_port_tab", ref port, 1, 100))
                    Config.AdbPort = Math.Clamp(port, 1, 65535);

                ImGui.Dummy(new Vector2(0f, 6f));
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.55f, 0.55f, 0.60f, _uiAlpha));
                ImGui.TextWrapped("Only this port is used (no random / auto other ports). Change here anytime after login.");
                ImGui.PopStyleColor();

                ImGui.Dummy(new Vector2(0f, 10f));
                if (CustomButton.CaxCustomButton("Apply Port & Reconnect", new Vector2(innerW, 30f),
                    new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, 0.9f)))
                {
                    RestartAdbConnectionFromUi();
                }

                ImGui.Dummy(new Vector2(0f, 10f));
                string status;
                Vector4 statusCol;
                if (_emulatorInitTask != null && !_emulatorInitTask.IsCompleted)
                {
                    status = $"Connecting to 127.0.0.1:{Config.AdbPort}…";
                    statusCol = new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, _uiAlpha);
                }
                else if (!string.IsNullOrEmpty(_initError))
                {
                    status = _initError;
                    statusCol = new Vector4(1f, 0.45f, 0.45f, _uiAlpha);
                }
                else if (Offsets.Il2Cpp != 0)
                {
                    status = $"Connected — libil2cpp @ 0x{Offsets.Il2Cpp:X} (port {Config.AdbPort})";
                    statusCol = new Vector4(0.3f, 1f, 0.45f, _uiAlpha);
                }
                else
                {
                    status = "Not connected. Set port and press Apply.";
                    statusCol = new Vector4(0.65f, 0.65f, 0.70f, _uiAlpha);
                }

                if (TextFont.IsLoaded()) ImGui.PushFont(TextFont);
                ImGui.PushStyleColor(ImGuiCol.Text, statusCol);
                ImGui.TextWrapped(status);
                ImGui.PopStyleColor();
                if (TextFont.IsLoaded()) ImGui.PopFont();
            });

            EndFeatureScrollArea();
        }

        private static void DrawEmperorPanelBackground1(Vector2 position, Vector2 size, float rounding)
        {
            var draw = ImGui.GetWindowDrawList();
            Vector2 endPos = position + size;
            draw.AddRectFilled(position, endPos,
                ImGui.GetColorU32(new Vector4(PanelBg.X, PanelBg.Y, PanelBg.Z, PanelBg.W * _uiAlpha)),
                rounding);
            // Border removed for compactness
            ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(6, 6));
        }


        private static void DrawEmperorLoader()
        {
            if (_flowPhase != FlowPhase.PostLoginLoad || _showHookSuccessScreen)
                return;

            float dt = ImGui.GetIO().DeltaTime;
            _loaderTime += dt;
            if (_connected && _connectedFade < 1f)
                _connectedFade = Math.Min(1f, _connectedFade + dt * 2.5f);

            HandleEmulatorInitCompleted();

            if (!string.IsNullOrEmpty(_initError))
            {
                DrawEmulatorInitErrorPanel();
                return;
            }

            _loaderAngle += dt * 2.4f; // smooth speed

            var draw = ImGui.GetWindowDrawList();
            Vector2 pos = ImGui.GetWindowPos();
            Vector2 size = ImGui.GetWindowSize();

            Vector2 panelCenter = new Vector2(
                pos.X + size.X * 0.5f,
                pos.Y + size.Y * 0.5f
            );

            bool done = _connected && _emulatorInitTask != null && _emulatorInitTask.IsCompleted;

            const float textGap = 26f;
            float radius = 22f;
            string text;
            Vector4 color;

            if (!_connected)
            {
                text = "Initializing...";
                color = new Vector4(1f, 1f, 1f, _uiAlpha);
            }
            else if (_emulatorInitTask != null && !_emulatorInitTask.IsCompleted)
            {
                text = "ADB / injection running…";
                color = new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, _uiAlpha);
            }
            else
            {
                text = "Connected";
                color = new Vector4(0.2f, 1f, 0.2f, _connectedFade * _uiAlpha);
            }

            if (TextFont.IsLoaded()) ImGui.PushFont(TextFont);
            Vector2 textSize = ImGui.CalcTextSize(text);
            if (TextFont.IsLoaded()) ImGui.PopFont();

            float iconBlockH = done ? 28f : radius * 2f;
            float totalBlockH = iconBlockH + textGap + textSize.Y;
            float blockTopY = panelCenter.Y - totalBlockH * 0.5f;
            Vector2 iconCenter = new Vector2(panelCenter.X, blockTopY + iconBlockH * 0.5f);

            // =========================
            // 🌙 BOLD FAT SMOOTH SPINNER
            // =========================
            if (!done)
            {
                float thickness = 6.0f; // BOLD fat spinner

                int segments = 24;
                float arcLength = MathF.PI * 1.5f;

                for (int i = 0; i < segments; i++)
                {
                    float t0 = (float)i / segments;
                    float t1 = (float)(i + 1) / segments;

                    float angle0 = _loaderAngle + t0 * arcLength;
                    float angle1 = _loaderAngle + t1 * arcLength;

                    // Fade tail backward instead of forward: t0=1 is the bright head!
                    float alpha = MathF.Pow(t0, 3.0f) * 0.9f * _uiAlpha;

                    Vector2 p0 = new Vector2(
                        iconCenter.X + MathF.Cos(angle0) * radius,
                        iconCenter.Y + MathF.Sin(angle0) * radius
                    );

                    Vector2 p1 = new Vector2(
                        iconCenter.X + MathF.Cos(angle1) * radius,
                        iconCenter.Y + MathF.Sin(angle1) * radius
                    );

                    draw.AddLine(
                        p0,
                        p1,
                        ImGui.GetColorU32(new Vector4(1f, 1f, 1f, alpha)),
                        thickness
                    );
                }
            }

            // =========================
            // ✅ CLEAN TICK (LIKE IMAGE)
            // =========================
            if (done)
            {
                float t = Math.Clamp(_connectedFade, 0f, 1f);
                t = 1f - MathF.Pow(1f - t, 3f); // smooth ease

                float thickness = 5.5f; // bolder stroke

                // perfect proportions longer professional checkmark
                Vector2 p0 = iconCenter + new Vector2(-12f, -1f);
                Vector2 p1 = iconCenter + new Vector2(-3f, 10f); // sharp drop down
                Vector2 p2 = iconCenter + new Vector2(17f, -14f); // extremely long sweeping tail upwards

                uint col = ImGui.GetColorU32(new Vector4(0.2f, 1f, 0.2f, t * _uiAlpha));

                // animate stroke
                float t1 = Math.Clamp(t * 1.3f, 0f, 1f);
                Vector2 a = Vector2.Lerp(p0, p1, t1);
                draw.AddLine(p0, a, col, thickness);

                if (t > 0.45f)
                {
                    float t2 = Math.Clamp((t - 0.45f) * 1.8f, 0f, 1f);
                    Vector2 b = Vector2.Lerp(p1, p2, t2);
                    draw.AddLine(p1, b, col, thickness);
                }
            }

            // =========================
            // 📝 TEXT (centered under icon as one block)
            // =========================
            Vector2 textPos = new Vector2(
                panelCenter.X - textSize.X * 0.5f,
                blockTopY + iconBlockH + textGap
            );

            if (TextFont.IsLoaded()) ImGui.PushFont(TextFont);
            draw.AddText(textPos, ImGui.GetColorU32(color), text);
            if (TextFont.IsLoaded()) ImGui.PopFont();
        }

        private static void DrawEmulatorInitErrorPanel()
        {
            var draw = ImGui.GetWindowDrawList();
            Vector2 pos = ImGui.GetWindowPos();
            Vector2 size = ImGui.GetWindowSize();
            Vector2 center = pos + size * 0.5f;

            string title = "Connection failed";
            string msg = _initError ?? "Unknown error";

            if (TextFont.IsLoaded()) ImGui.PushFont(TextFont);
            Vector2 titleSz = ImGui.CalcTextSize(title);
            Vector2 msgSz = ImGui.CalcTextSize(msg);
            if (TextFont.IsLoaded()) ImGui.PopFont();

            float btnW = 140f;
            float btnH = 32f;
            float gap = 14f;
            float blockH = titleSz.Y + gap + msgSz.Y + gap + btnH;

            Vector2 blockTop = new Vector2(center.X, center.Y - blockH * 0.5f);

            if (TextFont.IsLoaded()) ImGui.PushFont(TextFont);
            draw.AddText(
                new Vector2(center.X - titleSz.X * 0.5f, blockTop.Y),
                ImGui.GetColorU32(new Vector4(1f, 0.45f, 0.45f, _uiAlpha)),
                title);
            draw.AddText(
                new Vector2(center.X - msgSz.X * 0.5f, blockTop.Y + titleSz.Y + gap),
                ImGui.GetColorU32(new Vector4(0.85f, 0.85f, 0.9f, _uiAlpha)),
                msg);
            if (TextFont.IsLoaded()) ImGui.PopFont();

            Vector2 btnPos = new Vector2(center.X - btnW * 0.5f, blockTop.Y + titleSz.Y + gap + msgSz.Y + gap);
            Vector2 btnMax = btnPos + new Vector2(btnW, btnH);
            bool hover = ImGui.IsMouseHoveringRect(btnPos, btnMax);
            draw.AddRectFilled(btnPos, btnMax,
                ImGui.GetColorU32(new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, (hover ? 0.95f : 0.75f) * _uiAlpha)),
                6f);

            string retry = "Retry";
            if (TextFont.IsLoaded()) ImGui.PushFont(TextFont);
            Vector2 retrySz = ImGui.CalcTextSize(retry);
            draw.AddText(btnPos + (new Vector2(btnW, btnH) - retrySz) * 0.5f,
                ImGui.GetColorU32(new Vector4(1f, 1f, 1f, _uiAlpha)), retry);
            if (TextFont.IsLoaded()) ImGui.PopFont();

            if (hover && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                RestartEmulatorInit();
        }

        private static void DrawHookSuccessScreen()
        {
            if (_flowPhase != FlowPhase.PostLoginLoad || !_showHookSuccessScreen)
                return;

            float dt = ImGui.GetIO().DeltaTime;
            _hookSuccessAnimTime += dt;
            float t = _hookSuccessAnimTime;

            float availH = ImGui.GetContentRegionAvail().Y;
            ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(PanelBg.X, PanelBg.Y, PanelBg.Z, 0f));
            ImGui.BeginChild("##hook_success_body", new Vector2(0, availH),
                ImGuiChildFlags.None, ImGuiWindowFlags.NoBackground);

            var draw = ImGui.GetWindowDrawList();
            Vector2 wp = ImGui.GetWindowPos();
            Vector2 ws = ImGui.GetWindowSize();
            DrawEmperorPanelBackground1(wp, ws, 3f);

            Vector2 center = new Vector2(wp.X + ws.X * 0.5f, wp.Y + ws.Y * 0.46f);

            static float EaseOutBack(float x)
            {
                const float c1 = 1.70158f;
                const float c3 = c1 + 1f;
                return 1f + c3 * MathF.Pow(x - 1f, 3f) + c1 * MathF.Pow(x - 1f, 2f);
            }

            static float EaseOutCubic(float x) => 1f - MathF.Pow(1f - x, 3f);

            // No circular background: only animated green tick.
            float tCircle = Math.Min(1f, t / 0.38f);
            float circleScale = EaseOutBack(tCircle);
            // Compact (less “stretched” ring) + slightly bolder tick.
            circleScale = Math.Min(circleScale, 1.07f);

            const float baseR = 41f;
            float ringR = baseR * circleScale;

            float tStrokeRaw = Math.Max(0f, t - 0.32f) / 0.62f;
            float strokeProg = EaseOutCubic(Math.Clamp(tStrokeRaw, 0f, 1f));
            float chkAlpha = MathF.Min(1f, (t - 0.28f) / 0.15f) * _uiAlpha;
            uint chkCol = ImGui.GetColorU32(new Vector4(0.2f, 1f, 0.2f, chkAlpha));

            float k = ringR * 0.52f;
            // Compact tick geometry (tighter angles/lengths).
            Vector2 p1 = center + new Vector2(-0.46f * k, 0.00f * k);
            Vector2 p2 = center + new Vector2(-0.06f * k, 0.36f * k);
            Vector2 p3 = center + new Vector2(0.50f * k, -0.38f * k);

            float len1 = Vector2.Distance(p1, p2);
            float len2 = Vector2.Distance(p2, p3);
            float total = len1 + len2;
            float drawLen = total * strokeProg;
            // Match spinner “bold” feel (thicker stroke).
            const float lineThick = 6.25f;

            if (drawLen > 0f && len1 > 0.001f)
            {
                float u = MathF.Min(drawLen, len1) / len1;
                Vector2 q = p1 + (p2 - p1) * u;
                draw.AddLine(p1, q, chkCol, lineThick);
            }
            if (drawLen > len1 && len2 > 0.001f)
            {
                float u = MathF.Min(drawLen - len1, len2) / len2;
                Vector2 q = p2 + (p3 - p2) * u;
                draw.AddLine(p2, q, chkCol, lineThick);
            }

            // Soft round join to avoid a “sharp corner” at p2.
            if (drawLen > len1 + 0.01f)
            {
                float joinT = Math.Clamp((drawLen - len1) / (len2 * 0.25f), 0f, 1f);
                // Slightly lower alpha = smoother join (less “blob”).
                float joinA = chkAlpha * 0.32f * joinT * _uiAlpha;
                uint joinCol = ImGui.GetColorU32(new Vector4(0.2f, 1f, 0.2f, joinA));
                float rad = lineThick * 0.60f;
                draw.AddCircleFilled(p2, rad, joinCol, 14);
            }

            float capA = strokeProg * chkAlpha * 0.45f;
            if (capA > 0.01f && drawLen > 0.01f)
            {
                uint capCol = ImGui.GetColorU32(new Vector4(0.2f, 1f, 0.2f, capA));
                float rad = lineThick * 0.5f;
                if (drawLen < len1)
                {
                    float u = MathF.Min(drawLen, len1) / len1;
                    Vector2 tip = p1 + (p2 - p1) * u;
                    draw.AddCircleFilled(tip, rad, capCol, 12);
                }
                else
                {
                    float u = MathF.Min(drawLen - len1, len2) / len2;
                    Vector2 tip = p2 + (p3 - p2) * u;
                    draw.AddCircleFilled(tip, rad, capCol, 12);
                }
            }

            float textT = Math.Clamp((t - 0.58f) / 0.42f, 0f, 1f);
            float textEase = 1f - MathF.Pow(1f - textT, 2.5f);
            float textAlpha = textEase * _uiAlpha;
            float yLift = (1f - textEase) * 18f;
            string hookMsg = "Successfully Hooked!";
            ImGui.PushFont(LebelFont.IsLoaded() ? LebelFont : TextFont);
            Vector2 msgSz = ImGui.CalcTextSize(hookMsg);
            Vector2 msgPos = new Vector2(
                center.X - msgSz.X * 0.5f,
                // Compact message placement under the tick.
                center.Y + ringR + 22f - yLift);
            for (int i = 3; i >= 1; i--)
            {
                float a = textAlpha * 0.12f / i;
                draw.AddText(
                    msgPos + new Vector2(i, i * 0.5f),
                    ImGui.GetColorU32(new Vector4(0f, 0f, 0f, a)),
                    hookMsg);
            }
            draw.AddText(msgPos, ImGui.GetColorU32(new Vector4(1f, 1f, 1f, textAlpha)), hookMsg);
            ImGui.PopFont();

            ImGui.EndChild();
            ImGui.PopStyleColor();

            if (t >= HookSuccessAnimDuration)
            {
                _flowPhase = FlowPhase.Main;
                _activeTab = 0;
                _showHookSuccessScreen = false;
                _hookSuccessAnimTime = 0f;
                if (!_cheatsRuntimeStarted)
                {
                    _cheatsRuntimeStarted = true;
                    Config.ApplyStartupFromSingleConfigFile();
                    _pendingFeatureRuntimeApply = true;
                    _ = BootstrapRuntime.StartCheatsAsync();
                }
                CustomNotification.Notify("Notification", $"Success — emulator ready (ADB {Config.AdbPort})", 4.5f);
            }
        }

        private void DrawEmbeddedLoginPanel()
        {
            float contentW = ImGui.GetContentRegionAvail().X;
            float contentH = ImGui.GetContentRegionAvail().Y;
            var draw = ImGui.GetWindowDrawList();

            const float fieldH = 38f;
            const float fieldGap = 10f;
            const float btnH = 38f;
            const float btnGap = 18f;
            const float statusGap = 6f;

            float formW = MathF.Min(270f, MathF.Max(200f, contentW - 40f));
            float totalH = 26f + 8f + 14f + 20f + fieldH + fieldGap + fieldH + btnGap + btnH + 4f;

            float blockTop = MathF.Max(8f, (contentH - totalH) * 0.38f);
            float blockLeft = MathF.Max(0f, (contentW - formW) * 0.5f);
            Vector2 cardMin = ImGui.GetCursorScreenPos() + new Vector2(blockLeft - 14f, blockTop - 14f);
            Vector2 cardMax = cardMin + new Vector2(formW + 28f, totalH + 28f);

            uint cardBg = ImGui.GetColorU32(new Vector4(0.06f, 0.06f, 0.10f, 0.94f * _uiAlpha));
            draw.AddRectFilled(cardMin, cardMax, cardBg, 12f);
            draw.AddRect(cardMin, cardMax, ImGui.GetColorU32(new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, 0.25f * _uiAlpha)), 12f, ImDrawFlags.None, 1f);

            ImGui.SetCursorPosY(blockTop);
            ImGui.SetCursorPosX(blockLeft);
            ImGui.BeginGroup();

            if (HeaderFont.IsLoaded()) ImGui.PushFont(HeaderFont);
            Vector2 brandSz = ImGui.CalcTextSize(BrandTitle);
            Vector2 brandSz2 = ImGui.CalcTextSize(BrandNamePurple);
            float brandTotalW = brandSz.X + brandSz2.X + 4f;
            float brandX = (formW - brandTotalW) * 0.5f;
            ImGui.SetCursorPosX(brandX);
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, _uiAlpha));
            ImGui.TextUnformatted(BrandTitle);
            ImGui.PopStyleColor();
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 1f, 1f, _uiAlpha * 0.95f));
            ImGui.TextUnformatted(BrandNamePurple);
            ImGui.PopStyleColor();
            if (HeaderFont.IsLoaded()) ImGui.PopFont();

            ImGui.Dummy(new Vector2(0f, 8f));
            if (TextFont.IsLoaded()) ImGui.PushFont(TextFont);
            Vector2 subSz = ImGui.CalcTextSize("Sign in to continue");
            ImGui.SetCursorPosX((formW - subSz.X) * 0.5f);
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.48f, 0.50f, 0.56f, 0.85f * _uiAlpha));
            ImGui.TextUnformatted("Sign in to continue");
            ImGui.PopStyleColor();
            if (TextFont.IsLoaded()) ImGui.PopFont();

            float accentW = MathF.Min(60f, formW * 0.25f);
            Vector2 accentPos = ImGui.GetCursorScreenPos() + new Vector2((formW - accentW) * 0.5f, 0f);
            draw.AddRectFilledMultiColor(accentPos, accentPos + new Vector2(accentW, 1.5f),
                ImGui.GetColorU32(new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, 0f)),
                ImGui.GetColorU32(new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, 0.7f * _uiAlpha)),
                ImGui.GetColorU32(new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, 0.7f * _uiAlpha)),
                ImGui.GetColorU32(new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, 0f)));
            ImGui.Dummy(new Vector2(0f, 18f));

            ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.035f, 0.04f, 0.07f, 0.92f * _uiAlpha));
            ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new Vector4(0.055f, 0.06f, 0.10f, 0.95f * _uiAlpha));
            ImGui.PushStyleColor(ImGuiCol.FrameBgActive, new Vector4(0.07f, 0.08f, 0.12f, _uiAlpha));
            ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, 0.12f * _uiAlpha));
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 1f, 1f, _uiAlpha));
            ImGui.PushStyleColor(ImGuiCol.TextDisabled, new Vector4(0.70f, 0.72f, 0.78f, 0.85f * _uiAlpha));
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 8f);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(14f, 10f));
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1f);

            if (TextFont.IsLoaded()) ImGui.PushFont(TextFont);
            ImGui.SetNextItemWidth(formW);
            ImGui.InputTextWithHint("##UsernameInput", "Username", ref _loginUsername, 256);
            ImGui.Dummy(new Vector2(0f, fieldGap));
            ImGui.SetNextItemWidth(formW);
            ImGui.InputTextWithHint("##PasswordInput", "Password", ref _loginPassword, 256, ImGuiInputTextFlags.Password);
            if (TextFont.IsLoaded()) ImGui.PopFont();

            ImGui.PopStyleVar(3);
            ImGui.PopStyleColor(6);

            ImGui.Dummy(new Vector2(0f, btnGap));

            Vector2 btnPos = ImGui.GetCursorScreenPos();
            Vector2 btnSize = new Vector2(formW, btnH);

            uint trackBg = ImGui.GetColorU32(new Vector4(0.04f, 0.05f, 0.08f, 0.9f * _uiAlpha));
            draw.AddRectFilled(btnPos, btnPos + btnSize, trackBg, 19f);
            draw.AddRect(btnPos, btnPos + btnSize, ImGui.GetColorU32(new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, 0.2f * _uiAlpha)), 19f, ImDrawFlags.None, 1f);

            float handleRadius = 15f;
            float maxSwipe = btnSize.X - handleRadius * 2f - 8f;
            float currentX = btnPos.X + 4f + _loginSwipeProgress * maxSwipe;

            if (_loginSwipeProgress > 0.01f)
            {
                draw.AddRectFilled(btnPos + new Vector2(2f, 2f), new Vector2(currentX + handleRadius + 2f, btnPos.Y + btnSize.Y - 2f),
                    ImGui.GetColorU32(new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, 0.65f * _uiAlpha)), 17f);
            }

            string swipeTxt = _loginSwipeSuccess ? "Authenticating..." : "Swipe to Login";
            if (TextFont.IsLoaded()) ImGui.PushFont(TextFont);
            Vector2 swipeSz = ImGui.CalcTextSize(swipeTxt);
            draw.AddText(btnPos + new Vector2((btnSize.X - swipeSz.X) * 0.5f, (btnSize.Y - swipeSz.Y) * 0.5f),
                ImGui.GetColorU32(new Vector4(0.50f, 0.52f, 0.58f, _uiAlpha * 0.8f)), swipeTxt);
            if (TextFont.IsLoaded()) ImGui.PopFont();

            Vector2 handleCenter = new Vector2(currentX + handleRadius, btnPos.Y + btnSize.Y * 0.5f);
            draw.AddCircleFilled(handleCenter, handleRadius, ImGui.GetColorU32(new Vector4(1f, 1f, 1f, _uiAlpha * 0.95f)), 32);
            draw.AddCircle(handleCenter, handleRadius, ImGui.GetColorU32(new Vector4(ThemeColor.X, ThemeColor.Y, ThemeColor.Z, 0.4f * _uiAlpha)), 32, 1f);

            if (!_loginSwipeSuccess)
            {
                draw.AddTriangleFilled(handleCenter + new Vector2(-2f, -4f), handleCenter + new Vector2(-2f, 4f), handleCenter + new Vector2(5f, 0f),
                    ImGui.GetColorU32(new Vector4(0.06f, 0.06f, 0.10f, _uiAlpha)));
            }

            ImGui.SetCursorScreenPos(btnPos);
            ImGui.InvisibleButton("##swipe_login", btnSize);

            if (ImGui.IsItemActive() && !_loginSwipeSuccess && !_loginBusy)
            {
                float mouseX = ImGui.GetMousePos().X - btnPos.X - 4f - handleRadius;
                _loginSwipeProgress = Math.Clamp(mouseX / maxSwipe, 0f, 1f);
                if (_loginSwipeProgress >= 0.98f)
                {
                    _loginSwipeProgress = 1f;
                    _loginSwipeSuccess = true;
                }
            }
            else if (!_loginSwipeSuccess)
            {
                _loginSwipeProgress += (0f - _loginSwipeProgress) * ImGui.GetIO().DeltaTime * 15f;
            }

            if (_loginSwipeSuccess && !_loginBusy && !_loginOk && !string.IsNullOrEmpty(_loginResultMsg) && !_loginDone)
            {
                _loginSwipeSuccess = false;
                _loginSwipeProgress = 0f;
            }

            if (_loginSwipeSuccess && !_loginBusy && !_loginDone)
            {
                string user = _loginUsername.Trim();
                string pass = _loginPassword.Trim();
                if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
                {
                    _loginError = "Enter username and password";
                    _loginSwipeSuccess = false;
                    _loginSwipeProgress = 0f;
                }
                else
                {
                    _loginBusy = true;
                    _loginError = "";
                    Task.Run(() =>
                    {
                        try
                        {
                            BootstrapRuntime.EnsureAuthInitialized();
                            BootstrapRuntime.Auth.login(user, pass);
                            _loginOk = BootstrapRuntime.Auth.response.success;
                            _loginResultMsg = BootstrapRuntime.Auth.response.message ?? "";
                        }
                        catch (Exception ex)
                        {
                            _loginOk = false;
                            _loginResultMsg = ex.Message;
                        }
                        finally
                        {
                            _loginBusy = false;
                            _loginDone = true;
                        }
                    });
                }
            }

            string statusLine = "";
            if (_loginBusy) statusLine = "Signing in...";
            else if (!string.IsNullOrEmpty(_loginError)) statusLine = _loginError;
            else if (_loginDone && !_loginOk) statusLine = _loginResultMsg;

            if (!string.IsNullOrEmpty(statusLine))
            {
                ImGui.Dummy(new Vector2(0f, statusGap));
                if (TextFont.IsLoaded()) ImGui.PushFont(TextFont);
                Vector2 statusSz = ImGui.CalcTextSize(statusLine);
                ImGui.SetCursorPosX((formW - statusSz.X) * 0.5f);
                Vector4 statusCol = _loginBusy ? new Vector4(0.60f, 0.60f, 0.65f, _uiAlpha) : new Vector4(1f, 0.30f, 0.30f, _uiAlpha);
                ImGui.TextColored(statusCol, statusLine);
                if (TextFont.IsLoaded()) ImGui.PopFont();
            }
            ImGui.EndGroup();
        }

        public void KillProcess(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            foreach (var process in processes)
            {
                process.Kill();
                process.WaitForExit();
            }
        }

        private static byte[]? LoadFaSolidFontBytes()
        {
            if (_retainedFaSolidBytes != null)
                return _retainedFaSolidBytes;
            try
            {
                Assembly asm = typeof(ESP).Assembly;
                Stream? s = asm.GetManifestResourceStream("fa_solid_900.ttf");
                if (s == null)
                {
                    foreach (string res in asm.GetManifestResourceNames())
                    {
                        if (res.EndsWith("fa-solid-900.ttf", StringComparison.OrdinalIgnoreCase) ||
                            res.EndsWith("fa_solid_900.ttf", StringComparison.OrdinalIgnoreCase))
                        {
                            s = asm.GetManifestResourceStream(res);
                            break;
                        }
                    }
                }

                if (s != null)
                {
                    using (s)
                    using (var ms = new MemoryStream())
                    {
                        s.CopyTo(ms);
                        _retainedFaSolidBytes = ms.ToArray();
                        return _retainedFaSolidBytes;
                    }
                }
            }
            catch { }
            try
            {
                if (File.Exists(UiFonts.FaSolidPath))
                    _retainedFaSolidBytes = File.ReadAllBytes(UiFonts.FaSolidPath);
            }
            catch { }
            return _retainedFaSolidBytes;
        }

        private static string? ResolveUiBodyFontPath()
        {
            if (File.Exists(UiFonts.TahomabdPath))
                return UiFonts.TahomabdPath;

            string dir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            string[] names =
            {
                "TAHOMABD.TTF", "tahoma.ttf", "TAHOMA.TTF",
                "segoeuib.ttf", "seguisb.ttf", "seguiui.ttf", "arialbd.ttf", "arial.ttf"
            };
            foreach (string name in names)
            {
                string p = Path.Combine(dir, name);
                if (File.Exists(p))
                    return p;
            }
            return null;
        }

        protected override unsafe Task PostInitialized()
        {
            ReplaceFont(config =>
            {
                var io = ImGui.GetIO();
                _faMergedIntoTextFont = false;

                string? baseBodyPath = ResolveUiBodyFontPath();
                if (string.IsNullOrEmpty(baseBodyPath))
                    return;

                string impactPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "impact.ttf");
                string bodyPath = File.Exists(impactPath) ? impactPath : baseBodyPath;

                // UI body font — slightly larger for readable function labels
                const float uiBodyFontSize = 16f;
                TextFont = io.Fonts.AddFontFromFileTTF(bodyPath, uiBodyFontSize, null, io.Fonts.GetGlyphRangesDefault());

                // Merge extra Unicode blocks into TextFont so in-game names don't show as '?'
                unsafe
                {
                    config->MergeMode = 1;
                    config->PixelSnapH = 1;
                    io.Fonts.AddFontFromFileTTF(bodyPath, uiBodyFontSize, config, io.Fonts.GetGlyphRangesCyrillic());
                    io.Fonts.AddFontFromFileTTF(bodyPath, uiBodyFontSize, config, io.Fonts.GetGlyphRangesVietnamese());
                    io.Fonts.AddFontFromFileTTF(bodyPath, uiBodyFontSize, config, io.Fonts.GetGlyphRangesThai());
                    ushort[] devanagari = { 0x0900, 0x097F, 0 };
                    fixed (ushort* p = devanagari)
                        io.Fonts.AddFontFromFileTTF(bodyPath, uiBodyFontSize, config, (nint)p);
                    ushort[] arabic = { 0x0600, 0x06FF, 0 };
                    fixed (ushort* p = arabic)
                        io.Fonts.AddFontFromFileTTF(bodyPath, uiBodyFontSize, config, (nint)p);
                    ushort[] bengali = { 0x0980, 0x09FF, 0 };
                    fixed (ushort* p = bengali)
                        io.Fonts.AddFontFromFileTTF(bodyPath, uiBodyFontSize, config, (nint)p);
                    // Extra ranges often used in game nicknames (Tahoma/body may lack glyphs → '?' in ESP).
                    ushort[] latinExtMisc = { 0x0100, 0x024F, 0x2600, 0x26FF, 0x2700, 0x27BF, 0xFE00, 0xFE0F, 0 };
                    fixed (ushort* p = latinExtMisc)
                        io.Fonts.AddFontFromFileTTF(bodyPath, uiBodyFontSize, config, (nint)p);
                    // Segoe UI (Windows): broad fallback for symbols / CJK / extended Latin when body font lacks glyphs.
                    string segoeUi = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "segoeui.ttf");
                    if (File.Exists(segoeUi))
                    {
                        io.Fonts.AddFontFromFileTTF(segoeUi, uiBodyFontSize, config, io.Fonts.GetGlyphRangesDefault());
                        // Common CJK nicknames (avoid Japanese/Korean full ranges here — very large atlas).
                        io.Fonts.AddFontFromFileTTF(segoeUi, uiBodyFontSize, config, io.Fonts.GetGlyphRangesChineseSimplifiedCommon());
                    }
                    config->MergeMode = 0;
                    config->PixelSnapH = 0;
                }

                ushort[] faRange = { 0xE000, 0xF900, 0 };
                byte[]? faBytes = EmbeddedFonts.FontLucide;
                if (faBytes != null && faBytes.Length > 0)
                {
                    unsafe
                    {
                        fixed (byte* fp = faBytes)
                        fixed (ushort* gr = faRange)
                        {
                            TabIconFont = io.Fonts.AddFontFromMemoryTTF((nint)fp, faBytes.Length, 20f, null, (nint)gr);

                            config->MergeMode = 1;
                            config->PixelSnapH = 1;
                            io.Fonts.AddFontFromMemoryTTF((nint)fp, faBytes.Length, uiBodyFontSize, config, (nint)gr);
                            config->MergeMode = 0;
                            config->PixelSnapH = 0;
                        }
                    }
                    _faMergedIntoTextFont = true;
                }

                if (File.Exists(impactPath))
                {
                    HeaderFont = io.Fonts.AddFontFromFileTTF(impactPath, 22f, null, io.Fonts.GetGlyphRangesDefault());
                    LebelFont = io.Fonts.AddFontFromFileTTF(impactPath, 20f, null, io.Fonts.GetGlyphRangesDefault());
                }
                else
                {
                    HeaderFont = io.Fonts.AddFontFromFileTTF(baseBodyPath, 22f, null, io.Fonts.GetGlyphRangesDefault());
                    LebelFont = io.Fonts.AddFontFromFileTTF(baseBodyPath, 20f, null, io.Fonts.GetGlyphRangesDefault());
                }
                CustomNotification.TextFont = TextFont;

                string segoeUiPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "segoeui.ttf");
                if (File.Exists(segoeUiPath))
                {
                    unsafe
                    {
                        EspNameFont = io.Fonts.AddFontFromFileTTF(segoeUiPath, uiBodyFontSize, null, io.Fonts.GetGlyphRangesDefault());
                        config->MergeMode = 1;
                        config->PixelSnapH = 1;
                        io.Fonts.AddFontFromFileTTF(segoeUiPath, uiBodyFontSize, config, io.Fonts.GetGlyphRangesCyrillic());
                        io.Fonts.AddFontFromFileTTF(segoeUiPath, uiBodyFontSize, config, io.Fonts.GetGlyphRangesVietnamese());
                        io.Fonts.AddFontFromFileTTF(segoeUiPath, uiBodyFontSize, config, io.Fonts.GetGlyphRangesThai());
                        // Fallback fonts for extended characters
                        string seguiSymPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "seguisym.ttf");
                        string msyhPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "msyh.ttc");
                        string malgunPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "malgun.ttf");
                        string msgothicPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "msgothic.ttc");

                        if (File.Exists(msyhPath))
                            io.Fonts.AddFontFromFileTTF(msyhPath, uiBodyFontSize, config, io.Fonts.GetGlyphRangesChineseSimplifiedCommon());
                        else
                            io.Fonts.AddFontFromFileTTF(segoeUiPath, uiBodyFontSize, config, io.Fonts.GetGlyphRangesChineseSimplifiedCommon());

                        if (File.Exists(malgunPath))
                            io.Fonts.AddFontFromFileTTF(malgunPath, uiBodyFontSize, config, io.Fonts.GetGlyphRangesKorean());
                        
                        if (File.Exists(msgothicPath))
                            io.Fonts.AddFontFromFileTTF(msgothicPath, uiBodyFontSize, config, io.Fonts.GetGlyphRangesJapanese());

                        ushort[] greek = { 0x0370, 0x03FF, 0 };
                        fixed (ushort* sym = greek)
                            io.Fonts.AddFontFromFileTTF(segoeUiPath, uiBodyFontSize, config, (nint)sym);

                        ushort[] arabic = { 0x0600, 0x06FF, 0 };
                        fixed (ushort* sym = arabic)
                            io.Fonts.AddFontFromFileTTF(segoeUiPath, uiBodyFontSize, config, (nint)sym);

                        ushort[] devanagari = { 0x0900, 0x097F, 0 };
                        fixed (ushort* sym = devanagari)
                            io.Fonts.AddFontFromFileTTF(segoeUiPath, uiBodyFontSize, config, (nint)sym);

                        if (!string.IsNullOrEmpty(baseBodyPath))
                            io.Fonts.AddFontFromFileTTF(baseBodyPath, uiBodyFontSize, config, io.Fonts.GetGlyphRangesDefault());

                        ushort[] nameSymbols = { 0x00A0, 0x024F, 0x2000, 0x2BFF, 0x2600, 0x27BF, 0xFE00, 0xFE0F, 0 };
                        fixed (ushort* sym = nameSymbols)
                        {
                            if (File.Exists(seguiSymPath))
                                io.Fonts.AddFontFromFileTTF(seguiSymPath, uiBodyFontSize, config, (nint)sym);
                            else
                                io.Fonts.AddFontFromFileTTF(segoeUiPath, uiBodyFontSize, config, (nint)sym);
                        }

                        config->MergeMode = 0;
                        config->PixelSnapH = 0;
                    }
                }
                else
                {
                    EspNameFont = TextFont;
                }
            });

            return base.PostInitialized();
        }

    }

    internal class TimerManager
    {
        private bool isRunning = false;
        private bool timerFinished = false;
        private bool inactivePending = false;
        private uint lastMatchAddress = 0;
        private DateTime startTime;
        private DateTime inactiveSince;
        private const int TimerDurationSeconds = 180; // 3 minutes

        public void Update(uint currentMatch, uint matchStatus, bool featureEnabled)
        {
            if (!featureEnabled)
            {
                StopTimer();
                return;
            }

            bool inActiveMatch = (currentMatch != 0 && matchStatus == 1);

            if (!inActiveMatch)
            {
                DateTime now = DateTime.Now;
                if (!inactivePending)
                {
                    inactivePending = true;
                    inactiveSince = now;
                }
                else if ((now - inactiveSince).TotalSeconds >= 2)
                {
                    EndMatchSession();
                }
                return;
            }

            inactivePending = false;

            // New match instance (new pointer) — start a fresh 3:00 countdown.
            if (currentMatch != lastMatchAddress)
            {
                lastMatchAddress = currentMatch;
                StartTimer();
                return;
            }

            if (!isRunning)
                return;

            if (!timerFinished && GetRemainingSeconds() <= 0)
                timerFinished = true;
        }

        private static float TimerLerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        private static Vector4 TimerLerpVec4(Vector4 a, Vector4 b, float t)
        {
            return new Vector4(
                TimerLerp(a.X, b.X, t),
                TimerLerp(a.Y, b.Y, t),
                TimerLerp(a.Z, b.Z, t),
                TimerLerp(a.W, b.W, t)
            );
        }

        public unsafe void DrawTimer(float screenWidth, float screenHeight, bool featureEnabled, Vector2 emulatorScreenOffset)
        {
            if (!featureEnabled || !isRunning || lastMatchAddress == 0)
                return;

            var drawList = ImGui.GetForegroundDrawList();
            if (drawList.NativePtr == null)
                return;

            int remaining = GetRemainingSeconds();
            if (remaining < 0)
                remaining = 0;

            if (remaining == 0)
                timerFinished = true;

            const float DevTextY = 80.0f;
            const float GapBelowDev = 30.0f;
            const float TextScale = 1.20f;

            string text;
            if (!timerFinished)
            {
                int mm = remaining / 60;
                int ss = remaining % 60;
                text = $"{mm}:{ss:D2}";
            }
            else
            {
                text = "VALID MATCH";
            }

            ImFontPtr font = ImGui.GetFont();
            if (!font.IsLoaded() || string.IsNullOrEmpty(text))
                return;

            float fontSize = font.FontSize * TextScale;
            Vector2 textSize = font.CalcTextSizeA(fontSize, float.MaxValue, -1.0f, text);

            Vector2 basePos = new Vector2(screenWidth * 0.5f, DevTextY + GapBelowDev);
            Vector2 textPos = new Vector2(basePos.X - textSize.X * 0.5f, basePos.Y - 18.0f);

            Vector4 redBase = new Vector4(0.82f, 0.12f, 0.16f, 1.0f);
            Vector4 greenBase = new Vector4(0.02f, 0.82f, 0.40f, 1.0f);
            Vector4 whiteHi = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

            float t = (float)ImGui.GetTime();
            float phaseRed = (t * 0.9f) % 1.0f;
            float phaseGreen = (t * 1.35f) % 1.0f;

            float charX = textPos.X;
            for (int i = 0; i < text.Length; i++)
            {
                string ch = text[i].ToString();
                Vector2 chSize = font.CalcTextSizeA(fontSize, float.MaxValue, -1.0f, ch);

                float centerX = charX + chSize.X * 0.5f;
                float denom = (textSize.X > 1.0f) ? textSize.X : 1.0f;
                float rel = (centerX - textPos.X) / denom;

                float phase = timerFinished ? phaseGreen : phaseRed;
                float g = (rel + phase) % 1.0f;
                float band = 1.0f - Math.Abs(g - 0.5f) * 2.0f;
                band = Math.Clamp(band, 0.0f, 1.0f);

                Vector4 baseColor = timerFinished ? greenBase : redBase;
                float whiteMix = timerFinished ? (band * 0.93f) : (band * 0.82f);
                Vector4 col = TimerLerpVec4(baseColor, whiteHi, whiteMix);

                uint glowCol = timerFinished 
                    ? ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 170f / 255f)) 
                    : ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 220f / 255f));

                Vector2 chPos = new Vector2(charX, textPos.Y);

                if (timerFinished)
                {
                    drawList.AddText(font, fontSize, new Vector2(chPos.X - 1.0f, chPos.Y) + emulatorScreenOffset, glowCol, ch);
                    drawList.AddText(font, fontSize, new Vector2(chPos.X + 1.0f, chPos.Y) + emulatorScreenOffset, glowCol, ch);
                    drawList.AddText(font, fontSize, new Vector2(chPos.X, chPos.Y - 1.0f) + emulatorScreenOffset, glowCol, ch);
                    drawList.AddText(font, fontSize, new Vector2(chPos.X, chPos.Y + 1.0f) + emulatorScreenOffset, glowCol, ch);
                }
                drawList.AddText(font, fontSize, new Vector2(chPos.X + 1.0f, chPos.Y + 1.0f) + emulatorScreenOffset, glowCol, ch);

                uint mainCol = ImGui.ColorConvertFloat4ToU32(col);
                drawList.AddText(font, fontSize, chPos + emulatorScreenOffset, mainCol, ch);

                charX += chSize.X;
            }
        }

        public void StopForReset(bool featureEnabled)
        {
            if (!featureEnabled)
                StopTimer();
        }

        private void StartTimer()
        {
            startTime = DateTime.Now;
            isRunning = true;
            timerFinished = false;
            inactivePending = false;
        }

        private void EndMatchSession()
        {
            isRunning = false;
            timerFinished = false;
            lastMatchAddress = 0;
            inactivePending = false;
        }

        private void StopTimer()
        {
            EndMatchSession();
        }

        private int GetRemainingSeconds()
        {
            if (!isRunning)
                return TimerDurationSeconds;
            if (timerFinished)
                return 0;

            double elapsed = (DateTime.Now - startTime).TotalSeconds;
            return TimerDurationSeconds - (int)elapsed;
        }
    }
}