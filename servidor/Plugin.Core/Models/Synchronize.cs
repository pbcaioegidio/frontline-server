// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.Synchronize
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System.Net;

namespace Plugin.Core.Models
{
    public class Synchronize
    {
        public int RemotePort { get; set; }

        public IPEndPoint Connection { get; set; }

        public Synchronize(string A_1, int A_2)
        {
            this.Connection = new IPEndPoint(IPAddress.Parse(A_1), A_2);
        }
    }
}