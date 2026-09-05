// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_SHOP_LEAVE_REQ
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
    public class PROTOCOL_SHOP_LEAVE_REQ : GameClientPacket
    {
        public override void Read()
        {
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null || ComDiv.GetDuration(player.LastShopLeave) < 1.0)
                    return;
                player.Room?.ChangeSlotState(player.SlotId, SlotState.NORMAL, true);
                this.Client.SendPacket(new PROTOCOL_SHOP_LEAVE_ACK());
                player.LastShopLeave = DateTimeUtil.Now();
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_SHOP_LEAVE_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}