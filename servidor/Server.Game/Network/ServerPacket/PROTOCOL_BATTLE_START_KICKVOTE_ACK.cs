// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BATTLE_START_KICKVOTE_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BATTLE_START_KICKVOTE_ACK : GameServerPacket
    {
        private readonly VoteKickModel Field0;

        public PROTOCOL_BATTLE_START_KICKVOTE_ACK(VoteKickModel A_1) => this.Field0 = A_1;

        public override void Write()
        {
            this.WriteH((short)5193);
            this.WriteC((byte)this.Field0.CreatorIdx);
            this.WriteC((byte)this.Field0.VictimIdx);
            this.WriteC((byte)this.Field0.Motive);
        }
    }
}
