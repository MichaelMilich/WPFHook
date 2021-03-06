using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Media;
using WPFHook.Models;

namespace WPFHook.ViewModels.BackgroundLogic
{
    public static class Tagger
    {
        ///<summary>
        /// The Tagger is one of the most importnatn parts of this code!
        /// The Tagger uses a list of TagsModels and a list of Rules that are connected to the database.
        /// For each Rule there are the parameter, operation and constant along other properties. 
        /// The rule that is compiled returns bool that determines if the activity is within the rule.
        /// The rule also saves the corresponding TagId. if the rule returns true - than we use the TagID to make the Activity tag into the Tag with the same ID.
        /// we use this by calling TagList[j] where j= rules[i].TagId and i simply loops through the rules.
        /// Now the problem is that what happens when one tag gets deleted? all the associsated rules get deleted as well.
        /// but then we run into a new problem. The tags Id autoincrement. mmeaning the next tag id will be higher, no matter if the previous tags are dleted.
        /// It can be that the TagTAble will start from 6,7,8 because we deleted the rest of the tags. or something like 1,3,5,8 becasue we deleted the middle tags.
        /// this creates a problem with the TagList size. 
        /// Somehow i need to place each tag in its position according to its TagId. UPDATE: it was solved bia the BuildTagListFunction.
        /// Basicly i keep a big array with all the relevant tags in there relevant location. all other locations are null within the array.
        /// I keep track of all the tags id in the RuleModel and the TagModel so i never call an null location.
        /// 
        /// The GetTag function runs on all the functions in the rulefunctions one by one.
        /// This is important to remember because the user can make a rule Function that always return true at the middle of the ruleFunction List.
        /// This means the tagger never gets to continue to any other rule function if they exist.
        /// 
        /// </summary>


        /// <summary>
        /// all the key wods that the app looks foor to see if there is a distraction.
        /// </summary>
        public static string[] distractionWords;
        private static List<TagModel> tagList;
        private static List<RuleModel> ruleList;
        private static List<Func<ActivityLine, bool>> ruleFunctions;
        public static void StartUp()
        {
            Tagger.BuildTagList(SqliteDataAccess.LoadTags());
            BuildRules();
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
                if (tagModel != null)
                {
                    if (tagModel.TagName.Equals(tag))
                        return tagModel.TagColor;
                }
            }
            return Brushes.Black;
        }
        public static int getMaxTagId(List<TagModel> smalltagList)
        {
            int max = 0;
            for (int i=0;i<smalltagList.Count;i++)
            {
                if (max < smalltagList[i].TagID)
                    max = smalltagList[i].TagID;
            }
            return max;
        }
        public static void UpdateTagList()
        {
            Tagger.BuildTagList(SqliteDataAccess.LoadTags());
        }
        private static void BuildTagList(List<TagModel> smalltagList)
        {
            if (smalltagList.Count == 0)
            {
                tagList = new List<TagModel>();
            }
            else
            {
                var max = getMaxTagId(smalltagList);
                int j = 0;
                tagList = new List<TagModel>();
                for (int i = 0; i <= max; i++)
                {
                    while (i < smalltagList[j].TagID)
                    {
                        tagList.Add(null);
                        i++;
                    }
                    tagList.Add(smalltagList[j]);
                    j++;
                }
            }
            // builds the tagList with nulls were there are no TagID
        }
        public static List<RuleModel> BuildRules()
        {
            ruleList = SqliteDataAccess.LoadRules();
            ruleFunctions = new List<Func<ActivityLine, bool>>();
            foreach (RuleModel r in ruleList)
            {
                ruleFunctions.Add(RuleModel.CompileRule<ActivityLine>(r));
            }
            return ruleList;
        }
        public static List<RuleModel> getRulesList()
        {
            return ruleList;
        }
    }
}
