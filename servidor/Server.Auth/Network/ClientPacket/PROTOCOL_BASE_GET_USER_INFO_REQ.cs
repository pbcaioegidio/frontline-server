// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ClientPacket.PROTOCOL_BASE_GET_USER_INFO_REQ
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Server.Auth.Data.Models;
using Server.Auth.Data.Utils;
using Server.Auth.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Auth.Network.ClientPacket
{
    public class PROTOCOL_BASE_GET_USER_INFO_REQ : AuthClientPacket
    {
        public override void Read()
        {
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.Player;
                if (player == null || player.Inventory.Items.Count != 0)
                    return;
                AllUtils.ValidateAuthLevel(player);
                AllUtils.LoadPlayerInventory(player);
                AllUtils.LoadPlayerMissions(player);
                AllUtils.ValidatePlayerInventoryStatus(player);
                AllUtils.DiscountPlayerItems(player);
                AllUtils.CheckGameEvents(player);
                this.Client.SendPacket((AuthServerPacket)new PROTOCOL_BASE_GET_USER_INFO_ACK(player));
                if (!player.MyConfigsLoaded)
                    this.Client.SendPacket((AuthServerPacket)new PROTOCOL_BASE_GET_OPTION_ACK(0, player.Config));
                this.Client.SendPacket((AuthServerPacket)new PROTOCOL_BASE_GET_CHARA_INFO_ACK(player));
                AllUtils.ProcessBattlepass(player);
                this.Client.SendPacket((AuthServerPacket)new PROTOCOL_SEASON_CHALLENGE_SEASON_CHANGE());
                this.Client.SendPacket((AuthServerPacket)new PROTOCOL_SEASON_CHALLENGE_INFO_ACK(player));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BASE_GET_USER_INFO_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}