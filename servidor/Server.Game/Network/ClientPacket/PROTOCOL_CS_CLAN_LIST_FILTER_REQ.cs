// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CS_CLAN_LIST_FILTER_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Server.Game.Data.Managers;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CS_CLAN_LIST_FILTER_REQ : GameClientPacket
    {
        private byte Field0;
        private byte Field1;
        private ClanSearchType Field2;
        private string Field3;

        public override void Read()
        {
            this.Field0 = this.ReadC();
            this.Field3 = this.ReadU((int)this.ReadC() * 2);
            this.Field1 = this.ReadC();
            this.Field2 = (ClanSearchType)this.ReadC();
        }

        public override void Run()
        {
            try
            {
                int A_2_1 = 0;
                int A_2_3 = 0;
                using (SyncServerPacket A_2_2 = new SyncServerPacket())
                {
                    lock (ClanManager.Clans)
                    {
                        A_2_3 = ClanManager.Clans.Count;
                        for (byte field0 = this.Field0; (int)field0 < ClanManager.Clans.Count; ++field0)
                        {
                            this.Method0(ClanManager.Clans[(int)field0], A_2_2);
                            if (++A_2_1 == 15)
                                break;
                        }
                    }
                    byte[] rows = A_2_2.ToArray();
                    this.Client.SendPacket(new PROTOCOL_CS_CLAN_LIST_FILTER_ACK(this.Field0, A_2_1, A_2_3, rows));
                    this.Client.SendPacket(new PROTOCOL_CS_CLAN_LIST_FILTER_ACK(this.Field0, A_2_1, A_2_3, rows));
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        
        private void Method0(ClanModel A_1, SyncServerPacket A_2)
        {
            A_2.WriteD(A_1.Id);
            A_2.WriteC((byte)(A_1.Name.Length + 1));
            A_2.WriteN(A_1.Name, A_1.Name.Length + 2, "UTF-16LE");
            A_2.WriteD(A_1.Logo);
            A_2.WriteH((ushort)(A_1.Info.Length + 1));
            A_2.WriteN(A_1.Info, A_1.Info.Length + 2, "UTF-16LE");
            A_2.WriteC((byte)0);
        }
    }
}
