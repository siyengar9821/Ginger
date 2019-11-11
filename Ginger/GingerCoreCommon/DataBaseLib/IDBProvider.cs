using Ginger.Plugin.Platform.Database;
using GingerCore.Environments;

namespace Amdocs.Ginger.Common.DataBaseLib
{
    public interface IDBProvider
    {
        IDatabase GetDBImpl(Database database);
    }
}
