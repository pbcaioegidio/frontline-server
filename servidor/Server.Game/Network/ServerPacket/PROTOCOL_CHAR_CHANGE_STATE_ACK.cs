// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_CHAR_CHANGE_STATE_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CHAR_CHANGE_STATE_ACK : GameServerPacket
    {
        private readonly CharacterModel Field0;

        public PROTOCOL_CHAR_CHANGE_STATE_ACK(CharacterModel A_1) => this.Field0 = A_1;

        public override void Write()
        {
            this.WriteH((short)6153);
            this.WriteH((short)0);
            this.WriteD(0);
            this.WriteC((byte)20);
            this.WriteC((byte)this.Field0.Slot);
        }
    }
}