// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_CS_PAGE_CHATTING_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Server.Game.Data.Models;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_PAGE_CHATTING_ACK : GameServerPacket
    {
        private readonly string Field0;
        private readonly string Field1;
        private readonly int Field2;
        private readonly int Field3;
        private readonly int Field4;
        private readonly bool Field5;

        public PROTOCOL_CS_PAGE_CHATTING_ACK(Account A_1, string A_2)
        {
            this.Field0 = A_1.Nickname;
            this.Field1 = A_2;
            this.Field5 = A_1.UseChatGM();
            this.Field4 = A_1.NickColor;
        }

        public PROTOCOL_CS_PAGE_CHATTING_ACK(int A_1, int A_2)
        {
            this.Field2 = A_1;
            this.Field3 = A_2;
        }

        
        public override void Write()
        {
            this.WriteH((short)887);
            this.WriteC((byte)this.Field2);
            if (this.Field2 == 0)
            {
                this.WriteC((byte)(this.Field0.Length + 1));
                this.WriteN(this.Field0, this.Field0.Length + 2, "UTF-16LE");
                this.WriteC(this.Field5 ? (byte)1 : (byte)0);
                this.WriteC((byte)this.Field4);
                this.WriteC((byte)(this.Field1.Length + 1));
                this.WriteN(this.Field1, this.Field1.Length + 2, "UTF-16LE");
            }
            else
                this.WriteD(this.Field3);
        }
    }
}