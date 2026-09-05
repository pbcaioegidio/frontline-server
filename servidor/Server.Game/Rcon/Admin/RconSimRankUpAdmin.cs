using Plugin.Core.Managers;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;

namespace Server.Game.Rcon.Admin
{
    /// <summary>
    /// Area de desenvolvimento: forca uma promocao de rank sem partida, para validar a
    /// entrega de recompensa e o wire do PROTOCOL_BASE_RANK_UP_ACK.
    /// </summary>
    public class RconSimRankUpAdmin : RconReceive
    {
        private string Token;
        private long Id;

        public override void Run()
        {
            if (IsJsonMode)
            {
                Token = PopString("token");
                Id = PopLong("player_id");
            }

            if (!RconCommand.CheckToken(Token) || Token == "")
            {
                RconLogger.LogsPanel("An error occurred in the process, please try again later. ", 1);
                return;
            }

            if (Id <= 0)
            {
                RconLogger.LogsPanel("SimulateRankUp: player_id invalido.", 1);
                return;
            }

            Account player = AccountManager.GetAccount(Id, 0);
            if (player == null)
            {
                RconLogger.LogsPanel("SimulateRankUp: player nao esta online.", 1);
                return;
            }

            RconLogger.LogsPanel(AdvancedSimulation.SimulateRankUp(player), 0);
        }
    }
}
