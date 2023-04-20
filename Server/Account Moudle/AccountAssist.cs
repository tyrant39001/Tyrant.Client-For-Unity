using Great_Wisdom_Common;
using System.Linq.Expressions;
using Tyrant.GameCore.Net;
using Tyrant.Server;

namespace Account_Module
{
    public partial class AccountInfoAssist : AccountDataAssist<AccountData, RegistAccountParam, LoginAccountParam, AccountInfoAssist.DbContext>
    {
        protected override bool HasMultyDbContextWithThread => true;

        protected override Expression<Func<AccountData, bool>> OnDetermineAccountDataExpressionWhenLogin(LoginContext loginContext)
        {
            return i => i.Name == loginContext.LoginParam.Name && i.Password == loginContext.LoginParam.Password;
        }

        protected override bool OnGetIsAutoRegist(LoginContext loginContext) => true;

        protected override (string, RegistAccountParam) OnTransformLoginParameterToRegistParam(LoginContext loginContext)
            => (loginContext.LoginParam.Name, new RegistAccountParam { Password = loginContext.LoginParam.Password });

        protected override bool OnCheckNameWhenRegistAccount(ref string accountName, RegistContext registContext)
            => accountName.Length >= 4;

        protected override RPCError OnCheckRegistArg(string accountName, RegistContext registContext)
            => registContext.RegistParam.Password.Length >= 4 ? RPCError.OK : (RPCError)2;

        protected override AccountData OnCreateAccountWhenRegist(string accountName, RegistContext registContext)
            => new AccountData { Name = accountName, Password = registContext.RegistParam.Password };

        protected override bool OnHandleCustomErrorWhenSaveToDatabase(Exception exception, ref RPCError rpcError, RegistContext registContext)
        {
            if (exception != null)
            {
                if (exception.InnerException is MySqlConnector.MySqlException mySqlException)
                {
                    var keyName = Tyrant.EntityFrameworkCore.MySql.Extension.MySqlHelper.GetKeyNameFromDuplicateEntryExceptionMessage(mySqlException.Message);
                    switch (keyName)
                    {
                        case $"IX_{nameof(AccountData)}_Name":
                            rpcError = (RPCError)3; // 3为自定义的检查编号，含义为账号名已存在
                            return true;
                    }
                }
            }
            return base.OnHandleCustomErrorWhenSaveToDatabase(exception, ref rpcError, registContext);
        }

        protected override LoginReturnToClient OnGetReturnDataWhenCheckLoginConnection(ulong accountId, bool enterLoginServerGroup)
        {
            using (var dbContext = GetOrCreateAccountDbContext())
            {
                var name = dbContext.WhereQueryableByAccountId(accountId).Select(i => i.Name).FirstOrDefault();
                return new ReturnDataAfterLogin { Name = name };
            }
        }
    }
}