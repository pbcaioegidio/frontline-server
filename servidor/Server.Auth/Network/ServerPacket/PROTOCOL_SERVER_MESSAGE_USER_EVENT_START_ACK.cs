// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ServerPacket.PROTOCOL_SERVER_MESSAGE_USER_EVENT_START_ACK
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll


namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_SERVER_MESSAGE_USER_EVENT_START_ACK : AuthServerPacket
    {
        // Client 122 contract (S2MO PACKET_SERVER_MESSAGE_USER_EVENT_START, ctor 0xEAF9C5):
        // H pad, LTS_TARGET_INFO[83], D USER_EVENT_TYPE. A zeroed LTS with type 0 makes the
        // client skip the event (handler 0xEB1CC6 only acts when type == 1 and LTS.id != 0),
        // which preserves the current no-event behavior without starving its 89-byte read.
        public override void Write()
        {
            this.WriteH((short)3086);
            this.WriteH((short)0);
            this.WriteB(new byte[83]);
            this.WriteD(0);
        }
    }
}