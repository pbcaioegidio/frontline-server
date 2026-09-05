// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Commands.MessageCommand
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network;
using Server.Game.Network.ServerPacket;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Server.Game.Data.Commands
{
    public class MessageCommand : ICommand
    {
        public string Command
        {
            
            get => "sendmsg";
        }

        public string Description
        {
            
            get => "Send messages";
        }

        public string Permission
        {
            
            get => "moderatorcommand";
        }

        public string Args
        {
            
            get => "%options% %text%";
        }

        
        public string Execute(string Command, string[] Args, Account Player)
        {
            string lower = Args[0].ToLower();
            string A_1 = string.Join(" ", Args, 1, Args.Length - 1);
            switch (lower)
            {
                case "room":
                    RoomModel room = Player.Room;
                    if (room == null)
                        return "A room is required to execute the command.";
                    using (PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK Packet = new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(A_1))
                        room.SendPacketToPlayers(Packet);
                    return $"Message sended to current Room Id: {room.RoomId + 1}";
                case "channel":
                    ChannelModel channel = Player.GetChannel();
                    if (channel == null)
                        return "Please run the command in the lobby.";
                    using (PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK Packet = new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(A_1))
                        channel.SendPacketToWaitPlayers(Packet);
                    return $"Message sended to current Channel Id: {channel.Id + 1}";
                case "player":
                    int num = 0;
                    using (PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK messageAnnounceAck = new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(A_1))
                    {
                        SortedList<long, Account> accounts = AccountManager.Accounts;
                        if (accounts.Count == 0)
                            num = 0;
                        byte[] completeBytes = messageAnnounceAck.GetCompleteBytes("Player.MessageCommands");
                        foreach (Account account in (IEnumerable<Account>)accounts.Values)
                        {
                            account.SendCompletePacket(completeBytes, messageAnnounceAck.GetType().Name);
                            ++num;
                        }
                    }
                    return $"Message sended to {num} total of player(s)";
                default:
                    return $"Command {ComDiv.ToTitleCase(lower)} was not founded!";
            }
        }
    }
}