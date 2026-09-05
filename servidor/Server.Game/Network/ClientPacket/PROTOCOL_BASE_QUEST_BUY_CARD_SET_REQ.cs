// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BASE_QUEST_BUY_CARD_SET_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.XML;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_QUEST_BUY_CARD_SET_REQ : GameClientPacket
    {
        private int Field0;
        private EventErrorEnum Field1;

        public override void Read()
        {
            this.Field0 = (int)this.ReadC();
            int num = (int)this.ReadC();
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                PlayerMissions mission = player.Mission;
                if (mission == null || mission.OwnsCardSet(this.Field0))
                {
                    this.Client.SendPacket(new PROTOCOL_BASE_QUEST_BUY_CARD_SET_ACK(
                        EventErrorEnum.MISSION_NO_POINT_TO_GET_ITEM, null));
                    return;
                }

                MissionStore def = MissionConfigXML.GetMission(this.Field0);
                var item = def != null ? ShopManager.GetItemId(def.ItemId) : null;
                if (def == null || item == null)
                {
                    CLogger.Print("There is an error on Mission Config. Please check the configuration!", LoggerType.Warning);
                    this.Client.SendPacket(new PROTOCOL_BASE_QUEST_BUY_CARD_SET_ACK(
                        EventErrorEnum.MISSION_NO_POINT_TO_GET_ITEM, null));
                    return;
                }

                int slot = mission.FindFreeSlot();
                if (slot < 0)
                {
                    this.Client.SendPacket(new PROTOCOL_BASE_QUEST_BUY_CARD_SET_ACK(
                        EventErrorEnum.MISSION_LIMIT_CARD_COUNT, player));
                    return;
                }

                int price = item.PriceGold;
                if (player.Gold - price < 0)
                {
                    this.Client.SendPacket(new PROTOCOL_BASE_QUEST_BUY_CARD_SET_ACK(
                        EventErrorEnum.MISSION_NO_POINT_TO_GET_ITEM, player));
                    return;
                }
                if (price != 0 && !DaoManagerSQL.UpdateAccountGold(player.PlayerId, player.Gold - price))
                {
                    this.Client.SendPacket(new PROTOCOL_BASE_QUEST_BUY_CARD_SET_ACK(
                        EventErrorEnum.MISSION_FAIL_BUY_CARD_BY_NO_CARD_INFO, player));
                    return;
                }
                player.Gold -= price;

                if (!DaoManagerSQL.UpsertPlayerMissionSlot(player.PlayerId, slot, this.Field0))
                {
                    if (price != 0 && DaoManagerSQL.UpdateAccountGold(player.PlayerId, player.Gold + price))
                        player.Gold += price;
                    this.Client.SendPacket(new PROTOCOL_BASE_QUEST_BUY_CARD_SET_ACK(
                        EventErrorEnum.MISSION_FAIL_BUY_CARD_BY_NO_CARD_INFO, player));
                    return;
                }

                MissionSlot target = mission[slot];
                target.CardSetId = this.Field0;
                target.CurrentCard = 0;
                target.Progress = new byte[MissionSlot.ProgressSize];
                mission.ActualMission = slot;
                DaoManagerSQL.UpdatePlayerActiveMissionSlot(player.PlayerId, slot);

                this.Client.SendPacket(new PROTOCOL_BASE_QUEST_BUY_CARD_SET_ACK(EventErrorEnum.SUCCESS, player));
            }
            catch (Exception ex)
            {
                CLogger.Print($"{this.GetType().Name}: {ex.Message}", LoggerType.Error, ex);
            }
        }
    }
}