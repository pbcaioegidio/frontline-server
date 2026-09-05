using Npgsql;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.SQL;
using System.Data.SqlClient;

namespace Server.Game.Rcon
{
    public class RconLogger
    {
        public static string response;

        public static void Response(string pesan)
        {
            response = pesan;
            RconCommand.SendResponse(pesan);
        }
        public static void LogsPanel(string text, int condition)
        {
            if (condition == 0)
            {
                Response(RconCommand.Encode($"Success_{text}"));
                if (ConfigLoader.RconInfoCommand)
                    CLogger.Print($"{text}", LoggerType.Info);
                SaveHistory($"[RCON] {text}");
            }
            else
            {
                Response(RconCommand.Encode($"Fail_{text}"));
                if (ConfigLoader.RconInfoCommand)
                    CLogger.Print($"{text}", LoggerType.Error);
            }
        }
        public static void Logs(string text)
        {
            SaveHistory($"[RCON] {text}");
            CLogger.Print(text, LoggerType.Info);
        }
        private class RconData
        {
            public long id;
            public string log;

            public RconData()
            {
                this.log = "";
            }
        }
        private static RconData SaveHistory(string log)
        {
            RconData Data = new RconData()
            {
                log = log
            };
            try
            {
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = Connection.CreateCommand();
                    Connection.Open();
                    command.Parameters.AddWithValue("@log", Data.log);
                    command.CommandText = "INSERT INTO web_rcon_logger(log)VALUES(@log) RETURNING id";
                    object data = command.ExecuteScalar();
                    Data.id = (long)data;
                    command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                    return Data;
                }
            }
            catch
            {
                return (RconData)null;
            }
        }
    }
}
