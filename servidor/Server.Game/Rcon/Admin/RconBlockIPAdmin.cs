using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Game.Rcon;

namespace Server.Game.Rcon.Admin
{
    public class RconBlockIPAdmin : RconReceive
    {
        private string Token, IP;
        private int Duration;
        private long GM_ID;

        public override void Run()
        {
            if (IsJsonMode)
            {
                Token = PopString("token");
                IP = PopString("ip");
                Duration = PopInt("duration");
                GM_ID = PopLong("gm");
            }

            if (!RconCommand.CheckToken(Token) || Token == "")
            {
                RconLogger.LogsPanel("An error occurred in the process, please try again later. ", 1);
                return;
            }
        }
    }
}