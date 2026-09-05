// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BASE_ATTENDANCE_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_ATTENDANCE_ACK : GameServerPacket
    {
        private readonly EventVisitModel EvVisit;
        private readonly PlayerEvent Event;
        private readonly EventErrorEnum Error;
        
        public PROTOCOL_BASE_ATTENDANCE_ACK(EventErrorEnum Error, EventVisitModel EvVisit, PlayerEvent Event)
        {
            this.Error = Error;
            this.EvVisit = EvVisit;
            this.Event = Event;
            if (Event == null)
                return;
        }

        public override void Write()
        {
            WriteH((short)2337);
            WriteD((uint)Error);
            if (Error == EventErrorEnum.VISIT_EVENT_SUCCESS)
            {
                
                WriteD(EvVisit.Id);
                WriteC((byte)Event.LastVisitCheckDay);
                WriteC((byte)(Event.LastVisitCheckDay - 1));
                WriteC((byte)(Event.LastVisitDate < uint.Parse(DateTimeUtil.Now("yyMMddHHmm")) ? 1 : 2));
                WriteC((byte)Event.LastVisitSeqType);
                WriteC(1);
            }
        }
    }
}