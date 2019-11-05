using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core.DatabaseLib
{
    public interface ISQLDatabase
    {
        DbConnection GetDbConnection();

        // DataTable ExecuteQuery(string query); //  int? timeout = null : TODO // Return Data table         


        List<string> GetTablesList(DataTable TablesSchema);

        List<string> GetTableColumns(string table);
        
        // Int64 GetRecordCount(string Query);
    }
}
