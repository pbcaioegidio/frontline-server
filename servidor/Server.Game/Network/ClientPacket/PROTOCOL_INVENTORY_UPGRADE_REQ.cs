using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    // Cash-paid inventory upgrade (opcode 3333). The 122 client's capacity "+" button goes
    // through the shop extend purchase (opcode 1082) instead, so this path is legacy; it is kept
    // working and consistent with the client's 1000-slot ceiling rather than left to rot.
    public class PROTOCOL_INVENTORY_UPGRADE_REQ : GameClientPacket
    {
        private const int CAPACITY_INCREMENT = 100;
        private const int UPGRADE_COST = 10000;

        public override void Read()
        {
        }

        public override void Run()
        {
            try
            {
                Account account = this.Client.Player;
                if (account == null)
                    return;

                if (!AllUtils.CanExtendInventory(account, CAPACITY_INCREMENT))
                {
                    this.Client.SendPacket(new PACKET_INVENTORY_MAX_UP_ACK(PACKET_INVENTORY_MAX_UP_ACK.ERROR_MAX_CAPACITY_REACHED));
                    return;
                }

                if (account.Cash < UPGRADE_COST)
                {
                    this.Client.SendPacket(new PACKET_INVENTORY_MAX_UP_ACK(PACKET_INVENTORY_MAX_UP_ACK.ERROR_SHOP_BUY_FAIL));
                    return;
                }

                if (!DaoManagerSQL.UpdateAccountValuable(account.PlayerId, account.Gold, account.Cash - UPGRADE_COST, account.Tags))
                {
                    this.Client.SendPacket(new PACKET_INVENTORY_MAX_UP_ACK(PACKET_INVENTORY_MAX_UP_ACK.ERROR_GENERIC));
                    return;
                }
                account.Cash -= UPGRADE_COST;

                if (!AllUtils.ExtendInventory(this.Client, account, CAPACITY_INCREMENT))
                {
                    // Only mirror the refund in memory once the DB actually took it, otherwise
                    // the account would spend cash it still owes.
                    if (DaoManagerSQL.UpdateAccountValuable(account.PlayerId, account.Gold, account.Cash + UPGRADE_COST, account.Tags))
                        account.Cash += UPGRADE_COST;
                    else
                        CLogger.Print($"[PROTOCOL_INVENTORY_UPGRADE_REQ] refund failed for player {account.PlayerId}, {UPGRADE_COST} cash charged with no expansion", LoggerType.Error);
                    this.Client.SendPacket(new PACKET_INVENTORY_MAX_UP_ACK(PACKET_INVENTORY_MAX_UP_ACK.ERROR_GENERIC));
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"[PROTOCOL_INVENTORY_UPGRADE_REQ] Error: {ex.Message}", LoggerType.Error, ex);
            }
        }
    }
}
