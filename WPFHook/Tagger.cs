using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace WPFHook
{
    public static class Tagger
    {
        /// <summary>
        /// all the key wods that the app looks foor to see if there is a distraction.
        /// </summary>
        public static string[] distractionWords;
        public static void StartUp()
        {
            // get the key words to look for from the txt file at the beginning of the application.
            distractionWords= System.IO.File.ReadAllLines(@".\DistractionWords.txt");
        }
        /// <summary>
        /// sets the tag of the activity. can be "work" , "system" or "distraction"
        /// checks the key words that indicate "distraction" in the process name and the main window title.
        /// </summary>
        /// <param name="windowName"></param>
        /// <param name="processName"></param>
        /// <returns></returns>
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
