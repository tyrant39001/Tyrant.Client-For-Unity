using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Tyrant.GameCore.Data;
using Tyrant.Server;

#if DEBUG
Tyrant.GameCore.Net.RPCExecManager.IsDebug = true;
#endif
new HallServer().Start(args.Length == 0 ? null : args[0]);

public class HallServer : HallServerBase
{
    public override Action<DbContextOptionsBuilder, string, string, string, string, ushort, Action<DbConnectionStringBuilder, DbConnection>, Action<IRelationalDbContextOptionsBuilderInfrastructure>> OnGetDatabaseAdapterAction() => Tyrant.EntityFrameworkCore.MySql.Extension.Adapter.UseDatabase;

    protected override AccountDataAssist CreateAccountDataAssist() => new Account_Module.AccountInfoAssist();

    protected override bool OnGetIsSingleRole() => true;
    protected override PlayerDataAssist OnCreatePlayerDataAssist() => null;
}