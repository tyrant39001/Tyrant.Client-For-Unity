using System;
using Tyrant.GameCore.Net;

namespace Great_Wisdom_Common
{
    public class AccountData : Tyrant.GameCore.Data.AccountData
    {
        public string Name { get; set; }
        [Exclude]
        public string Password { get; set; } // 密码是敏感数据，附加ExcludeAttribute属性后密码不会发送给客户端
    }

    public class RegistAccountParam : RegistAccountParamExtra
    {
        public string Password { get; set; }
    }

    public class LoginAccountParam : LoginAccountExtraParameter
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }

    public class ReturnDataAfterLogin : LoginReturnToClient
    {
        public string Name { get; set; }
    }

    public class CreateRoomParam : RPCMapParamExtra
    {
        /// <summary>
        /// 定义索引，例如：1为新手村，2为某某镇
        /// </summary>
        public int MapIndex { get; set; }
        public string Password { get; set; }
        public string Desc { get; set; }
        public bool IsPublic { get; set; }
        public string Creater { get; set; }
    }

    public class EnterRoomParam : RPCMapParamExtra
    {
        /// <summary>
        /// 地图实例索引等价于房间实例索引
        /// </summary>
        public Guid RoomInstanceId { get; set; }
    }
}