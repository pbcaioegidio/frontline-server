using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Plugin.Core.XML;
using System.Runtime.CompilerServices;


namespace Server.Auth.Data.Sync.Client
{
    public class EventInfo
    {
        
        public static void LoadEventInfo(SyncClientPacket C)
        {
            int A_0 = (int)C.ReadC();
            if (!EventInfo.StaticMethod0(A_0))
                return;
            CLogger.Print($"Refresh event; Type: {A_0};", LoggerType.Command);
        }

        private static bool StaticMethod0(int A_0)
        {
            switch (A_0)
            {
                case 0:
                    EventVisitXML.Reload();
                    return true;
                case 1:
                    EventLoginXML.Reload();
                    return true;
                case 2:
                    EventBoostXML.Reload();
                    return true;
                case 3:
                    EventPlaytimeJSON.Reload();
                    return true;
                case 4:
                    EventQuestXML.Reload();
                    return true;
                case 5:
                    EventRankUpXML.Reload();
                    return true;
                case 6:
                    EventXmasXML.Reload();
                    return true;
                default:
                    return false;
            }
        }
    }
}