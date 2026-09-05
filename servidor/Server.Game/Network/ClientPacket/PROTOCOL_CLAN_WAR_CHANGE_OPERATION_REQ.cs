using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Text;

namespace Server.Game.Network.ClientPacket
{
    // Client 122 (OBSERVED, factory 0xD14CF4 uiMsg 0x82BC, ctor 0xD12B3D): opcode
    // 6924 registers one S2MOStringW<64> -> the payload is
    // `[u8 count][wchar_t x count]`, the team's operation/notice line.
    // Reply CHANGE_OPERATION_ACK (6925) echoes it to the whole team.
    public class PROTOCOL_CLAN_WAR_CHANGE_OPERATION_REQ : GameClientPacket
    {
        private string Operation;

        public override void Read()
        {
            int chars = (int)this.ReadC();
            if (chars > PROTOCOL_CLAN_WAR_CHANGE_OPERATION_ACK.MaxChars)
                chars = PROTOCOL_CLAN_WAR_CHANGE_OPERATION_ACK.MaxChars;
            this.Operation = chars > 0 ? this.ReadU(chars * 2) : string.Empty;
        }

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                MatchModel match = player.Match;
                if (match == null || player.MatchSlot != match.Leader)
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_CHANGE_OPERATION_ACK(2147483648U /*0x80000000*/));
                    return;
                }
                match.Operation = this.Operation;
                using (PROTOCOL_CLAN_WAR_CHANGE_OPERATION_ACK Packet = new PROTOCOL_CLAN_WAR_CHANGE_OPERATION_ACK(0U, this.Operation))
                    match.SendPacketToPlayers(Packet);
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CLAN_WAR_CHANGE_OPERATION_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
