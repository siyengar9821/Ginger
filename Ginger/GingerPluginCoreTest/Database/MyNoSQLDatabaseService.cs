using Ginger.Plugin.Platform.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace GingerPluginCoreTest.Database
{

    

    public class MyNoSQLDatabaseService : IDatabase, INoSQLDatabase
    {
        public string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public List<string> ExecuteQuery(string query)
        {
            throw new NotImplementedException();
        }
    }
}
