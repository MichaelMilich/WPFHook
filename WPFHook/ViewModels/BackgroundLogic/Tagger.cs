using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Media;
using WPFHook.Models;

namespace WPFHook.ViewModels.BackgroundLogic
{
    public static class Tagger
    {
        /// <summary>
        /// all the key wods that the app looks foor to see if there is a distraction.
        /// </summary>
        public static string[] distractionWords;
        private static List<TagModel> tagList;
        private static List<Rule> ruleList;
        private static List<Func<ActivityLine, bool>> ruleFunctions;
        public static void StartUp()
        {
            // get the key words to look for from the txt file at the beginning of the application.
            // from checking, AppDomain.CurrentDomain.BaseDirectory is the path! 
            distractionWords = System.IO.File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "DistractionWords.txt");
            tagList = SqliteDataAccess.LoadTags();
            ruleList = SqliteDataAccess.LoadRules();
            ruleFunctions = new List<Func<ActivityLine, bool>>();
            foreach (Rule r in ruleList)
            {
                ruleFunctions.Add(Rule.CompileRule<ActivityLine>(r));
            }
        }
        /// <summary>
        /// sets the tag of the activity. can be "work" , "system" or "distraction"
        /// checks the key words that indicate "distraction" in the process name and the main window title.
        /// </summary>
        /// <param name="windowName"></param>
        /// <param name="processName"></param>
        /// <returns></returns>
        public static (string, Brush) getTag(string windowName, string processName)
        {
            if (windowName.Equals(""))
                return ("system",Brushes.Blue);
            else
            {
                foreach(string word in distractionWords)
                {
                    if(Contains(word,windowName) || Contains(word, processName))
                    {
                        return ("distraction", Brushes.Red);
                    }
                }
                return ("work", Brushes.Green);
            }
        }
        public static (string, Brush) getTag(ActivityLine line)
        {
            for(int i=0;i<ruleFunctions.Count;i++)
            {
                if (ruleFunctions[i](line))
                    return (tagList[ruleList[i].TagId].TagName, tagList[ruleList[i].TagId].TagColor);
            }
            return ("null", Brushes.Black);
        }
        public static Brush UpdateTagColor(string tag)
        {
            foreach(TagModel tagModel in tagList)
            {
                if (tagModel.TagName.Equals(tag))
                    return tagModel.TagColor;
            }
            return Brushes.Black;
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
