// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Commands.SystemCommand
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Microsoft.Extensions.Logging;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network;
using Server.Game.Network.ServerPacket;
using System.Runtime.CompilerServices;

namespace Server.Game.Data.Commands
{
    public class SystemCommand : ICommand
    {
        private EventVisitModel Event;

        public string Command
        {
            get => "sys";
        }

        public string Description
        {
            get => "change server settings";
        }

        public string Permission
        {
            get => "developercommand";
        }

        public string Args
        {
            get => "%options% %value%";
        }

        public string Execute(string Command, string[] Args, Account Player)
        {
            string lower1 = Args[0].ToLower();

            string lower2 = Args[1].ToLower();

            switch (lower1)
            {
                case "udp":
                    int udpType = (int)ConfigLoader.UdpType;
                    int num1 = int.Parse(lower2);
                    if (num1.Equals(udpType))
                        return $"UDP State Already Changed To: {udpType}";
                    if (num1 < 1 || num1 > 4)
                        return $"Cannot Change UDP State To: {num1}";
                    switch (num1)
                    {
                        case 1:
                            ConfigLoader.UdpType = (UdpState)num1;
                            return $"{ComDiv.ToTitleCase(lower1)} State Changed ({num1} - {ConfigLoader.UdpType})";

                        case 2:
                            ConfigLoader.UdpType = (UdpState)num1;
                            return $"{ComDiv.ToTitleCase(lower1)} State Changed ({num1} - {ConfigLoader.UdpType})";

                        case 3:
                            ConfigLoader.UdpType = (UdpState)num1;
                            return $"{ComDiv.ToTitleCase(lower1)} State Changed ({num1} - {ConfigLoader.UdpType})";

                        case 4:
                            ConfigLoader.UdpType = (UdpState)num1;
                            return $"{ComDiv.ToTitleCase(lower1)} State Changed ({num1} - {ConfigLoader.UdpType})";

                        default:
                            ConfigLoader.UdpType = UdpState.RELAY;
                            return $"{ComDiv.ToTitleCase(lower1)} State Changed (3 - {ConfigLoader.UdpType}). Wrong Value";
                    }
                case "debug":
                    bool flag1 = int.Parse(lower2).Equals(1);
                    if (flag1.Equals(ConfigLoader.DebugMode))
                        return $"Debug Mode Already Changed To: {flag1}";
                    if (!flag1)
                    {
                        ConfigLoader.DebugMode = flag1;
                        return $"{ComDiv.ToTitleCase(lower1)} Mode '{(flag1 ? "Enabled" : "Disabled")}'";
                    }
                    ConfigLoader.DebugMode = flag1;
                    return $"{ComDiv.ToTitleCase(lower1)} Mode '{(flag1 ? "Enabled" : "Disabled")}'";

                case "test":
                    bool flag2 = int.Parse(lower2).Equals(1);
                    if (flag2.Equals(ConfigLoader.IsTestMode))
                        return $"Test Mode Already Changed To: {flag2}";
                    if (flag2)
                    {
                        ConfigLoader.IsTestMode = flag2;
                        return $"{ComDiv.ToTitleCase(lower1)} Mode '{(flag2 ? "Enabled" : "Disabled")}'";
                    }
                    ConfigLoader.IsTestMode = flag2;
                    return $"{ComDiv.ToTitleCase(lower1)} Mode '{(flag2 ? "Enabled" : "Disabled")}'";

                case "ping":
                    bool flag3 = int.Parse(lower2).Equals(1);
                    if (flag3.Equals(ConfigLoader.IsDebugPing))
                        return $"Ping Mode Already Changed To: {flag3}";
                    if (flag3)
                    {
                        ConfigLoader.IsDebugPing = flag3;
                        return $"{ComDiv.ToTitleCase(lower1)} Mode '{(flag3 ? "Enabled" : "Disabled")}'";
                    }
                    ConfigLoader.IsDebugPing = flag3;
                    return $"{ComDiv.ToTitleCase(lower1)} Mode '{(flag3 ? "Enabled" : "Disabled")}'";

                case "reload":
                    if (lower2.Equals("pase"))
                    {
                        BattlePassManager.Reload();
                        Player.SendPacket(new PROTOCOL_SEASON_CHALLENGE_INFO_ACK(Player));
                        CLogger.Print("PROTOCOL_SEASON_CHALLENGE_INFO_ACK Refresh", LoggerType.Debug);
                        return $"Refresh CHALLENGE_INFO_ACK!";
                    }
                    else if (lower2.Equals("classic_mode"))
                    {
                        ClassicModeManager.Reload();
                        CLogger.Print("ClassicModeManager Refresh", LoggerType.Debug);
                        return $"Refresh Classic Mode Manager!";
                    }
                    return $"{ComDiv.ToTitleCase(lower1)} Opción de 'reload' no válida: {lower2}. Use 'pase' o 'classic_mode'.";

                case "title":
                    if (!lower2.Equals("all"))
                        return ComDiv.ToTitleCase(lower1) + " Arguments was not valid!";
                    if (Player.Title.OwnerId == 0L)
                    {
                        DaoManagerSQL.CreatePlayerTitlesDB(Player.PlayerId);
                        Player.Title = new PlayerTitles()
                        {
                            OwnerId = Player.PlayerId
                        };
                    }
                    PlayerTitles title1 = Player.Title;
                    int num2 = 0;
                    for (int titleId = 1; titleId <= 44; ++titleId)
                    {
                        TitleModel title2 = TitleSystemXML.GetTitle(titleId);
                        if (title2 != null && !title1.Contains(title2.Flag))
                        {
                            ++num2;
                            title1.Add(title2.Flag);
                            if (title1.Slots < title2.Slot)
                                title1.Slots = title2.Slot;
                        }
                    }
                    if (num2 > 0)
                    {
                        ComDiv.UpdateDB("player_titles", "slots", (object)title1.Slots, "owner_id", (object)Player.PlayerId);
                        DaoManagerSQL.UpdatePlayerTitlesFlags(Player.PlayerId, title1.Flags);
                        Player.SendPacket(new PROTOCOL_BASE_USER_TITLE_INFO_ACK(Player));
                    }
                    return ComDiv.ToTitleCase(lower1) + " Successfully Opened!";

                default:
                    return $"Command {ComDiv.ToTitleCase(lower1)} was not founded!";
            }
        }
    }
}