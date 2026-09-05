// Decompiled with JetBrains decompiler
// Type: Server.Match.Data.Managers.AssistManager
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Server.Match.Data.Models;
using System.Collections.Generic;

namespace Server.Match.Data.Managers
{
    public static class AssistManager
    {
        public static List<AssistServerData> Assists = new List<AssistServerData>();

        public static void AddAssist(AssistServerData Assist)
        {
            lock (AssistManager.Assists)
                AssistManager.Assists.Add(Assist);
        }

        public static bool RemoveAssist(AssistServerData Assist)
        {
            lock (AssistManager.Assists)
                return AssistManager.Assists.Remove(Assist);
        }

        public static AssistServerData GetAssist(int VictimId, int RoomId)
        {
            lock (AssistManager.Assists)
            {
                foreach (AssistServerData assist in AssistManager.Assists)
                {
                    if (assist.Victim == VictimId && assist.RoomId == RoomId)
                        return assist;
                }
            }
            return (AssistServerData)null;
        }
    }
}