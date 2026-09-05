//using Server.Game.Data.Sync.Server;
//using Server.Game.Network.ServerPacket;
//using Server.Game.Network;
//using Server.Game;
//using Server.Game.Data.Models;
//using Plugin.Core.Models;

//namespace PointBlank.Game.Network.ClientPacket
//{
//    public class PROTOCOL_SEASON_CHALLENGE_REWARD_REQ : GameClientPacket
//    {
//        public PROTOCOL_SEASON_CHALLENGE_REWARD_REQ(GameClient client, byte[] data)
//        {
//            Makeme(client, data);
//        }
//        public override void Read()
//        {

//        }
//        public override void Run()
//        {
//            Account player = Client?.Player;
//            if (player == null) return;
//            PlayerBattlepassInfo info = SeasonPass.LoadBattlepassInfo(player.PlayerId);
//            if (info == null) return;
//            var reward = SeasonPass.GrantLevelUpReward(info);
//            if (reward != null)
//            {
//                SendItemInfo.AddRewardItem(player, reward.NormalReward);
//                SendItemInfo.AddRewardItem(player, reward.PremiumAReward);
//                SendItemInfo.AddRewardItem(player, reward.PremiumBReward);
//                if (reward.NormalReward > 0) info.finished_level++;
//                if (reward.PremiumAReward > 0 || reward.PremiumBReward > 0) info.finished_pab_level++;
//                PlayerBattlepassInfo.Save(info);
//                Client.SendPacket(new PROTOCOL_SEASON_CHALLENGE_SEND_REWARD_ACK(0, reward.NormalReward, reward.PremiumAReward, reward.PremiumBReward));
//            }
//            else
//            {
//                Client.SendPacket(new PROTOCOL_SEASON_CHALLENGE_SEND_REWARD_ACK(0x80000000, 0, 0, 0));
//            }
//        }
//    }
//}
