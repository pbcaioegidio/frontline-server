// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BASE_MEDAL_GET_INFO_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Server.Game.Data.Models;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_MEDAL_GET_INFO_ACK : GameServerPacket
    {
        private readonly Account Field0;

        public PROTOCOL_BASE_MEDAL_GET_INFO_ACK(Account A_1) => this.Field0 = A_1;

        public override void Write()
        {
            this.WriteH((short)2363);
            this.WriteQ(this.Field0.PlayerId);
            this.WriteD(this.Field0.Ribbon);
            this.WriteD(this.Field0.Ensign);
            this.WriteD(this.Field0.Medal);
            this.WriteD(this.Field0.MasterMedal);
        }
    }
}