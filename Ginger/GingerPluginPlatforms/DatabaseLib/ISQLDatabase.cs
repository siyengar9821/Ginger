using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Ginger.Plugin.Platform.Database
{
    public interface ISQLDatabase
    {
        DbConnection GetDbConnection();

        List<string> GetTablesList(DataTable tablesSchema);

        List<string> GetTableColumns(DataTable tableSchema);
                
    }
}
