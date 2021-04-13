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
        public List<ActivityLine> LoadActivities()
        {
            // apperently dapper enables me to make ActivityLine list if ActivityLine has a constructor that gets all the parameters types of the database.
            using (IDbConnection cnn =new SQLiteConnection(connectionString))
            {
                var output = cnn.Query<ActivityLine>("select * from Activity", new DynamicParameters());
                return output.ToList();
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
