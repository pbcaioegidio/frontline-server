// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_MESSENGER_NOTE_RECEIVE_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_MESSENGER_NOTE_RECEIVE_ACK : GameServerPacket
    {
        private readonly MessageModel Field0;

        public PROTOCOL_MESSENGER_NOTE_RECEIVE_ACK(MessageModel A_1) => this.Field0 = A_1;

        public override void Write()
        {
            this.WriteH((short)1931);
            this.WriteD((uint)this.Field0.ObjectId);
            this.WriteQ(this.Field0.SenderId);
            if (this.Field0.Type != NoteMessageType.ClanAsk)
                this.WriteC((byte)this.Field0.Type);
            else
                this.WriteC((byte)6);
            this.WriteC((byte)this.Field0.State);
            this.WriteC((byte)this.Field0.DaysRemaining);
            this.WriteD(this.Field0.ClanId);
            this.WriteB(this.NoteClanData(this.Field0));
        }

        
        public byte[] NoteClanData(MessageModel Message)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                syncServerPacket.WriteC((byte)(Message.SenderName.Length + 1));
                syncServerPacket.WriteC(Message.Type == NoteMessageType.Insert || Message.Type == NoteMessageType.ClanAsk || Message.Type == NoteMessageType.Clan && Message.ClanNote != NoteMessageClan.None ? (byte)0 : (byte)(Message.Text.Length + 1));
                syncServerPacket.WriteN(Message.SenderName, Message.SenderName.Length + 2, "UTF-16LE");
                if (Message.Type != NoteMessageType.ClanAsk && Message.Type != NoteMessageType.Clan)
                    syncServerPacket.WriteN(Message.Text, Message.Text.Length + 2, "UTF-16LE");
                else if (Message.ClanNote >= NoteMessageClan.JoinAccept && Message.ClanNote <= NoteMessageClan.Secession)
                {
                    syncServerPacket.WriteH((short)(Message.Text.Length + 1));
                    syncServerPacket.WriteH((short)Message.ClanNote);
                    syncServerPacket.WriteN(Message.Text, Message.Text.Length + 1, "UTF-16LE");
                }
                else if (Message.ClanNote == NoteMessageClan.None)
                {
                    syncServerPacket.WriteN(Message.Text, Message.Text.Length + 2, "UTF-16LE");
                }
                else
                {
                    syncServerPacket.WriteH((short)3);
                    syncServerPacket.WriteD((int)Message.ClanNote);
                    if (Message.ClanNote != NoteMessageClan.Master || Message.ClanNote != NoteMessageClan.Staff || Message.ClanNote != NoteMessageClan.Regular)
                        syncServerPacket.WriteH((short)0);
                }
                return syncServerPacket.ToArray();
            }
        }
    }
}