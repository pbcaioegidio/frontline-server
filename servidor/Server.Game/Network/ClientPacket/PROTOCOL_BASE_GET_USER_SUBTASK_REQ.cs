// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BASE_GET_USER_SUBTASK_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_GET_USER_SUBTASK_REQ : GameClientPacket
    {
        private int Field0;

        public override void Read() => this.Field0 = this.ReadD();

        
        public override void Run()
        {
            try
            {
                Account player1 = this.Client.GetAccount();
                if (player1 == null)
                    return;
                PlayerSession player2 = player1.GetChannel().GetPlayer(this.Field0);
                if (player2 == null)
                    return;
                this.Client.SendPacket(new PROTOCOL_BASE_GET_USER_SUBTASK_ACK(player2));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BASE_GET_USER_SUBTASK_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}