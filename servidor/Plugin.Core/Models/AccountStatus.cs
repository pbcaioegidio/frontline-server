// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.AccountStatus
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Utility;
using System;
using System.Runtime.CompilerServices;

namespace Plugin.Core.Models
{
    public class AccountStatus
    {
        public long PlayerId { get; set; }

        public byte ChannelId { get; set; }

        public byte RoomId { get; set; }

        public byte ClanMatchId { get; set; }

        public byte ServerId { get; set; }

        public byte[] Buffer { get; set; }

        public AccountStatus() => this.Buffer = new byte[4];

        
        public void ResetData(long PlayerId)
        {
            if (PlayerId == 0L)
                return;
            int channelId1 = (int)this.ChannelId;
            int roomId = (int)this.RoomId;
            int clanMatchId = (int)this.ClanMatchId;
            int serverId = (int)this.ServerId;
            this.SetData(uint.MaxValue, PlayerId);
            int channelId2 = (int)this.ChannelId;
            if (channelId1 == channelId2 && roomId == (int)this.RoomId && clanMatchId == (int)this.ClanMatchId && serverId == (int)this.ServerId)
                return;
            ComDiv.UpdateDB("accounts", "status", (object)(long)uint.MaxValue, "player_id", (object)PlayerId);
        }

        public void SetData(uint Data, long PlayerId)
        {
            this.SetData(BitConverter.GetBytes(Data), PlayerId);
        }

        public void SetData(byte[] Buffer, long PlayerId)
        {
            this.PlayerId = PlayerId;
            this.Buffer = Buffer;
            this.ChannelId = Buffer[0];
            this.RoomId = Buffer[1];
            this.ServerId = Buffer[2];
            this.ClanMatchId = Buffer[3];
        }

        public void UpdateChannel(byte ChannelId)
        {
            this.ChannelId = ChannelId;
            this.Buffer[0] = ChannelId;
            this.Method0();
        }

        public void UpdateRoom(byte RoomId)
        {
            this.RoomId = RoomId;
            this.Buffer[1] = RoomId;
            this.Method0();
        }

        public void UpdateServer(byte ServerId)
        {
            this.ServerId = ServerId;
            this.Buffer[2] = ServerId;
            this.Method0();
        }

        public void UpdateClanMatch(byte ClanMatchId)
        {
            this.ClanMatchId = ClanMatchId;
            this.Buffer[3] = ClanMatchId;
            this.Method0();
        }

        
        private void Method0()
        {
            ComDiv.UpdateDB("accounts", "status", (object)(long)BitConverter.ToUInt32(this.Buffer, 0), "player_id", (object)this.PlayerId);
        }
    }
}