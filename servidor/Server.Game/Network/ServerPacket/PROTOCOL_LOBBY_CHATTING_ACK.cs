// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_LOBBY_CHATTING_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Server.Game.Data.Models;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_LOBBY_CHATTING_ACK : GameServerPacket
    {
        private readonly string Field0;
        private readonly string Field1;
        private readonly int Field2;
        private readonly int Field3;
        private readonly bool Field4;

        public PROTOCOL_LOBBY_CHATTING_ACK(Account A_1, string A_2, bool A_3 = false)
        {
            if (!A_3)
            {
                this.Field3 = A_1.NickColor;
                this.Field4 = A_1.UseChatGM();
            }
            else
                this.Field4 = true;
            this.Field0 = A_1.Nickname;
            this.Field2 = A_1.GetSessionId();
            this.Field1 = A_2;
        }

        public PROTOCOL_LOBBY_CHATTING_ACK(string A_1, int A_2, int A_3, bool A_4, string A_5)
        {
            this.Field0 = A_1;
            this.Field2 = A_2;
            this.Field3 = A_3;
            this.Field4 = A_4;
            this.Field1 = A_5;
        }

        
        public override void Write()
        {
            this.WriteH((short)2571);
            this.WriteD(this.Field2);
            this.WriteC((byte)(this.Field0.Length + 1));
            this.WriteN(this.Field0, this.Field0.Length + 2, "UTF-16LE");
            this.WriteC((byte)this.Field3);
            this.WriteC(this.Field4 ? (byte)1 : (byte)0);
            this.WriteH((ushort)(this.Field1.Length + 1));
            this.WriteN(this.Field1, this.Field1.Length + 2, "UTF-16LE");
        }
    }
}