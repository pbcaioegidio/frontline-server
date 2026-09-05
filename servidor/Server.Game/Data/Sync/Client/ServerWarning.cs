using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Network;
using Plugin.Core.XML;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network;
using Server.Game.Network.ServerPacket;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Server.Game.Data.Sync.Client
{
    public class ServerWarning
    {
        public static void LoadGMWarning(SyncClientPacket C, GameManager Owner)
        {
            string valor = C.ReadS((int)C.ReadC());
            string str = C.ReadS((int)C.ReadC());
            string A_1 = C.ReadS((int)C.ReadH());
            Account accountDb = AccountManager.GetAccountDB((object)valor, 0, 31 /*0x1F*/);
            if (accountDb == null || !(accountDb.Password == str) || accountDb.Access < AccessLevel.GAMEMASTER)
                return;
            int num = 0;
            using (PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK Packet = new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(A_1))
                num = Owner.SendPacketToAllClients(Packet);
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

        public static void LoadShutdown(SyncClientPacket C, GameManager Owner)
        {
            string valor = C.ReadS((int)C.ReadC());
            string str = C.ReadS((int)C.ReadC());
            Account accountDb = AccountManager.GetAccountDB((object)valor, 0, 31 /*0x1F*/);
            if (accountDb == null || !(accountDb.Password == str) || accountDb.Access < AccessLevel.GAMEMASTER)
                return;
            int num = 0;
            foreach (GameClient gameClient in (IEnumerable<GameClient>)Owner.SocketSessions.Values)
            {
                gameClient.Client.Shutdown(SocketShutdown.Both);
                gameClient.Client.Close(10000);
                ++num;
            }
            CLogger.Print($"Disconnected Players: {num} (By: {valor})", LoggerType.Warning);
            Owner.ServerIsClosed = true;
            Owner.MainSocket.Close(5000);
            CLogger.Print("1/2 Step", LoggerType.Warning);
            Thread.Sleep(5000);
            Owner.Sync.Close();
            CLogger.Print("2/2 Step", LoggerType.Warning);
            foreach (GameClient gameClient in (IEnumerable<GameClient>)Owner.SocketSessions.Values)
                gameClient.Close(0, true);
            CLogger.Print($"{$"Shutdowned Server: {num} players disconnected; by Login: '"}{valor};", LoggerType.Command);
        }
    }
}