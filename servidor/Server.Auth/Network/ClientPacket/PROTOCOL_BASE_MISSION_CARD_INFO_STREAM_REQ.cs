using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.XML;
using Server.Auth.Network.ServerPacket;
using System;

namespace Server.Auth.Network.ClientPacket
{
    public class PROTOCOL_BASE_MISSION_CARD_INFO_STREAM_REQ : AuthClientPacket
    {
        public override void Read()
        {
        }

        public override void Run()
        {
            try
            {
                if (Client.Player == null)
                    return;

                var missions = MissionStreamXML.GetAllMissions();
                if (missions.Count == 0)
                {
                    CLogger.Print("Mission Stream: nenhum deck carregado", LoggerType.Warning);
                    return;
                }

                short total = (short)missions.Count;
                foreach (MissionStreamEntry mission in missions)
                {
                    byte[] payload = MissionStreamXML.BuildPayload(mission);
                    Client.SendPacket(new PROTOCOL_BASE_MISSION_CARD_INFO_STREAM_ACK(payload, total, 1));
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
