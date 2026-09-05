// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BASE_RANK_UP_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_RANK_UP_ACK : GameServerPacket
    {
        private readonly int NewRank;
        private readonly int OldRank;
        private readonly int NoticeGold;

        public PROTOCOL_BASE_RANK_UP_ACK(int NewRank, int OldRank, int NoticeGold)
        {
            this.NewRank = NewRank;
            this.OldRank = OldRank;
            this.NoticeGold = NoticeGold;
        }

        // Client 122 ClientTCPSocket__Parse_Base_RankUp (0xEC30E4) reads an 18-byte body:
        // D newRank, D oldRank, D noticeGold, H expRankingPos, D (read then discarded).
        // newRank > oldRank is what arms the lobby rank-up notice (singleton+0x79) and
        // updates the rank watermark; sending the rank twice keeps that path dormant.
        // The third dword is rendered by the notice as the gold credited for the promotion
        // (OBSERVED: a rank 6 promotion sent OnNextLevel 11000 and the client printed
        // "11000 Gold", while the gold actually credited was OnGoldUp 8000), so callers
        // must pass the gold award, not the experience threshold.
        // The 2-byte field lands in singleton+0x2C, the EXP ranking position read by
        // UserInfo__BuildExpTooltip. ReadData (0x1027050) skips a read that would overrun,
        // leaving the destination untouched, so a short packet left stack garbage there.
        public override void Write()
        {
            this.WriteH((short)2343);
            this.WriteD(this.NewRank);
            this.WriteD(this.OldRank);
            this.WriteD(this.NoticeGold);
            this.WriteH(0);
            this.WriteD(0);
        }
    }
}
