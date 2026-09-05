// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_ROOM_INVITE_LOBBY_USER_LIST_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Server.Game.Data.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_ROOM_INVITE_LOBBY_USER_LIST_ACK : GameServerPacket
    {
        private readonly List<Account> Field0;
        private readonly List<int> Field1;

        public PROTOCOL_ROOM_INVITE_LOBBY_USER_LIST_ACK(ChannelModel A_1)
        {
            this.Field0 = A_1.GetWaitPlayers();
            this.Field1 = this.Method0(this.Field0.Count, this.Field0.Count >= 8 ? 8 : this.Field0.Count);
        }

        
        public override void Write()
        {
            this.WriteH((short)3677);
            this.WriteD(this.Field1.Count);
            foreach (int index in this.Field1)
            {
                Account account = this.Field0[index];
                this.WriteD(account.GetSessionId());
                this.WriteD(account.GetRank());
                this.WriteC((byte)(account.Nickname.Length + 1));
                this.WriteN(account.Nickname, account.Nickname.Length + 2, "UTF-16LE");
                this.WriteC((byte)account.NickColor);
            }
        }

        private List<int> Method0(int A_1, int A_2)
        {
            if (A_1 == 0 || A_2 == 0)
                return new List<int>();
            Random random = new Random();
            List<int> intList = new List<int>();
            for (int index = 0; index < A_1; ++index)
                intList.Add(index);
            for (int index1 = 0; index1 < intList.Count; ++index1)
            {
                int index2 = random.Next(intList.Count);
                int num = intList[index1];
                intList[index1] = intList[index2];
                intList[index2] = num;
            }
            return intList.GetRange(0, A_2);
        }
    }
}