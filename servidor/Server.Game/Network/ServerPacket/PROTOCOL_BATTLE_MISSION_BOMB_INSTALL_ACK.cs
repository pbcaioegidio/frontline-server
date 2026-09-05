// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BATTLE_MISSION_BOMB_INSTALL_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BATTLE_MISSION_BOMB_INSTALL_ACK : GameServerPacket
    {
        private readonly int Field0;
        private readonly float Field1;
        private readonly float Field2;
        private readonly float Field3;
        private readonly byte Field4;
        private readonly ushort Field5;

        public PROTOCOL_BATTLE_MISSION_BOMB_INSTALL_ACK(
          int A_1,
          byte A_2,
          ushort A_3,
          float A_4,
          float A_5,
          float A_6)
        {
            this.Field4 = A_2;
            this.Field0 = A_1;
            this.Field5 = A_3;
            this.Field1 = A_4;
            this.Field2 = A_5;
            this.Field3 = A_6;
        }

        public override void Write()
        {
            this.WriteH((short)5157);
            this.WriteD(this.Field0);
            this.WriteC(this.Field4);
            this.WriteH(this.Field5);
            this.WriteT(this.Field1);
            this.WriteT(this.Field2);
            this.WriteT(this.Field3);
        }
    }
}