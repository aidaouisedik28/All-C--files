using System.Media;
using System.Reflection;

namespace AotForms
{
    internal static class Config
    {

        internal static AimBotType AimBotType;
        internal static bool EnemyPullEnabled = false;
        internal static bool AimBot = false;
        internal static Keys AimbotKey = Keys.LButton;
        internal static int AimFovCircle = 500;
        internal static float test = 0.400f;
        internal static int SmoothnessX = 0;
        internal static int UppDistence = 200;
        internal static bool TeleMe = false;
        internal static bool FastFire = false;
        internal static bool SPAWN = false;
        internal static bool hamba = false;
        internal static int AimbotSmoothness = 0;
        internal static bool SilentAim1 = false;
        internal static bool Speed = false;
        internal static int EspLineThickNess = 2;
        public static float GlowRadius = 9;
        public static float GlowRadius1 = 10;
        public static float GlowRadiusdot = 10;
        public static float FeatherAmount = 2f;
        internal static bool enmyteleport2 = false;
        public static float GlowOpacity = 0.02f;
        public static float dotRadius = 3.0f;
        internal static bool IgnoreKnocked = false;
        internal static bool NoCache = false;
        internal static bool StreamMode = false;
        internal static bool ESPLine = false;
        internal static bool SniperSwitch = false;
        internal static bool SniperSwitchOff = false;
        internal static bool FASTRELOAD = false;
        internal static bool Wallhack = false;
        internal static Color SkeletonColor = Color.White;
        internal static Color ESPLineColor = Color.White;
        internal static Color ESPFullBoxColor = Color.White;
        internal static Color MocoColor = Color.White;
        internal static bool SilentAim = false;
        internal static bool SilentAim2 = false;
        //internal static bool UpPlayer = false;
        internal static bool Tele = false;
        internal static bool ESPWeapon = false;
        internal static bool ESPWeaponIcon = false;

        internal static Color ESPCorneredBoxColor = Color.White;
        internal static Color ESPFillBoxColor = Color.White;
        internal static bool espbg = false;
        internal static bool ESPMoco = false;
        internal static bool ESPCorneredBox = false;
        internal static bool ESPFillBox = false;
        internal static bool ESPFullBox = false;
        internal static bool Playerup = false;
        internal static int playerupdelay = 25;
        internal static bool UpPlayer = false;
        internal static bool enmyteleport = false;
        internal static float fly = 0.150f;
        public static float FontSize = 18f;

        // Flag to toggle the particle effect on/off
        public static bool PARTICLE_OFF = false;

        // Keybind-related variables
        public static bool WaitingForKeybind = false;      // Waiting for user to set a keybind
        public static Keys AimBotKey = Keys.None;          // The selected key for toggling AimBot
        public static string AimBotKeyLabel = "None";      // Label for the selected key
        public static bool KeyAlreadyPressed = false;      // Prevent repeated toggling while holding a key

        public static Keys SpeedHackKey = Keys.None;     // The selected key for toggling SpeedHack
        public static string SpeedHackKeyLabel = "None"; // Label for the selected SpeedHack key

        // Flag to toggle particle color change
        public static bool ColorChange = false;
        public static string FontName = "Arial";
        //  public static float FontSize = 30f;

        //internal static Color ESPLineColor = Color.White;
        internal static bool ESPBox = false;
        internal static Color ESPBoxColor = Color.White;
        internal static bool ESPName = false;
        internal static Color ESPNameColor = Color.White;
        internal static bool ESPHealth = false;
        internal static bool ESPDistance = false;
        internal static bool EspPopupMessage = false;
        internal static bool Cross = false;
        internal static float CrosshairBim = 2000;
        internal static Color CrossColor = Color.White;
        internal static bool NoRecoil = false;
        public static bool Skeleton = false;
        internal static string ESPMode = "1";

        internal static bool enableAimBotV2 = false;
        internal static bool AimBotRageV2 = false;

