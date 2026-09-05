using System;
using System.Collections.Generic;
using Plugin.Core.Managers;
using Server.Game.Rcon;
using Server.Game.Data.Feature;
using Plugin.Core;
using Plugin.Core.Enums;

namespace Server.Game.Rcon.UserArea
{
    public class RconWebshopUser : RconReceive
    {
        private static Dictionary<long, HashSet<string>> processedCommands = new Dictionary<long, HashSet<string>>();

        private string Token, LauncherKey;
        private long Id;
        private int ItemID;

        public override void Run()
        {
            if (IsJsonMode)
            {
                Token = PopString("token");
                LauncherKey = PopString("launcherkey");
                Id = PopLong("player_id");
                ItemID = PopInt("item_id");
            }

            RconLogger.LogsPanel($"RconWeb User: Token: {Token}, LauncherKey: {LauncherKey}, Id: {Id}, ItemID: {ItemID}", 1);

            if (!RconCommand.CheckToken(Token) || Token == "")
            {
                RconLogger.LogsPanel("An error occurred in the process, please try again later. ", 1);
                return;
            }

            if (!RconManager.CheckLauncherKey(Id, LauncherKey) || LauncherKey == "")
            {
                RconLogger.LogsPanel("An error occurred in the process, please try again later. ", 1);
                return;
            }

            if (processedCommands.ContainsKey(Id) && processedCommands[Id].Contains(LauncherKey))
            {
                RconLogger.LogsPanel("This redeem with the same LauncherKey has already been processed for this user.", 1);
                return;
            }

            try
            {
                if (!processedCommands.ContainsKey(Id))
                {
                    processedCommands[Id] = new HashSet<string>();
                }
                processedCommands[Id].Add(LauncherKey);

                Webshop.CreateItemDaysByWebshop(Id, ItemID);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                RconLogger.LogsPanel("An error occurred in the process, please try again later. ", 1);
                return;
            }
        }
    }
}