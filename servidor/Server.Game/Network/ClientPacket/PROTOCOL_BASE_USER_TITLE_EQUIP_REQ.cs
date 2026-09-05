// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BASE_USER_TITLE_EQUIP_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.XML;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_USER_TITLE_EQUIP_REQ : GameClientPacket
    {
        private byte Field0;
        private byte Field1;

        public override void Read()
        {
            this.Field0 = this.ReadC();
            this.Field1 = this.ReadC();
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                PlayerTitles title1 = player.Title;
                TitleModel title2 = TitleSystemXML.GetTitle((int)this.Field1);
                TitleModel title1_1;
                TitleModel title2_1;
                TitleModel title3;
                TitleSystemXML.Get3Titles(title1.Equiped1, title1.Equiped2, title1.Equiped3, out title1_1, out title2_1, out title3, false);
                if (this.Field0 < (byte)3 && this.Field1 < (byte)45 && title1 != null && title2 != null && (title2.ClassId != title1_1.ClassId || this.Field0 == (byte)0) && (title2.ClassId != title2_1.ClassId || this.Field0 == (byte)1) && (title2.ClassId != title3.ClassId || this.Field0 == (byte)2) && title1.Contains(title2.Flag) && title1.Equiped1 != (int)this.Field1 && title1.Equiped2 != (int)this.Field1 && title1.Equiped3 != (int)this.Field1)
                {
                    if (!DaoManagerSQL.UpdateEquipedPlayerTitle(title1.OwnerId, (int)this.Field0, (int)this.Field1))
                    {
                        this.Client.SendPacket(new PROTOCOL_BASE_USER_TITLE_EQUIP_ACK(2147483648U /*0x80000000*/, -1, -1));
                    }
                    else
                    {
                        title1.SetEquip((int)this.Field0, (int)this.Field1);
                        this.Client.SendPacket(new PROTOCOL_BASE_USER_TITLE_EQUIP_ACK(0U, (int)this.Field0, (int)this.Field1));
                    }
                }
                else
                    this.Client.SendPacket(new PROTOCOL_BASE_USER_TITLE_EQUIP_ACK(2147483648U /*0x80000000*/, -1, -1));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BASE_USER_TITLE_EQUIP_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}