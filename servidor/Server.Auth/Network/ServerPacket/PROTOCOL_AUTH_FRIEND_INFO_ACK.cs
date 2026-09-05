using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_AUTH_FRIEND_INFO_ACK : AuthServerPacket
    {
        private readonly List<FriendModel> listfriendModels;

        public PROTOCOL_AUTH_FRIEND_INFO_ACK(List<FriendModel> ListfriendModels) => listfriendModels = ListfriendModels;

        public override void Write()
        {
            try
            {
                if (listfriendModels == null)
                {
                    WriteH(1810);
                    WriteC(0); // Count is 0
                    return;
                }

                WriteH(1810);
                WriteC((byte)listfriendModels.Count);
                foreach (FriendModel f in listfriendModels)
                {
                    PlayerInfo info = f.Info;
                    if (info == null)
                    {
                        WriteB(new byte[31]);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(info.Nickname) && info.PlayerId > 0)
                        {
                            WriteC((byte)(info.Nickname.Length + 1));
                            WriteN(info.Nickname, info.Nickname.Length + 2, "UTF-16LE");
                            WriteQ(info.PlayerId);
                            WriteD(ComDiv.GetFriendStatus(f));
                            WriteD(uint.MaxValue);
                            WriteC((byte)info.Rank);
                            WriteB(new byte[8]);      // pad blob[17..24]
                            WriteD(info.ClanLogo);    // node+32 clanMark: real clan logo, or 0xFFFFFFFF (no clan) -> emblem hidden
                            WriteC((byte)0);          // node+36 = 0 (no ClanEffect overlay)
                        }
                        else
                        {
                            WriteB(new byte[31]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"{this.GetType().Name}: {ex.Message}", LoggerType.Error, ex);
            }
        }
    }
}