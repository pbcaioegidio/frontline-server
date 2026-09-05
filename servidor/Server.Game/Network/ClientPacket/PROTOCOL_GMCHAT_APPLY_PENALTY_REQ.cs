using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Logging;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_GMCHAT_APPLY_PENALTY_REQ : GameClientPacket
    {
        private long PlayerId;
        private int BanTime;
        private byte Type;
        private string Reason;
        public override void Read()
        
        {
            CLogger.Emit(new LogEvent { Level = LogLevel.Debug, Cat = LogCat.Packet, Dir = Direction.In, Pkt = "GM_APPLY_PENALTY_REQ", Len = this._raw != null ? this._raw.Length : 0, Hex = this._raw != null ? BitConverter.ToString(this._raw) : null });

            try
            {
                MStream.Seek(0, System.IO.SeekOrigin.Begin);
                Reason = ReadU((int)ReadC() * 2);


                if (MStream.Length >= 14)
                {
                    MStream.Seek(MStream.Length - 14, System.IO.SeekOrigin.Begin);
                    byte ReasonId = ReadC();    
                    BanTime = ReadD();        
                    Type = ReadC();           
                    PlayerId = ReadQ();        
                }

            }
            catch (Exception ex)
            {
                CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_REQ: Read error: {ex.Message}", LoggerType.Error);
            }
        }
        public override void Run()
        {
            try
            {
                Account Player = Client.Player;
                if (Player == null)
                {
                    return;
                }
                string durText = (Type == 0 || Type == 1) ? $"{BanTime / 60} Min ({BanTime} Sec)" : $"{BanTime} Min";
                CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_REQ: From={Player.Nickname}, TargetId={PlayerId}, Type={Type}, Duration={durText}, Reason={Reason}", LoggerType.Info);
                Account TargetUser = AccountManager.GetAccount(PlayerId, 31);
                if (TargetUser != null)
                {
                    Client.SendPacket(new PROTOCOL_GMCHAT_APPLY_PENALTY_ACK(0, TargetUser, Type, BanTime, Reason) { GmId = Player.PlayerId, GmNick = Player.Nickname });
                }
                else
                {
                    Client.SendPacket(new PROTOCOL_GMCHAT_APPLY_PENALTY_ACK(0U, null, 0, 0, ""));
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_REQ: {ex.Message}", LoggerType.Error, ex);
            }
        }
    }
}
