// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ServerPacket.PROTOCOL_AUTH_FRIEND_INFO_CHANGE_ACK
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using System.Runtime.CompilerServices;

namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_AUTH_FRIEND_INFO_CHANGE_ACK : AuthServerPacket
    {
        private readonly FriendModel Field0;
        private readonly int Field1;
        private readonly FriendState Field2;
        private readonly FriendChangeState Field3;

        public PROTOCOL_AUTH_FRIEND_INFO_CHANGE_ACK(
          FriendChangeState A_1,
          FriendModel A_2,
          FriendState A_3,
          int A_4)
        {
            this.Field2 = A_3;
            this.Field0 = A_2;
            this.Field3 = A_1;
            this.Field1 = A_4;
        }

        
        public override void Write()
        {
            this.WriteH((short)1815);
            this.WriteC((byte)this.Field3);
            this.WriteC((byte)this.Field1);
            if (this.Field3 == FriendChangeState.Insert || this.Field3 == FriendChangeState.Update)
            {
                PlayerInfo info = this.Field0.Info;
                if (info == null)
                {
                    this.WriteB(new byte[24]);
                }
                else
                {
                    this.WriteC((byte)(info.Nickname.Length + 1));
                    this.WriteN(info.Nickname, info.Nickname.Length + 2, "UTF-16LE");
                    this.WriteQ(this.Field0.PlayerId);
                    this.WriteD(ComDiv.GetFriendStatus(this.Field0, this.Field2));
                    this.WriteD(uint.MaxValue);
                    this.WriteC((byte)info.Rank);
                    this.WriteB(new byte[6]);
                }
            }
            else
                this.WriteB(new byte[24]);
        }
    }
}