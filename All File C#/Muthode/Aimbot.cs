   private Dictionary<long, int> orginalValues9 = new Dictionary<long, int>();
   private Dictionary<long, int> orginalValues10 = new Dictionary<long, int>();
   private Dictionary<long, int> orginalValues11 = new Dictionary<long, int>();
   private Dictionary<long, int> orginalValues12 = new Dictionary<long, int>();

   long Offset5 = 0x80;
   long offset6 = 0x7C;

/////////////////////////////////////////////////////////////////////////////////////

             PlaySound("CLICK.wav");
            try
            {
                orginalValues9.Clear();
                orginalValues10.Clear();
                orginalValues11.Clear();
                orginalValues12.Clear();

                long readOffset = Convert.ToInt64(Offset1);
                long writeOffset = Convert.ToInt64(offset2);

                int proc = Process.GetProcessesByName("HD-Player")[0].Id;
                MEM.OpenProcess(proc);

                var result = await MEM.AoBScan(0x0000000000000000, 0x00007fffffffffff, "FF FF FF FF 00 00 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 00 A5 43", true, true);


                if (result != null && result.Any())
                {
                    foreach (var addrObj in result)
                    {
                        long currentAddress = Convert.ToInt64(addrObj); // ✅ التصحيح هنا

                        long headAddr = currentAddress + readOffset;
                        long chestAddr = currentAddress + writeOffset;

                        int headValue = BitConverter.ToInt32(
                            MEM.SunIsKind(headAddr.ToString("X"), sizeof(int)), 0);

                        int chestValue = BitConverter.ToInt32(
                            MEM.SunIsKind(chestAddr.ToString("X"), sizeof(int)), 0);

                        orginalValues9[chestAddr] = chestValue;
                        orginalValues10[headAddr] = headValue;

                        MEM.WriteMemory(chestAddr.ToString("X"), "int", headValue.ToString());
                        MEM.WriteMemory(headAddr.ToString("X"), "int", chestValue.ToString());

                        orginalValues11[chestAddr] = BitConverter.ToInt32(
                            MEM.SunIsKind(chestAddr.ToString("X"), sizeof(int)), 0);

                        orginalValues12[headAddr] = BitConverter.ToInt32(
                            MEM.SunIsKind(headAddr.ToString("X"), sizeof(int)), 0);
                    }

                }

                // تشغيل صوت التفعيل
                PlaySound("activate.wav");
            }
            catch
            {
                // أي خطأ → صوت التعطيل
                PlaySound("desactivar.wav");
            }
        }
    }
}
    

// UP BUTTON
   private Dictionary<long, int> originalvalues = new Dictionary<long, int>();
   private Dictionary<long, int> originallvalues = new Dictionary<long, int>();
   private Dictionary<long, int> originalvalues2 = new Dictionary<long, int>();
   private Dictionary<long, int> originallvalues2 = new Dictionary<long, int>();


   string AOBHEAD = "FF FF FF FF 00 00 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 00 A5 43";
   string READHEAD = "0xAA";
   string writeHEAD = "0xA6";

// IN BUTTON


         try
         {
             Stopwatch stopwatch = new Stopwatch();
             stopwatch.Start();

             // ربط اللعبة
             if (!MEM.SetProcess(new[] { "HD-Player" }))
             {
                 PlaySound("desactivar.wav");
                 return;
             }

             long readOffset = Convert.ToInt64(READHEAD, 16);
             long writeOffset = Convert.ToInt64(writeHEAD, 16);


             var result = await MEM.AoBScan(AOBHEAD);

             if (result == null || !result.Any())
             {
                 PlaySound("desactivar.wav");
                 return;
             }

             foreach (var currentAddress in result)
             {
                 long headAddr = currentAddress + readOffset;
                 long chestAddr = currentAddress + writeOffset;

      
                 int headValue = MEM.ReadInt(headAddr);
                 int chestValue = MEM.ReadInt(chestAddr);

  
                 MEM.AobReplace(chestAddr, headValue);
                 MEM.AobReplace(headAddr, chestValue);
             }

             stopwatch.Stop();
  
             PlaySound("activate.wav");
         }
         catch (Exception ex)
         {   
             PlaySound("desactivar.wav");
         }



