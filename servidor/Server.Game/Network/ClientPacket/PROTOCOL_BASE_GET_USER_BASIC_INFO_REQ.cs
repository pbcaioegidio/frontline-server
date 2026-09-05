// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BASE_GET_USER_BASIC_INFO_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_GET_USER_BASIC_INFO_REQ : GameClientPacket
    {
        private uint Error;
        private long PlayerId;

        public override void Read() => PlayerId = ReadQ();

        public override void Run()
        {
            try
            {
                Account player = Client.Player;
                if (player == null)
                    return;
                Account accountDb = AccountManager.GetAccountDB((object)PlayerId, 2, 31);
                if (accountDb != null && player.Nickname.Length > 0 && player.PlayerId != PlayerId)
                {
                    if (player.Nickname != accountDb.Nickname)
                        player.FindPlayer = accountDb.Nickname;
                    Client.SendPacket(new PROTOCOL_BASE_GET_USER_DETAIL_INFO_ACK(Error, accountDb, int.MaxValue));
                }
                else
                    Error = 2147489795U;
                Client.SendPacket(new PROTOCOL_BASE_GET_USER_BASIC_INFO_ACK(Error));
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}