using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Windows;
using WPFHook.Models;

namespace WPFHook.ViewModels.BackgroundLogic
{
    /// <summary>
    /// The connection to the data base.
    /// A static class that provides all the nessery functions to read write and delete the database.
    /// The database i am using is 2 differrent datas.
    /// 1 - the activity line database that holds all the information of what happened in during the time the application was running.
    /// 2 - the Tag and Rule tables (inside Tags.db) that hold all the logic the user wants to do. 
    /// uses dapper and SQLite.core.
    /// </summary>
    public static class SqliteDataAccess
    {
        private static string connectionStringActivity;
        private static string connectionStringTags;
        /// <summary>
        /// Sets up the connection strings for the databases the application uses
        /// </summary>
        public static void StartUp()
        {
            connectionStringActivity = GetConnectionStringByName("DailyLogDefault");
            connectionStringTags = GetConnectionStringByName("TagsDefault");
        }
        // Retrieves a connection string by name.
        // Returns null if the name is not found.
        private static string GetConnectionStringByName(string name)
        {
            // Assume failure.
            string returnValue = null;

            // Look for the name in the connectionStrings section.
            ConnectionStringSettings settings =
                ConfigurationManager.ConnectionStrings[name];

            // If found, return the connection string.
            if (settings != null)
                returnValue = "Data Source="+ AppDomain.CurrentDomain.BaseDirectory+ settings.ConnectionString;

            return returnValue;
        }
        /// <summary>
        /// connects to the ActivityDB.db and queries the whole database
        /// </summary>
        /// <returns>List of ActivityLines that the database has (the whole database)</returns>
        public static List<ActivityLine> LoadActivities()
        {
            // apperently dapper enables me to make ActivityLine list if ActivityLine has a constructor that gets all the parameters types of the database.
            using (IDbConnection cnn =new SQLiteConnection(connectionStringActivity))
            {
                var output = cnn.Query<ActivityLine>("select * from Activity", new DynamicParameters());
                return output.ToList();
            }
        }
        /// <summary>
        /// connects to the ActivityDB.db and queries data according the a parameter with a certian value
        /// </summary>
        /// <param name="parameter"> what we looking for, Date or Tag or App name and so on</param>
        /// <param name="value">the value of the parameter for example 23/04/2021 or "work" or "chrome" and so on </param>
        /// <returns></returns>
        public static List<ActivityLine> LoadActivities(string parameter, string value)
        {
            // apperently dapper enables me to make ActivityLine list if ActivityLine has a constructor that gets all the parameters types of the database.
            using (IDbConnection cnn = new SQLiteConnection(connectionStringActivity))
            {
                string query = "select * from Activity where "+ parameter+" = '"+value+"'";
                var output = cnn.Query<ActivityLine>(query, new DynamicParameters());
                return output.ToList();
            }
        }
        /// <summary>
        /// connects to the ActivityDB.db and queries the second to last line
        /// </summary>
        /// <returns></returns>
        public static ActivityLine LoadSecondToLastActivity()
        {
            // apperently dapper enables me to make ActivityLine list if ActivityLine has a constructor that gets all the parameters types of the database.
            using (IDbConnection cnn = new SQLiteConnection(connectionStringActivity))
            {
                string query = "select * from Activity order by ID DESC LIMIT 1,1 ";
                var output = cnn.Query<ActivityLine>(query, new DynamicParameters());
                return output.ToList().ElementAt(0);
            }
        }
        /// <summary>
        /// writes into the ActivityDB.db the activity line
        /// </summary>
        /// <param name="activity"></param>
        public static void saveActivityLine(ActivityLine activity)
        {
            using (IDbConnection cnn = new SQLiteConnection(connectionStringActivity))
            {
                cnn.Execute("insert into Activity (Date,Time,FGWindowName,FGProcessName,inAppTime,Tag) values (@Date,@Time,@FGWindowName,@FGProcessName,@inAppTime,@Tag)", activity);
            }
        }
        /// <summary>
        /// Loads from the Tags.db all the Tags defined by the user.
        /// The Tag Model is constructed by int - tagId, string -name , string - color.tostring();
        /// </summary>
        /// <returns></returns>
        public static List<TagModel> LoadTags()
        {
            // apperently dapper enables me to make ActivityLine list if ActivityLine has a constructor that gets all the parameters types of the database.
            using (IDbConnection cnn = new SQLiteConnection(connectionStringTags))
            {
                var output = cnn.Query<(int,string,string)>("select * from Tags", new DynamicParameters());
                //output.ToList()
                var list = output.ToList();
                List<TagModel> taglist = new List<TagModel>();
                foreach ((int, string, string) p in list)
                {
                    int num;
                    string name;
                    string colorstring;
                    (num, name, colorstring) = p;
                    taglist.Add(new TagModel(name, colorstring,num));
                }
                return taglist;
            }
        }
        /// <summary>
        /// Saves the Tag model into the Tags.db
        /// </summary>
        /// <param name="tagModel"></param>
        public static void saveTag(TagModel tagModel)
        {
            using (IDbConnection cnn = new SQLiteConnection(connectionStringTags))
            {
                cnn.Execute("PRAGMA foreign_keys=ON");
                cnn.Execute("insert into Tags (tagName,tagColor) values (@TagName,@TagColorString)", tagModel);
            }
        }
        /// <summary>
        /// Saves the Tag Model into the Tags.db and returns the Tag id number in the Tags.db table.
        /// This function is nessery because some tags may be deleted and The Application doesent know what tag id is assigned in thet database.
        /// </summary>
        /// <param name="tagModel"></param>
        /// <returns></returns>
        public static int saveTagAndGetId(TagModel tagModel)
        {
            using (IDbConnection cnn = new SQLiteConnection(connectionStringTags))
            {
                cnn.Execute("PRAGMA foreign_keys=ON");
                cnn.Execute("insert into Tags (tagName,tagColor) values (@TagName,@TagColorString)", tagModel);
                var output = cnn.Query<int>("select Max(id) from Tags");
                var list = output.ToList();
                return list[0];
            }
        }
        /// <summary>
        /// returns a list of rules from the Tags.db , Rule table.
        /// These rules , represented by Operation, Parameter, Constant and TagId are the functions defined by thet user.
        /// RuleModel stores these strings and later makes them into code using reflection and lambda expressions.
        /// </summary>
        /// <returns>list of rules</returns>
        public static List<RuleModel> LoadRules()
        {
            // apperently dapper enables me to make ActivityLine list if ActivityLine has a constructor that gets all the parameters types of the database.
            using (IDbConnection cnn = new SQLiteConnection(connectionStringTags))
            {
                var output = cnn.Query<RuleModel>("select rowid,Parameter,Operation,Constant,TagId from Rule", new DynamicParameters());
                var list = output.ToList();
                return list;
            }
        }
        /// <summary>
        /// Writes a rule into the rule table in Tags.db
        /// </summary>
        /// <param name="rule"></param>
        public static void saveRule(RuleModel rule)
        {
            using (IDbConnection cnn = new SQLiteConnection(connectionStringTags))
            {
                cnn.Execute("PRAGMA foreign_keys=ON");
                cnn.Execute("insert into Rule (Parameter,Operation,Constant,TagId) values (@Parameter,@Operation,@Constant,@TagId)", rule);
            }
        }
        /// <summary>
        /// Legac code, isn't relevant.
        /// This function saves the rule into the database while keeping the last rule (the everything else rule) last.
        /// I desided that the user should be aware that the everything else should always be last. instead of making my life harder and checking if there even is 
        /// </summary>
        /// <param name="rule"></param>
        public static void saveRuleLast(RuleModel rule)
        {
            var ruleList = Tagger.getRulesList();
            var lastRule = ruleList[ruleList.Count - 1];
            using (IDbConnection cnn = new SQLiteConnection(connectionStringTags))
            {
                cnn.Execute("PRAGMA foreign_keys=ON");
                cnn.Execute("DELETE FROM Rule where rowid = @RowId", lastRule);
                try
                {
                    cnn.Execute("insert into Rule (Parameter,Operation,Constant,TagId) values (@Parameter,@Operation,@Constant,@TagId)", rule);
                    cnn.Execute("insert into Rule (Parameter,Operation,Constant,TagId) values (@Parameter,@Operation,@Constant,@TagId)", lastRule);
                }
                catch(SQLiteException e)
                {
                    if (e.Message.Contains("UNIQUE"))
                    {
                        MessageBox.Show("This Rule existis already. \n Can't add same rule for different Tag!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        cnn.Execute("insert into Rule (Parameter,Operation,Constant,TagId) values (@Parameter,@Operation,@Constant,@TagId)", lastRule);
                    }
                    else
                        MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        /// <summary>
        /// Delete the Tag from the the Tags table.
        /// Also makes sure that the corresponding rules in the Rule table are deleted.
        /// I added the Delete from rule code as a just-in-case the rule table doesn't delete on cascade.
        /// you know, if its stupid but it works it aint stupid.
        /// </summary>
        /// <param name="tag"></param>
        public static void DeleteTag(TagModel tag)
        {
            using (IDbConnection cnn = new SQLiteConnection(connectionStringTags))
            {
                cnn.Execute("PRAGMA foreign_keys=ON");
                cnn.Execute("DELETE FROM Rule where TagId = @TagID", tag);
                cnn.Execute("DELETE FROM Tags where id = @TagID", tag);
            }
        }
        /// <summary>
        /// Simply deletes Rule from the Rule table.
        /// </summary>
        /// <param name="rule"></param>
        public static void DeleteRule(RuleModel rule)
        {
            using (IDbConnection cnn = new SQLiteConnection(connectionStringTags))
            {
                cnn.Execute("PRAGMA foreign_keys=ON");
                cnn.Execute("DELETE FROM Rule where rowid = @RowId", rule);

            }
        }
    }
}
