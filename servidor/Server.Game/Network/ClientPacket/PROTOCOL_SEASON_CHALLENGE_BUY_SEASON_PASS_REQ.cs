using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_SEASON_CHALLENGE_BUY_SEASON_PASS_REQ : GameClientPacket
    {
        public override void Read()
        {
        }


        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;

                PlayerBattlepass seasonPass = player.Battlepass;
                BattlePassSeason activeSeason = BattlePassLoader.GetActiveSeasons();

                if (seasonPass == null || activeSeason == null || activeSeason.SeasonEnabled != 1 || !activeSeason.SeasonEnabledForPremium)
                {
                    this.Client.SendPacket(new PROTOCOL_SEASON_CHALLENGE_BUY_SEASON_PASS_ACK(AllUtils.SEASON_ERR_SEASON_NOT_GOING, player));
                    return;
                }

                if (seasonPass.HavePremium)
                {
                    this.Client.SendPacket(new PROTOCOL_SEASON_CHALLENGE_BUY_SEASON_PASS_ACK(AllUtils.SEASON_ERR_ALREADY_SEASON_PASS, player));
                    return;
                }

                if (!int.TryParse(activeSeason.SeasonPrice, out int price) || price <= 0)
                {
                    CLogger.Print($"Season pass premium has no valid SeasonPrice (SeasonId {activeSeason.SeasonId}): '{activeSeason.SeasonPrice}'", LoggerType.Warning);
                    this.Client.SendPacket(new PROTOCOL_SEASON_CHALLENGE_BUY_SEASON_PASS_ACK(AllUtils.SEASON_ERR_NO_GOODS, player));
                    return;
                }

                if (player.Cash < price)
                {
                    this.Client.SendPacket(new PROTOCOL_SEASON_CHALLENGE_BUY_SEASON_PASS_ACK(AllUtils.SEASON_ERR_NOT_ENOUGH_MONEY, player));
                    return;
                }

                if (!DaoManagerSQL.UpdateAccountValuable(player.PlayerId, player.Gold, player.Cash - price, player.Tags))
                {
                    this.Client.SendPacket(new PROTOCOL_SEASON_CHALLENGE_BUY_SEASON_PASS_ACK(AllUtils.SEASON_ERR_TRANS_ERROR, player));
                    return;
                }
                player.Cash -= price;

                if (!AllUtils.ProcessBattlepassPremiumBuy(player))
                {
                    // Pass failed to persist: refund the cash. Only restore memory if
                    // the refund write commits, else keep it synced with the debited row.
                    if (DaoManagerSQL.UpdateAccountValuable(player.PlayerId, player.Gold, player.Cash + price, player.Tags))
                        player.Cash += price;
                    else
                        CLogger.Print($"BUY_SEASON_PASS_REQ: refund FAILED to persist for player {player.PlayerId}; cash stays debited.", LoggerType.Error);
                    this.Client.SendPacket(new PROTOCOL_SEASON_CHALLENGE_BUY_SEASON_PASS_ACK(AllUtils.SEASON_ERR_TRANS_ERROR, player));
                    return;
                }
                player.UpdateSeasonpass = false;
                this.Client.SendPacket(new PROTOCOL_SEASON_CHALLENGE_BUY_SEASON_PASS_ACK(0, player));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_SEASON_CHALLENGE_BUY_SEASON_PASS_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
