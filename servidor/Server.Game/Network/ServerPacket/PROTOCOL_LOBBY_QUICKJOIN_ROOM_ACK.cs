// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_LOBBY_QUICKJOIN_ROOM_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using Server.Game.Data.Models;
using System.Collections.Generic;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_LOBBY_QUICKJOIN_ROOM_ACK : GameServerPacket
    {
        private readonly uint Field0;
        private readonly List<QuickstartModel> Field1;
        private readonly QuickstartModel Field2;
        private readonly RoomModel Field3;

        public PROTOCOL_LOBBY_QUICKJOIN_ROOM_ACK(
          uint A_1,
          List<QuickstartModel> A_2,
          RoomModel A_3,
          QuickstartModel A_4)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
            this.Field2 = A_4;
            this.Field3 = A_3;
        }

        public override void Write()
        {
            this.WriteH((short)5378);
            this.WriteD(this.Field0);
            foreach (QuickstartModel quickstartModel in this.Field1)
            {
                this.WriteC((byte)quickstartModel.MapId);
                this.WriteC((byte)quickstartModel.Rule);
                this.WriteC((byte)quickstartModel.StageOptions);
                this.WriteC((byte)quickstartModel.Type);
            }
            if (this.Field0 != 0U)
                return;
            this.WriteC((byte)this.Field3.ChannelId);
            this.WriteD(this.Field3.RoomId);
            this.WriteH((short)1);
            if (this.Field0 != 0U)
            {
                this.WriteC((byte)this.Field2.MapId);
                this.WriteC((byte)this.Field2.Rule);
                this.WriteC((byte)this.Field2.StageOptions);
                this.WriteC((byte)this.Field2.Type);
            }
            else
            {
                this.WriteC((byte)0);
                this.WriteC((byte)0);
                this.WriteC((byte)0);
                this.WriteC((byte)0);
            }
            this.WriteD(0);
            this.WriteD(0);
            this.WriteD(0);
            this.WriteD(0);
        }
    }
}