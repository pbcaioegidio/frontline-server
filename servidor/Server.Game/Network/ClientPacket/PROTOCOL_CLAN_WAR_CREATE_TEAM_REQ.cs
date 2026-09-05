// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CLAN_WAR_CREATE_TEAM_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CLAN_WAR_CREATE_TEAM_REQ : GameClientPacket
    {
        private int Field0;
        private List<int> Field1 = new List<int>();

        public override void Read() => this.Field0 = (int)this.ReadC();

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                ChannelModel channel = player.GetChannel();
                if (channel != null && channel.Type == ChannelType.Clan && player.Room == null)
                {
                    if (player.Match == null)
                    {
                        if (player.ClanId != 0)
                        {
                            int num1 = -1;
                            int num2 = -1;
                            lock (channel.Matches)
                            {
                                for (int id = 0; id < 250; ++id)
                                {
                                    if (channel.GetMatch(id) == null)
                                    {
                                        num1 = id;
                                        break;
                                    }
                                }
                                for (int index = 0; index < channel.Matches.Count; ++index)
                                {
                                    MatchModel match = channel.Matches[index];
                                    if (match.Clan.Id == player.ClanId)
                                        this.Field1.Add(match.FriendId);
                                }
                            }
                            for (int index = 0; index < 25; ++index)
                            {
                                if (!this.Field1.Contains(index))
                                {
                                    num2 = index;
                                    break;
                                }
                            }
                            if (num1 != -1)
                            {
                                if (num2 != -1)
                                {
                                    MatchModel matchModel = new MatchModel(ClanManager.GetClan(player.ClanId))
                                    {
                                        MatchId = num1,
                                        FriendId = num2,
                                        Training = this.Field0,
                                        ChannelId = player.ChannelId,
                                        ServerId = player.ServerId
                                    };
                                    matchModel.AddPlayer(player);
                                    channel.AddMatch(matchModel);
                                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_CREATE_TEAM_ACK(0U, matchModel));
                                    matchModel.BroadcastRoster();
                                }
                                else
                                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_CREATE_TEAM_ACK(2147487881U));
                            }
                            else
                                this.Client.SendPacket(new PROTOCOL_CLAN_WAR_CREATE_TEAM_ACK(2147487880U));
                        }
                        else
                            this.Client.SendPacket(new PROTOCOL_CLAN_WAR_CREATE_TEAM_ACK(2147487835U));
                    }
                    else
                        this.Client.SendPacket(new PROTOCOL_CLAN_WAR_CREATE_TEAM_ACK(2147487879U));
                }
                else
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_CREATE_TEAM_ACK(2147483648U /*0x80000000*/));
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}