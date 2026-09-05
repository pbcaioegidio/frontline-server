// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CS_CHECK_JOIN_AUTHORITY_ERQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CS_CHECK_JOIN_AUTHORITY_ERQ : GameClientPacket
    {
        private int Field0;
        private uint Field1;

        public override void Read() => this.Field0 = this.ReadD();

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                ClanModel clan = ClanManager.GetClan(this.Field0);
                if (clan.Id != 0)
                {
                    if (clan.RankLimit > player.Rank)
                        this.Field1 = 2147487867U;
                }
                else
                    this.Field1 = 2147483648U /*0x80000000*/;
                this.Client.SendPacket(new PROTOCOL_CS_CHECK_JOIN_AUTHORITY_ACK(this.Field1));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CS_CHECK_JOIN_AUTHORITY_ERQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}