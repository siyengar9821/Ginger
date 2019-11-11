using Ginger.Plugin.Platform;
using GingerCore.Environments;

namespace Amdocs.Ginger.Common.DataBaseLib
{
    public interface IDBProvider
    {
        IDatabase GetDBImpl(Database database);
    }
}
