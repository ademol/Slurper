using System;

namespace Slurper
{
    static class Spinner
    {
        private static char lastSearchChar = '/';
        private static char lastRipChar = '-';

        public static void SearchSpin()
        {
            Flip(ref lastSearchChar, '/', '\\');
        }

        public static void RipSpin()
        {
            Flip(ref lastRipChar, '-', '|');
        }

        public static void Flip(ref char lastChar,char lhChar, char rhChar)
        {
            char newChar = (lastChar == lhChar) ? rhChar : lhChar;
            //set the spinner position
            Console.CursorLeft = 0;
            //write the new character to the console
            Console.Write(newChar);
            lastChar = newChar;
        }
    }
}
