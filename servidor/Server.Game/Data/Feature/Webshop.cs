using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Data.Sync.Server;
using Server.Game.Network.ServerPacket;
using Server.Game.Rcon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Server.Game.Data.Feature
{
    public static class Webshop
    {
        private static string webshop = "[Artículo de la tienda web enviado]\nEl artículo de la tienda web se ha enviado.\nRevise su inventario para ver el artículo.\n\n{col:255:0:0:255}[Regalos]{/col}\n";

        public static string CreateItemDaysByWebshop(long player_ID, int goodid)
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
                    RconLogger.LogsPanel("ItemID no encontrado en la base de datos", 1);
                    return "ItemID no encontrado en la base de datos";
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

                    MessageModel msgF6 = CreateMessage("GM", playerO.PlayerId, 0, webshop + Item.Name);

                    // Verificar si es un personaje (por el rango de ID)
                    if (Good.Id > 60100000 && Good.Id < 60299999 || Good.Id > 63200100 && Good.Id < 66499999)
                    {
                        // Verificar si el jugador ya tiene este personaje
                        if (playerO.Character.Characters.Find(x => x.Id == Item.Id) == null)
                        {
                            // El personaje no existe, crear uno nuevo
                            int nextSlot = 0;
                            if (playerO.Character.Characters.Count > 0)
                            {
                                // Obtener el máximo slot y sumar 1
                                nextSlot = playerO.Character.Characters.Max(x => x.Slot) + 1;
                            }

                            CharacterModel Character = new CharacterModel()
                            {
                                ObjectId = Item.ObjectId,
                                Id = Item.Id,
                                Name = Item.Name,
                                Slot = nextSlot,
                                CreateDate = uint.Parse(DateTimeUtil.Now("yyMMddHHmm")),
                                PlayTime = 0
                            };

                            playerO.Character.AddCharacter(Character);
                            if (DaoManagerSQL.CreatePlayerCharacter(Character, playerO.PlayerId))
                            {
                                playerO.SendPacket(new PROTOCOL_CHAR_CREATE_CHARA_ACK(0, 3, Character, playerO));
                            }
                        }
                    }

                    if (msgF6 != null)
                    {
                        playerO.SendPacket(new PROTOCOL_MESSENGER_NOTE_RECEIVE_ACK(msgF6));
                    }
                    Randomwinload();
                    ComDiv.UpdateDB("accounts", "token", finalString, "player_id", playerO.PlayerId);

                    RconLogger.LogsPanel($"Success Buy Webshop. Item Will Be Sent Directly Into {playerO.Nickname} Inventory", 0);
                    return $"Success Gift {playerO.Nickname}";
                }
            }
            catch (Exception ex)
            {
                RconLogger.LogsPanel($"Error occurred when adding the weapon to the player: {ex.Message}", 1);
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