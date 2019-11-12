using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Amdocs.Ginger.Common.DataBaseLib
{
    public interface IDatabaseProxy
    {
        string ConnectionString { get; set; }
        bool TestConnection();
        List<string> GetTablesList();
        DataTable ExecuteQuery(string query);
        int ExecuteNonQuery(string command);
    }
}
