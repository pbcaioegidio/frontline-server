// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_CS_REPLACE_NAME_RESULT_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_REPLACE_NAME_RESULT_ACK : GameServerPacket
    {
        private readonly string Field0;

        public PROTOCOL_CS_REPLACE_NAME_RESULT_ACK(string A_1) => this.Field0 = A_1;

        public override void Write()
        {
            this.WriteH((short)864);
            this.WriteU(this.Field0, 34);
        }
    }
}