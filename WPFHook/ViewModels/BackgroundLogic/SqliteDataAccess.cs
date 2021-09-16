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
    /// should make into bigger class that enables me to get spesific activities and so on.
    /// uses dapper and SQLite.core.
    /// ow can only write to the database and read all of the database.
    /// </summary>
    public static class SqliteDataAccess
    {
        private static string connectionStringActivity;
        private static string connectionStringTags;
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

        public static void saveActivityLine(ActivityLine activity)
        {
            using (IDbConnection cnn = new SQLiteConnection(connectionStringActivity))
            {
                cnn.Execute("insert into Activity (Date,Time,FGWindowName,FGProcessName,inAppTime,Tag) values (@Date,@Time,@FGWindowName,@FGProcessName,@inAppTime,@Tag)", activity);
            }
        }
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
        public static void saveTag(TagModel tagModel)
        {
            using (IDbConnection cnn = new SQLiteConnection(connectionStringTags))
            {
                cnn.Execute("PRAGMA foreign_keys=ON");
                cnn.Execute("insert into Tags (tagName,tagColor) values (@TagName,@TagColorString)", tagModel);
            }
        }
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
        public static void saveRule(RuleModel rule)
        {
            using (IDbConnection cnn = new SQLiteConnection(connectionStringTags))
            {
                cnn.Execute("PRAGMA foreign_keys=ON");
                cnn.Execute("insert into Rule (Parameter,Operation,Constant,TagId) values (@Parameter,@Operation,@Constant,@TagId)", rule);
            }
        }
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
        public static void DeleteTag(TagModel tag)
        {
            using (IDbConnection cnn = new SQLiteConnection(connectionStringTags))
            {
                cnn.Execute("PRAGMA foreign_keys=ON");
                cnn.Execute("DELETE FROM Rule where TagId = @TagID", tag);
                cnn.Execute("DELETE FROM Tags where id = @TagID", tag);
            }
        }

    }
}
