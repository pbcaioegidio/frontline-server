// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_MATCH_CLAN_SEASON_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_MATCH_CLAN_SEASON_ACK : GameServerPacket
    {
        private const int LadderSeasonSize = 121;
        private const int LadderSeasonCount = 2;

        private static readonly byte[] EmptyClanSeasonBlock =
            new byte[LadderSeasonSize * LadderSeasonCount];

        public PROTOCOL_MATCH_CLAN_SEASON_ACK(bool A_1)
        {
        }

        
        public override void Write()
        {
            this.WriteH((short)7702);
            this.WriteB(EmptyClanSeasonBlock);
        }
    }
}
