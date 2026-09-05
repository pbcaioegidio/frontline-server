// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BATTLE_MISSION_TUTORIAL_ROUND_END_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Server.Game.Data.Models;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BATTLE_MISSION_TUTORIAL_ROUND_END_ACK : GameServerPacket
    {
        private readonly RoomModel Field0;

        public PROTOCOL_BATTLE_MISSION_TUTORIAL_ROUND_END_ACK(RoomModel A_1) => this.Field0 = A_1;

        public override void Write()
        {
            this.WriteH((short)5189);
            this.WriteC((byte)3);
            this.WriteH((short)(this.Field0.GetTimeByMask() * 60 - this.Field0.GetInBattleTime()));
        }
    }
}