// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Sync.Client.ClanSync
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Network;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;


namespace Server.Game.Data.Sync.Client
{
    public static class ClanSync
    {
        public static void Load(SyncClientPacket C)
        {
            long id = C.ReadQ();
            int num1 = (int)C.ReadC();
            Account account = AccountManager.GetAccount(id, true);
            if (account == null || num1 != 3)
                return;
            int num2 = C.ReadD();
            int num3 = (int)C.ReadC();
            account.ClanId = num2;
            account.ClanAccess = num3;
        }
    }
}
