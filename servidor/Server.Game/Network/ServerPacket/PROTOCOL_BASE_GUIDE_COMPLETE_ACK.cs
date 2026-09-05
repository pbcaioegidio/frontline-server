// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BASE_GUIDE_COMPLETE_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_GUIDE_COMPLETE_ACK : GameServerPacket
    {
        public override void Write()
        {
            this.WriteH((short)2341);
            this.WriteD(0);
            this.WriteD(0);
            this.WriteD(0);
            this.WriteD(0);
            this.WriteD(0);
            this.WriteD(0);
        }
    }
}