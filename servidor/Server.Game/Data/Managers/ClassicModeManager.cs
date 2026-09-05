using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using Plugin.Core;
using Plugin.Core.SQL;
using Plugin.Core.Enums;

namespace Server.Game.Data.Managers
{
    public static class ClassicModeManager
    {
        public static List<RuleItem> items_latam = new List<RuleItem>();
        public static List<RuleItem> items_torneo = new List<RuleItem>();
        public static List<RuleItem> items_ic = new List<RuleItem>();
        public static List<RuleItem> items_ligacuchillera = new List<RuleItem>();
        public static List<RuleItem> items_evento1 = new List<RuleItem>();
        public static List<RuleItem> items_evento2 = new List<RuleItem>();
        public static List<RuleItem> items_evento3 = new List<RuleItem>();

        public static string Reload()
        {
            LoadList();
            return "Success Reload Rules";
        }

        public class RuleItem
        {
            public int item_id { get; set; }
            public string item_name { get; set; }

            public RuleItem(int item_id, string item_name)
            {
                this.item_id = item_id;
                this.item_name = item_name;
            }
        }

        public static void LoadList()
        {
            CLogger.Print("ClassicMode Load Success", LoggerType.Info);
            items_latam.Clear();
            items_torneo.Clear();
            items_ic.Clear();
            items_ligacuchillera.Clear();
            items_evento1.Clear();
            items_evento2.Clear();
            items_evento3.Clear();

            try
            {
                using (NpgsqlConnection connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = connection.CreateCommand();
                    connection.Open();

                    command.CommandText = "SELECT * FROM system_rules_room WHERE item_id > 100000 and (latam = TRUE OR torneo = TRUE OR ic = TRUE OR ligacuchillera = TRUE OR evento1 = TRUE OR evento2 = TRUE OR evento3 = TRUE)";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader data = command.ExecuteReader();
                    while (data.Read())
                    {
                        int itemId = data.GetInt32(data.GetOrdinal("item_id"));
                        string itemName = data.GetString(data.GetOrdinal("item_name"));

                        var item = new RuleItem(itemId, itemName);

                        if (data.GetBoolean(data.GetOrdinal("latam")))
                        {
                            items_latam.Add(item);
                        }
                        if (data.GetBoolean(data.GetOrdinal("torneo")))
                        {
                            items_torneo.Add(item);
                        }
                        if (data.GetBoolean(data.GetOrdinal("ic")))
                        {
                            items_ic.Add(item);
                        }
                        if (data.GetBoolean(data.GetOrdinal("ligacuchillera")))
                        {
                            items_ligacuchillera.Add(item);
                        }
                        if (data.GetBoolean(data.GetOrdinal("evento1")))
                        {
                            items_evento1.Add(item);
                        }
                        if (data.GetBoolean(data.GetOrdinal("evento2")))
                        {
                            items_evento2.Add(item);
                        }
                        if (data.GetBoolean(data.GetOrdinal("evento3")))
                        {
                            items_evento3.Add(item);
                        }
                    }
                    command.Dispose();
                    data.Close();
                    connection.Dispose();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("[ClassicModeManager.LoadList] Erro fatal!", Plugin.Core.Enums.LoggerType.Error, ex);
            }
        }
    }
}