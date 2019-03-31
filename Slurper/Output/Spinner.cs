using System;

namespace Slurper.Output
{
    static class Spinner
    {
        private static char _lastSearchChar = '/';
        private static char _lastRipChar = '-';

        public static void SearchSpin()
        {
            Flip(ref _lastSearchChar, '/', '\\');
        }

        public static void RipSpin()
        {
            Flip(ref _lastRipChar, '-', '|');
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
