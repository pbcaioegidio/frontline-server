// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Sync.Client.PlayerSync
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Network;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;


namespace Server.Game.Data.Sync.Client
{
    public static class PlayerSync
    {
        public static void Load(SyncClientPacket C)
        {
            long id = C.ReadQ();
            int num1 = (int)C.ReadC();
            int num2 = (int)C.ReadC();
            int num3 = C.ReadD();
            int num4 = C.ReadD();
            int num5 = C.ReadD();
            Account account = AccountManager.GetAccount(id, true);
            if (account == null || num1 != 0)
                return;
            account.Rank = num2;
            account.Gold = num3;
            account.Cash = num4;
            account.Tags = num5;
        }
    }
}