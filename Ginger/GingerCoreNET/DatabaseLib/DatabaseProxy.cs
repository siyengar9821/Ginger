using Amdocs.Ginger.Common.Actions;
using Amdocs.Ginger.Common.DataBaseLib;
using Amdocs.Ginger.CoreNET.Run;
using Amdocs.Ginger.Plugin.Core.ActionsLib;
using GingerCore.Actions;
using GingerCore.Environments;
using System;
using System.Collections.Generic;
using System.Data;

namespace Amdocs.Ginger.CoreNET.DatabaseLib
{
    public class DatabaseProxy : IDatabaseProxy
    {
        Database mDatabase;
        // GingerNodeProxy gingerNodeProxy = new GingerNodeProxy();


        public DatabaseProxy(Database database)
        {
            mDatabase = database;
        }

        
        public string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IPlatformActionHandler PlatformActionHandler { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int ExecuteNonQuery(string command)
        {
            throw new NotImplementedException();
        }

        public DataTable ExecuteQuery(string query)
        {
            throw new NotImplementedException();
        }

        public List<string> GetTablesList()
        {
            throw new NotImplementedException();
        }

        public bool TestConnection()
        {
            ActDatabase actDatabase = new ActDatabase();
            actDatabase.PluginId = "OracleDatabasePlugin"; // mDatabase.ServiceID;
            actDatabase.ServiceId = mDatabase.ServiceID;
            actDatabase.ActionId = "TestConnection";
            // ExecuteOnPlugin.FindNodeAndRunAction(actDatabase);
            // ExecuteOnPlugin.ExecutePlatformAction(mDatabase., actDatabase);
            ExecuteOnPlugin.FindNodeAndRunAction(actDatabase);
            if (actDatabase.Status == Execution.eRunStatus.Passed)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
