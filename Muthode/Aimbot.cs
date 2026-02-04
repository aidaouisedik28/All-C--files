   private Dictionary<long, int> orginalValues9 = new Dictionary<long, int>();
   private Dictionary<long, int> orginalValues10 = new Dictionary<long, int>();
   private Dictionary<long, int> orginalValues11 = new Dictionary<long, int>();
   private Dictionary<long, int> orginalValues12 = new Dictionary<long, int>();

   long Offset5 = 0x80;
   long offset6 = 0x7C;

/////////////////////////////////////////////////////////////////////////////////////

  {
      orginalValues9.Clear();
      orginalValues10.Clear();
      orginalValues11.Clear();
      orginalValues12.Clear();


      Int64 readOffset = Convert.ToInt64(Offset5);
      Int64 writeOffset = Convert.ToInt64(offset6);

      Int32 proc = Process.GetProcessesByName("HD-Player")[0].Id;
      meme.OpenProcess(proc);

      var result = await meme.AoBScan(0x0000000000000000, 0x00007fffffffffff, "FF FF FF FF FF FF FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 00 00 00 00 00 00 00 00 00 00 A5 43", true, true);

      if (result.Count() != 0)
      {
          foreach (var CurrentAddress in result)
          {
              Int64 addressToSave = CurrentAddress + writeOffset;
              var currentBytes = meme.SunIsKind(addressToSave.ToString("X"), sizeof(int));
              int currentValue = BitConverter.ToInt32(currentBytes, 0); orginalValues9[addressToSave] = currentValue;
              Int64 addressToSave9 = CurrentAddress + readOffset;

              var currentBytes9 = meme.SunIsKind(addressToSave9.ToString("X"), sizeof(int));
              int currentValue9 = BitConverter.ToInt32(currentBytes9, 0); orginalValues10[addressToSave9] = currentValue9;
              Int64 headbytes = CurrentAddress + readOffset;
              Int64 chestbytes = CurrentAddress + writeOffset;

              var bytes = meme.SunIsKind(headbytes.ToString("X"), sizeof(int));
              int Read = BitConverter.ToInt32(bytes, 0);
              var bytes2 = meme.SunIsKind(chestbytes.ToString("X"), sizeof(int));
              int Read2 = BitConverter.ToInt32(bytes2, 0);

              meme.WriteMemory(chestbytes.ToString("X"), "int", Read.ToString());
              meme.WriteMemory(headbytes.ToString("X"), "int", Read2.ToString());

              Int64 addressToSave1 = CurrentAddress + writeOffset;
              var currentBytes1 = meme.SunIsKind(addressToSave1.ToString("X"), sizeof(int));
              int curentValue1 = BitConverter.ToInt32(currentBytes1, 0); orginalValues11[addressToSave1] = curentValue1;

              Int64 addressToSave19 = CurrentAddress + readOffset;
              var currentBytes19 = meme.SunIsKind(addressToSave19.ToString("X"), sizeof(int));
              int currentValues19 = BitConverter.ToInt32(currentBytes19, 0); orginalValues12[addressToSave19] = currentValues19;
          }
          orginalValues9.Clear();
          orginalValues10.Clear();
          orginalValues11.Clear();
          orginalValues12.Clear();
          MessageBox.Show("Aimbot Head Active");
          Console.Beep(900, 600);

      }
  }
