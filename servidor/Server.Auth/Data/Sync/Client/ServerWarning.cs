// Decompiled with JetBrains decompiler
// Type: Server.Auth.Data.Sync.Client.ServerWarning
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Network;
using Plugin.Core.XML;
using Server.Auth.Data.Managers;
using Server.Auth.Data.Models;
using Server.Auth.Network;
using Server.Auth.Network.ServerPacket;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;


namespace Server.Auth.Data.Sync.Client
{
    public static class ServerWarning
    {
        
        public static void LoadGMWarning(SyncClientPacket C)
        {
            string valor = C.ReadS((int)C.ReadC());
            string valor2 = C.ReadS((int)C.ReadC());
            string A_1 = C.ReadS((int)C.ReadH());
            Account accountDb = AccountManager.GetAccountDB((object)valor, (object)valor2, 2, 31 /*0x1F*/);
            if (accountDb == null || !(accountDb.Password == valor2) || accountDb.Access < AccessLevel.GAMEMASTER)
                return;
            int num = 0;
            using (PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK Packet = new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(A_1))
                num = AuthXender.Client.SendPacketToAllClients((AuthServerPacket)Packet);
            CLogger.Print($"Message sent to '{num}' Players: '{A_1}'; by Username: '{valor}'", LoggerType.Command);
        }

        
        public static void LoadShopRestart(SyncClientPacket C)
        {
            int Type = (int)C.ReadC();
            ShopManager.Reset();
            ShopManager.Load(Type);
            CLogger.Print($"Shop restarted. (Type: {Type})", LoggerType.Command);
        }

        
        public static void LoadServerUpdate(SyncClientPacket C)
        {
            int ServerId = (int)C.ReadC();
            SChannelXML.UpdateServer(ServerId);
            CLogger.Print($"Server updated. (Id: {ServerId})", LoggerType.Command);
        }

        
        public static void LoadShutdown(SyncClientPacket C)
        {
            string valor = C.ReadS((int)C.ReadC());
            string valor2 = C.ReadS((int)C.ReadC());
            Account accountDb = AccountManager.GetAccountDB((object)valor, (object)valor2, 2, 31 /*0x1F*/);
            if (accountDb == null || !(accountDb.Password == valor2) || accountDb.Access < AccessLevel.GAMEMASTER)
                return;
            int num = 0;
            foreach (AuthClient authClient in (IEnumerable<AuthClient>)AuthXender.SocketSessions.Values)
            {
                authClient.Client.Shutdown(SocketShutdown.Both);
                authClient.Client.Close(10000);
                ++num;
            }
            CLogger.Print($"Disconnected Players: {num} (By: {valor})", LoggerType.Warning);
            AuthXender.Client.ServerIsClosed = true;
            AuthXender.Client.MainSocket.Close(5000);
            CLogger.Print("1/2 Step", LoggerType.Warning);
            Thread.Sleep(5000);
            AuthXender.Sync.Close();
            CLogger.Print("2/2 Step", LoggerType.Warning);
            foreach (AuthClient authClient in (IEnumerable<AuthClient>)AuthXender.SocketSessions.Values)
                authClient.Close(0, true);
            CLogger.Print($"Shutdowned Server: {num} players disconnected; by Username: '{valor};", LoggerType.Command);
        }
    }
}