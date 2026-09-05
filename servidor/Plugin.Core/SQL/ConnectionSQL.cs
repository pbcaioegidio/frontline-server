// Decompiled with JetBrains decompiler
// Type: Plugin.Core.SQL.ConnectionSQL
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Npgsql;
using System.Data.Common;


namespace Plugin.Core.SQL
{
    public class ConnectionSQL
    {
        private static ConnectionSQL Field0 = new ConnectionSQL();
        protected NpgsqlConnectionStringBuilder ConnBuilder;

        public ConnectionSQL()
        {
            this.ConnBuilder = new NpgsqlConnectionStringBuilder()
            {
                Database = ConfigLoader.DatabaseName,
                Host = ConfigLoader.DatabaseHost,
                Username = ConfigLoader.DatabaseUsername,
                Password = ConfigLoader.DatabasePassword,
                Port = ConfigLoader.DatabasePort
            };
        }

        public static ConnectionSQL GetInstance() => ConnectionSQL.Field0;

        public NpgsqlConnection Conn()
        {
            return new NpgsqlConnection(((DbConnectionStringBuilder)this.ConnBuilder).ConnectionString);
        }
    }
}
