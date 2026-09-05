using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Logging;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_GMCHAT_APPLY_PENALTY_MULTI_REQ : GameClientPacket
    {
        private string UserNotice;
        private string BlockDescription;
        private byte ReasonId;
        private int ChatBanTime;
        private byte ChatBanType;
        private byte AccountBanType;
        private int AccountBanTime;
        private List<string> Nicknames = new List<string>();

        public override void Read()
        {
            CLogger.Emit(new LogEvent { Level = LogLevel.Debug, Cat = LogCat.Packet, Dir = Direction.In, Pkt = "GM_APPLY_PENALTY_MULTI_REQ", Len = this._raw != null ? this._raw.Length : 0, Hex = this._raw != null ? BitConverter.ToString(this._raw) : null });
            try
            {
                UserNotice = ReadU((int)ReadC() * 2);
                BlockDescription = ReadU((int)ReadC() * 2);
                ReasonId = ReadC();
                ChatBanTime = ReadD();
                ChatBanType = ReadC();
                AccountBanType = ReadC();
                AccountBanTime = ReadD();
                
                ReadB(10); 
                            
                while (MStream.Position < MStream.Length)
                {
                    byte nickLen = ReadC();
                    if (nickLen > 0)
                    {
                        string nick = ReadU((int)nickLen * 2);
                        Nicknames.Add(nick);
                    }
                    else
                    {
                        break; 
                    }
                }
                
                CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_MULTI_REQ: Notice='{UserNotice}', Desc='{BlockDescription}', Chat[Time={ChatBanTime}, Type={ChatBanType}], Acc[Time={AccountBanTime}, Type={AccountBanType}], TargetCount={Nicknames.Count}", LoggerType.Debug);
            }
            catch (Exception ex)
            {
                CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_MULTI_REQ: Read error: {ex.Message}", LoggerType.Error);
            }
        }

        public override void Run()
        {
            try
            {
                Account Gm = Client.Player;
                if (Gm == null || Gm.Access < AccessLevel.MODERATOR)
                {
                    return;
                }

                CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_MULTI_REQ: GM={Gm.Nickname}, Notice={UserNotice}, Chat[Time={ChatBanTime}, Type={ChatBanType}], Acc[Time={AccountBanTime}, Type={AccountBanType}], TargetCount={Nicknames.Count}", LoggerType.Info);

                foreach (string nickname in Nicknames)
                {
                    Account Target = AccountManager.GetAccount(nickname, 1, 31);
                    if (Target != null)
                    {
                        CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_MULTI_REQ: Processing target '{nickname}' (ID: {Target.PlayerId})", LoggerType.Info);
                        
                        // Apply Penalty Slot 1 if Type is specified
                        if (ChatBanType > 0)
                        {
                            CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_MULTI_REQ: Sending ACK 1 for '{nickname}' Type={ChatBanType}, Time={ChatBanTime}", LoggerType.Debug);
                            Client.SendPacket(new PROTOCOL_GMCHAT_APPLY_PENALTY_ACK(0, Target, ChatBanType, ChatBanTime, UserNotice));
                        }

                        // Apply Penalty Slot 2 if Type is specified
                        if (AccountBanType > 0)
                        {
                            CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_MULTI_REQ: Sending ACK 2 for '{nickname}' Type={AccountBanType}, Time={AccountBanTime}", LoggerType.Debug);
                            Client.SendPacket(new PROTOCOL_GMCHAT_APPLY_PENALTY_ACK(0, Target, AccountBanType, AccountBanTime, UserNotice));
                        }
                    }
                    else
                    {
                        CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_MULTI_REQ: Target '{nickname}' not found!", LoggerType.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_MULTI_REQ: Run error: {ex.Message}", LoggerType.Error, ex);
            }
        }
    }
}
