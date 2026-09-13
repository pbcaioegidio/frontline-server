using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Logging;
using Plugin.Core.Models;
using Plugin.Core.Security;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_USER_ENTER_REQ : GameClientPacket
    {
        private long PlayerId;
        private string Username;

        public override void Read()
        {
            Username = ReadS(ReadC());
            PlayerId = ReadQ();
        }

        public override void Run()
        {
            try
            {
                if (Client != null && Client.Player == null)
                {
                    Account Player = AccountManager.GetAccountDB(PlayerId, 2, 31);
                    string enterIp = Client.GetIPAddress();
                    CLogger.Event(LogCat.Enter, new { reqPid = PlayerId, reqUser = Username, dbUser = (Player == null ? "<null>" : Player.Username), srvId = (Player == null ? -1 : Player.Status.ServerId), match = (Player != null && Player.Username == Username) });

                    bool identityOk = Player != null
                        && (string.IsNullOrEmpty(Username) || Player.Username == Username)
                        && Player.Status.ServerId != byte.MaxValue;

                    string ticketReason = null;
                    bool ticketOk = identityOk && LoginSessionStore.TryConsume(PlayerId, Username ?? Player?.Username, enterIp, out ticketReason);

                    if (identityOk && ticketOk)
                    {
                        Client.PlayerId = Player.PlayerId;
                        Player.Connection = Client;
                        Player.ServerId = Client.ServerId;
                        Player.GetAccountInfos(7935);
                        AllUtils.ValidateAuthLevel(Player);
                        AllUtils.LoadPlayerInventory(Player);
                        AllUtils.LoadPlayerMissions(Player);
                        AllUtils.EnableQuestMission(Player);
                        AllUtils.ValidatePlayerInventoryStatus(Player);
                        Player.SetPublicIP(Client.GetAddress());
                        Player.Session = new PlayerSession()
                        {
                            SessionId = Client.SessionId,
                            PlayerId = Client.PlayerId
                        };
                        Player.Status.UpdateServer((byte)Client.ServerId);
                        Player.UpdateCacheInfo();
                        Client.Player = Player;
                        ComDiv.UpdateDB("accounts", "ip4_address", Player.PublicIP.ToString(), "player_id", Player.PlayerId);
                        Client.SendPacket(new PROTOCOL_BASE_USER_ENTER_ACK((uint)EventErrorEnum.SUCCESS));
                        Client.SendPacket(new PROTOCOL_BASE_GET_MYINFO_BASIC_ACK(Player));
                        // Push the player's cosmetic state (nick color, border, fake rank/nick,
                        // crosshair/muzzle color) at login. MYINFO_BASIC alone doesn't carry
                        // NickColor, so without this the colored nick / border reset every relog.
                        Client.SendPacket(new PROTOCOL_BASE_INV_ITEM_DATA_ACK(0, Player));
                        Client.SendPacket(new PROTOCOL_AUTH_GET_POINT_CASH_ACK(0U, Player));
                        Client.SendPacket(new PROTOCOL_BASE_GET_MYINFO_RECORD_ACK(Player.Statistic));
                        Client.SendPacket(new PROTOCOL_BASE_GET_CHARA_INFO_ACK(Player));
                        Client.SendPacket(new PROTOCOL_SERVER_MESSAGE_CHANGE_INVENTORY_ACK(Player));

                        // Garante packed shop (Throw2 só existe no servidor; Shop.dat local não tem).
                        if (!Player.LoadedShop)
                        {
                            Player.LoadedShop = true;
                            Player.LoadedPackedGoods = true;
                            ShopCatalog121Sender.SendFullCatalog(Client, Player, true);
                            CLogger.Print($"Throw2 shop catalog enviado no USER_ENTER PlayerId={Player.PlayerId}", LoggerType.Info);
                        }

                        // Arma Especial 2: ACK de compra grátis (cupom 1700109) → RemainingDays.
                        var throw2Cart = InventoryUnlocks.Throw2UnlockCart();
                        if (throw2Cart != null)
                        {
                            Client.SendPacket(new PROTOCOL_INVENTORY_GET_INFO_ACK(0, Player, throw2Cart));
                            Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(1U, throw2Cart, Player));
                            CLogger.Print($"Throw2 unlock ACK enviado PlayerId={Player.PlayerId} GoodId={throw2Cart[0].Id}", LoggerType.Info);
                        }
                    }
                    else
                    {
                        if (identityOk && !ticketOk)
                        {
                            CLogger.Print(
                                $"[LoginSession] USER_ENTER rejeitado PlayerId={PlayerId} User={Username} IP={enterIp} motivo={ticketReason}",
                                LoggerType.Warning);
                        }
                        Client.SendPacket(new PROTOCOL_BASE_USER_ENTER_ACK((uint)EventErrorEnum.FAIL));
                        Client.Close(0, true);
                    }
                }
                else
                {
                    Client.SendPacket(new PROTOCOL_BASE_USER_ENTER_ACK((uint)EventErrorEnum.FAIL));
                    Client.Close(0, true);
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"PROTOCOL_BASE_USER_ENTER_REQ: {ex.Message}", LoggerType.Error, ex);
            }
        }
    }
}
