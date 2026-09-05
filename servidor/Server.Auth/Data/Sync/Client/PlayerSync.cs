using Plugin.Core.Network;
using Server.Auth.Data.Managers;
using Server.Auth.Data.Models;

namespace Server.Auth.Data.Sync.Client
{
    public static class PlayerSync
    {
        public static void Load(SyncClientPacket C)
        {
            long id = C.ReadQ();
            int num1 = (int)C.ReadC();
            int num2 = (int)C.ReadC();
            int num3 = C.ReadD();
            int num4 = C.ReadD();
            int num5 = C.ReadD();
            Account account = AccountManager.GetAccount(id, true);
            if (account == null || num1 != 0)
                return;
            account.Rank = num2;
            account.Gold = num3;
            account.Cash = num4;
            account.Tags = num5;
        }
    }
}
