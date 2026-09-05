
using Plugin.Core.Network;
using Server.Auth.Data.Managers;
using Server.Auth.Data.Models;


namespace Server.Auth.Data.Sync.Client
{
    public class AccountInfo
    {
        public static void Load(SyncClientPacket C)
        {
            long id = C.ReadQ();
            int num = (int)C.ReadC();
            string PacketName = C.ReadS((int)C.ReadC());
            byte[] Data = C.ReadB((int)C.ReadUH());
            Account account = AccountManager.GetAccount(id, true);
            if (account == null)
                return;
            if (num != 0)
                account.SendCompletePacket(Data, PacketName);
            else
                account.SendPacket(Data, PacketName);
        }
    }
}
