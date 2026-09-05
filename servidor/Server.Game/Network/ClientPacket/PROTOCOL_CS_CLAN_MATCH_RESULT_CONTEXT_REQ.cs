// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CS_CLAN_MATCH_RESULT_CONTEXT_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CS_CLAN_MATCH_RESULT_CONTEXT_REQ : GameClientPacket
    {
        private int Field0;

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
                if (player.ClanId > 0)
                {
                    ChannelModel channel = player.GetChannel();
                    if (channel != null && channel.Type == ChannelType.Clan)
                    {
                        lock (channel.Matches)
                        {
                            for (int index = 0; index < channel.Matches.Count; ++index)
                            {
                                if (channel.Matches[index].Clan.Id == player.ClanId)
                                    ++this.Field0;
                            }
                        }
                    }
                }
                this.Client.SendPacket(new PROTOCOL_CS_CLAN_MATCH_RESULT_CONTEXT_ACK(this.Field0));
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}