using System;

namespace Slurper
{
    static class Spinner
    {
        private static char[] spinChars = new char[] { '|', '/', '-', '\\' };
        private static int spinCharIdx = 0;

        public static void Spin()
        {
            // fold back to begin char when needed
            if (spinCharIdx + 1 == spinChars.Length)
            { spinCharIdx = 0; }
            else
            { spinCharIdx++; }

            char spinChar = spinChars[spinCharIdx];

            //set the spinner position
            Console.CursorLeft = 0;

            //write the new character to the console
            Console.Write(spinChar);
        }

    }
}
