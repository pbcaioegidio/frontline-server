// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BATTLE_PRESTARTBATTLE_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BATTLE_PRESTARTBATTLE_ACK : GameServerPacket
    {
        private readonly Account Field0;
        private readonly RoomModel Field1;
        private readonly bool Field2;
        private readonly bool Field3;
        private readonly uint Field4;
        private readonly uint Field5;

        public PROTOCOL_BATTLE_PRESTARTBATTLE_ACK(Account A_1, bool A_2)
        {
            this.Field0 = A_1;
            this.Field3 = A_2;
            this.Field1 = A_1.Room;
            if (this.Field1 == null)
                return;
            this.Field2 = this.Field1.IsPreparing();
            this.Field4 = this.Field1.UniqueRoomId;
            this.Field5 = this.Field1.Seed;
        }

        public PROTOCOL_BATTLE_PRESTARTBATTLE_ACK()
        {
        }

        public override void Write()
        {
            this.WriteH((short)5130);
            this.WriteD(this.Field2 ? 1 : 0);
            if (!this.Field2)
                return;
            this.WriteD(this.Field0.SlotId);
            this.WriteC(this.Field1.RoomType == RoomCondition.Tutorial ? (byte)1 : (byte)ConfigLoader.UdpType);
            this.WriteB(ComDiv.AddressBytes(ConfigLoader.HOST[2]));
            this.WriteB(ComDiv.AddressBytes(ConfigLoader.HOST[2]));
            this.WriteH((ushort)ConfigLoader.DEFAULT_PORT[2]);
            this.WriteD(this.Field4);
            this.WriteD(this.Field5);
            this.WriteB(this.Method0(this.Field3));
        }

        
        private byte[] Method0(bool A_1)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                if (A_1)
                {
                    string HexString = "02 14 03 15 04 16 05 17 06 18 07 19 08 1A 09 1B0A 1C 0B 1D 0C 1E 0D 1F 0E 20 0F 21 10 22 11 0012 01 13";
                    syncServerPacket.WriteB(Bitwise.HexStringToByteArray(HexString));
                }
                return syncServerPacket.ToArray();
            }
        }
    }
}