// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BASE_EVENT_PORTAL_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_EVENT_PORTAL_REQ : GameClientPacket
    {
        private string Field0;

        public override void Read() => this.Field0 = this.ReadS(32 /*0x20*/);


        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                if (!player.LoadedShop)
                    player.LoadedShop = true;
                if (!(Bitwise.ReadFile(Environment.CurrentDirectory + "/Data/Raws/EventPortal.dat") == this.Field0))
                    this.Client.SendPacket(new PROTOCOL_BASE_EVENT_PORTAL_ACK(true));
                else
                    this.Client.SendPacket(new PROTOCOL_BASE_EVENT_PORTAL_ACK(false));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BASE_EVENT_PORTAL_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}