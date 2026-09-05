// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_CS_CHATTING_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Server.Game.Data.Models;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_CHATTING_ACK : GameServerPacket
    {
        private readonly string Field0;
        private readonly Account Field1;
        private readonly int Field2;
        private readonly int Field3;

        public PROTOCOL_CS_CHATTING_ACK(string A_1, Account A_2)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
        }

        public PROTOCOL_CS_CHATTING_ACK(int A_1, int A_2)
        {
            this.Field2 = A_1;
            this.Field3 = A_2;
        }

        
        public override void Write()
        {
            this.WriteH((short)855);
            if (this.Field2 == 0)
            {
                this.WriteC((byte)(this.Field1.Nickname.Length + 1));
                this.WriteN(this.Field1.Nickname, this.Field1.Nickname.Length + 2, "UTF-16LE");
                this.WriteC(this.Field1.UseChatGM() ? (byte)1 : (byte)0);
                this.WriteC((byte)this.Field1.NickColor);
                this.WriteC((byte)(this.Field0.Length + 1));
                this.WriteN(this.Field0, this.Field0.Length + 2, "UTF-16LE");
            }
            else
                this.WriteD(this.Field3);
        }
    }
}