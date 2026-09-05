// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CS_MEMBER_CONTEXT_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.SQL;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CS_MEMBER_CONTEXT_REQ : GameClientPacket
    {
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
                int ClanId = player.ClanId == 0 ? player.FindClanId : player.ClanId;
                int A_1;
                int A_2;
                if (ClanId != 0)
                {
                    A_1 = 0;
                    A_2 = DaoManagerSQL.GetClanPlayers(ClanId);
                }
                else
                {
                    A_1 = -1;
                    A_2 = 0;
                }
                this.Client.SendPacket(new PROTOCOL_CS_MEMBER_CONTEXT_ACK(A_1, A_2));
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}