// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BATTLEBOX_AUTH_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Server.Game.Data.Models;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BATTLEBOX_AUTH_ACK : GameServerPacket
    {
        private readonly uint Field0;
        private readonly Account Field1;
        private readonly int Field2;

        public PROTOCOL_BATTLEBOX_AUTH_ACK(uint A_1, Account A_2 = null, int A_3 = 0)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
            this.Field2 = A_3;
        }

        public override void Write()
        {
            this.WriteH((short)7430);
            this.WriteH((short)0);
            this.WriteD(this.Field0);
            if (this.Field0 != 0U)
                return;
            this.WriteD(this.Field2);
            this.WriteB(new byte[5]);
            this.WriteD(this.Field1.Tags);
        }
    }
}