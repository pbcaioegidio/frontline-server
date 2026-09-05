using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using System.Collections.Generic;

namespace Plugin.Core.Managers
{
    public static class ChannelTypeConditionManager
    {
        public const int FirstChannelType = 0;
        public const int ChannelTypeCount = 100;

        public const int FirstChampionshipChannelType = 101;
        public const int ChampionshipChannelTypeCount = 30;

        private static readonly ChannelTypeConditionRow Unconfigured = new ChannelTypeConditionRow();

        private static Dictionary<int, ChannelTypeConditionRow> Conditions =
            new Dictionary<int, ChannelTypeConditionRow>();

        public static void Load()
        {
            List<ChannelTypeConditionRow> Rows = DaoManagerSQL.GetSystemChannelTypeConditions();
            if (Rows == null)
            {
                CLogger.Print("system_channel_type_conditions unreachable, every channel type stays unconfigured", LoggerType.Error);
                return;
            }

            Dictionary<int, ChannelTypeConditionRow> Loaded = new Dictionary<int, ChannelTypeConditionRow>();
            foreach (ChannelTypeConditionRow Row in Rows)
            {
                Loaded[Row.ChannelType] = Row;
            }

            Conditions = Loaded;
        }

        public static void Reload()
        {
            Load();
        }

        public static ChannelTypeConditionRow Get(int ChannelType)
        {
            ChannelTypeConditionRow Row;
            return Conditions.TryGetValue(ChannelType, out Row) ? Row : Unconfigured;
        }
    }
}
