using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data.Common;
using Tyrant.GameCore.Data;

#if DEBUG
Tyrant.GameCore.Net.RPCExecManager.IsDebug = true;
#endif
new MasterServer().Start(args.Length == 0 ? null : args[0]);

public class MasterServer : Tyrant.Server.MasterServerBase
{
    public override Action<DbContextOptionsBuilder, string, string, string, string, ushort, Action<DbConnectionStringBuilder, DbConnection>, Action<IRelationalDbContextOptionsBuilderInfrastructure>> OnGetDatabaseAdapterAction() => Tyrant.EntityFrameworkCore.MySql.Extension.Adapter.UseDatabase;

    private ServerGroupData serverGroupData;
    protected override List<ServerGroupData> OnGetServerGroupDatas()
    {
        if (serverGroupData == null)
        {
            using (var fs = File.Open("ServerGroupsData.json", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    serverGroupData = new JsonSerializer().Deserialize(sr, typeof(ServerGroupData)) as ServerGroupData;
                }
            }
        }

        // 返回服务器组或大区列表
        return new List<ServerGroupData> { serverGroupData };
    }
}
