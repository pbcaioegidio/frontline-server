using Plugin.Core.Enums;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using System;
using System.Collections.Generic;

namespace Plugin.Core.Security
{
    /// <summary>
    /// Cache da tabela base_ban_hwid para bloquear login por fingerprint (UserFileList/HWID).
    /// </summary>
    public static class HwIdBanCache
    {
        private static readonly object Gate = new object();
        private static HashSet<string> Banned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static DateTime LoadedAt = DateTime.MinValue;
        private static readonly TimeSpan RefreshEvery = TimeSpan.FromSeconds(60);

        public static bool IsBanned(string hardwareId)
        {
            if (string.IsNullOrWhiteSpace(hardwareId))
                return false;

            EnsureLoaded();
            return Banned.Contains(hardwareId.Trim());
        }

        public static void Invalidate()
        {
            lock (Gate)
            {
                LoadedAt = DateTime.MinValue;
            }
        }

        private static void EnsureLoaded()
        {
            DateTime now = DateTimeUtil.Now();
            if ((now - LoadedAt) < RefreshEvery && Banned.Count >= 0 && LoadedAt != DateTime.MinValue)
                return;

            lock (Gate)
            {
                if ((now - LoadedAt) < RefreshEvery && LoadedAt != DateTime.MinValue)
                    return;

                List<string> list = DaoManagerSQL.GetHwIdList();
                var next = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (list != null)
                {
                    foreach (string id in list)
                    {
                        if (!string.IsNullOrWhiteSpace(id))
                            next.Add(id.Trim());
                    }
                }
                Banned = next;
                LoadedAt = now;
                CLogger.Print($"[HwIdBan] Cache carregado: {Banned.Count} HWID(s)", LoggerType.Info);
            }
        }
    }
}
