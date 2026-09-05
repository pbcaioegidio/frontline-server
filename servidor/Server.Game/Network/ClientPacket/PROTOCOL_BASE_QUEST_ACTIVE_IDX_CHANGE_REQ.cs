// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BASE_QUEST_ACTIVE_IDX_CHANGE_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_QUEST_ACTIVE_IDX_CHANGE_REQ : GameClientPacket
    {
        private int Field0;
        private int Field1;
        private int Field2;

        public override void Read()
        {
            this.Field1 = (int)this.ReadC();
            this.Field0 = (int)this.ReadC();
            this.Field2 = (int)this.ReadUH();
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                PlayerMissions mission = player.Mission;

                if (this.Field1 >= 0 && this.Field1 < PlayerMissions.SlotCount
                    && mission[this.Field1].CurrentCard != this.Field0)
                {
                    mission[this.Field1].CurrentCard = this.Field0;
                    DaoManagerSQL.UpdatePlayerMissionSlotCard(player.PlayerId, this.Field1, this.Field0);
                }

                mission.SelectedCard = this.Field2 == (int)ushort.MaxValue;

                if (mission.ActualMission != this.Field1
                    && this.Field1 >= 0 && this.Field1 < PlayerMissions.SlotCount)
                {
                    mission.ActualMission = this.Field1;
                    DaoManagerSQL.UpdatePlayerActiveMissionSlot(player.PlayerId, this.Field1);
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BASE_QUEST_ACTIVE_IDX_CHANGE_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}