        internal static float AimBotSmooth = 0f;
        internal static bool enableAimBot = false;
        internal static bool AimBotRage = false;
        internal static TargetingMode TargetingMode = TargetingMode.ClosestToCrosshair;
        internal static float AimFov = 200f;


        internal static Color NameCheat = Color.Cyan;
        internal static bool ESPHealthText = false;
        internal static Color ESPHealthColor = Color.Green;
        internal static bool ESPSkeleton = false;
        internal static Color ESPSkeletonColor = Color.White;
        internal static bool FOVEnabled = false;

        internal static Color FOVColor = Color.White;
        internal static bool ESPCrosshair = false;
        internal static Color ESPCrosshairColor = Color.Red;
        internal static bool ESPCrosshairRGB = false;
        public static float CrosshairSize = 15f; // Size in pixels
        internal static float ESPCrosshairThickness = 1.2f;
        //glowing line
        internal static bool GlowingLines = false;

        // Crosshair glow and rotation controls
        internal static bool CrosshairGlowing = false;
        internal static float CrosshairGlowRadius = 10f;
        internal static float CrosshairFeatherAmount = 1.5f;
        internal static float CrosshairRotationSpeed = 30f; // RPM (rotations per minute)

        internal static LinePosition ESPLinePosition = LinePosition.Top;
        // New properties from second codebase
        internal static bool ESP3DBox = false;
        internal static bool ESPBG = false;
        internal static float espran = 200f; // ESP Range

        internal static int AimBotMaxDistance = 150;
        internal static bool sound = false;
        internal static bool EspBottom = false;
        internal static bool EspUp = false;
        internal static bool ESPLinegr = false;
        internal static bool ESPLinegrf = false;
        internal static bool AimbotVisible;
        internal static bool Crosshair;
        internal static Color KnockedlineClr = Color.Red;
        internal static bool RGB = false;
        internal static bool CrossHair;
        internal static Color CrossHairColor = Color.White;
        internal static bool EnemyCount;
        internal static Color EnemyCountColor = Color.White;
        public static Color CrosshairColor = Color.White;
        internal static bool CrosshairEnabled;




        internal static bool AimbotMouseControl = false;
        internal static int HeadBoneId = 8;
        internal static bool AimBotVisible = false;
        internal static bool AimbotVisible2 = false;
        internal static bool shakekill = false;
        internal static bool GhostLagEnabled = false;
        internal static bool SilentAimMax = false;
        internal static bool UpdateEntities = false;
        internal static bool teli = false;
        internal static bool MouseLeftButtonEnabled = true;
        internal static bool SILENT = false;
        internal static bool AimSilentV2 = false; // Enhanced silent aim v2
        internal static uint pomba = 0x454;

        // RAGE Aimbot Configuration
        internal static int MouseLockStrength = 350; // قوة القفل على الماوس (100 - 1000)
        internal static bool spwan = false;
        internal static bool ShowRageFOV = false; // Show FOV circle when RAGE is active
        internal static float RageFOVSize = 100f; // RAGE FOV circle size
        internal static float cameraVal = 1.0f;
        internal static Color ESPWeaponColor = Color.White;

        internal static float TeleSpeed = 200f;
        internal static float TeleportRange = 500f;
        internal static float siMaxDistance = 500f;

        internal static float TeleportHeightOffset = 10f;

        internal static bool fastfireactivate = false;

        internal static bool TeleportMarkEnabled = false;

        //cheak if enable or not to stop activating again and again
        internal static bool fastfire = false;

        // Fast Reload
        internal static bool FastReload = false;

        // Customizable hotkeys
        internal static Keys ToggleSilentAimKey = Keys.None;
        internal static Keys ToggleFastFireKey = Keys.None;
        internal static Keys ToggleAimbotKey = Keys.None;
        internal static Keys ToggleTelekillKey = Keys.None;
        internal static Keys ToggleUpPlayerKey = Keys.None;
        internal static Keys ToggleGhostLagKey = Keys.None;
        internal static Keys ToggleTeleportV2Key = Keys.None;



