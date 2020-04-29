using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slurper
{
    public class ConsoleSpinner
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
