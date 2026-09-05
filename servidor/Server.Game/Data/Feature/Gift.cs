using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using Server    .Game.Rcon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game.Data.Feature
{
    public static class Gift
    {
        private static string gift = "[Artículo de regalo enviado]\nEl artículo de donación se ha enviado.\nPor favor, revise su inventario para ver el artículo.\n\n{col:255:0:0:255}[Entrega de regalos]{/col}\n";
        public static string CreateItemDaysByGift(long player_ID, int goodid, long gm_id)
        {
            try
            {

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
                    RconLogger.LogsPanel("{Good} Not Found in Database", 1);
                    return "ItemID Not Found in Database";
                }
                else
                {
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
                    MessageModel msgF6 = CreateMessage("GM", playerO.PlayerId, 0, gift + Item.Name);
                    if (msgF6 != null)
                    {
                        playerO.SendPacket(new PROTOCOL_MESSENGER_NOTE_RECEIVE_ACK(msgF6));
                    }
                }
                
                RconLogger.LogsPanel($"Successfully Gift The Code. Reward Will Be Sent Directly Into {playerO.Nickname} Inventory. {gm_id}", 0);
                return $"Success Gift Ke {playerO.Nickname}";
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
    }
}
