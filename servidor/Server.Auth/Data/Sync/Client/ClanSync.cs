using Plugin.Core.Network;
using Server.Auth.Data.Managers;
using Server.Auth.Data.Models;
using Server.Auth.Data.Sync.Update;

namespace Server.Auth.Data.Sync.Client
{
    public static class ClanSync
    {
        public static void Load(SyncClientPacket C)
        {
            long id = C.ReadQ();
            int num1 = (int)C.ReadC();
            Account account = AccountManager.GetAccount(id, true);
            if (account == null)
                return;
            switch (num1)
            {
                case 0:
                    ClanInfo.ClearList(account);
                    break;
                case 1:
                    long PlayerId1 = C.ReadQ();
                    string str = C.ReadS((int)C.ReadC());
                    byte[] Buffer = C.ReadB(4);
                    byte num2 = C.ReadC();
                    Account Member = new Account()
                    {
                        PlayerId = PlayerId1,
                        Nickname = str,
                        Rank = (int)num2
                    };
                    Member.Status.SetData(Buffer, PlayerId1);
                    ClanInfo.AddMember(account, Member);
                    break;
                case 2:
                    long PlayerId2 = C.ReadQ();
                    ClanInfo.RemoveMember(account, PlayerId2);
                    break;
                case 3:
                    int num3 = C.ReadD();
                    int num4 = (int)C.ReadC();
                    account.ClanId = num3;
                    account.ClanAccess = num4;
                    break;
            }
        }
    }
}