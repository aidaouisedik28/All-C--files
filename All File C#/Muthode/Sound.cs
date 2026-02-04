  private bool soundMuted = false;
  private void PlaySound(string soundFileName)
  {
      if (soundMuted)
          return;

      var assembly = Assembly.GetExecutingAssembly();
      var resourceName = $"PlantillaChanchoV9.{soundFileName}";

      using (Stream soundStream = assembly.GetManifestResourceStream(resourceName))
      {
          if (soundStream != null)
          {
              using (SoundPlayer player = new SoundPlayer(soundStream))
              {
                  player.Stop();   // يوقف أي صوت شغال
                  player.Play();   // يشغل الجديد
              }
          }
      }
  }
