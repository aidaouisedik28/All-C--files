using ImGuiNET;
using ImGuiNET;
using Memory;
using Reborn;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using static AotForms.WinAPI;
using Color = System.Drawing.Color;
using Size = System.Drawing.Size;
namespace AotForms
{
    internal class ESP : ClickableTransparentOverlay.Overlay
    {
        IntPtr hWnd;
        IntPtr HDPlayer;
        private static Form1 settingsForm = null;

        public static void SetSettingsForm(Form1 form)
        {
            settingsForm = form;
        }

        private void RenderUI()
        {
            // هنا لاحقًا تحط رسم ESP أو UI
        }

        private const int ESP_DELAY = 8;
        private const short DefaultMaxHealth = 200;
        private Vector4 lineColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private Vector4 boxColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private Vector4 fillboxColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private Vector4 skeletonColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private Vector4 WeaponColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        internal static float GlowRadius = 20f;
        internal static float FeatherAmount = 2f;
        private uint cyanColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0.8f, 1f, 1f));
        bool showWindow = false;

        private ConcurrentDictionary<int, EntityRenderData> processedEntities = new();
        private Task entityProcessingTask;

        private struct EntityRenderData
        {
            public Vector2 headScreenPos;
            public Vector2 bottomScreenPos;
            public float Distance;
            public bool IsValid;
        }

        private bool isProcessing = false;
        private float progress = 0.0f;
        private Random particleRandom = new Random();
        private DateTime lastTriangleUpdate = DateTime.UtcNow;
        private static bool fontLoaded = false;
        private static bool Running = true;
        private CancellationTokenSource entityCts;

        public static void Stop()
        {
            Running = false;
        }

        private void StartEntityProcessing()
        {
            entityCts = new CancellationTokenSource();
            entityProcessingTask = Task.Run(async () =>
            {
                const int IDLE_DELAY = 12;
                const int ACTIVE_DELAY = 8;
                const int MAX_DISTANCE = 200;

                while (!entityCts.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (!Core.HaveMatrix || Core.Entities.Count == 0)
                        {
                            await Task.Delay(IDLE_DELAY, entityCts.Token);
                            continue;
                        }

                        var newEntities = new Dictionary<int, EntityRenderData>();

                        foreach (var entity in Core.Entities.Values)
                        {
                            if (entity.IsDead || !entity.IsKnown) continue;

                            float dist = Vector3.Distance(Core.LocalMainCamera, entity.Head);
                            if (dist > MAX_DISTANCE) continue;

                            var head = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                            var bottom = W2S.WorldToScreen(Core.CameraMatrix, entity.Root, Core.Width, Core.Height);

                            if (head.X <= 0 || head.Y <= 0 || bottom.X <= 0 || bottom.Y <= 0) continue;

                            newEntities[entity.GetHashCode()] = new EntityRenderData
                            {
                                headScreenPos = head,
                                bottomScreenPos = bottom,
                                Distance = dist,
                                IsValid = true
                            };
                        }

                        processedEntities = new ConcurrentDictionary<int, EntityRenderData>(newEntities);
                    }
                    catch { }

                    await Task.Delay(ACTIVE_DELAY, entityCts.Token);
                }
            }, entityCts.Token);
        }

        protected override unsafe void Render()
        {
            CreateHandle();

            // Toggle Settings Form (Insert key)
            if (GetAsyncKeyState(Keys.Insert) < 0)
            {
                Thread.Sleep(200);
                if (settingsForm == null || settingsForm.IsDisposed)
                {
                    settingsForm = new Form1();
                }

                if (settingsForm.Visible)
                {
                    settingsForm.Hide();
                }
                else
                {
                    settingsForm.Show();
                }
            }

            // Exit Application (Delete key)
            if (GetAsyncKeyState(Keys.Delete) < 0)
            {
                Environment.Exit(0);
            }

            // ===== HOTKEY TOGGLES - DEFINITIONS ONLY =====
            if (Config.ToggleSilentAimKey != Keys.None && GetAsyncKeyState(Config.ToggleSilentAimKey) < 0)
            {
                Config.SILENT = !Config.SILENT;
                Thread.Sleep(200);
            }

            if (Config.ToggleFastFireKey != Keys.None && GetAsyncKeyState(Config.ToggleFastFireKey) < 0)
            {
                Config.fastfireactivate = !Config.fastfireactivate;
                Thread.Sleep(300);
            }

            if (Config.ToggleAimbotKey != Keys.None && GetAsyncKeyState(Config.ToggleAimbotKey) < 0)
            {
                Config.AimBotVisible = !Config.AimBotVisible;
                Thread.Sleep(200);
            }

            if (Config.ToggleTelekillKey != Keys.None && GetAsyncKeyState(Config.ToggleTelekillKey) < 0)
            {
                Config.teli = !Config.teli;
                Thread.Sleep(200);
            }

            if (Config.ToggleUpPlayerKey != Keys.None && GetAsyncKeyState(Config.ToggleUpPlayerKey) < 0)
            {
                Config.UpPlayer = !Config.UpPlayer;
                Thread.Sleep(200);
            }

            if (Config.ToggleEnemyPullKey != Keys.None && GetAsyncKeyState(Config.ToggleEnemyPullKey) < 0)
            {
                Config.EnemyPullEnabled = !Config.EnemyPullEnabled;
                Thread.Sleep(200);
            }

            if (Config.ToggleTeleportV2Key != Keys.None && GetAsyncKeyState(Config.ToggleTeleportV2Key) < 0)
            {
                Config.TeleportV2 = !Config.TeleportV2;
                Thread.Sleep(300);
            }

            if (Config.ToggleGhostLagKey != Keys.None && GetAsyncKeyState(Config.ToggleGhostLagKey) < 0)
            {
                Config.GhostLagEnabled = !Config.GhostLagEnabled;
                Thread.Sleep(300);
            }

            if (Config.ToggleForwardTeleportKey != Keys.None && GetAsyncKeyState(Config.ToggleForwardTeleportKey) < 0)
            {
                Config.EnableForwardTeleport = !Config.EnableForwardTeleport;
                Thread.Sleep(200);
            }

            if (Config.ToggleDownPlayerKey != Keys.None && GetAsyncKeyState(Config.ToggleDownPlayerKey) < 0)
            {
                Config.DownPlayer = !Config.DownPlayer;
                Thread.Sleep(300);
            }

            if (Config.ToggleWallhackKey != Keys.None && GetAsyncKeyState(Config.ToggleWallhackKey) < 0)
            {
                Config.WallhackEnabled = !Config.WallhackEnabled;
                Thread.Sleep(300);
            }

            if (Config.ToggleSpeedX99Key != Keys.None && GetAsyncKeyState(Config.ToggleSpeedX99Key) < 0)
            {
                Config.SpeedX99Enabled = !Config.SpeedX99Enabled;
                Thread.Sleep(300);
            }

            if (showWindow)
            {
                RenderUI();
            }

            string windowName = "Overlay";
            hWnd = FindWindow(null!, windowName);
            HDPlayer = FindWindow("BlueStacksApp", null!);

            if (hWnd != IntPtr.Zero)
            {
                long extendedStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
                SetWindowLong(hWnd, GWL_EXSTYLE, (extendedStyle | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);
            }

            if (!Core.HaveMatrix) return;

            if (Config.FOVEnabled)
            {
                DrawGlowingFOVCircle(Config.AimFov);
            }

            if (Config.enableAimBot && Config.AimBotRage && Config.ShowRageFOV)
            {
                DrawRageFOVCircle(Config.RageFOVSize);
            }

            RenderEntities();
        }

        private void RenderEntities()
        {
            if (Config.StreamMode) return;

            var drawList = ImGui.GetBackgroundDrawList();

            foreach (var entity in Core.Entities.Values)
            {
                if (!processedEntities.TryGetValue(entity.GetHashCode(), out var entityData) || !entityData.IsValid)
                    continue;

                float CornerHeight = Math.Abs(entityData.headScreenPos.Y - entityData.bottomScreenPos.Y);
                float CornerWidth = CornerHeight * 0.65f;

                // ═══════════════════════════════════════════════════════
                // 1. شريط الصحة العمودي على اليمين
                // ═══════════════════════════════════════════════════════
                if (Config.ESPHealth)
                {
                    float hp = entity.Health;
                    if (hp < 0) hp = 0;
                    if (hp > 100) hp = 100;
                    float hpPercent = hp / 100f;

                    // موقع الشريط: على يمين الـ Box
                    float barWidth = 5f;
                    float barX = entityData.headScreenPos.X + CornerWidth / 2f + 8f;
                    float barTopY = entityData.headScreenPos.Y;
                    float barBottomY = entityData.bottomScreenPos.Y;
                    float barHeight = barBottomY - barTopY;
                    float filledHeight = barHeight * hpPercent;

                    // خلفية الشريط
                    drawList.AddRectFilled(
                        new Vector2(barX, barTopY),
                        new Vector2(barX + barWidth, barBottomY),
                        ImGui.ColorConvertFloat4ToU32(new Vector4(0.1f, 0.1f, 0.1f, 0.8f))
                    );

                    // الصحة المملوءة
                    Vector4 healthColor;
                    if (hpPercent > 0.6f)
                        healthColor = new Vector4(0.1f, 0.95f, 0.2f, 1f);
                    else if (hpPercent > 0.3f)
                        healthColor = new Vector4(1f, 0.9f, 0.2f, 1f);
                    else
                        healthColor = new Vector4(1f, 0.2f, 0.2f, 1f);

                    drawList.AddRectFilled(
                        new Vector2(barX, barBottomY - filledHeight),
                        new Vector2(barX + barWidth, barBottomY),
                        ImGui.ColorConvertFloat4ToU32(healthColor)
                    );

                    // إطار الشريط
                    drawList.AddRect(
                        new Vector2(barX, barTopY),
                        new Vector2(barX + barWidth, barBottomY),
                        ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.9f)),
                        0f, ImDrawFlags.None, 1.2f
                    );

                    // رقم الصحة
                    string hpText = $"{(int)hp}";
                    Vector2 hpTextSize = ImGui.CalcTextSize(hpText);
                    Vector2 hpTextPos = new Vector2(
                        barX + barWidth + 4f,
                        barBottomY - filledHeight - hpTextSize.Y / 2f
                    );
                    drawList.AddText(hpTextPos, ColorToUint32(Color.White), hpText);
                }

                // ═══════════════════════════════════════════════════════
                // 2. الخط
                // ═══════════════════════════════════════════════════════
                if (Config.ESPLine)
                {
                    Vector2 endPos = entityData.headScreenPos;
                    Vector2 lineStart;

                    switch (Config.ESPLinePosition)
                    {
                        case LinePosition.Top:
                            lineStart = new Vector2(Core.Width / 2f, 0f);
                            break;
                        case LinePosition.Center:
                            lineStart = new Vector2(Core.Width / 2f, Core.Height / 2f);
                            break;
                        case LinePosition.Bottom:
                            lineStart = new Vector2(Core.Width / 2f, Core.Height - 10f);
                            endPos = entityData.bottomScreenPos;
                            break;
                        default:
                            lineStart = new Vector2(Core.Width / 2f, 0f);
                            break;
                    }

                    uint lineColor = entity.IsKnocked
                        ? ColorToUint32(Color.Red)
                        : ColorToUint32(Config.ESPLineColor);

                    if (Config.GlowingLines)
                    {
                        DrawGlowLine(lineStart, endPos, lineColor, 1f, GlowRadius, FeatherAmount);
                    }
                    else
                    {
                        drawList.AddLine(lineStart, endPos, lineColor, 1f);
                    }
                }

                // ═══════════════════════════════════════════════════════
                // 3. Fill Box - أحمق أكثر
                // ═══════════════════════════════════════════════════════
                // ═══════════════════════════════════════════════════════
                if (Config.ESPFillBox)
                {
                    uint fillBoxColor = ColorToUint32(Color.FromArgb(
                        120, // شفافية واضحة وليست شبه شفافة
                        Config.ESPFillBoxColor.R, // الأحمر كما هو
                        Config.ESPFillBoxColor.G, // الأخضر كما هو
                        Config.ESPFillBoxColor.B  // الأزرق كما هو
                    ));

                    DrawFilledBox(
                            entityData.headScreenPos.X - (CornerWidth / 2),
                            entityData.headScreenPos.Y,
                            CornerWidth,
                            CornerHeight,
                            fillBoxColor
                        );
                }

                // ═══════════════════════════════════════════════════════
                // 4. الصندوق
                // ═══════════════════════════════════════════════════════
                if (Config.ESPBox)
                {
                    uint boxColor = ColorToUint32(Config.ESPBoxColor);
                    DrawCorneredBox(
                        entityData.headScreenPos.X - (CornerWidth / 2),
                        entityData.headScreenPos.Y,
                        CornerWidth,
                        CornerHeight,
                        boxColor,
                        1f
                    );
                }

                // ═══════════════════════════════════════════════════════
                // 5. السلاح فوق الاسم
                // ═══════════════════════════════════════════════════════
                float yOffset = -35f;

                if (Config.ESPWeapon && !string.IsNullOrEmpty(entity.WeaponName))
                {
                    string weaponIcon = "";

                    if (entity.WeaponName.ToLower().Contains("katana") || entity.WeaponName.ToLower().Contains("sword"))
                        weaponIcon = "⚔️";
                    else if (entity.WeaponName.ToLower().Contains("box") || entity.WeaponName.ToLower().Contains("supply"))
                        weaponIcon = "📦";
                    else if (entity.WeaponName.ToLower().Contains("rifle") || entity.WeaponName.ToLower().Contains("ak") || entity.WeaponName.ToLower().Contains("m4"))
                        weaponIcon = "🔫";
                    else if (entity.WeaponName.ToLower().Contains("sniper") || entity.WeaponName.ToLower().Contains("awm"))
                        weaponIcon = "🎯";
                    else if (entity.WeaponName.ToLower().Contains("pistol") || entity.WeaponName.ToLower().Contains("glock"))
                        weaponIcon = "🔫";
                    else if (entity.WeaponName.ToLower().Contains("grenade") || entity.WeaponName.ToLower().Contains("bomb"))
                        weaponIcon = "💣";
                    else if (entity.WeaponName.ToLower().Contains("medkit") || entity.WeaponName.ToLower().Contains("health"))
                        weaponIcon = "💊";
                    else
                        weaponIcon = "🔪";

                    string weaponText = $"{weaponIcon} {entity.WeaponName}";
                    Vector2 weaponSize = ImGui.CalcTextSize(weaponText);
                    Vector2 weaponPos = new Vector2(
                        entityData.headScreenPos.X - weaponSize.X / 2f,
                        entityData.headScreenPos.Y + yOffset
                    );

                    // خلفية السلاح
                    Vector2 bgMin = new Vector2(weaponPos.X - 4f, weaponPos.Y - 2f);
                    Vector2 bgMax = new Vector2(weaponPos.X + weaponSize.X + 4f, weaponPos.Y + weaponSize.Y + 2f);
                    drawList.AddRectFilled(bgMin, bgMax, ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.7f)), 3f);
                    drawList.AddRect(bgMin, bgMax, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 0.3f)), 3f, ImDrawFlags.None, 1f);

                    drawList.AddText(weaponPos, ColorToUint32(Config.ESPWeaponColor), weaponText);

                    yOffset -= weaponSize.Y + 6f;
                }

                // ═══════════════════════════════════════════════════════
                // 6. الاسم المزخرف
                // ═══════════════════════════════════════════════════════
                string decoratedName = DecoratePlayerName(entity.Name);
                Vector2 nameSize = ImGui.CalcTextSize(decoratedName);
                Vector2 namePos = new Vector2(
                    entityData.headScreenPos.X - nameSize.X / 2f,
                    entityData.headScreenPos.Y + yOffset
                );

                if (Config.ESPName)
                {
                    if (Config.ESPBG)
                    {
                        Vector2 bgMin = new Vector2(namePos.X - 6f, namePos.Y - 2f);
                        Vector2 bgMax = new Vector2(namePos.X + nameSize.X + 6f, namePos.Y + nameSize.Y + 2f);

                        for (int i = 2; i >= 1; i--)
                        {
                            float grow = i * 2f;
                            Vector4 glow = new Vector4(1f, 1f, 1f, 0.04f * i);
                            drawList.AddRectFilled(
                                new Vector2(bgMin.X - grow, bgMin.Y - grow),
                                new Vector2(bgMax.X + grow, bgMax.Y + grow),
                                ImGui.GetColorU32(glow), 5f
                            );
                        }

                        drawList.AddRectFilled(bgMin, bgMax, ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.75f)), 5f);
                        drawList.AddRect(bgMin, bgMax, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 0.5f)), 5f, ImDrawFlags.None, 1.2f);
                    }

                    drawList.AddText(new Vector2(namePos.X + 1, namePos.Y + 1), ColorToUint32(Color.Black), decoratedName);
                    drawList.AddText(namePos, ColorToUint32(Color.White), decoratedName);
                }

                // ═══════════════════════════════════════════════════════
                // 7. المسافة
                // ═══════════════════════════════════════════════════════
                if (Config.ESPDistance)
                {
                    string distText = $"{MathF.Round(entity.Distance)}M";
                    Vector2 distSize = ImGui.CalcTextSize(distText);
                    Vector2 distPos = new Vector2(
                        entityData.headScreenPos.X - distSize.X / 2f,
                        namePos.Y + nameSize.Y + 4f
                    );

                    drawList.AddText(new Vector2(distPos.X + 1, distPos.Y + 1), ColorToUint32(Color.Black), distText);
                    drawList.AddText(distPos, ColorToUint32(Color.Cyan), distText);
                }

                // ═══════════════════════════════════════════════════════
                // 8. الهيكل العظمي المحسّن - أرفع
                // ═══════════════════════════════════════════════════════
                if (Config.ESPSkeleton)
                {
                    DrawEnhancedSkeleton(entity);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // تزيين الأسماء
        // ═══════════════════════════════════════════════════════════════
        private string DecoratePlayerName(string originalName)
        {
            if (string.IsNullOrWhiteSpace(originalName) || originalName.ToLower() == "bot")
                return "🤖 BOT";

            bool isArabic = originalName.Any(c => c >= 0x0600 && c <= 0x06FF);

            if (isArabic)
            {
                return $"⚡ {originalName} ⚡";
            }
            else
            {
                return $"★ {originalName} ★";
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // الهيكل العظمي المحسّن - أرفع وأخف
        // ═══════════════════════════════════════════════════════════════
        private void DrawEnhancedSkeleton(Entity entity)
        {
            var dl = ImGui.GetBackgroundDrawList();
            uint color = ColorToUint32(Config.ESPSkeletonColor);

            bool IsValid3DPos(Vector3 pos)
            {
                if (pos == Vector3.Zero) return false;
                if (float.IsNaN(pos.X) || float.IsNaN(pos.Y) || float.IsNaN(pos.Z)) return false;
                if (float.IsInfinity(pos.X) || float.IsInfinity(pos.Y) || float.IsInfinity(pos.Z)) return false;
                float distToCamera = Vector3.Distance(pos, Core.LocalMainCamera);
                if (distToCamera > 300f || distToCamera < 0.1f) return false;
                return true;
            }

            Vector2 GetScreenPos(Vector3 worldPos)
            {
                if (!IsValid3DPos(worldPos)) return Vector2.Zero;
                return W2S.WorldToScreen(Core.CameraMatrix, worldPos, Core.Width, Core.Height);
            }

            Vector2 head = GetScreenPos(entity.Head);
            Vector2 neck = GetScreenPos(entity.Spine);
            Vector2 spine = GetScreenPos(entity.Spine);
            Vector2 hip = GetScreenPos(entity.Hip);
            Vector2 lElbow = GetScreenPos(entity.LeftElbow);
            Vector2 rElbow = GetScreenPos(entity.RightElbow);
            Vector2 lWrist = GetScreenPos(entity.LeftWristJoint);
            Vector2 rWrist = GetScreenPos(entity.RightWristJoint);
            Vector2 lKnee = GetScreenPos(entity.LeftCalf);
            Vector2 rKnee = GetScreenPos(entity.RightCalf);
            Vector2 lFoot = GetScreenPos(entity.LeftFoot);
            Vector2 rFoot = GetScreenPos(entity.RightFoot);

            // سُمك أرفع بكثير
            float distance = Math.Max(1f, entity.Distance);
            float thickness = Math.Clamp(1.2f / distance, 0.6f, 1.2f);

            float maxBoneDistance = Core.Height * 0.3f;

            bool IsValidScreen(Vector2 point)
            {
                if (point == Vector2.Zero) return false;
                if (float.IsNaN(point.X) || float.IsNaN(point.Y)) return false;
                if (point.X < -50 || point.X > Core.Width + 50) return false;
                if (point.Y < -50 || point.Y > Core.Height + 50) return false;
                return true;
            }

            void DrawBoneWithGlow(Vector2 a, Vector2 b, uint boneColor)
            {
                if (!IsValidScreen(a) || !IsValidScreen(b)) return;

                float boneLen = Vector2.Distance(a, b);
                if (boneLen > maxBoneDistance || boneLen < 2f) return;

                // توهج أخف
                Vector4 colorVec = ImGui.ColorConvertU32ToFloat4(boneColor);
                for (float i = 2f; i > 0; i -= 0.5f)
                {
                    float alpha = colorVec.W * (i / 2f) * 0.08f;
                    uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(
                        colorVec.X, colorVec.Y, colorVec.Z, alpha
                    ));
                    dl.AddLine(a, b, glowColor, thickness + i * 0.5f);
                }

                dl.AddLine(a, b, boneColor, thickness);
            }

            // رسم الهيكل
            DrawBoneWithGlow(head, neck, color);
            DrawBoneWithGlow(neck, spine, color);
            DrawBoneWithGlow(spine, hip, color);
            DrawBoneWithGlow(neck, lElbow, color);
            DrawBoneWithGlow(lElbow, lWrist, color);
            DrawBoneWithGlow(neck, rElbow, color);
            DrawBoneWithGlow(rElbow, rWrist, color);
            DrawBoneWithGlow(hip, lKnee, color);
            DrawBoneWithGlow(lKnee, lFoot, color);
            DrawBoneWithGlow(hip, rKnee, color);
            DrawBoneWithGlow(rKnee, rFoot, color);

            // نقاط المفاصل أصغر
            float jointSize = thickness * 0.8f;
            dl.AddCircleFilled(head, jointSize, color);
            dl.AddCircleFilled(neck, jointSize, color);
            dl.AddCircleFilled(spine, jointSize, color);
            dl.AddCircleFilled(hip, jointSize, color);
            dl.AddCircleFilled(lElbow, jointSize, color);
            dl.AddCircleFilled(rElbow, jointSize, color);
            dl.AddCircleFilled(lWrist, jointSize, color);
            dl.AddCircleFilled(rWrist, jointSize, color);
            dl.AddCircleFilled(lKnee, jointSize, color);
            dl.AddCircleFilled(rKnee, jointSize, color);
            dl.AddCircleFilled(lFoot, jointSize, color);
            dl.AddCircleFilled(rFoot, jointSize, color);
        }

        void DrawWeaponTextESP(Entity entity, Vector2 headScreenPos, float boxHeight)
        {
            if (!Config.ESPWeapon || entity == null || string.IsNullOrEmpty(entity.WeaponName)) return;

            Vector2 weaponTextSize = ImGui.CalcTextSize(entity.WeaponName);
            Vector2 weaponTextPos = new Vector2(
                headScreenPos.X + 40f,
                headScreenPos.Y + (boxHeight / 2) - (weaponTextSize.Y / 2)
            );

            uint textColor = ColorToUint32(Config.ESPWeaponColor);
            ImGui.GetForegroundDrawList().AddText(weaponTextPos, textColor, entity.WeaponName);
        }

        private void DrawFilledBox(float X, float Y, float W, float H, uint color)
        {
            var vList = ImGui.GetBackgroundDrawList();
            vList.AddRectFilled(new Vector2(X, Y), new Vector2(X + W, Y + H), color);
        }

        public void DrawEnhancedHealthBar(short health, short maxHealth, float X, float Y, float height, float width)
        {
            var vList = ImGui.GetBackgroundDrawList();

            if (maxHealth <= 0) maxHealth = 100;

            float hp = Math.Clamp((float)health / maxHealth, 0f, 1f);
            float filledW = Math.Max(0f, width * hp);

            Vector4 red = new Vector4(1f, 0.2f, 0.2f, 0.95f);
            Vector4 yellow = new Vector4(1f, 0.9f, 0.2f, 0.95f);
            Vector4 green = new Vector4(0.1f, 0.95f, 0.2f, 0.95f);

            Vector4 leftCol;
            Vector4 rightCol;

            if (hp < 0.5f)
            {
                float t = hp / 0.5f;
                leftCol = LerpV4(red, yellow, t);
                rightCol = leftCol;
            }
            else
            {
                float t = (hp - 0.5f) / 0.5f;
                leftCol = LerpV4(yellow, green, t);
                rightCol = leftCol;
            }

            Vector4 bgTop = new Vector4(0f, 0f, 0f, 0.45f);
            Vector4 bgBot = new Vector4(0f, 0f, 0f, 0.35f);

            Vector2 pMin = new Vector2(X, Y - height);
            Vector2 pMax = new Vector2(X + width, Y);

            for (int i = 3; i >= 1; i--)
            {
                float grow = i * 1.5f;
                float a = 0.08f * i;
                Vector4 glow = new Vector4(leftCol.X, leftCol.Y, leftCol.Z, a);
                vList.AddRectFilled(new Vector2(pMin.X - grow, pMin.Y - grow), new Vector2(pMax.X + grow, pMax.Y + grow), ImGui.GetColorU32(glow), 4f);
            }

            vList.AddRectFilledMultiColor(pMin, pMax, ImGui.GetColorU32(bgTop), ImGui.GetColorU32(bgTop), ImGui.GetColorU32(bgBot), ImGui.GetColorU32(bgBot));

            if (filledW > 0f)
            {
                Vector2 fMax = new Vector2(X + filledW, Y);
                vList.AddRectFilledMultiColor(new Vector2(X, Y - height), fMax, ImGui.GetColorU32(leftCol), ImGui.GetColorU32(rightCol), ImGui.GetColorU32(rightCol), ImGui.GetColorU32(leftCol));
            }

            vList.AddRect(pMin, pMax, ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.9f)), 4f, ImDrawFlags.None, 1.2f);
        }

        public void DrawKnockedHealthBar(short health, short maxHealth, float X, float Y, float height, float width)
        {
            var vList = ImGui.GetBackgroundDrawList();

            if (maxHealth <= 0) maxHealth = 100;

            float hp = Math.Clamp((float)health / maxHealth, 0f, 1f);
            float filledW = Math.Max(0f, width * hp);

            Vector4 red = new Vector4(1f, 0.15f, 0.15f, 0.95f);
            Vector4 darkRed = new Vector4(0.35f, 0.01f, 0.01f, 0.9f);

            Vector2 pMin = new Vector2(X, Y - height);
            Vector2 pMax = new Vector2(X + width, Y);

            for (int i = 3; i >= 1; i--)
            {
                float grow = i * 1.5f;
                float a = 0.08f * i;
                Vector4 glow = new Vector4(red.X, red.Y, red.Z, a);
                vList.AddRectFilled(new Vector2(pMin.X - grow, pMin.Y - grow), new Vector2(pMax.X + grow, pMax.Y + grow), ImGui.GetColorU32(glow), 4f);
            }

            vList.AddRectFilledMultiColor(pMin, pMax, ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.5f)), ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.5f)), ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.35f)), ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.35f)));

            if (filledW > 0f)
            {
                Vector2 fMax = new Vector2(X + filledW, Y);
                vList.AddRectFilledMultiColor(new Vector2(X, Y - height), fMax, ImGui.GetColorU32(red), ImGui.GetColorU32(darkRed), ImGui.GetColorU32(darkRed), ImGui.GetColorU32(red));
            }

            vList.AddRect(pMin, pMax, ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.9f)), 4f, ImDrawFlags.None, 1.2f);
        }


        private void DrawHealthBar(ImDrawListPtr drawList, float health, Vector2 head, Vector2 bottom, float boxWidth)
        {
            // ==== غير 100 إلى 200 إذا لعبتك دمها 200 ====
            float maxHealth = 100f;

            health = Math.Clamp(health, 0, maxHealth);
            float hpPercent = health / maxHealth;

            float barWidth = 5f;
            float x = head.X + boxWidth / 2f + 6f;
            float top = head.Y;
            float bot = bottom.Y;
            float h = bot - top;
            float filled = h * hpPercent;

            // خلفية
            drawList.AddRectFilled(
                new Vector2(x, top),
                new Vector2(x + barWidth, bot),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0.6f))
            );

            // لون الدم
            Vector4 color =
                hpPercent > 0.6f ? new Vector4(0, 1f, 0, 1f) :
                hpPercent > 0.3f ? new Vector4(1f, 1f, 0, 1f) :
                                   new Vector4(1f, 0, 0, 1f);

            // الدم
            drawList.AddRectFilled(
                new Vector2(x, bot - filled),
                new Vector2(x + barWidth, bot),
                ImGui.ColorConvertFloat4ToU32(color)
            );

            // إطار
            drawList.AddRect(
                new Vector2(x, top),
                new Vector2(x + barWidth, bot),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 1f))
            );
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
            InternalMemory.Cache = new();
            Core.Entities = new();
            Thread.Sleep(1000);
        }

        private void DrawLine(ImDrawListPtr drawList, Vector2 startPos, Vector2 endPos, uint color)
        {
            if (startPos.X > 0 && startPos.Y > 0 && endPos.X > 0 && endPos.Y > 0)
            {
                drawList.AddLine(startPos, endPos, color, 1.5f);
            }
        }

        private void DrawSkeleton(Entity entity)
        {
            var dl = ImGui.GetBackgroundDrawList();
            uint color = ColorToUint32(Config.ESPSkeletonColor);

            bool IsValid3DPos(Vector3 pos)
            {
                if (pos == Vector3.Zero) return false;
                if (float.IsNaN(pos.X) || float.IsNaN(pos.Y) || float.IsNaN(pos.Z)) return false;
                if (float.IsInfinity(pos.X) || float.IsInfinity(pos.Y) || float.IsInfinity(pos.Z)) return false;
                float distToCamera = Vector3.Distance(pos, Core.LocalMainCamera);
                if (distToCamera > 300f || distToCamera < 0.1f) return false;
                return true;
            }

            Vector2 GetScreenPos(Vector3 worldPos)
            {
                if (!IsValid3DPos(worldPos)) return Vector2.Zero;
                return W2S.WorldToScreen(Core.CameraMatrix, worldPos, Core.Width, Core.Height);
            }

            Vector2 head = GetScreenPos(entity.Head);
            Vector2 spine = GetScreenPos(entity.Spine);
            Vector2 hip = GetScreenPos(entity.Hip);
            Vector2 lElbow = GetScreenPos(entity.LeftElbow);
            Vector2 rElbow = GetScreenPos(entity.RightElbow);
            Vector2 lWrist = GetScreenPos(entity.LeftWristJoint);
            Vector2 rWrist = GetScreenPos(entity.RightWristJoint);
            Vector2 lKnee = GetScreenPos(entity.LeftCalf);
            Vector2 rKnee = GetScreenPos(entity.RightCalf);
            Vector2 lFoot = GetScreenPos(entity.LeftFoot);
            Vector2 rFoot = GetScreenPos(entity.RightFoot);

            float distance = Math.Max(1f, entity.Distance);
            float thickness = Math.Clamp(2.0f / distance, 1.0f, 1.8f);

            float maxBoneDistance = Core.Height * 0.25f;

            bool IsValidScreen(Vector2 point)
            {
                if (point == Vector2.Zero) return false;
                if (float.IsNaN(point.X) || float.IsNaN(point.Y)) return false;
                if (point.X < 5 || point.X > Core.Width - 5) return false;
                if (point.Y < 5 || point.Y > Core.Height - 5) return false;
                return true;
            }

            void Bone(Vector2 a, Vector2 b)
            {
                if (!IsValidScreen(a) || !IsValidScreen(b)) return;

                float boneLen = Vector2.Distance(a, b);
                if (boneLen > maxBoneDistance || boneLen < 2f) return;

                dl.AddLine(a, b, color, thickness);
            }

            Bone(hip, spine);
            Bone(spine, head);
            Bone(spine, lElbow);
            Bone(spine, rElbow);
            Bone(lElbow, lWrist);
            Bone(rElbow, rWrist);
            Bone(hip, lKnee);
            Bone(hip, rKnee);
            Bone(lKnee, lFoot);
            Bone(rKnee, rFoot);
        }

        public void DrawGlowLine(Vector2 start, Vector2 end, uint color, float thickness, float glowRadius, float feather)
        {
            var drawList = ImGui.GetBackgroundDrawList();
            Vector4 colorVec = ImGui.ColorConvertU32ToFloat4(color);

            for (float i = glowRadius; i > 0; i -= feather)
            {
                float alpha = colorVec.W * (i / glowRadius) * 0.02f;
                uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(colorVec.X, colorVec.Y, colorVec.Z, alpha));
                drawList.AddLine(start, end, glowColor, thickness + i);
            }

            drawList.AddLine(start, end, color, thickness);
        }

        public void DrawCorneredBox(float X, float Y, float W, float H, uint color, float thickness)
        {
            var vList = ImGui.GetBackgroundDrawList();

            float lineW = W / 3;
            float lineH = H / 3;

            vList.AddLine(new Vector2(X, Y - thickness / 2), new Vector2(X, Y + lineH), color, thickness);
            vList.AddLine(new Vector2(X - thickness / 2, Y), new Vector2(X + lineW, Y), color, thickness);
            vList.AddLine(new Vector2(X + W - lineW, Y), new Vector2(X + W + thickness / 2, Y), color, thickness);
            vList.AddLine(new Vector2(X + W, Y - thickness / 2), new Vector2(X + W, Y + lineH), color, thickness);
            vList.AddLine(new Vector2(X, Y + H - lineH), new Vector2(X, Y + H + thickness / 2), color, thickness);
            vList.AddLine(new Vector2(X - thickness / 2, Y + H), new Vector2(X + lineW, Y + H), color, thickness);
            vList.AddLine(new Vector2(X + W - lineW, Y + H), new Vector2(X + W + thickness / 2, Y + H), color, thickness);
            vList.AddLine(new Vector2(X + W, Y + H - lineH), new Vector2(X + W, Y + H + thickness / 2), color, thickness);
        }

        static uint ColorToUint32(Color color)
        {
            return ImGui.ColorConvertFloat4ToU32(new Vector4(
                color.R / 255.0f,
                color.G / 255.0f,
                color.B / 255.0f,
                color.A / 255.0f));
        }

        private int lastWidth = 0, lastHeight = 0;

        void CreateHandle()
        {
            if (Config.StreamMode)
            {
                SetWindowDisplayAffinity(hWnd, WDA_EXCLUDEFROMCAPTURE);
            }
            else
            {
                SetWindowDisplayAffinity(hWnd, WDA_NONE);
            }

            RECT rect;
            GetWindowRect(Core.Handle, out rect);

            int x = rect.Left;
            int y = rect.Top;
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            if (Core.Width != lastWidth || Core.Height != lastHeight)
            {
                lastWidth = Core.Width;
                lastHeight = Core.Height;
                ImGui.SetWindowSize(new Vector2(Core.Width, Core.Height));
            }

            Size = new Size(width, height);
            Position = new Point(x, y);

            Core.Width = width;
            Core.Height = height;
        }

        public void DrawGlowingFOVCircle(float radius)
        {
            var drawList = ImGui.GetBackgroundDrawList();
            var center = new Vector2(Core.Width / 2f, Core.Height / 2f);

            uint color = ColorToUint32(Config.FOVColor);
            Vector4 colorVec = ImGui.ColorConvertU32ToFloat4(color);

            for (float i = GlowRadius; i > 0; i -= FeatherAmount)
            {
                float alpha = colorVec.W * (i / GlowRadius) * 0.03f;
                uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(
                    colorVec.X, colorVec.Y, colorVec.Z, alpha));

                drawList.AddCircle(center, radius + i, glowColor, 0, 1f);
            }

            drawList.AddCircle(center, radius, color, 0, 1f);
        }

        public void DrawRageFOVCircle(float radius)
        {
            var drawList = ImGui.GetBackgroundDrawList();
            var center = new Vector2(Core.Width / 2f, Core.Height / 2f);

            Vector4 rageColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            uint color = ImGui.ColorConvertFloat4ToU32(rageColor);
            Vector4 colorVec = ImGui.ColorConvertU32ToFloat4(color);

            for (float i = 15f; i > 0; i -= 2f)
            {
                float alpha = colorVec.W * (i / 15f) * 0.05f;
                uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(
                    colorVec.X, colorVec.Y, colorVec.Z, alpha));

                drawList.AddCircle(center, radius + i, glowColor, 0, 2f);
            }

            drawList.AddCircle(center, radius, color, 0, 2f);

            float crossSize = 10f;
            drawList.AddLine(
                new Vector2(center.X - crossSize, center.Y),
                new Vector2(center.X + crossSize, center.Y),
                color, 2f);
            drawList.AddLine(
                new Vector2(center.X, center.Y - crossSize),
                new Vector2(center.X, center.Y + crossSize),
                color, 2f);
        }

        private Dictionary<string, float> animMap = new();

        private float Lerp(float a, float b, float t) => a + (b - a) * t;

        private Vector4 LerpV4(Vector4 a, Vector4 b, float t)
        {
            return new Vector4(
                a.X + (b.X - a.X) * t,
                a.Y + (b.Y - a.Y) * t,
                a.Z + (b.Z - a.Z) * t,
                a.W + (b.W - a.W) * t
            );
        }

        bool RightAlignedCheckbox(string label, ref bool value)
        {
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            Vector2 startPos = ImGui.GetCursorScreenPos();
            Vector2 boxSize = new Vector2(24, 24);
            float radius = 2f;

            ImGui.PushID(label);

            float rowHeight = MathF.Max(boxSize.Y, 28f);
            float fullWidth = ImGui.GetContentRegionAvail().X;

            ImGui.InvisibleButton("##row", new Vector2(fullWidth, rowHeight));
            bool clicked = ImGui.IsItemClicked();

            if (clicked) value = !value;

            float speed = 0.2f;
            if (!animMap.ContainsKey(label)) animMap[label] = value ? 1f : 0f;

            animMap[label] = Lerp(animMap[label], value ? 1f : 0f, speed);

            float t = animMap[label];

            Vector4 fillEnabled = ImGui.GetStyle().Colors[(int)ImGuiCol.CheckMark];
            Vector4 fillDisabled = new Vector4(9f / 255f, 9f / 255f, 9f / 255f, 1f);
            Vector4 borderEnabled = fillEnabled;
            Vector4 borderDisabled = new Vector4(70f / 255f, 70f / 255f, 70f / 255f, 1f);
            Vector4 labelEnabled = new Vector4(1f, 1f, 1f, 1f);
            Vector4 labelDisabled = new Vector4(0.5f, 0.5f, 0.5f, 1f);

            float rightX = startPos.X + fullWidth - boxSize.X;
            Vector2 boxMin = new Vector2(rightX, startPos.Y + (rowHeight - boxSize.Y) * 0.5f);
            Vector2 boxMax = boxMin + boxSize;

            Vector4 baseFillColor = value ? new Vector4(0f, 0f, 0f, 1f) : fillDisabled;
            drawList.AddRectFilled(boxMin, boxMax, ImGui.GetColorU32(baseFillColor), radius);

            if (value && t > 0f)
            {
                float fillHeight = boxSize.Y * t;
                Vector2 fillMin = new Vector2(boxMin.X, boxMin.Y + boxSize.Y - fillHeight);
                Vector2 fillMax = new Vector2(boxMin.X + boxSize.X, boxMin.Y + boxSize.Y);
                drawList.AddRectFilled(fillMin, fillMax, ImGui.GetColorU32(fillEnabled), radius);
            }

            drawList.AddRect(boxMin, boxMax, ImGui.GetColorU32(value ? borderEnabled : borderDisabled), radius, ImDrawFlags.None, 1f);

            float textHeight = ImGui.GetFontSize();
            Vector2 labelPos = new Vector2(startPos.X + 4, startPos.Y + (rowHeight - textHeight) * 0.5f);
            drawList.AddText(labelPos, ImGui.ColorConvertFloat4ToU32(value ? labelEnabled : labelDisabled), label);

            ImGui.SetCursorScreenPos(new Vector2(startPos.X, startPos.Y + rowHeight + 10));

            ImGui.PopID();

            return clicked;
        }

        protected override unsafe Task PostInitialized()
        {
            StartEntityProcessing();
            return base.PostInitialized();
        }
    }
}