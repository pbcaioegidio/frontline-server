using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Utility;
using Server.Game.Data.Models;

namespace Server.Game.Network.ClientPacket
{
    /// <summary>
    /// Keep-alive do client. Atualiza timestamp da sessão; se RequireLauncherHeartbeat,
    /// também verifica se o FL Guard ainda está vivo.
    /// </summary>
    public class PROTOCOL_BASE_KEEP_ALIVE_REQ : GameClientPacket
    {
        public override void Read()
        {
        }

        public override void Run()
        {
            try
            {
                Account player = Client?.GetAccount();
                if (player == null) return;

                Client.SessionDate = DateTimeUtil.Now();

                if (ConfigLoader.RequireLauncherHeartbeat &&
                    !Plugin.Core.Security.SecurityDao.IsLauncherAlive(player.PlayerId, ConfigLoader.HeartbeatTimeoutSeconds))
                {
                    Plugin.Core.Security.SecurityDao.LogEvent(
                        Plugin.Core.Security.SecurityDao.SourceGame,
                        player.PlayerId, player.Username, player.Nickname,
                        "heartbeat_lost", "HEARTBEAT_LOST",
                        "Keep-alive sem FL Guard", "{}", 4, "FG-110");
                    Data.Utils.AllUtils.KickPlayer(player, "FL Guard desconectado (FG-110)", "HEARTBEAT_LOST",
                        Plugin.Core.Security.SecurityDao.SourceGame, 0, "FG-110", false);
                }
            }
            catch (System.Exception ex)
            {
                CLogger.Print("KEEP_ALIVE: " + ex.Message, LoggerType.Warning, ex);
            }
        }
    }
}
