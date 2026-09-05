// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BASE_GET_USER_MANAGEMENT_POPUP_REQ
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
    public class PROTOCOL_BASE_GET_USER_MANAGEMENT_POPUP_REQ : GameClientPacket
    {
        private string Field0;

        public override void Read() => this.Field0 = this.ReadU(66); // fix: ReadU takes BYTES, so 33 unicode chars are 66

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null || player.Nickname.Length == 0 || player.Nickname == this.Field0)
                    return;
                this.Client.SendPacket(new PROTOCOL_BASE_GET_USER_MANAGEMENT_POPUP_ACK());
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}