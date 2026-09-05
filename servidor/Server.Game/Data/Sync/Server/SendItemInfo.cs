// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Sync.Server.SendItemInfo
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Game.Data.Models;
using Server.Game.Data.Sync;
using System;
using System.Net;


namespace Server.Game.Data.Sync.Server
{
    public class SendItemInfo
    {
        public static void LoadItem(Account Player, ItemsModel Item)
        {
            try
            {
                if (Player == null || Player.Status.ServerId == (byte)0)
                    return;
                GameSync sync = GameXender.SyncOf(Player.ServerId);
                if (sync == null)
                    return;
                SChannelModel server = sync.GetServer(Player.Status);
                if (server == null)
                    return;
                IPEndPoint connection = SynchronizeXML.GetServer((int)server.Port).Connection;
                using (SyncServerPacket syncServerPacket = new SyncServerPacket())
                {
                    syncServerPacket.WriteH((short)18);
                    syncServerPacket.WriteQ(Player.PlayerId);
                    syncServerPacket.WriteQ(Item.ObjectId);
                    syncServerPacket.WriteD(Item.Id);
                    syncServerPacket.WriteC((byte)Item.Equip);
                    syncServerPacket.WriteC((byte)Item.Category);
                    syncServerPacket.WriteD(Item.Count);
                    syncServerPacket.WriteC((byte)Item.Name.Length);
                    syncServerPacket.WriteS(Item.Name, Item.Name.Length);
                    sync.SendPacket(syncServerPacket.ToArray(), connection);
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        public static void LoadGoldCash(Account Player)
        {
            try
            {
                if (Player == null)
                    return;
                GameSync sync = GameXender.SyncOf(Player.ServerId);
                if (sync == null)
                    return;
                SChannelModel server = sync.GetServer(Player.Status);
                if (server == null)
                    return;
                IPEndPoint connection = SynchronizeXML.GetServer((int)server.Port).Connection;
                using (SyncServerPacket syncServerPacket = new SyncServerPacket())
                {
                    syncServerPacket.WriteH((short)19);
                    syncServerPacket.WriteQ(Player.PlayerId);
                    syncServerPacket.WriteC((byte)0);
                    syncServerPacket.WriteC((byte)Player.Rank);
                    syncServerPacket.WriteD(Player.Gold);
                    syncServerPacket.WriteD(Player.Cash);
                    syncServerPacket.WriteD(Player.Tags);
                    sync.SendPacket(syncServerPacket.ToArray(), connection);
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }

        }

        public static void AddRewardItem(Account player, int itemId)
        {
            if (player == null || itemId <= 0) return;
            ItemsModel item = new ItemsModel
            {
                ObjectId = 0,
                Id = itemId,
                Equip = ItemEquipType.Durable,
                Category = ComDiv.GetItemCategory(itemId),
                Count = 1,
                Name = "Battlepass Reward"
            };
            ComDiv.TryCreateItem(item, player.Inventory, player.PlayerId);
            SendItemInfo.LoadItem(player, item);
        }
    }
}