// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BASE_QUEST_DELETE_CARD_SET_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_QUEST_DELETE_CARD_SET_REQ : GameClientPacket
    {
        private uint Field0;
        private int Field1;

        public override void Read() => this.Field1 = (int)this.ReadC();

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                PlayerMissions mission = player.Mission;

                if (this.Field1 < 0 || this.Field1 >= PlayerMissions.SlotCount || mission[this.Field1].IsEmpty)
                {
                    this.Field0 = 2147487824U /*0x80001050*/;
                }
                else if (!DaoManagerSQL.DeletePlayerMissionSlot(player.PlayerId, this.Field1))
                {
                    this.Field0 = 2147487824U /*0x80001050*/;
                }
                else
                {
                    mission[this.Field1].Reset();
                    if (mission.ActualMission == this.Field1)
                    {
                        mission.ActualMission = mission.FirstOwnedSlot();
                        DaoManagerSQL.UpdatePlayerActiveMissionSlot(player.PlayerId, mission.ActualMission);
                    }
                }

                this.Client.SendPacket(new PROTOCOL_BASE_QUEST_DELETE_CARD_SET_ACK(this.Field0, player));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BASE_QUEST_DELETE_CARD_SET_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}