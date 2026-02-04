using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Client
{
    internal class KeyHelper
    {
        // Import the user32.dll for accessing the GetAsyncKeyState function
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        /// <summary>
        /// Checks if a specific key is currently pressed down.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is down, otherwise false.</returns>
        public static bool IsKeyDown(Keys key)
        {
            // The high-order bit is set if the key is down
            return (GetAsyncKeyState(key) & 0x8000) != 0;
        }
    }
}
