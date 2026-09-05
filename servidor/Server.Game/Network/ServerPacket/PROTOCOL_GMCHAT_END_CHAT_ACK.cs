// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_GMCHAT_END_CHAT_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Server.Game.Data.Models;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_GMCHAT_END_CHAT_ACK : GameServerPacket
    {
        private readonly Account Field0;
        private readonly uint Field1;

        public PROTOCOL_GMCHAT_END_CHAT_ACK(uint A_1, Account A_2)
        {
            this.Field1 = A_1;
            this.Field0 = A_2;
        }

        public override void Write()
        {
            this.WriteH((short)6662);
            this.WriteH((short)0);
            this.WriteD(this.Field1);
        }
    }
}