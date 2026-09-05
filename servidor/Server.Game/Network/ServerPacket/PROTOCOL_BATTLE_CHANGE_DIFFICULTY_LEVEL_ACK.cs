// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BATTLE_CHANGE_DIFFICULTY_LEVEL_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Server.Game.Data.Models;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BATTLE_CHANGE_DIFFICULTY_LEVEL_ACK : GameServerPacket
    {
        private readonly RoomModel Field0;

        public PROTOCOL_BATTLE_CHANGE_DIFFICULTY_LEVEL_ACK(RoomModel A_1) => this.Field0 = A_1;

        public override void Write()
        {
            this.WriteH((short)5173);
            this.WriteC(this.Field0.IngameAiLevel);
            for (int index = 0; index < 18; ++index)
                this.WriteD(this.Field0.Slots[index].AiLevel);
        }
    }
}