        internal static float AimStrength = 0.7f; // 0..1 duty cycle of active frames
        internal static float AimBotFov = 200f; // separate aimbot FOV
        internal static bool UseAimFovForAimbot = false; // tie aimbot FOV to visual AimFov


        internal static int EnemyPullTickMs = 6;
        internal static float EnemyPullMaxDistance = 500f; // Increased max distance for brutal pull
        internal static float EnemyPullStrength = 1.0f; // Increased strength for brutal pull

        // Weapon Swap settings
        internal static bool WeaponSwapEnabled = false;

        // TeleportV2 settings
        internal static bool TeleportV2 = false;

        // Forward Teleport settings
        internal static bool EnableForwardTeleport = false;

        // Hotkeys for new features
        internal static Keys ToggleEnemyPullKey = Keys.None;
        internal static Keys ToggleForwardTeleportKey = Keys.None;

        // FlyHack settings
        internal static bool FlyHackEnabled = false;
        internal static float FlyHeight = 50f; // Default fly height
        internal static Keys ToggleFlyHackKey = Keys.None;

        // Down Player settings
        internal static bool DownPlayer = false;
        internal static bool down = false;
        internal static float DownSpeed = 2.0f;
        internal static float downSpeed = 0.8f;
        internal static Keys ToggleDownPlayerKey = Keys.None;

        // Speed X99 settings
        internal static bool SpeedX99Enabled = false;
        internal static Keys ToggleSpeedX99Key = Keys.None;

        // Wallhack settings
        internal static bool WallhackEnabled = false;
        internal static Keys ToggleWallhackKey = Keys.None;

        // OBB settings
        internal static bool OBBEnabled = false;

        // No Reload settings
        internal static bool NoReloadEnabled = false;
        internal static Keys ToggleNoReloadKey = Keys.None;

        // Config.cs
        internal static bool TeleportMap = false;
        internal static int TeleportIndex = 0;

        internal static bool TeleportEnabled;

        internal static bool AimBotRageFixWall = false;

        public static bool FixEsp { get; internal set; }


        internal static int PullEnemyBone = 0;
        internal static float maxDistance = 300.0f;
        internal static float fov = 100.0f;

        internal static bool MagnetPullEnable = false;
        internal static int MagnetPullKey = 0;

        internal static bool TeleportBaseEnable = false;
        internal static float teleportbasedistance = 500.0f;
        internal static int TeleportKey = 0;

        internal static bool TeleKillEnable = false;
        internal static float telekilldistance = 500.0f;
        internal static int TeleKillKey = 0;

        internal static bool SpeedTeleportEnable = false;
        internal static float SpeedTeleportRange = 500.0f;
        internal static float SpeedTeleportSpeed = 200.0f;
        internal static int SpeedTeleportKey = 0;

        internal static bool ClimbUpEnable = false;
        internal static int ClimbUpKey = 0;

        internal static bool DiveKillEnable = false;
        internal static int DiveKillKey = 0;

        internal static bool DownPlayerEnable = false;
        internal static float DownPlayerSpeed = 0.5f;

        internal static bool EnemyPullChestEnable = false;

        internal static bool Aimlock = false;
        internal static int AimLockKey = 0;

        internal static bool WallhackFlyEnable = false;
        internal static int WallhackFlyKey = 0;

        public static void Notif()
        {


            if (!sound)
            {
                // Replace "YourNamespace.YourMP3File.mp3" with the correct namespace and file name
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Client.clicksound.wav");

                if (stream != null)
                {
                    using (SoundPlayer player = new SoundPlayer(stream))
                    {
                        player.Play();
                    }
                }
                else
                {

                }
            }

            else
            {


            }
        }

    }
    public enum AimBotType
    {
        Silent,
        Rage
    }
    public enum LinePosition
    {
        Top,
        Center,
        Bottom
    }

    public enum TargetingMode
    {
        ClosestToCrosshair,
        Target360,
        ClosestToPlayer,
        LowestHealth,
    }
}