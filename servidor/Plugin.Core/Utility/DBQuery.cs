// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Utility.DBQuery
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System.Collections.Generic;


namespace Plugin.Core.Utility
{
    public class DBQuery
    {
        private readonly List<string> Field0;
        private readonly List<object> Field1;

        public DBQuery()
        {
            this.Field0 = new List<string>();
            this.Field1 = new List<object>();
        }

        public void AddQuery(string table, object value)
        {
            this.Field0.Add(table);
            this.Field1.Add(value);
        }

        public string[] GetTables() => this.Field0.ToArray();

        public object[] GetValues() => this.Field1.ToArray();
    }
}