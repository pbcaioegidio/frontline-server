using Plugin.Core.Managers;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;

namespace Server.Game.Rcon.Admin
{
    public class RconSimInvExtendAdmin : RconReceive
    {
        private string Token;
        private long Id;
        private long Slots;

        public override void Run()
        {
            if (IsJsonMode)
            {
                Token = PopString("token");
                Id = PopLong("player_id");
                Slots = PopLong("slots");
            }

            if (!RconCommand.CheckToken(Token) || Token == "")
            {
                RconLogger.LogsPanel("An error occurred in the process, please try again later. ", 1);
                return;
            }

            if (Id <= 0)
            {
                RconLogger.LogsPanel("SimulateInventoryExtend: player_id invalido.", 1);
                return;
            }

            Account player = AccountManager.GetAccount(Id, 0);
            if (player == null)
            {
                RconLogger.LogsPanel("SimulateInventoryExtend: player nao esta online.", 1);
                return;
            }

            RconLogger.LogsPanel(AdvancedSimulation.SimulateInventoryExtend(player, (int)Slots), 0);
        }
    }
}
