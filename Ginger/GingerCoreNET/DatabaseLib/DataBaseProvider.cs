using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.DataBaseLib;
using Amdocs.Ginger.Repository;
using Ginger.Plugin.Platform.Database;
using GingerCore.Environments;
using System;
using System.IO;
using System.Reflection;

namespace Amdocs.Ginger.CoreNET.DatabaseLib
{
    class DatabaseProvider : IDBProvider
    {
        public IDatabase GetDBImpl(Database database)
        {
            // TODO: 
            // Start Plugin
            // Save GingerNodeProxy on DatabaseImpl
            // Provide Idatabase which goes via socket to the DB plugin to run DB actions


            // Support old style - auto convert to ServiceID
            if (database.ServiceID == null)
            {
                UpdateServiceIDFromDBType(database);

            }


            PluginPackage pluginPackage;
            IDatabase databaseImpl = null;

            if (database.ServiceID != null)
            {
                pluginPackage = WorkSpace.Instance.PlugInsManager.GetDatabasePluginPackage(database.ServiceID);
                string fileName = Path.Combine(pluginPackage.Folder, pluginPackage.StartupDLL);
                Assembly assembly = Assembly.LoadFrom(fileName);
                string serviceClass = GetServiceClass(database.ServiceID);
                databaseImpl = (IDatabase)assembly.CreateInstance(serviceClass);
                if (databaseImpl == null)
                {
                    throw new Exception("Cannot create class: " + serviceClass);
                }
                return databaseImpl;
            }


            //TODO: FIXME: Temp until we switch to db run using sockets !!!

            return databaseImpl;
        }



        private void UpdateServiceIDFromDBType(Database database)
        {
            switch (database.DBType)
            {
                case Database.eDBTypes.MSAccess:
                    database.ServiceID = "MSAccessService";
                    break;
                case Database.eDBTypes.MySQL:
                    database.ServiceID = "MySQLDatabaseService";
                    break;
                case Database.eDBTypes.Oracle:
                    database.ServiceID = "OracleService";
                    break;
                case Database.eDBTypes.MongoDb:
                    database.ServiceID = "MongoDB";
                    break;


                    // TODO: all the rest
            }
        }

        // TODO: temp remove me - make generic get the class from plugin using reflection
        private string GetServiceClass(string serviceId)
        {
            switch (serviceId)
            {
                case "MSAccessService":
                    return "MSAccessDB.MSAccessDBCon";
                case "MySQLDatabaseService":
                    return "MySQLDatabase.MYSQLDatabaseService";
                case "OracleService":
                    return "Oracle.GingerOracleConnection";
                case "MongoDBService":
                    return "MongoDB.MongoDbConnection";
                default:
                    throw new ArgumentException(serviceId);
            }

        }
    }
}