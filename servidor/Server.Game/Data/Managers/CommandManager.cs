// Deobfuscated with JetBrains decompiler
// Type: Server.Game.Data.Managers.CommandManager
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Server.Game.Data.Commands;
using Server.Game.Data.Models;
using Server.Game.Network;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Server.Game.Data.Managers
{
    public static class CommandManager
    {
        private static readonly Dictionary<string, ICommand> commandDictionary = new Dictionary<string, ICommand>();

        static CommandManager()
        {
            // Load all command classes that implement ICommand interface from all assemblies
            foreach (ICommand command in AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(ICommand).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                .Select(type => Activator.CreateInstance(type))
                .Cast<ICommand>())
            {
                commandDictionary.Add(command.Command, command);
            }
        }

        
        public static bool TryParse(string text, Account player)
        {
            text = text.Trim();
            if (text.Length == 0 || player == null || !text.StartsWith(":"))
                return false;

            string commandName = text.Substring(1);
            string[] arguments = new string[0];

            if (commandName.Contains(" "))
            {
                string[] parts = commandName.Split(new string[1] { " " }, StringSplitOptions.None);
                commandName = parts[0];
                arguments = parts.Skip(1).ToArray();
            }

            return ExecuteCommand(player, commandDictionary, commandName, arguments);
        }

        public static IEnumerable<ICommand> GetCommandsForPlayer(Account player)
        {
            if (player == null)
                return Enumerable.Empty<ICommand>();

            return commandDictionary.Values.Where(command => player.HavePermission(command.Permission));
        }

        
        private static bool ExecuteCommand(
            Account player,
            Dictionary<string, ICommand> commands,
            string commandName,
            string[] arguments)
        {
            if (!commands.ContainsKey(commandName))
                return false;

            ICommand command = commands[commandName];
            if (command == null || !player.HavePermission(command.Permission))
                return false;

            string result = command.Execute(commandName, arguments, player);
            player.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, result));
            return true;
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int limit)
        {
            return list.Select((item, index) => new { item, index })
                      .GroupBy(x => x.index / limit)
                      .Select(group => group.Select(x => x.item));
        }
    }
}