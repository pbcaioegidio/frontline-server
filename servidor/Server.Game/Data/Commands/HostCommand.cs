// Deobfuscated with JetBrains decompiler
// Type: Server.Game.Data.Commands.HostCommand
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.JSON;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using System;
using System.Runtime.CompilerServices;

namespace Server.Game.Data.Commands
{
    public class HostCommand : ICommand
    {
        public string Command
        {
            
            get => "host";
        }

        public string Description
        {
            
            get => "Change room options (AI Only)";
        }

        public string Permission
        {
            
            get => "hostcommand";
        }

        public string Args
        {
            
            get => "%options% %value%";
        }

        internal static uint ComputeFnv1aHash(string input)
        {
            // Constantes del algoritmo FNV-1a (32-bit)
            const uint FNV_OFFSET_BASIS = 2166136261U;  // Valor inicial FNV-1a
            const uint FNV_PRIME = 16777619U;           // Número primo FNV

            // Bug fix: inicializar el hash correctamente
            uint hash = FNV_OFFSET_BASIS;

            if (input != null)
            {
                // Implementación del algoritmo FNV-1a:
                // Para cada byte en la entrada:
                // 1. XOR el hash actual con el byte
                // 2. Multiplicar por el primo FNV
                for (int i = 0; i < input.Length; i++)
                {
                    hash = (hash ^ (uint)input[i]) * FNV_PRIME;
                }
            }

            return hash;
        }

        public string Execute(string command, string[] args, Account player)
        {
            RoomModel room = player.Room;
            if (room == null)
                return "A room is required to execute the command.";

            Account roomLeader;
            if (!room.GetLeader(out roomLeader) || roomLeader != player)
                return "This Command Is Only Valid For Host (Room Leader).";

            string option = args[0].ToLower();
            string value = args[1].ToLower();

            switch (option)
            {
                case "wpn":
                    CommandHelper weaponsConfig = CommandHelperJSON.GetTag("WeaponsFlag");

                    // Use FNV-1a hash for fast string comparison instead of multiple string.Equals()
                    switch (ComputeFnv1aHash(value))
                    {
                        case 321211332: // Hash for "all"
                            if (value == "all")
                            {
                                room.WeaponsFlag = (RoomWeaponsFlag)weaponsConfig.AllWeapons;
                                room.UpdateRoomInfo();
                                return "All Weapons (AR, SMG, SR, SG, MG, SHD)";
                            }
                            break;

                        case 978454399: // Hash for "sg"
                            if (value == "sg")
                            {
                                room.WeaponsFlag = (RoomWeaponsFlag)weaponsConfig.ShotGun;
                                room.UpdateRoomInfo();
                                return "Weapon Shot Gun (Only)";
                            }
                            break;

                        case 1163008208: // Hash for "sr"
                            if (value == "sr")
                            {
                                room.WeaponsFlag = (RoomWeaponsFlag)weaponsConfig.SniperRifle;
                                room.UpdateRoomInfo();
                                return "Weapon Sniper Rifle (Only)";
                            }
                            break;

                        case 1406715976: // Hash for "rpg"
                            if (value == "rpg")
                            {
                                room.WeaponsFlag = (RoomWeaponsFlag)weaponsConfig.RPG7;
                                room.UpdateRoomInfo();
                                return "Weapon RPG-7 (Only) - Hot Glitch";
                            }
                            break;

                        case 1562713850: // Hash for "ar"
                            if (value == "ar")
                            {
                                room.WeaponsFlag = (RoomWeaponsFlag)weaponsConfig.AssaultRifle;
                                room.UpdateRoomInfo();
                                return "Weapon Assault Rifle (Only)";
                            }
                            break;

                        case 1781411277: // Hash for "mg"
                            if (value == "mg")
                            {
                                room.WeaponsFlag = (RoomWeaponsFlag)weaponsConfig.MachineGun;
                                room.UpdateRoomInfo();
                                return "Weapon Machine Gun (Only)";
                            }
                            break;

                        case 3879387822: // Hash for "smg"
                            if (value == "smg")
                            {
                                room.WeaponsFlag = (RoomWeaponsFlag)weaponsConfig.SubMachineGun;
                                room.UpdateRoomInfo();
                                return "Weapon Sub Machine Gun (Only)";
                            }
                            break;
                    }

                    // Default fallback if no hash matches
                    room.WeaponsFlag = (RoomWeaponsFlag)weaponsConfig.AllWeapons;
                    room.UpdateRoomInfo();
                    return "Weapon Default (Value Not Founded)";

                case "time":
                    CommandHelper timeConfig = CommandHelperJSON.GetTag("PlayTime");
                    int minutes = int.Parse(value);

                    if (minutes > 15)
                    {
                        switch (minutes)
                        {
                            case 20:
                                room.KillTime = timeConfig.Minutes20;
                                room.UpdateRoomInfo();
                                return ComDiv.ToTitleCase(option) + " 20 Minutes";
                            case 25:
                                room.KillTime = timeConfig.Minutes25;
                                room.UpdateRoomInfo();
                                return ComDiv.ToTitleCase(option) + " 25 Minutes";
                            case 30:
                                room.KillTime = timeConfig.Minutes30;
                                room.UpdateRoomInfo();
                                return ComDiv.ToTitleCase(option) + " 30 Minutes";
                        }
                    }
                    else
                    {
                        switch (minutes)
                        {
                            case 5:
                                room.KillTime = timeConfig.Minutes05;
                                room.UpdateRoomInfo();
                                return ComDiv.ToTitleCase(option) + " 5 Minutes";
                            case 10:
                                room.KillTime = timeConfig.Minutes10;
                                room.UpdateRoomInfo();
                                return ComDiv.ToTitleCase(option) + " 10 Minutes";
                            case 15:
                                room.KillTime = timeConfig.Minutes15;
                                room.UpdateRoomInfo();
                                return ComDiv.ToTitleCase(option) + " 15 Minutes";
                        }
                    }
                    return ComDiv.ToTitleCase(option) + " None! (Wrong Value)";

                case "compe":
                    switch (value.ToLower())
                    {
                        case "on":
                            if (room.GetSlotCount() != 6 && room.GetSlotCount() != 8 && room.GetSlotCount() != 10)
                                return "Please change the slots to (3v3) or (4v4) or (5v5)";
                            room.Name = "Competitive!!!";
                            room.Competitive = true;
                            AllUtils.SendCompetitiveInfo(player);
                            return ComDiv.ToTitleCase(option) + "titive Enabled!";
                        case "off":
                            room.Name = room.RandomName(new Random().Next(1, 4));
                            room.Competitive = false;
                            return ComDiv.ToTitleCase(option) + "titive Disabled!";
                        default:
                            return $"Unable to use Competitive command! (Wrong Value: {value})";
                    }

                default:
                    return $"Command {ComDiv.ToTitleCase(option)} was not founded!";
            }
        }
    }
}