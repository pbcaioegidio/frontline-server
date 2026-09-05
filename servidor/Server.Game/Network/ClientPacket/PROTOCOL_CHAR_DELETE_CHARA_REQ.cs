using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Linq;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CHAR_DELETE_CHARA_REQ : GameClientPacket
    {
        private int Slot;

        public override void Read() => this.Slot = (int)this.ReadC();

        public override void Run()
        {
            try
            {
                Account Player = Client.Player;
                if (Player == null)
                {
                    CLogger.Print("PROTOCOL_CHAR_DELETE_CHARA_REQ: Player is null", LoggerType.Warning);
                    return;
                }

                CharacterModel Character = Player.Character.GetCharacterSlot(Slot);
                if (Character == null)
                {
                    CLogger.Print($"PROTOCOL_CHAR_DELETE_CHARA_REQ: Character not found at slot {Slot}", LoggerType.Warning);
                    Client.SendPacket(new PROTOCOL_CHAR_DELETE_CHARA_ACK(0x800010A7, -1, null, null));
                    return;
                }

                ItemsModel Item = Player.Inventory.GetItem(Character.Id);
                if (Item == null)
                {
                    CLogger.Print($"PROTOCOL_CHAR_DELETE_CHARA_REQ: Item not found for character {Character.Id}", LoggerType.Warning);
                    Client.SendPacket(new PROTOCOL_CHAR_DELETE_CHARA_ACK(0x800010A7, -1, null, null));
                    return;
                }

                // Delete from database first
                bool characterDeleted = DaoManagerSQL.DeletePlayerCharacter(Character.ObjectId, Player.PlayerId);
                bool itemDeleted = DaoManagerSQL.DeletePlayerInventoryItem(Item.ObjectId, Player.PlayerId);

                if (!characterDeleted || !itemDeleted)
                {
                    CLogger.Print($"PROTOCOL_CHAR_DELETE_CHARA_REQ: Failed to delete from database. Char:{characterDeleted}, Item:{itemDeleted}", LoggerType.Error);
                    Client.SendPacket(new PROTOCOL_CHAR_DELETE_CHARA_ACK(0x800010A7, -1, null, null));
                    return;
                }

                // Remove from memory
                Player.Character.RemoveCharacter(Character);
                Player.Inventory.RemoveItem(Item);

                // Reindex remaining characters
                int Index = 0;
                var remainingCharacters = Player.Character.Characters
                    .OrderBy(c => c.Slot)
                    .ToList();

                foreach (CharacterModel Chara in remainingCharacters)
                {
                    if (Chara.Slot != Index)
                    {
                        Chara.Slot = Index;
                        DaoManagerSQL.UpdatePlayerCharacter(Index, Chara.ObjectId, Player.PlayerId);
                    }
                    Index++;
                }

                // Send success response
                Client.SendPacket(new PROTOCOL_CHAR_DELETE_CHARA_ACK(0, Slot, Player, Item));

                // Refresh character info
                Client.SendPacket(new PROTOCOL_BASE_GET_CHARA_INFO_ACK(Player));

                CLogger.Print($"PROTOCOL_CHAR_DELETE_CHARA_REQ: Character deleted successfully. Slot:{Slot}, CharId:{Character.Id}", LoggerType.Info);
            }
            catch (Exception ex)
            {
                CLogger.Print($"PROTOCOL_CHAR_DELETE_CHARA_REQ: {ex.Message}", LoggerType.Error, ex);

                // Send error response on exception
                try
                {
                    Client.SendPacket(new PROTOCOL_CHAR_DELETE_CHARA_ACK(0x800010A7, -1, null, null));
                }
                catch (Exception sendEx)
                {
                    CLogger.Print($"PROTOCOL_CHAR_DELETE_CHARA_REQ: Failed to send error packet: {sendEx.Message}", LoggerType.Error);
                }
            }
        }
    }
}