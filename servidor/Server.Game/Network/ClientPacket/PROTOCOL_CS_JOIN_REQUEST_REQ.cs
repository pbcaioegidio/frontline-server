// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CS_JOIN_REQUEST_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CS_JOIN_REQUEST_REQ : GameClientPacket
    {
        private int Field0;
        private string Field1;
        private uint Field2;

        public override void Read()
        {
            this.Field0 = this.ReadD();
            this.Field1 = this.ReadU((int)this.ReadC() * 2);
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                ClanInvite invite = new ClanInvite()
                {
                    Id = this.Field0,
                    PlayerId = this.Client.PlayerId,
                    Text = this.Field1,
                    InviteDate = uint.Parse(DateTimeUtil.Now("yyyyMMdd"))
                };
                if (player.ClanId <= 0 && player.Nickname.Length != 0)
                {
                    if (ClanManager.GetClan(this.Field0).Id == 0)
                        this.Field2 = 2147483648U /*0x80000000*/;
                    else if (DaoManagerSQL.GetRequestClanInviteCount(this.Field0) >= 100)
                        this.Field2 = 2147487831U;
                    else if (!DaoManagerSQL.CreateClanInviteInDB(invite))
                        this.Field2 = 2147487848U;
                }
                else
                    this.Field2 = 2147487836U;
                this.Client.SendPacket(new PROTOCOL_CS_JOIN_REQUEST_ACK(this.Field2, this.Field0));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CS_JOIN_REQUEST_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}