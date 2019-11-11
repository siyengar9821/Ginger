using Amdocs.Ginger.Plugin.Core.ActionsLib;
using Ginger.Plugin.Platform;
using GingerCoreNET.RunLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.DatabaseLib
{
    public class DatabaseProxy : IDatabase
    {
        // GingerNodeProxy gingerNodeProxy = new GingerNodeProxy()
        public string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IPlatformActionHandler PlatformActionHandler { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
