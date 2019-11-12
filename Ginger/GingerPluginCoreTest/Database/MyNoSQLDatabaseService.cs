using Amdocs.Ginger.Plugin.Core.ActionsLib;
using Ginger.Plugin.Platform;
using System;
using System.Collections.Generic;

namespace GingerPluginCoreTest.Database
{

    public class MyNoSQLDatabaseService : IDatabase, INoSQLDatabase
    {
        public string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IPlatformActionHandler PlatformActionHandler { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public List<string> ExecuteQuery(string query)
        {
            throw new NotImplementedException();
        }
    }
}
