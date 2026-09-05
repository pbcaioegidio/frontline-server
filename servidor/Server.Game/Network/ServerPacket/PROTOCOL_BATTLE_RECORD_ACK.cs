// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BATTLE_RECORD_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using Server.Game.Data.Models;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BATTLE_RECORD_ACK : GameServerPacket
    {
        private readonly RoomModel Field0;

        public PROTOCOL_BATTLE_RECORD_ACK(RoomModel A_1) => this.Field0 = A_1;

        public override void Write()
        {
            this.WriteH((short)5163);
            this.WriteH((ushort)this.Field0.FRKills);
            this.WriteH((ushort)this.Field0.FRDeaths);
            this.WriteH((ushort)this.Field0.FRAssists);
            this.WriteH((ushort)this.Field0.CTKills);
            this.WriteH((ushort)this.Field0.CTDeaths);
            this.WriteH((ushort)this.Field0.CTAssists);
            foreach (SlotModel slot in this.Field0.Slots)
            {
                this.WriteH((ushort)slot.AllKills);
                this.WriteH((ushort)slot.AllDeaths);
                this.WriteH((ushort)slot.AllAssists);
            }
        }
    }
}