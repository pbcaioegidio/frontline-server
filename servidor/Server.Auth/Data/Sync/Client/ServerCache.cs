// Decompiled with JetBrains decompiler
// Type: Server.Auth.Data.Sync.Client.ServerCache
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.XML;


namespace Server.Auth.Data.Sync.Client
{
    public class ServerCache
    {
        public static void Load(SyncClientPacket C)
        {
            int id = C.ReadD();
            int num = C.ReadD();
            SChannelModel server = SChannelXML.GetServer(id);
            if (server == null)
                return;
            server.LastPlayers = num;
        }
    }
}