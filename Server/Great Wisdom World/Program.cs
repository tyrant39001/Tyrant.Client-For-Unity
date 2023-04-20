using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data.Common;
using Tyrant.GameCore;
using Tyrant.Server;

#if DEBUG
Tyrant.GameCore.Net.RPCExecManager.IsDebug = true;
#endif
new WorldServer().Start(args.Length == 0 ? null : args[0]);

public class WorldServer : WorldServerBase
{
    public override Action<DbContextOptionsBuilder, string, string, string, string, ushort, Action<DbConnectionStringBuilder, DbConnection>, Action<IRelationalDbContextOptionsBuilderInfrastructure>> OnGetDatabaseAdapterAction() => Tyrant.EntityFrameworkCore.MySql.Extension.Adapter.UseDatabase;

    protected override Tyrant.Server.MapInfo OnCreateMapInfo(ref HallServerInfoManager hallServerInfoManager, HallOnWorldServerInfo info)
        => ObjectPool<MapInfo>.Get();
}

public class MapInfo : Tyrant.Server.MapInfo
{
    public int TemplateIndex { get; set; }
}