using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AotForms;
using ClickableTransparentOverlay;
using AotForms;
using ImGuiNET;
using Microsoft.VisualBasic.ApplicationServices;
using Reborn;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Client
{
  
   



    internal class RenderClass : ClickableTransparentOverlay.Overlay

    {


     
        public static api KeyAuthApp = new api(
name: "intrnalpanel", // Application Name
ownerid: "SAQCFIZfXl", // Owner ID
secret: "eda73b54360b7497f3deaae7f9c85e7d0be49c5d46114f7bbde371fcb9be569b", // Application Secret
version: "1.3" // Application Version /*
);




        string LIeances = "";
        protected override unsafe void Render()
        {

            KeyAuthApp.init();
            // Call your ImGui rendering functions here
            ImGui.Begin("XENO-CHEATS");

            ImGui.SetWindowSize(new Vector2(300, 300));



            ImGui.Text("LIeances");
            ImGui.InputText("##LIeances", ref LIeances, 20, ImGuiInputTextFlags.Password);
            ImGui.SameLine();
            if (ImGui.Button("login"))
            {
                KeyAuthApp.license(LIeances);

                if (KeyAuthApp.response.success)
                {
                   /* StartBackgroundTasks();*/

                }
                else
                {


                }


            }


            if (ImGui.Button("LOAD"))
            {

                LOAD();

            }


            bool aimBotChecked = Config.AimBot;
            if (ImGui.Checkbox("Enable AimBot B", ref aimBotChecked))
            {
                Config.AimBot = aimBotChecked;
            }


            ImGui.End();

      



        }


       /* private async void StartBackgroundTasks()
        {
            new Thread(Data.Work) { IsBackground = true }.Start();
            new Thread(Aimbot.Work) { IsBackground = true }.Start();

            new Thread(Aimbot.teleportenemy) { IsBackground = true }.Start();
            new Thread(Aimbot.upplayercontroler) { IsBackground = true }.Start();
        }*/

        private async void LOAD()
        {
            var processes = Process.GetProcessesByName("HD-Player");

            if (processes.Length != 1)
            {
                MessageBox.Show("Status: Open just one emulator.");
                return;
            }

            var process = processes[0];
            var mainModulePath = Path.GetDirectoryName(process.MainModule.FileName);
            var adbPath = Path.Combine(mainModulePath, "HD-Adb.exe");

            if (!File.Exists(adbPath))
            {
                MessageBox.Show("Status: Reinstall the emulator.");
                return;
            }

            var adb = new Adb(adbPath);

            // Kill any running adb process
            await adb.Kill();

            // Start adb
            var started = await adb.Start();
            if (!started)
            {
                MessageBox.Show("Status: Failed to Start ADB");
                return;
            }

            // Define the package and library
            String pkg = "com.dts.freefireth";
            String lib = "libil2cpp.so";

            bool isFreeFireMax = false;
            if (isFreeFireMax)
            {
                pkg = "com.dts.freefiremax";
            }




            // Find the module address
            var moduleAddr = await adb.FindModule(pkg, lib);

            // Set the offsets and handle
            Offsets.Il2Cpp = moduleAddr;
            //Core.Handle = FindRenderWindow(mainHandle);


         //   new Thread(Data.Work) { IsBackground = true }.Start();
          //  new Thread(Aimbot.Work) { IsBackground = true }.Start();




            MessageBox.Show("Status: Found Start Address: " + moduleAddr);



        }

        //static IntPtr FindRenderWindow(IntPtr parent)
        //{
        //    IntPtr renderWindow = IntPtr.Zero;
        //    WinAPI.EnumChildWindows(parent, (hWnd, lParam) =>
        //    {
        //        StringBuilder sb = new StringBuilder(256);
        //        WinAPI.GetWindowText(hWnd, sb, sb.Capacity);
        //        string windowName = sb.ToString();
        //        if (!string.IsNullOrEmpty(windowName))
        //        {
        //            if (windowName != "HD-Player")
        //            {
        //                renderWindow = hWnd;
        //            }
        //        }
        //        return true;
        //    }, IntPtr.Zero);

        //    return renderWindow;
        //}
    }
}
