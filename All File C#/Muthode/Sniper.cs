 {
     DisableControls();
     RIG.Text = "Applying...";

     var proc = System.Diagnostics.Process.GetProcessesByName("HD-Player").FirstOrDefault();
     if (proc == null)
     {
         RIG.Text = "Process not found!";
         EnableControls();
         return;
     }

     if (!MEM.SetProcess(new[] { "HD-Player" }))
     {
         RIG.Text = "Failed to open process!";
         EnableControls();
         return;
     }

     scanResultsteleport = await MEM.AoBScan(
         0x0000000000010000,
         0x00007ffffffeffff,
         "SEARCH");

     if (scanResultsteleport != null && scanResultsteleport.Length > 0)
     {
         RXC = "0X" + scanResultsteleport.FirstOrDefault().ToString("X");
         RIG.Text = "LEFT CAMERA LOADED";
         Console.Beep(2000, 600);
     }
     else
     {
         RIG.Text = "No Matches Found.";
     }
 }
 catch (Exception ex)
 {
     RIG.Text = "Error: " + ex.Message;
 }
 finally
 {
     EnableControls();
 } 
