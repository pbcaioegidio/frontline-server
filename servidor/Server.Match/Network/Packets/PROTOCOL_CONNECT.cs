// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Packets.PROTOCOL_CONNECT
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core.Network;


namespace Server.Match.Network.Packets
{
    public class PROTOCOL_CONNECT
    {
        public static byte[] GET_CODE()
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                syncServerPacket.WriteC((byte)66);
                syncServerPacket.WriteC((byte)0);
                syncServerPacket.WriteT(0.0f);
                syncServerPacket.WriteC((byte)0);
                syncServerPacket.WriteH((short)14);
                syncServerPacket.WriteD(0);
                syncServerPacket.WriteC((byte)8);
                return syncServerPacket.ToArray();
            }
        }
    }
}