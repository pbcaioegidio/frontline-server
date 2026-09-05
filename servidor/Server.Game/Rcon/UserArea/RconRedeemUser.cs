using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Core.Managers;
using Server.Game.Rcon;
using Server.Game.Data.Feature;

namespace Server.Game.Rcon.UserArea
{
    public class RconRedeemUser : RconReceive
    {
        private static Dictionary<long, HashSet<string>> processedCommands = new Dictionary<long, HashSet<string>>();
        private string Token, LauncherKey;
        private long Id;
        private int Money, Webcoin, GoodID;

        public override void Run()
        {
            if (IsJsonMode)
            {
                Token = PopString("token");
                LauncherKey = PopString("launcher_key");
                Id = PopLong("player_id");
                Money = PopInt("cash");
                Webcoin = PopInt("webcoin");
                GoodID = PopInt("item_id");
            }

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

                string command = $"xxxx {Id} {GoodID} {Money} {Webcoin}";
                Redeem.CreateItemDaysByRedeem(command);
            }
            catch
            {
                RconLogger.LogsPanel("An error occurred in the process, please try again later. ", 1);
                return;
            }
        }
    }
}