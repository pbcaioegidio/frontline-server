// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Commands.TeamCashCommand
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using Plugin.Core.SQL;
using Server.Game.Data.Models;
using Server.Game.Data.Sync.Server;
using Server.Game.Network;
using Server.Game.Network.ServerPacket;
using System.Runtime.CompilerServices;


namespace Server.Game.Data.Commands
{
    public class TeamCashCommand : ICommand
    {
        public string Command
        {
            
            get => "teamcash";
        }

        public string Description
        {
            
            get => "Send cash to a team";
        }

        public string Permission
        {
            
            get => "gamemastercommand";
        }

        public string Args
        {
            
            get => "%team% %cash% (Team = FR/CT)";
        }

        
        public string Execute(string Command, string[] Args, Account Player)
        {
            if (Player.Room == null)
                return "Please execute the command in a room";
            if (Args.Length != 2)
                return "Please use correct command usage, :teamcash %team% %cash%";
            int result;
            if (!int.TryParse(Args[1], out result))
                return "Please use correct number as value";
            string lower = Args[0].ToLower();
            if (lower != "red" && lower != "blue")
                return "Please use correct team, 'red' or 'blue'";
            int num = lower == "red" ? 0 : 1;
            RoomModel room = Player.Room;
            for (int SlotIdx = 0; SlotIdx < 18; ++SlotIdx)
            {
                if (SlotIdx % 2 == num)
                {
                    SlotModel slot = room.GetSlot(SlotIdx);
                    if (slot.PlayerId > 0L)
                    {
                        Account playerBySlot = room.GetPlayerBySlot(slot);
                        if (playerBySlot != null && DaoManagerSQL.UpdateAccountCash(playerBySlot.PlayerId, playerBySlot.Cash + result))
                        {
                            playerBySlot.Cash += result;
                            playerBySlot.SendPacket(new PROTOCOL_AUTH_GET_POINT_CASH_ACK(0U, playerBySlot));
                            SendItemInfo.LoadGoldCash(playerBySlot);
                        }
                    }
                }
            }
            return $"'{result}' cash sended to team {lower}";
        }
    }
}