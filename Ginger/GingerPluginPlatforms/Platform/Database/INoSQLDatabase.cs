using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform
{
    public interface INoSQLDatabase
    {
        List<string> ExecuteQuery(string query);

        
    }
}
