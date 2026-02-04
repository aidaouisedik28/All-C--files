         private Dictionary<long, int> orginalValues9 = new Dictionary<long, int>();
        private Dictionary<long, int> orginalValues10 = new Dictionary<long, int>();
        private Dictionary<long, int> orginalValues11 = new Dictionary<long, int>();
        private Dictionary<long, int> orginalValues12 = new Dictionary<long, int>();

        long Offset1 = 0xAA;
        long offset2 = 0xA6;
        private async void C1_CheckedChanged_1(object sender, EventArgs e)
        {
            try
            {
                orginalValues9.Clear();
                orginalValues10.Clear();
                orginalValues11.Clear();
                orginalValues12.Clear();

                long readOffset = Convert.ToInt64(Offset1);
                long writeOffset = Convert.ToInt64(offset2);

                // ربط اللعبة - نفس الطريقة
                if (!MEM.SetProcess(new[] { "HD-Player" }))
                {
                    MessageBox.Show("Emulator not found!");
                    return;
                }

                // البحث - نفس الطريقة (معامل واحد فقط)
                var result = await MEM.AoBScan("FF FF FF FF 00 00 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 00 A5 43");

                if (result == null || !result.Any())
                {
                    MessageBox.Show("Pattern not found!");
                    return;
                }

                // المعالجة - نفس المنطق
                foreach (var CurrentAddress in result)
                {
                    Int64 headAddr = CurrentAddress + readOffset;
                    Int64 chestAddr = CurrentAddress + writeOffset;

                    // قراءة القيم
                    int headValue = MEM.ReadInt(headAddr);
                    int chestValue = MEM.ReadInt(chestAddr);

                    // حفظ القيم الأصلية
                    orginalValues9[chestAddr] = chestValue;
                    orginalValues10[headAddr] = headValue;

                    // تبديل القيم
                    MEM.AobReplace(chestAddr, headValue);
                    MEM.AobReplace(headAddr, chestValue);

                    // حفظ القيم الجديدة
                    orginalValues11[chestAddr] = headValue;
                    orginalValues12[headAddr] = chestValue;
                }

                MessageBox.Show("Aimbot Head Active");
                Console.Beep(900, 600);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
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



