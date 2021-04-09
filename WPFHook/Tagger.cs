using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace WPFHook
{
    public static class Tagger
    {
        public static string[] distractionWords;
        public static void StartUp()
        {
            distractionWords= System.IO.File.ReadAllLines(@".\DistractionWords.txt");
        }
        public static string getTag(string windowName, string processName)
        {
            if (windowName.Equals(""))
                return "system";
            else
            {
                foreach(string word in distractionWords)
                {
                    if(Contains(word,windowName) || Contains(word, processName))
                    {
                        return "distraction";
                    }
                }
                return "work";
            }
        }
        /// <summary>
        /// checks if a substring is contained in the string (in my case - window title or process name)
        /// Used Regex to find if the exprassion contains the window title and so on.
        /// checked if any of my distraction are inside the window title and vis versa
        /// </summary>
        /// <param name="source"></param>
        /// <param name="toCheck"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        public static bool Contains(string source, string toCheck)
        {
            return Regex.IsMatch(source, Regex.Escape(toCheck), RegexOptions.IgnoreCase) || Regex.IsMatch(toCheck, Regex.Escape(source), RegexOptions.IgnoreCase);
        }
    }
}
