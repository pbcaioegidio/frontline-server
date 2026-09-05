// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_ROOM_CREATE_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Server.Game.Data.Models;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_ROOM_CREATE_ACK : GameServerPacket
    {
        private readonly RoomModel Field0;
        private readonly uint Field1;

        public PROTOCOL_ROOM_CREATE_ACK(uint A_1, RoomModel A_2)
        {
            this.Field1 = A_1;
            this.Field0 = A_2;
        }

        public override void Write()
        {
            this.WriteH((short)3593);
            this.WriteD(this.Field1 == 0U ? (uint)this.Field0.RoomId : this.Field1);
            if (this.Field1 != 0U)
                return;
            this.WriteRoomInfoBlock(this.Field0);
        }
    }
}
