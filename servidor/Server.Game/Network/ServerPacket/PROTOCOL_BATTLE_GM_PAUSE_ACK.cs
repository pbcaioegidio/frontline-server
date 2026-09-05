// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BATTLE_GM_PAUSE_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BATTLE_GM_PAUSE_ACK : GameServerPacket
    {
        private readonly uint Field0;

        public PROTOCOL_BATTLE_GM_PAUSE_ACK(uint A_1) => this.Field0 = A_1;

        public override void Write()
        {
            this.WriteH((short)5206);
            this.WriteD(this.Field0);
            // The client tests this status SIGNED at 0xEE1BFA (`if (v7 < 0)`), so an unsigned
            // `!= 0U` bail omitted the trailing dword on every positive non-zero status while
            // the client still took the success arm and read it.
            if ((int)this.Field0 < 0)
                return;
            this.WriteD(1);
        }
    }
}