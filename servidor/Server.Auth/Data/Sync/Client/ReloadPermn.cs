// Decompiled with JetBrains decompiler
// Type: Server.Auth.Data.Sync.Client.ReloadPermn
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Plugin.Core.XML;
using System.Runtime.CompilerServices;

namespace Server.Auth.Data.Sync.Client
{
    public class ReloadPermn
    {
        
        public static void Load(SyncClientPacket C)
        {
            int num = (int)C.ReadC();
            switch (num)
            {
                case 1:
                    EventVisitXML.Reload();
                    EventLoginXML.Reload();
                    EventBoostXML.Reload();
                    EventPlaytimeJSON.Reload();
                    EventQuestXML.Reload();
                    EventRankUpXML.Reload();
                    EventXmasXML.Reload();
                    CLogger.Print("All Events Successfully Reloaded!", LoggerType.Command);
                    break;
                case 2:
                    PermissionXML.Load();
                    CLogger.Print("Permission Successfully Reloaded!", LoggerType.Command);
                    break;
            }
            CLogger.Print($"Updating null part: {num}", LoggerType.Command);
        }
    }
}