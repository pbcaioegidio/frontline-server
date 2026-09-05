using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Plugin.Core;
using Server.Game.Rcon;
using Server.Game.Data.Sync.Server;
using Server.Game.Network.ServerPacket;
using Server.Game.Data.Models;
using Plugin.Core.Enums;
using Plugin.Core.SQL;
using Server.Game.Data.Managers;

namespace Server.Game.Data.Feature
{
    public static class Redeem
    {
        private static string redeem = "[Item Redeem Dikirimkan]\nRedeem Item telah dikirim.\nSilahkan cek inventory anda untuk melihat item.\n\n{col:255:0:0:255}[Pemberian Hadiah]{/col}\n";

        public static string CreateItemDaysByRedeem(string str)
        {
            try
            {
                string txt = str.Substring(str.IndexOf(" ") + 1);
                string[] split = txt.Split(' ');
                long player_ID = long.Parse(split[0]);
                int goodid = Convert.ToInt32(split[1]);
                int Money = Convert.ToInt32(split[2]);
                int Webcoin = Convert.ToInt32(split[3]);
                GoodsItem Good = ShopManager.GetGood(goodid);
                Account playerO = AccountManager.GetAccount(player_ID, true);
                if (playerO == null)
                {
                    ComDiv.UpdateDB("accounts", "online", false, "player_id", player_ID);
                    RconLogger.LogsPanel($"Error in Account Check, Try Again", 1);
                    return $"Error in Account Check, Try Again";
                }
                if (Good == null)
                {
                    RconLogger.LogsPanel("{Good} Tersebut Tidak Ditemukan di Database", 1);
                    return "ItemID Tersebut Tidak Ditemukan di Database";
                }
                else
                {
                    if (DaoManagerSQL.UpdateAccountCash(playerO.PlayerId, (playerO.Cash + Money)))
                    {
                        int WebcoinPlayer = RconManager.GetWebcoin(player_ID);
                        playerO.Cash += Money;

                        int addWebcoin = WebcoinPlayer + Webcoin;
                        ComDiv.UpdateDB("accounts", "progressive_coin", addWebcoin, "player_id", playerO.PlayerId);

                        playerO.SendPacket(new PROTOCOL_AUTH_GET_POINT_CASH_ACK(0, playerO));
                        SendItemInfo.LoadGoldCash(playerO);
                    }
                    List<ItemsModel> Items = new List<ItemsModel>();
                    ItemsModel Item = new ItemsModel(Good.Item);
                    if (Item == null)
                    {
                        return $"Goods Id: {Good} was error!";
                    }
                    ComDiv.TryCreateItem(Item, playerO.Inventory, playerO.PlayerId);
                    playerO.SendPacket(new PROTOCOL_INVENTORY_GET_INFO_ACK(1, playerO, Item));
                    if (Good.Id > 60100000 && Good.Id < 60299999 || Good.Id > 63200100 && Good.Id < 66499999) //
                    {
                        int Slots = playerO.Character.Characters.Count;
                        CharacterModel Character = new CharacterModel()
                        {
                            ObjectId = Item.ObjectId,
                            Id = Item.Id,
                            Name = Item.Name,
                            Slot = Slots++,
                            CreateDate = uint.Parse(DateTimeUtil.Now("yyMMddHHmm")),
                            PlayTime = 0
                        };
                        if (playerO.Character.Characters.Find(x => x.Id == Character.Id) == null)
                        {
                            playerO.Character.AddCharacter(Character);
                            if (DaoManagerSQL.CreatePlayerCharacter(Character, playerO.PlayerId))
                            {
                                playerO.SendPacket(new PROTOCOL_CHAR_CREATE_CHARA_ACK(0, 3, Character, playerO));
                            }
                        }
                    }
                    MessageModel msgF6 = CreateMessage("GM", playerO.PlayerId, 0, redeem + Item.Name);
                    if (msgF6 != null)
                    {
                        playerO.SendPacket(new PROTOCOL_MESSENGER_NOTE_RECEIVE_ACK(msgF6));
                    }

                    Randomwinload();
                    ComDiv.UpdateDB("accounts", "token", finalString, "player_id", playerO.PlayerId);

                    RconLogger.LogsPanel($"Successfully Redeem The Code. Reward Will Be Sent Directly Into {playerO.Nickname} Inventory", 0);
                    return $"Success Gift Ke {playerO.Nickname}";
                }
            }
            catch
            {
                RconLogger.LogsPanel("Error occurred when adding the weapon to the player.", 1);
                return "Error occurred when adding the weapon to the player.";
            }
        }

        private static MessageModel CreateMessage(string senderName, long owner, long senderId, string text)
        {
            MessageModel msg = new MessageModel(15)
            {
                SenderName = senderName,
                SenderId = senderId,
                Text = text,
                State = NoteMessageState.Unreaded
            };
            if (!DaoManagerSQL.CreateMessage(owner, msg))
                return null;
            return msg;
        }

        private static string finalString;

        private static void Randomwinload()
        {
            var chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[32];
            var random = new Random();
            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            finalString = new string(stringChars);
        }
    }
}