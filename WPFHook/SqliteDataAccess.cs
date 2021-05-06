using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace WPFHook
{
    /// <summary>
    /// The connection to the data base.
    /// should make into bigger class that enables me to get spesific activities and so on.
    /// uses dapper and SQLite.core.
    /// ow can only write to the database and read all of the database.
    /// </summary>
    class SqliteDataAccess
    {
        private string connectionString;
        public SqliteDataAccess()
        {
            connectionString = @"Data Source =.\ActivityDB.db; Version = 3;";
        }
        /// <summary>
        /// connects to the ActivityDB.db and queries the whole database
        /// </summary>
        /// <returns>List of ActivityLines that the database has (the whole database)</returns>
        public List<ActivityLine> LoadActivities()
        {
            // apperently dapper enables me to make ActivityLine list if ActivityLine has a constructor that gets all the parameters types of the database.
            using (IDbConnection cnn =new SQLiteConnection(connectionString))
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
        public List<ActivityLine> LoadActivities(string parameter, string value)
        {
            // apperently dapper enables me to make ActivityLine list if ActivityLine has a constructor that gets all the parameters types of the database.
            using (IDbConnection cnn = new SQLiteConnection(connectionString))
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
        public ActivityLine LoadSecondToLastActivity()
        {
            // apperently dapper enables me to make ActivityLine list if ActivityLine has a constructor that gets all the parameters types of the database.
            using (IDbConnection cnn = new SQLiteConnection(connectionString))
            {
                string query = "select * from Activity order by ID DESC LIMIT 1,1 ";
                var output = cnn.Query<ActivityLine>(query, new DynamicParameters());
                return output.ToList().ElementAt(0);
            }
        }

        public void saveActivityLine(ActivityLine activity)
        {
            using (IDbConnection cnn = new SQLiteConnection(connectionString))
            {
                cnn.Execute("insert into Activity (Date,Time,FGWindowName,FGProcessName,inAppTime,Tag) values (@Date,@Time,@FGWindowName,@FGProcessName,@inAppTime,@Tag)", activity);
            }
        }

    }
}
