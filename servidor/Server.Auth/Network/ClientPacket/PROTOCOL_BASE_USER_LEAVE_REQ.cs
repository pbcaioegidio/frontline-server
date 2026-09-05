
using Plugin.Core;
using Plugin.Core.Enums;
using Server.Auth.Network.ServerPacket;
using System;


namespace Server.Auth.Network.ClientPacket
{
    public class PROTOCOL_BASE_USER_LEAVE_REQ : AuthClientPacket
    {
        public override void Read()
        {
        }

        public override void Run()
        {
            try
            {
                this.Client.SendPacket(new PROTOCOL_BASE_USER_LEAVE_ACK(0));
                this.Client.Close(0, false);
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}