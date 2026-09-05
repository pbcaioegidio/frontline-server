// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_AUTH_FRIEND_INFO_CHANGE_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_AUTH_FRIEND_INFO_CHANGE_ACK : GameServerPacket
    {
        private readonly FriendModel Field0;
        private readonly int Field1;
        private readonly FriendState Field2;
        private readonly FriendChangeState Field3;

        public PROTOCOL_AUTH_FRIEND_INFO_CHANGE_ACK(FriendChangeState A_1, FriendModel A_2, int A_3)
        {
            this.Field3 = A_1;
            this.Field2 = (FriendState)A_2.State;
            this.Field0 = A_2;
            this.Field1 = A_3;
        }

        public PROTOCOL_AUTH_FRIEND_INFO_CHANGE_ACK(
          FriendChangeState A_1,
          FriendModel A_2,
          int A_3,
          int A_4)
        {
            this.Field3 = A_1;
            this.Field2 = (FriendState)A_3;
            this.Field0 = A_2;
            this.Field1 = A_4;
        }

        public PROTOCOL_AUTH_FRIEND_INFO_CHANGE_ACK(
          FriendChangeState A_1,
          FriendModel A_2,
          FriendState A_3,
          int A_4)
        {
            this.Field3 = A_1;
            this.Field2 = A_3;
            this.Field0 = A_2;
            this.Field1 = A_4;
        }

        
        public override void Write()
        {
            this.WriteH((short)1815);
            this.WriteC((byte)this.Field3);
            this.WriteC((byte)this.Field1);
            if (this.Field3 != FriendChangeState.Insert && this.Field3 != FriendChangeState.Update)
            {
                this.WriteB(new byte[24]);
            }
            else
            {
                PlayerInfo info = this.Field0.Info;
                if (info != null)
                {
                    this.WriteC((byte)(info.Nickname.Length + 1));
                    this.WriteN(info.Nickname, info.Nickname.Length + 2, "UTF-16LE");
                    this.WriteQ(this.Field0.PlayerId);
                    this.WriteD(ComDiv.GetFriendStatus(this.Field0, this.Field2));
                    this.WriteD(0U);
                    this.WriteC((byte)info.Rank);
                    this.WriteB(new byte[6]);
                }
                else
                    this.WriteB(new byte[24]);
            }
        }
    }
}