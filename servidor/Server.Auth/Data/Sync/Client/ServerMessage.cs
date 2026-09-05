// Decompiled with JetBrains decompiler
// Type: Server.Auth.Data.Sync.Client.ServerMessage
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Server.Auth.Network;
using Server.Auth.Network.ServerPacket;
using System.Runtime.CompilerServices;


namespace Server.Auth.Data.Sync.Client
{
    public static class ServerMessage
    {
        
        public static void Load(SyncClientPacket C)
        {
            byte Length = C.ReadC();
            string A_1 = C.ReadS((int)Length);
            if (string.IsNullOrEmpty(A_1) || Length > (byte)60)
                return;
            int num = 0;
            using (PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK Packet = new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(A_1))
                num = AuthXender.Client.SendPacketToAllClients((AuthServerPacket)Packet);
            CLogger.Print($"Message sent to '{num}' Players", LoggerType.Command);
        }
    }
}