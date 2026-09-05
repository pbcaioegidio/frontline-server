// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CLAN_WAR_LEAVE_TEAM_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CLAN_WAR_LEAVE_TEAM_REQ : GameClientPacket
    {
        private uint Field0;

        public override void Read()
        {
        }

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                MatchModel match = player.Match;
                int leftSlot = player.MatchSlot;
                if (match == null || !match.RemovePlayer(player))
                    this.Field0 = 2147483648U /*0x80000000*/;
                bool disbanded = this.Field0 == 0U && match.GetCountPlayers() == 0;
                this.Client.SendPacket(new PROTOCOL_CLAN_WAR_LEAVE_TEAM_ACK(
                    this.Field0, disbanded, leftSlot, disbanded || match == null ? -1 : match.Leader));
                if (this.Field0 != 0U)
                    return;
                player.Status.UpdateClanMatch(byte.MaxValue);
                AllUtils.SyncPlayerToClanMembers(player);
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}