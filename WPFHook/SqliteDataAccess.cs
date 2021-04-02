using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace WPFHook
{
    class SqliteDataAccess
    {
        private string connectionString;
        public SqliteDataAccess()
        {
            connectionString = @"Data Source =.\ActivityDB.db; Version = 3;";
        }
        public List<ActivityLine> LoadActivities()
        {
            using(IDbConnection cnn =new SQLiteConnection(connectionString))
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
