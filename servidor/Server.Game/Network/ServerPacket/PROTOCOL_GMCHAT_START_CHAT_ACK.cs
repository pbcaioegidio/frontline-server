// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_GMCHAT_START_CHAT_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Server.Game.Data.Models;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_GMCHAT_START_CHAT_ACK : GameServerPacket
    {
        private readonly Account Field0;
        private readonly uint Field1;

        public PROTOCOL_GMCHAT_START_CHAT_ACK(uint A_1, Account A_2)
        {
            this.Field1 = A_1;
            this.Field0 = A_2;
        }

        
        public override void Write()
        {
            this.WriteH((short)6658);
            this.WriteH((short)0);
            this.WriteD(this.Field1);
            if (this.Field1 != 0U)
                return;
            this.WriteC((byte)(this.Field0.Nickname.Length + 1));
            this.WriteN(this.Field0.Nickname, this.Field0.Nickname.Length + 2, "UTF-16LE");
            this.WriteQ(this.Field0.PlayerId);
        }
    }
}