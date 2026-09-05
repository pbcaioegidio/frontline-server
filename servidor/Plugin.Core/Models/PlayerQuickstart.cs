// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.PlayerQuickstart
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System.Collections.Generic;

namespace Plugin.Core.Models
{
    public class PlayerQuickstart
    {
        public long OwnerId { get; set; }

        public List<QuickstartModel> Quickjoins { get; set; }

        public PlayerQuickstart() => this.Quickjoins = new List<QuickstartModel>();

        public QuickstartModel GetMapList(byte MapId)
        {
            lock (this.Quickjoins)
            {
                foreach (QuickstartModel quickjoin in this.Quickjoins)
                {
                    if (quickjoin.MapId == (int)MapId)
                        return quickjoin;
                }
            }
            return (QuickstartModel)null;
        }
    }
}