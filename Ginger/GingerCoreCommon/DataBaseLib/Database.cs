
#region License
/*
Copyright © 2014-2019 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.DataBaseLib;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Plugin.Core.DatabaseLib;
using Amdocs.Ginger.Repository;
using GingerCore.DataSource;

namespace GingerCore.Environments
{
    public class Database : RepositoryItemBase 
    {
        IDatabase mDatabaseImpl;

        // For SQL Database
        // In case we reuse the connection
        DbConnection mDbConnection;

        ISQLDatabase mSQLDatabaseImpl { get { return (ISQLDatabase)mDatabaseImpl; } }

        // TODO: change to Service ID

        public enum eDBTypes
        {
            Oracle,
            MSSQL,            
            MSAccess,
            DB2,
            Cassandra,
            PostgreSQL,
            MySQL,
            Couchbase,
            MongoDb,
        }

        

        public enum eConfigType
        {
            Manual = 0,
            ConnectionString =1,            
        }

        public ProjEnvironment ProjEnvironment { get; set; }
       
        private BusinessFlow mBusinessFlow;
        public BusinessFlow BusinessFlow
        {
            get { return mBusinessFlow; }
            set
            {
                if (!object.ReferenceEquals(mBusinessFlow, value))
                {
                    mBusinessFlow = value;
                }
            }
        }

        

        public ObservableList<DataSourceBase> DSList { get; set; }
        public bool mKeepConnectionOpen;
        [IsSerializedForLocalRepository(true)]
        public bool KeepConnectionOpen
        {
            get
            {
                
                return mKeepConnectionOpen;
            }
            set
            {
                mKeepConnectionOpen = value;
                OnPropertyChanged(nameof(KeepConnectionOpen));
            }
        }


        private string mName;
        [IsSerializedForLocalRepository]
        public string Name { get { return mName; } set { mName = value; OnPropertyChanged(nameof(Name)); } }

        [IsSerializedForLocalRepository]
        public string Description { get; set; }


        private string mServiceID;
        [IsSerializedForLocalRepository]
        public string ServiceID { get { return mServiceID; } set { mServiceID = value; OnPropertyChanged(nameof(ServiceID)); } }


        [IsSerializedForLocalRepository]
        public ObservableList<DatabaseParam> DBParmas { get; set; } = new ObservableList<DatabaseParam>();
        

        // TODO: Obsolete remove after moving to DB pluguins
        public eDBTypes mDBType;
        [IsSerializedForLocalRepository]
        public eDBTypes DBType 
        { 
            get 
            { 
                return mDBType; 
            }
            set 
            {
                // TODO: update service ID and add relevant DB plugin

                if (mDBType != value)
                {
                    mDatabaseImpl = null;
                    mDBType = value;
                    OnPropertyChanged(nameof(DBType));

                    // TODO: remove from here
                    if (DBType == eDBTypes.Cassandra)
                    {
                        DBVer = "2.2";
                    }
                    else
                    {
                        DBVer = "";
                    }
                }
            } 
        }


        IValueExpression mVE = null;
        IValueExpression VE
        {
            get
            {
                if (mVE == null)
                {
                    if (ProjEnvironment == null)
                    {
                        ProjEnvironment = new ProjEnvironment();
                    }

                    if (BusinessFlow == null)
                    {
                        BusinessFlow = new BusinessFlow();
                    }

                    mVE = RepositoryItemHelper.RepositoryItemFactory.CreateValueExpression(ProjEnvironment, BusinessFlow, DSList);
                }
                return mVE;
            }
            set
            {
                mVE = value;
            }
        }

        private string mConnectionString;
        [IsSerializedForLocalRepository]
        public string ConnectionString { get { return mConnectionString; } set { mConnectionString = value; OnPropertyChanged(nameof(ConnectionString)); } }
        public string ConnectionStringCalculated
        {
            get
            {
                VE.Value = ConnectionString;
                return mVE.ValueCalculated;
            }
        }

        private string mTNS;
        [IsSerializedForLocalRepository]
        public string TNS  {  get  { return mTNS; } set { mTNS = value; OnPropertyChanged(nameof(TNS)); } }
        public string TNSCalculated
        {
            get
            {
                VE.Value = TNS;
                return mVE.ValueCalculated;
            }
        }

        private string mDBVer;
        [IsSerializedForLocalRepository]
        public string DBVer { get { return mDBVer; } set { mDBVer = value; OnPropertyChanged(nameof(DBVer)); } }

        private string mUser;
        [IsSerializedForLocalRepository]
        public string User { get { return mUser; } set { mUser = value; OnPropertyChanged(nameof(User)); } }
        public string UserCalculated
        {
            get
            {
                VE.Value = User;
                return mVE.ValueCalculated;
            }
        }

        private string mPass;
        [IsSerializedForLocalRepository]
        public string Pass { get { return mPass; } set { mPass = value; OnPropertyChanged(nameof(Pass)); } }
        public string PassCalculated
        {
            get
            {
                VE.Value = Pass;
                return mVE.ValueCalculated;
            }
        }

        //TODO: Why it is needed?!
        public static List<string> DbTypes 
        {
            get
            {
                return Enum.GetNames(typeof(eDBTypes)).ToList();
            }
            set
            {
                //DbTypes = value;
            }
        }

        public string NameBeforeEdit;

       
        
        
        private DateTime LastConnectionUsedTime;


        //private bool MakeSureConnectionIsOpen()
        //{
        //    Boolean isCoonected = true;

        //    if ((oConn == null) || (oConn.State != ConnectionState.Open))
        //        isCoonected= Connect();

        //    //make sure that the connection was not refused by the server               
        //    TimeSpan timeDiff = DateTime.Now - LastConnectionUsedTime;
        //    if (timeDiff.TotalMinutes > 5)
        //    {
        //        isCoonected= Connect();                
        //    }
        //    else
        //    {
        //        LastConnectionUsedTime = DateTime.Now;                
        //    }
        //    return isCoonected;
        //}
        
        public static string GetMissingDLLErrorDescription()
        {
            string message = "Connect to the DB failed." + Environment.NewLine + "The file Oracle.ManagedDataAccess.dll is missing," + Environment.NewLine + "Please download the file, place it under the below folder, restart Ginger and retry." + Environment.NewLine + AppDomain.CurrentDomain.BaseDirectory + Environment.NewLine + "Links to download the file:" + Environment.NewLine + "https://docs.oracle.com/database/121/ODPNT/installODPmd.htm#ODPNT8149" + Environment.NewLine + "http://www.oracle.com/technetwork/topics/dotnet/downloads/odacdeploy-4242173.html";
            return message;
        }




        public Boolean TestConnection()
        {
            VerifyDBImpl();
            DbConnection dbConnection =  mSQLDatabaseImpl.GetDbConnection();            
            dbConnection.Open();
            bool b = dbConnection.State == ConnectionState.Open;
            dbConnection.Close();
            return b;

            // Add try catch !!!!!!!!!!!!!!!!!!
        }

            
       

        public static IDBProvider iDBProvider { get; set; }

        void VerifyDBImpl()
        {            
            if (mDatabaseImpl != null) 
            {
                UpdateDBImplFromParams();
                return;
            }
            
            //TODO: Add check that the db is as DBType else replace or any prop change then reset conn string


            if (iDBProvider == null)
            {
                throw new ArgumentNullException("iDBProvider cannot be null and must be initialized");
            }
            
            mDatabaseImpl = iDBProvider.GetDBImpl(this);
            UpdateDBParamsFromDBImpl();
            UpdateDBImplFromParams();
        }

        private void UpdateDBParamsFromDBImpl()
        {
            PropertyInfo[] properties = mDatabaseImpl.GetType().GetProperties();
            foreach (PropertyInfo propertyInfo in properties)
            {
                DatabaseParamAttribute attr = (DatabaseParamAttribute)propertyInfo.GetCustomAttribute(typeof(DatabaseParamAttribute));
                if (attr != null)
                {
                    // Add only missing params, will happen when Database is first created
                    DatabaseParam databaseParam = (from x in DBParmas where x.Name == attr.Param select x).SingleOrDefault();
                    if (databaseParam == null)
                    {
                        // TODO: get default value !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        DBParmas.Add(new DatabaseParam() { Name = propertyInfo.Name });
                    }
                    else
                    {
                        // Update impl
                    }
                }
            }
        }

        private void UpdateDBImplFromParams()
        {            
            if (!string.IsNullOrEmpty(ConnectionString ))
            {
                mDatabaseImpl.ConnectionString = ConnectionString;                
            }


            // Update params using reflection
            foreach (DatabaseParam databaseParam in DBParmas)
            {
                PropertyInfo propertyInfo = mDatabaseImpl.GetType().GetProperty(databaseParam.Name);
                if (databaseParam.Value != null)
                {
                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        propertyInfo.SetValue(mDatabaseImpl, databaseParam.Value);
                    }
                    else if (propertyInfo.PropertyType == typeof(int))
                    {
                        propertyInfo.SetValue(mDatabaseImpl, int.Parse((string)databaseParam.Value));
                    }

                    // TODO: handle other types
                }

            }
        }

        public Boolean Connect(bool displayErrorPopup = false)
        {
            //VerifyDBImpl();
            //if (mDatabaseImpl != null)
            //{
            //    return mDatabaseImpl.OpenConnection(); 
            //}
            //else
            //{
            //    return false;
            //}
            return true;

        }
       
        public void CloseConnection()
        {
            //try
            //{
            //    if (mDatabaseImpl != null)
            //    {
            //        mDatabaseImpl.CloseConnection();
            //    }
            //}
            //catch (Exception e)
            //{
            //    Reporter.ToLog(eLogLevel.ERROR, "Failed to close DB Connection", e);
            //    throw;
            //}            
        }

       

        public List<string> GetTablesList() // string Keyspace = null
        {
            VerifyDBImpl();

            DbConnection dbConnection = mSQLDatabaseImpl.GetDbConnection();            
            dbConnection.Open();
            DataTable dataTable = dbConnection.GetSchema("Tables");
            dbConnection.Close();
            List<string> tables = mSQLDatabaseImpl.GetTablesList(dataTable);

            return tables;

        }


        public List<string> GetTablesColumns(string table)
        {
            //VerifyDBImpl();
            //mDatabaseImpl.OpenConnection();
            //List<string> columns = mSQLDatabaseImpl.GetTablesColumns(table); ;
            //mDatabaseImpl.CloseConnection();
            //return columns;
            return null;
        }


        
        
        // For SQL database
        public string GetSingleValue(string table, string column, string where)
        {         
            string sql = $"SELECT {column} FROM {table} WHERE {where}";
            DataTable dataTable = ExecuteQuery(sql);
            return dataTable.Rows[0][0].ToString();            
        }


        

        // For SQL database
        public long GetRecordCount(string query)
        {            
            string sql = $"SELECT COUNT(1) FROM {query}";
            DataTable dataTable = ExecuteQuery(sql);
            return long.Parse(dataTable.Rows[0][0].ToString());            
        }


        public override string ItemName
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }


        // For SQLDatatbase
        public virtual DataTable ExecuteQuery(string query)
        {
            VerifyDBImpl();

            DbConnection dbConnection = mSQLDatabaseImpl.GetDbConnection();            
            dbConnection.Open();
            DbCommand dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = query;
            DbDataReader rr = dbCommand.ExecuteReader();
            DataTable dataTable = new DataTable();
            dataTable.Load(rr);
            dbConnection.Close();
            return dataTable;
        }

        // !!!!!!!!!!!!!!!!! cleanup
        //public int UpdateDB(string updateCmd, bool commit)   // commit !???
        //{
        //    VerifyDBImpl();

        //    // mDbConnection.ConnectionString = mDatabaseImpl.ConnectionString;
        //    mDbConnection.Open();
        //    DbCommand dbCommand = mDbConnection.CreateCommand();
        //    dbCommand.CommandText = updateCmd;
        //    int count = dbCommand.ExecuteNonQuery();
        //    mDbConnection.Close();
        //    return count;
        //}


        public virtual int ExecuteNonQuery(string command)
        {
            DbCommand dbCommand = mDbConnection.CreateCommand();
            dbCommand.CommandText = command;
            // dbCommand.CommandTimeout
            int rows = dbCommand.ExecuteNonQuery();
            return rows;
        }

    }
}
