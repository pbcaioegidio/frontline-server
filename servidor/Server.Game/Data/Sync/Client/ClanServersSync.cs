// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Sync.Client.ClanServersSync
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using Plugin.Core.Network;
using Server.Game.Data.Managers;


namespace Server.Game.Data.Sync.Client
{
    public class ClanServersSync
    {
        public static void Load(SyncClientPacket C)
        {
            int num1 = (int)C.ReadC();
            int ClanId = C.ReadD();
            ClanModel clan = ClanManager.GetClan(ClanId);
            if (num1 == 0)
            {
                if (clan != null)
                    return;
                long num2 = C.ReadQ();
                uint num3 = C.ReadUD();
                string str1 = C.ReadS((int)C.ReadC());
                string str2 = C.ReadS((int)C.ReadC());
                ClanManager.AddClan(new ClanModel()
                {
                    Id = ClanId,
                    Name = str1,
                    OwnerId = num2,
                    Logo = 0U,
                    Info = str2,
                    CreationDate = num3
                });
            }
            else
            {
                if (clan == null)
                    return;
                ClanManager.RemoveClan(clan);
            }
        }
    }
}