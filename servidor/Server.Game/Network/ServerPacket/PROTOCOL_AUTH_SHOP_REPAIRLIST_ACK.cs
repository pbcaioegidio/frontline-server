// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_AUTH_SHOP_REPAIRLIST_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_AUTH_SHOP_REPAIRLIST_ACK : GameServerPacket
    {
        private readonly int Total;
        private readonly int Offset;
        private readonly byte[] Chunk;

        public PROTOCOL_AUTH_SHOP_REPAIRLIST_ACK(int total, int offset, byte[] chunk)
        {
            this.Total = total;
            this.Offset = offset;
            this.Chunk = chunk ?? new byte[0];
        }

        public override void Write()
        {
            this.WriteH((short)1075);
            this.WriteD(this.Total);
            this.WriteD(this.Offset);
            this.WriteD(this.Chunk.Length);
            this.WriteB(this.Chunk);
        }
    }
}
