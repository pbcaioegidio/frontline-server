using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_AUTH_FRIEND_INFO_ACK : GameServerPacket
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
                               //return;
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
                        if (info.Nickname != string.Empty && info.PlayerId > 0)
                        {
                            WriteC((byte)(info.Nickname.Length + 1));
                            WriteN(info.Nickname, info.Nickname.Length + 2, "UTF-16LE");
                            WriteQ(info.PlayerId);
                            WriteD(ComDiv.GetFriendStatus(f));
                            WriteD(0U);
                            WriteC((byte)info.Rank);
                            WriteB(new byte[13]);
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