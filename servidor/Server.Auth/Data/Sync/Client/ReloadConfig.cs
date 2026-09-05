// Decompiled with JetBrains decompiler
// Type: Server.Auth.Data.Sync.Client.ReloadConfig
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.JSON;
using Plugin.Core.Models;
using Plugin.Core.Network;
using System.Runtime.CompilerServices;

namespace Server.Auth.Data.Sync.Client
{
    public class ReloadConfig
    {
        
        public static void Load(SyncClientPacket C)
        {
            int ConfigId = (int)C.ReadC();
            ServerConfig config = ServerConfigJSON.GetConfig(ConfigId);
            if (config == null || config.ConfigId <= 0)
                return;
            AuthXender.Client.Config = config;
            CLogger.Print($"Configuration (Database) Refills; Config: {ConfigId}", LoggerType.Command);
        }
    }
}