// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_ROOM_CHANGE_PASSWD_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_ROOM_CHANGE_PASSWD_ACK : GameServerPacket
    {
        private readonly string Field0;

        public PROTOCOL_ROOM_CHANGE_PASSWD_ACK(string A_1) => this.Field0 = A_1;

        public override void Write()
        {
            this.WriteH((short)3603);
            this.WriteS(this.Field0, 4);
        }
    }
}