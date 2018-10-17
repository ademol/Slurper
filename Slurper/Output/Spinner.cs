using System;

namespace Slurper
{
    static class Spinner
    {
        private static char char1 = '/';
        private static char char2 = '\\';
        private static char newChar = char2;

        public static void Spin()
        {
                newChar = (newChar == char1) ? char2 : char1;
                //set the spinner position
                Console.CursorLeft = 0;
                //write the new character to the console
                Console.Write( newChar );
        }

    }
}
