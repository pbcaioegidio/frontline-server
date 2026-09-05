// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Sync.Client.AuthLogin
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Network;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network;
using Server.Game.Network.ServerPacket;


namespace Server.Game.Data.Sync.Client
{
    public class AuthLogin
    {
        public static void Load(SyncClientPacket C)
        {
            Account account = AccountManager.GetAccount(C.ReadQ(), true);
            if (account == null)
                return;
            account.SendPacket(new PROTOCOL_AUTH_ACCOUNT_KICK_ACK(1));
            account.SendPacket(new PROTOCOL_SERVER_MESSAGE_ERROR_ACK(2147487744U /*0x80001000*/));
            account.Close(1000);
        }
    }
}