using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data.Common;
using Tyrant.GameCore;
using Tyrant.Server;
using Great_Wisdom_Common;
using Tyrant.GameCore.Data;
using Tyrant.GameCore.Net;

#if DEBUG
Tyrant.GameCore.Net.RPCExecManager.IsDebug = true;
#endif
new WorldServer().Start(args.Length == 0 ? null : args[0]);

public class WorldServer : WorldServerBase
{
    public override Action<DbContextOptionsBuilder, string, string, string, string, ushort, Action<DbConnectionStringBuilder, DbConnection>, Action<IRelationalDbContextOptionsBuilderInfrastructure>> OnGetDatabaseAdapterAction() => Tyrant.EntityFrameworkCore.MySql.Extension.Adapter.UseDatabase;

    protected override Tyrant.Server.MapInfo OnGetMapInHallServerToEnter(ref HallServerInfoManager serverInfoManager)
    {
        if (serverInfoManager.ExEnterMapParamData is CreateRoomParam) // 创建房间
            return serverInfoManager.CreateOnMinPressureInstance();
        else if (serverInfoManager.ExEnterMapParamData is EnterRoomParam enterRoomParam) // 进入房间
        {
            var result = serverInfoManager.GetMapInfoWithId(enterRoomParam.MapInstanceId);
            if (result == null)
                throw new InvalidDataException($"can not find room id {enterRoomParam.MapInstanceId} when enter room");
            return result;
        }
        else
            throw new InvalidDataException("unknown EnterMapParamData when enter room");
    }

    protected override Tyrant.Server.MapInfo OnCreateMapInfo(ref HallServerInfoManager hallServerInfoManager, HallOnWorldServerInfo info)
        => ObjectPool<MapInfo>.Get();

    private const int ItemsCountPerPageInRoomsList = 2;
    public void GetRoomsList(C2S_GetRoomsList arg)
    {
        Tyrant.GameCore.Debug.Output("GetRoomsList On World");
        var allMapDatas = GetMapInfos().ToArray();
        var currentPage = arg.PageIndex;
        var totalPages = (int)Math.Ceiling(((float)allMapDatas.Length) / ItemsCountPerPageInRoomsList); 
        var skipCount = currentPage * ItemsCountPerPageInRoomsList;
        IEnumerable<(MapData, int)> mapDatas = null;
        if (allMapDatas.Length <= skipCount)
        {
            currentPage = totalPages - 1;
            mapDatas = allMapDatas.TakeLast(ItemsCountPerPageInRoomsList);
        }
        else
            mapDatas = allMapDatas.Skip(arg.PageIndex * ItemsCountPerPageInRoomsList).Take(ItemsCountPerPageInRoomsList);
        arg.DoReturn(new C2S_GetRoomsList.Return
            {
                CurrentPage = currentPage,
                TotalPages = totalPages,
                RoomInfos = mapDatas.Select(i => new RoomInfo
                    {
                        InstanceId = i.Item1.MapId,
                        MapIndex = i.Item1.MapIndex,
                        NeedPassword = !string.IsNullOrEmpty(i.Item1.Password),
                        Desc = i.Item1.Desc,
                        Creater = i.Item1.Creater,
                        CurrentPlayers = i.Item2,
                }).ToArray()
            });
    }

    private IEnumerable<(MapData, int)> GetMapInfos()
    {
        foreach (var serverInfo in MapHallServerds.Where(i => !i.Invalid))
        {
            foreach (MapInfo mapInfo in serverInfo.MapInfos)
            {
                if (mapInfo == null)
                    continue;
                if (mapInfo.MapData is MapData mapData && mapData.IsPublic)
                    yield return (mapData, mapInfo.PlayerNums);
            }
        }
    }
}

public class MapInfo : Tyrant.Server.MapInfo
{
    protected override MapData OnCreateMapData(ref HallServerInfoManager hallServerInfoManager)
    {
        if (hallServerInfoManager.ExEnterMapParamData is CreateRoomParam createRoomParam)
        {
            var result = ObjectPool<MapData>.Get();
            result.MapIndex = createRoomParam.MapIndex;
            result.Password = createRoomParam.Password;
            result.Desc = createRoomParam.Desc;
            result.IsPublic = createRoomParam.IsPublic;
            result.Creater = createRoomParam.Creater;
            return result;
        }
        else
            throw new InvalidDataException("unknown EnterMapParamData when create map data");
    }
}

public class MapData : Tyrant.GameCore.Data.MapData
{
    public int MapIndex { get; set; }
    public string Password { get; set; }
    public string Desc { get; set; }
    public bool IsPublic { get; set; }
    public string Creater { get; set; }
}