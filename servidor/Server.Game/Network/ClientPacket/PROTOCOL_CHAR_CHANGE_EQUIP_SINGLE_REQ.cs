using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CHAR_CHANGE_EQUIP_SINGLE_REQ : GameClientPacket
    {
        private const uint ErrorGeneric = 2147483648U;

        private long ObjectId;
        private byte Slot;

        public override void Read()
        {
            this.ObjectId = (long)this.ReadUD();
            this.Slot = this.ReadC();
        }

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;

                ItemsModel item = player.Inventory.GetItem(this.ObjectId);
                if (item == null)
                {
                    this.Client.SendPacket(new PROTOCOL_CHAR_CHANGE_EQUIP_ACK(ErrorGeneric));
                    return;
                }

                int itemClass = ComDiv.GetIdStatics(item.Id, 1);

                // Emoticons (item class 41) also come through this opcode on "Usar" -> "Confirmar"
                // (verified live: C2S 1050 objId + slot byte). They are neither weapon-activatable
                // nor a "use" item, so both the activation branch below and SHOP_ITEM_AUTH reject
                // them (class 41 -> default -> 0x80000000 -> "O uso do item falhou"). Handle them
                // here: start the durable timer (the "Deseja equipar?" confirm) and equip into the
                // emote wheel (loadout+164), then refresh via CHANGE_INVENTORY. The client-sent slot
                // byte is not a 0..5 wheel index for emoticons, so auto-place in the first free slot.
                if (itemClass == 41)
                {
                    bool ok = this.ActivateDurableItem(player, item, itemClass);
                    if (ok)
                    {
                        AllUtils.EquipEmoticonFirstFreeSlot(player, item.Id);
                        this.Client.SendPacket(new PROTOCOL_SERVER_MESSAGE_CHANGE_INVENTORY_ACK(player));
                    }
                    this.Client.SendPacket(new PROTOCOL_CHAR_CHANGE_EQUIP_ACK(ok ? 0U : ErrorGeneric));
                    return;
                }

                // The 121 client routes every inventory "Usar" -> "Confirmar" through this
                // opcode, not just weapon activation. Function/use items (coupons, buffs,
                // point-ups, battle pass, capsules, stat resets) are not activatable
                // equipment; hand them to the unified item-use handler, which owns that
                // routing and sends its own ITEM_AUTH ack.
                if (!IsActivatableEquipment(itemClass))
                {
                    new PROTOCOL_AUTH_SHOP_ITEM_AUTH_REQ().RunFrom(this.Client, this.ObjectId);
                    return;
                }

                uint error = this.ActivateDurableItem(player, item, itemClass) ? 0U : ErrorGeneric;
                if (error == 0U)
                {
                    RoomModel room = player.Room;
                    if (room != null)
                        AllUtils.UpdateSlotEquips(player, room);
                }

                this.Client.SendPacket(new PROTOCOL_CHAR_CHANGE_EQUIP_ACK(error));
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        private bool ActivateDurableItem(Account player, ItemsModel item, int itemClass)
        {
            if (item.Equip != ItemEquipType.Durable)
                return true;

            uint expiresAt = Convert.ToUInt32(DateTimeUtil.Now().AddSeconds((double)item.Count).ToString("yyMMddHHmm"));
            if (!ComDiv.UpdateDB("player_items", "object_id", (object)this.ObjectId, "owner_id", (object)player.PlayerId, new string[2]
            {
                "count",
                "equip"
            }, (object)(long)expiresAt, (object)(int)ItemEquipType.Temporary))
                return false;

            item.Equip = ItemEquipType.Temporary;
            item.Count = expiresAt;
            this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_ITEM_AUTH_ACK(1U, item, player));

            if (itemClass == 6)
            {
                CharacterModel character = player.Character.GetCharacter(item.Id);
                if (character != null)
                    this.Client.SendPacket(new PROTOCOL_CHAR_CHANGE_STATE_ACK(character));
            }

            return true;
        }

        private static bool IsActivatableEquipment(int itemClass)
        {
            return itemClass >= 1 && itemClass <= 8 || itemClass == 15 || itemClass == 27 || itemClass >= 30 && itemClass <= 35;
        }
    }
}
