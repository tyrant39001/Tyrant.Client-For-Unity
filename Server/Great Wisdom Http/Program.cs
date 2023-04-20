using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Tyrant.GameCore.Data;

#if DEBUG
Tyrant.GameCore.Net.RPCExecManager.IsDebug = true;
#endif
new HttpServer().Start(args.Length == 0 ? null : args[0]);

public class HttpServer : Tyrant.Server.HttpServer
{
    public override Action<DbContextOptionsBuilder, string, string, string, string, ushort, Action<DbConnectionStringBuilder, DbConnection>, Action<IRelationalDbContextOptionsBuilderInfrastructure>> OnGetDatabaseAdapterAction() => Tyrant.EntityFrameworkCore.MySql.Extension.Adapter.UseDatabase;

    protected override Tyrant.Server.AccountDataAssist CreateAccountDataAssist() => new Account_Module.AccountInfoAssist();
}