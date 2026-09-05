// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Sync.Client.ReloadConfig
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.JSON;
using Plugin.Core.Models;
using Plugin.Core.Network;
using System.Runtime.CompilerServices;


namespace Server.Game.Data.Sync.Client
{
    public class ReloadConfig
    {
        
        public static void Load(SyncClientPacket C)
        {
            int ConfigId = (int)C.ReadC();
            ServerConfig config = ServerConfigJSON.GetConfig(ConfigId);
            if (config == null || config.ConfigId <= 0)
                return;
            foreach (GameManager Manager in GameXender.All)
                Manager.Config = config;
            CLogger.Print($"Configuration (Database) Refills; Config: {ConfigId}", LoggerType.Command);
        }
    }
}