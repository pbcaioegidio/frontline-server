// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BASE_QUEST_DELETE_CARD_SET_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Game.Data.Models;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_QUEST_DELETE_CARD_SET_ACK : GameServerPacket
    {
        private readonly uint Field0;
        private readonly Account Field1;

        public PROTOCOL_BASE_QUEST_DELETE_CARD_SET_ACK(uint A_1, Account A_2)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
        }

        public override void Write()
        {
            this.WriteH((short)2367);
            this.WriteD(this.Field0);
            if (this.Field0 != 0U)
                return;
            this.WriteB(MissionWire.BuildQuestingInfo(this.Field1.Mission)); // 89B block, no gold/cash
        }
    }
}