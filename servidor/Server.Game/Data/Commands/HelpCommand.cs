// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Commands.HelpCommand
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network;
using Server.Game.Network.ServerPacket;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;


namespace Server.Game.Data.Commands
{
    public class HelpCommand : ICommand
    {
        private readonly int Field0 = 5;

        public string Command
        {
            
            get => "help";
        }

        public string Description
        {
            
            get => "Show available commands";
        }

        public string Permission
        {
            
            get => "helpcommand";
        }

        public string Args
        {
            
            get => "%page% (optional)";
        }

        
        public string Execute(string Command, string[] Args, Account Player)
        {
            int num1 = 1;
            if (Args.Length != 0)
            {
                int result;
                if (int.TryParse(Args[0], out result))
                {
                    if (result == 0)
                        result = 1;
                    num1 = result;
                }
                else
                    num1 = 1;
            }
            IEnumerable<ICommand> commandsForPlayer = CommandManager.GetCommandsForPlayer(Player);
            int num2 = (commandsForPlayer.Count<ICommand>() + this.Field0 - 1) / this.Field0;
            if (num1 > num2)
                return $"Please insert a valid page. Pages: {num2}";
            IEnumerable<ICommand> commands = commandsForPlayer.Split<ICommand>(this.Field0).ToArray<IEnumerable<ICommand>>()[num1 - 1];
            string A_1 = $"Commands ({num1}/{num2}):\n";
            foreach (ICommand command in commands)
                A_1 = $"{A_1}:{command.Command} {command.Args} -> {command.Description}\n";
            Player.Connection.SendPacket(new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(A_1));
            return $"Displayed commands page '{num1}' of '{num2}'";
        }
    }
}