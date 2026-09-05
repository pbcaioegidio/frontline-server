using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Server.Game.Rcon;
using Server.Game.Data.Feature;
using Plugin.Core;
using Plugin.Core.Enums;

namespace Server.Game.Rcon.Admin
{
    public class RconGiftItemAdmin : RconReceive
    {
        string Token;
        long Player_ID, GM_ID;
        int GoodID;
        public override void Run()
        {
            

            if (IsJsonMode)
            {
                Token = PopString("token");
                Player_ID = PopLong("player_id");
                GoodID = PopInt("item_id");
                GM_ID = PopLong("gm");
            }

            RconLogger.LogsPanel($"RconGiftItemAdmin: Token: {Token}, Player_ID: {Player_ID}, GoodID: {GoodID}, GM_ID: {GM_ID}", 1);

            if (!RconCommand.CheckToken(Token) || Token == "")
            {
                RconLogger.LogsPanel("An error occurred in the process, please try again later. ", 1);
                return;
            }
            try
            {
                Gift.CreateItemDaysByGift(Player_ID, GoodID,GM_ID);
            }
            catch
            {
                RconLogger.LogsPanel("An error occurred in the process, please try again later. ", 1);
                return;
            }
        }
    }
}
