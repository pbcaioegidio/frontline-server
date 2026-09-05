// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.RecordInfo
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System.Runtime.CompilerServices;


namespace Plugin.Core.Models
{
    public class RecordInfo
    {
        public long PlayerId { get; set; }

        public int RecordValue { get; set; }

        public RecordInfo(string[] A_1)
        {
            this.PlayerId = this.GetPlayerId(A_1);
            this.RecordValue = this.GetPlayerValue(A_1);
        }

        public long GetPlayerId(string[] Split)
        {
            try
            {
                return long.Parse(Split[0]);
            }
            catch
            {
                return 0;
            }
        }

        public int GetPlayerValue(string[] Split)
        {
            try
            {
                return int.Parse(Split[1]);
            }
            catch
            {
                return 0;
            }
        }

        
        public string GetSplit() => $"{this.PlayerId.ToString()}-{this.RecordValue.ToString()}";
    }
}