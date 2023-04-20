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
}