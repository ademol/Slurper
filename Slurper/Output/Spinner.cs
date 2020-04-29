using System;

namespace Slurper.Output
{
    static class Spinner
    {
        private static readonly char[] SpinChars = { '|', '/', '-', '\\' };
        private static int _spinCharIdx;

        public static void Spin()
        {
            // fold back to begin char when needed
            if (_spinCharIdx + 1 == SpinChars.Length)
            { _spinCharIdx = 0; }
            else
            { _spinCharIdx++; }

            var spinChar = SpinChars[_spinCharIdx];

            //set the spinner position
            Console.CursorLeft = 0;

            //write the new character to the console
            Console.Write(spinChar);
        }
    }
}
