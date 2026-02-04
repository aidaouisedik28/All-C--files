using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace YourNamespace
{
    public class KeyMouseBindingManager : IDisposable
    {
        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")] private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")] private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")] private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll")] private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;

        private const int WM_KEYDOWN = 0x0100;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_XBUTTONDOWN = 0x020B;

        private enum BindType { None, Keyboard, Mouse }
        private enum MouseBtn { None, Left, Right, Middle, X1, X2 }

        private class BindData
        {
            public Guna2Button Button;
            public Guna2CustomCheckBox CheckBox;
            public BindType Type;
            public Keys Key;
            public MouseBtn Mouse;
            public EventHandler ClickHandler; // ✅ أضف هذا السطر
        }
        private readonly Dictionary<Guna2Button, BindData> binds = new();
        private Guna2Button waitingButton;

        private readonly HookProc kbProc;
        private readonly HookProc mouseProc;
        private IntPtr kbHook;
        private IntPtr mouseHook;

        public KeyMouseBindingManager()
        {
            kbProc = KeyboardHook;
            mouseProc = MouseHook;
            kbHook = SetHook(kbProc, WH_KEYBOARD_LL);
            mouseHook = SetHook(mouseProc, WH_MOUSE_LL);
        }
        public void SetupButton(Guna2Button button, Guna2CustomCheckBox checkBox, EventHandler checkBoxClickHandler = null)
        {
            binds[button] = new BindData
            {
                Button = button,
                CheckBox = checkBox,
                Type = BindType.None,
                ClickHandler = checkBoxClickHandler // ✅ حفظ الـ handler
            };

            button.Text = "None";
            button.Click += (s, e) =>
            {
                waitingButton = button;
                button.Text = "Press key / mouse";
            };
        }
        private IntPtr KeyboardHook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                Keys key = (Keys)Marshal.ReadInt32(lParam);

                if (waitingButton != null)
                {
                    var b = binds[waitingButton];
                    b.Type = BindType.Keyboard;
                    b.Key = key;
                    b.Mouse = MouseBtn.None;
                    waitingButton.Text = key.ToString();
                    waitingButton = null;
                    return (IntPtr)1;
                }

                foreach (var b in binds.Values)
                {
                    if (b.Type == BindType.Keyboard && b.Key == key)
                    {
                        b.CheckBox.Invoke((MethodInvoker)(() =>
                        {
                            b.CheckBox.Checked = !b.CheckBox.Checked;
                            b.ClickHandler?.Invoke(b.CheckBox, EventArgs.Empty); // ✅ تشغيل الـ event!
                        }));
                    }
                }
            }
            return CallNextHookEx(kbHook, nCode, wParam, lParam);
        }

        private IntPtr MouseHook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                MouseBtn btn = MouseBtn.None;

                if (wParam == (IntPtr)WM_LBUTTONDOWN) btn = MouseBtn.Left;
                else if (wParam == (IntPtr)WM_RBUTTONDOWN) btn = MouseBtn.Right;
                else if (wParam == (IntPtr)WM_MBUTTONDOWN) btn = MouseBtn.Middle;
                else if (wParam == (IntPtr)WM_XBUTTONDOWN)
                {
                    int data = Marshal.ReadInt32(lParam + 8) >> 16;
                    btn = data == 1 ? MouseBtn.X1 : MouseBtn.X2;
                }

                if (btn == MouseBtn.None)
                    return CallNextHookEx(mouseHook, nCode, wParam, lParam);

                if (waitingButton != null)
                {
                    var b = binds[waitingButton];
                    b.Type = BindType.Mouse;
                    b.Mouse = btn;
                    b.Key = Keys.None;
                    waitingButton.Text = "Mouse " + btn;
                    waitingButton = null;
                    return (IntPtr)1;
                }

                foreach (var b in binds.Values)
                {
                    if (b.Type == BindType.Mouse && b.Mouse == btn)
                    {
                        b.CheckBox.Invoke((MethodInvoker)(() =>
                        {
                            b.CheckBox.Checked = !b.CheckBox.Checked;
                            b.ClickHandler?.Invoke(b.CheckBox, EventArgs.Empty); // ✅ تشغيل الـ event!
                        }));
                    }
                }
            }
            return CallNextHookEx(mouseHook, nCode, wParam, lParam);
        }
        private IntPtr SetHook(HookProc proc, int type)
        {
            using var p = Process.GetCurrentProcess();
            using var m = p.MainModule;
            return SetWindowsHookEx(type, proc, GetModuleHandle(m.ModuleName), 0);
        }

        public void Dispose()
        {
            if (kbHook != IntPtr.Zero) UnhookWindowsHookEx(kbHook);
            if (mouseHook != IntPtr.Zero) UnhookWindowsHookEx(mouseHook);
        }
    }
}
