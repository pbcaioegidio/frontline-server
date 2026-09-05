// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ServerPacket.PROTOCOL_MATCH_CLAN_SEASON_ACK
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using System.Runtime.CompilerServices;


namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_MATCH_CLAN_SEASON_ACK : AuthServerPacket
    {
        private const int LadderSeasonSize = 121;
        private const int LadderSeasonCount = 2;

        private static readonly byte[] EmptyClanSeasonBlock =
            new byte[LadderSeasonSize * LadderSeasonCount];

        public PROTOCOL_MATCH_CLAN_SEASON_ACK(int A_1)
        {
        }

        
        public override void Write()
        {
            this.WriteH((short)7702);
            this.WriteB(EmptyClanSeasonBlock);
        }
    }
}
