using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Filters;
using Plugin.Core.SQL;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_CHECK_NICK_REQ : GameClientPacket
    {
        private string Field0;

        public override void Read() => this.Field0 = this.ReadU(66);

        public override void Run()
        {
            try
            {
                if (string.IsNullOrEmpty(this.Field0)
                    || this.Field0.Length < ConfigLoader.MinNickSize
                    || this.Field0.Length > ConfigLoader.MaxNickSize)
                {
                    this.Client.SendPacket(new PROTOCOL_BASE_CHECK_NICK_ACK(0x80000000U));
                    return;
                }

                foreach (string filter in NickFilter.Filters)
                {
                    if (this.Field0.Contains(filter))
                    {
                        this.Client.SendPacket(new PROTOCOL_BASE_CHECK_NICK_ACK(0x80000000U));
                        return;
                    }
                }

                this.Client.SendPacket(new PROTOCOL_BASE_CHECK_NICK_ACK(
                    !DaoManagerSQL.IsPlayerNameExist(this.Field0) ? 0U : 0x80000113U));
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
