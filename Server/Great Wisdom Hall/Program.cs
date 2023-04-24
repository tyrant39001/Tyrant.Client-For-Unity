using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Tyrant.GameCore.Data;
using Tyrant.Server;
using Great_Wisdom_Common;
using Tyrant.GameCore.Net;
using System.Diagnostics;

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

    //protected override RPCError OnCheckCanEnterMap(ulong accountId, ulong RoleId, RPCMapParamExtra ExParamData)
    //{
    //    // 这里做能不能创建或进入房间的角色属性判断，比如等级限制等
    //    if (ExParamData is CreateRoomInfo)
    //    {

    //    }
    //    return base.OnCheckCanEnterMap(accountId, RoleId, ExParamData);
    //}

    public async void GetRoomsList(C2S_GetRoomsList arg)
    {
        Tyrant.GameCore.Debug.Output("GetRoomsList On Hall Concurrent");
        var result = await arg.S2S_CallAsync(ERouteTarget.World);
        arg.DoReturn(result);
    }
}