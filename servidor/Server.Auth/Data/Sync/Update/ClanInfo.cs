// Decompiled with JetBrains decompiler
// Type: Server.Auth.Data.Sync.Update.ClanInfo
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Server.Auth.Data.Models;

namespace Server.Auth.Data.Sync.Update
{
    public class ClanInfo
    {
        public static void AddMember(Account Player, Account Member)
        {
            lock (Player.ClanPlayers)
                Player.ClanPlayers.Add(Member);
        }

        public static void RemoveMember(Account Player, long PlayerId)
        {
            lock (Player.ClanPlayers)
            {
                for (int index = 0; index < Player.ClanPlayers.Count; ++index)
                {
                    if (Player.ClanPlayers[index].PlayerId == PlayerId)
                    {
                        Player.ClanPlayers.RemoveAt(index);
                        break;
                    }
                }
            }
        }

        public static void ClearList(Account Player)
        {
            lock (Player.ClanPlayers)
            {
                Player.ClanId = 0;
                Player.ClanAccess = 0;
                Player.ClanPlayers.Clear();
            }
        }
    }
}