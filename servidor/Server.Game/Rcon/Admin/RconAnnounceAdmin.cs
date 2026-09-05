using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Core;
using Server.Game.Rcon;
using Server.Game.Network.ServerPacket;

namespace Server.Game.Rcon.Admin
{
    internal class RconAnnounceAdmin : RconReceive
    {
        string Token, text;
        long Id;
        int count;
        public override void Run()
        {


            

            if (IsJsonMode)
            {
                Token = PopString("token");
                text = PopString("text");
                Id = PopLong("gm");
                count = PopInt("count");
            }


            if (!RconCommand.CheckToken(Token) || Token == "")
            {
                RconLogger.LogsPanel("An error occurred in the process, please try again later. ", 1);
                return;
            }
            using (PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK packet = new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(text))
            {
                count = GameXender.BroadcastToAll(packet);
            }
            RconLogger.LogsPanel($"[!] Announcement: {text} Send to {count} Players", 0);
        }
    }
}
