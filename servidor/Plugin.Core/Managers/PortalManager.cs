// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Managers.PortalManager
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Plugin.Core.Managers
{
    public static class PortalManager
    {
        // ponytail: rebuilt per request instead of cached, so a reload, a new event or an
        // expiring window is reflected without any reload path having to remember the portal.
        public static SortedList<string, PortalEvents> Build()
        {
            SortedList<string, PortalEvents> events = new SortedList<string, PortalEvents>();
            foreach (EventBoostModel eventBoostModel in EventBoostXML.Events)
            {
                if (eventBoostModel != null && eventBoostModel.EventIsEnabled())
                    events[$"Boost_{eventBoostModel.Id}"] = PortalEvents.BoostEvent;
            }
            foreach (EventRankUpModel eventRankUpModel in EventRankUpXML.Events)
            {
                if (eventRankUpModel != null && eventRankUpModel.EventIsEnabled())
                    events[$"RankUp_{eventRankUpModel.Id}"] = PortalEvents.RankUpEvent;
            }
            foreach (EventLoginModel eventLoginModel in EventLoginXML.Events)
            {
                if (eventLoginModel != null && eventLoginModel.EventIsEnabled())
                    events[$"Login_{eventLoginModel.Id}"] = PortalEvents.LoginEvent;
            }
            foreach (EventPlaytimeModel eventPlaytimeModel in EventPlaytimeJSON.Events)
            {
                if (eventPlaytimeModel != null && eventPlaytimeModel.EventIsEnabled())
                    events[$"Playtime_{eventPlaytimeModel.Id}"] = PortalEvents.PlaytimeEvent;
            }
            return events;
        }

        public static void Load()
        {
            CLogger.Print($"Plugin carregado: {PortalManager.Build().Count} Listed Event Portal", LoggerType.Info);
        }


        public static int GetInitialId(string Input)
        {
            Match match = Regex.Match(Input, "\\d+");
            int result;
            return match.Success && int.TryParse(match.Value, out result) ? result : -1;
        }

        // El bloque comun son 341 bytes (0x155). El cliente lee TRES cadenas UTF-16
        // separadas, no dos: +0x13 (60B) -> Title, +0x4F (60B) -> Subtitle y +0x8B (200B) ->
        // Description (BoostEvent__BuildDescriptionList, 0xBF8BB0-0xBF8BD8, cada una via el
        // ctor de string 0x8A9306, que corta en el primer NUL). Escribir el nombre con 120
        // bytes tapaba el hueco del subtitulo y, con un nombre de 30+ caracteres, dejaba al
        // cliente leyendo el resto del nombre como subtitulo.
        // Los dos ultimos bytes tambien son campos distintos: +0x153 es u8 (movzx en
        // 0xBF8BF0) y elige la imagen de fondo, +0x154 es una bandera aparte (cmp ..,1 en
        // 0xBF8C5A).
        public static byte[] InitEventData(
          PortalEvents Portal,
          int Id,
          uint[] DateTime,
          string[] Info,
          byte[] Type,
          byte Image)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                syncServerPacket.WriteC((byte)Portal);
                syncServerPacket.WriteD(Id);
                syncServerPacket.WriteC(Type[0]);
                syncServerPacket.WriteD(DateTime[0]);
                syncServerPacket.WriteD(DateTime[1]);
                syncServerPacket.WriteD(0);
                syncServerPacket.WriteC(Type[1]);
                syncServerPacket.WriteU(Fit(Info[0], 29), 60);
                syncServerPacket.WriteU(Fit(Info[1], 29), 60);
                syncServerPacket.WriteU(Fit(Info[2], 99), 200);
                syncServerPacket.WriteC(Image);
                syncServerPacket.WriteC(0);
                return syncServerPacket.ToArray();
            }
        }

        // WriteU rellena Count bytes sin reservar el NUL final, asi que un texto que ocupe
        // el campo entero se derrama sobre el siguiente al leerlo el cliente. Recortamos un
        // caracter antes para que el terminador siempre quepa.
        private static string Fit(string Text, int MaxChars)
        {
            if (string.IsNullOrEmpty(Text) || Text.Length <= MaxChars)
                return Text;
            int length = MaxChars;
            if (char.IsHighSurrogate(Text[length - 1]))
                --length;
            return Text.Substring(0, length);
        }

        public static byte[] InitRankUpData(EventRankUpModel RankUp)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                syncServerPacket.WriteC((byte)RankUp.Ranks.Count);
                foreach (int[] rank in RankUp.Ranks)
                {
                    syncServerPacket.WriteD(rank[0]);
                    syncServerPacket.WriteD(ComDiv.Percentage(rank[1], rank[3]));
                    syncServerPacket.WriteD(ComDiv.Percentage(rank[2], rank[3]));
                }
                return syncServerPacket.ToArray();
            }
        }

        // ponytail: el cliente hace FindGoods(goodId) y desreferencia el resultado sin
        // comprobar null (CalcIndexesWithCheck @0xC5D4FD, mov eax,[esi+0x4C]; probado en
        // unicorn: esi==0 -> lectura no mapeada en 0x4C). GetGood() no sirve de guarda:
        // recorre ShopAllList, que incluye variantes recortadas por MAX_BUY_INFO_PER_ITEM
        // y por el tope MAX_SHOP_GOODS, o sea goods que el cliente nunca recibe.
        private static List<int> ValidGoods(List<int> Goods, string Source)
        {
            List<int> valid = new List<int>();
            if (Goods == null)
                return valid;
            foreach (int goodId in Goods)
            {
                if (goodId == 0)
                    continue;
                if (!ShopManager.IsPackedGood(goodId))
                {
                    CLogger.Print($"Event Portal: {Source} skipped good {goodId} (not in the packed client catalog)", LoggerType.Warning);
                    continue;
                }
                valid.Add(goodId);
            }
            return valid;
        }

        public static byte[] InitPlaytimeData(EventPlaytimeModel Playtime)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                List<int> goods1 = ValidGoods(Playtime.Goods1, $"Playtime_{Playtime.Id}");
                List<int> goods2 = ValidGoods(Playtime.Goods2, $"Playtime_{Playtime.Id}");
                List<int> goods3 = ValidGoods(Playtime.Goods3, $"Playtime_{Playtime.Id}");
                syncServerPacket.WriteD(Playtime.Minutes1 * 60);
                syncServerPacket.WriteD(Playtime.Minutes2 * 60);
                syncServerPacket.WriteD(Playtime.Minutes3 * 60);
                foreach (int num in goods1)
                    syncServerPacket.WriteD(num);
                syncServerPacket.WriteB(new byte[(20 - goods1.Count) * 4]);
                foreach (int num in goods2)
                    syncServerPacket.WriteD(num);
                syncServerPacket.WriteB(new byte[(20 - goods2.Count) * 4]);
                foreach (int num in goods3)
                    syncServerPacket.WriteD(num);
                syncServerPacket.WriteB(new byte[(20 - goods3.Count) * 4]);
                return syncServerPacket.ToArray();
            }
        }

        // Layout OBSERVED en BoostEvent__BuildDescriptionList (0xBF8CCF-0xBF8CE9), que lee
        // el puntero crudo devuelto por GetBoost (0xD1807C); AddBoost guarda los 14 bytes
        // del hilo sin reorganizar, o sea offset de consumidor == offset de hilo:
        //   +0x00 u16  -> switch de EVENT_BOOSTEVENT_NORMAL_DESCRIPTION (kind 1..12)
        //   +0x02 u32  -> segundo argumento de esa descripcion
        //   +0x06 u32  -> substance+0x18 -> widget EVENT_REWARD_TOOL_TIP_EXP
        //   +0x0A u32  -> substance+0x14 -> widget EVENT_REWARD_TOOL_TIP_POINT
        // El u16 va DELANTE. Escribirlo al final corria los tres u32 dos bytes y el panel
        // mostraba porcentajes de millones (50 -> 0x00320000).
        public static byte[] InitBoostData(EventBoostModel Boost)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                syncServerPacket.WriteH((ushort)Boost.BoostType);
                syncServerPacket.WriteD(Boost.BoostValue);
                syncServerPacket.WriteD(ComDiv.Percentage(Boost.BonusExp, Boost.Percent));
                syncServerPacket.WriteD(ComDiv.Percentage(Boost.BonusGold, Boost.Percent));
                return syncServerPacket.ToArray();
            }
        }

        // Layout OBSERVED en BoostEvent__BuildDescriptionList case 3 (0xBF8F6C-0xBF8FB9):
        //   +0x00 u8   -> no lo lee este consumidor
        //   +0x01 u8   -> COUNT, controla el bucle (cmp byte[eax+1],0 / cmp ecx,eax)
        //   +0x02 u32 x4 -> good_id (lea edi,[eax+2]; mov ecx,[edi]; add edi,4)
        // Escribir los cuatro u32 primero hacia que el count saliera del byte 1 del primer
        // good_id (10327904 -> 0x9D9760 -> count 151), el cliente leia 606 bytes de un
        // bloque de 18 y pasaba basura a FindGoods; el retorno NULL revienta en
        // CalcIndexesWithCheck 0xC5D4FD (mov eax,[esi+0x4C]), probado en unicorn.
        public static byte[] InitLoginData(EventLoginModel Login)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                List<int> goods = ValidGoods(Login.Goods, $"Login_{Login.Id}");
                int count = goods.Count > 4 ? 4 : goods.Count;
                syncServerPacket.WriteC((byte)0);
                syncServerPacket.WriteC((byte)count);
                for (int index = 0; index < 4; ++index)
                    syncServerPacket.WriteD(index < count ? goods[index] : 0);
                return syncServerPacket.ToArray();
            }
        }
    }
}