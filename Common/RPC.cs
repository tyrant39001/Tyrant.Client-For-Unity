using Tyrant.GameCore.Net;

namespace Great_Wisdom_Common
{
    [RPCParameter(IsConcurrent = true)]
    public class C2S_GetRoomsList : RPCAuxParameterReturn<C2S_GetRoomsList, C2S_GetRoomsList.Return>
    {
        public int PageIndex { get; set; }
        public class Return : RPCReturnValue
        {
            public RoomInfo[] RoomInfos { get; set; }
        }
    }

    public class RoomInfo : RPCSerialize
    {
        public int MapIndex { get; set; }
        public bool NeedPassword { get; set; }
        public string Desc { get; set; }
        public string Creater { get; set; }
        public int CurrentPlayers { get; set; }
    }
}