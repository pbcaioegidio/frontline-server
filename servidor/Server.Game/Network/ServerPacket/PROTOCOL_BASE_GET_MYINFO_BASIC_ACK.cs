using Plugin.Core.Models;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_GET_MYINFO_BASIC_ACK : GameServerPacket
    {
        private readonly Account account;
        private readonly ClanModel clanModel;

        public PROTOCOL_BASE_GET_MYINFO_BASIC_ACK(Account Account)
        {
            account = Account;
            if (Account != null)
            {
                clanModel = ClanManager.GetClan(Account.ClanId);
            }
        }

        // The 185-byte struct is the whole packet body; see GameServerPacket
        // .WriteUserInfoBasicBlock for the byte map and its evidence.
        public override void Write()
        {
            WriteH(2371);
            WriteUserInfoBasicBlock(account, clanModel);
        }
    }
}