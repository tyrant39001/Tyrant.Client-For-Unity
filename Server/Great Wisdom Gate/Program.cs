using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

#if DEBUG
Tyrant.GameCore.Net.RPCExecManager.IsDebug = true;
#endif
new GateServer().Start(args.Length == 0 ? null : args[0]);

public class GateServer : Tyrant.Server.GateServerBase
{
    public override Action<DbContextOptionsBuilder, string, string, string, string, ushort, Action<DbConnectionStringBuilder, DbConnection>, Action<IRelationalDbContextOptionsBuilderInfrastructure>> OnGetDatabaseAdapterAction() => Tyrant.EntityFrameworkCore.MySql.Extension.Adapter.UseDatabase;
}