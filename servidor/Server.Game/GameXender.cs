using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.JSON;
using Plugin.Core.Models;
using Plugin.Core.XML;
using Server.Game.Data.Sync;
using Server.Game.Network;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Numerics;

namespace Server.Game
{
    public class GameXender
    {
        private static readonly ConcurrentDictionary<int, GameManager> Servers = new ConcurrentDictionary<int, GameManager>();

        public static IEnumerable<GameManager> All => Servers.Values;

        public static GameManager Get(int ServerId)
        {
            GameManager Manager;
            return Servers.TryGetValue(ServerId, out Manager) ? Manager : null;
        }

        public static GameSync SyncOf(int ServerId)
        {
            GameManager Manager = Get(ServerId);
            return Manager == null ? null : Manager.Sync;
        }

        // Config is process-wide (same ServerConfigJSON entry for every server), so any
        // registered manager answers for all of them.
        public static ServerConfig Config
        {
            get
            {
                foreach (GameManager Manager in All)
                {
                    return Manager.Config;
                }
                return null;
            }
        }

        public static bool GetPlugin(int ServerId, string Host, int Port)
        {
            try
            {
                Synchronize Sync = SynchronizeXML.GetServer(Port);
                if (Sync == null)
                {
                    CLogger.Print($"Game channel {ServerId}: nenhum sync endpoint para a porta {Port}; adicione a linha em system_sync_endpoints", LoggerType.Error);
                    return false;
                }

                IPEndPoint EP = Sync.Connection;
                GameManager Manager = new GameManager(ServerId, Host, ConfigLoader.IsUseProxy ? ConfigLoader.PROXY_PORT[1]++ : Port);
                Manager.Sync = new GameSync(EP, Manager);
                Servers[ServerId] = Manager;
                Manager.Sync.Start();
                Manager.Start();
                Server.Game.Security.HeartbeatGuard.Start();
                return true;
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
                return false;
            }
        }

        public static int BroadcastToAll(GameServerPacket Packet)
        {
            int Count = 0;
            foreach (GameManager Manager in All)
            {
                Count += Manager.SendPacketToAllClients(Packet);
            }
            return Count;
        }

        public static void UpdateEvents()
        {
            foreach (GameManager Manager in All)
            {
                UpdateEvents(Manager);
            }
        }

        public static void UpdateShop()
        {
            foreach (GameManager Manager in All)
            {
                UpdateShop(Manager);
            }
        }

        public static void UpdateEvents(GameManager Manager)
        {
            foreach (var Client in Manager.SocketSessions.Values)
            {
                if (Client != null)
                {
                    Client.SendPacket(new PROTOCOL_BASE_EVENT_PORTAL_ACK(true));
                }
            }
        }

        public static void UpdateShop(GameManager Manager)
        {
            foreach (var gameClient in Manager.SocketSessions.Values)
            {
                if (gameClient != null)
                {
                    var player = gameClient.GetAccount();
                    ShopCatalog121Sender.SendFullCatalog(gameClient, player, true);
                    gameClient.SendPacket(new PROTOCOL_SHOP_TAG_INFO_ACK());
                    gameClient.SendPacket(new PROTOCOL_SHOP_GET_SAILLIST_ACK(true));
                }
            }
        }

        /// <summary>
        /// Broadcasts limited sale stock sync to all connected clients
        /// </summary>
        public static void BroadcastLimitedSaleSync()
        {
            foreach (GameManager Manager in All)
            {
                foreach (var Client in Manager.SocketSessions.Values)
                {
                    if (Client != null)
                    {
                        try
                        {
                            Client.SendPacket(new PROTOCOL_SHOP_LIMITED_SALE_SYNC_ACK());
                        }
                        catch { }
                    }
                }
            }
        }

    }
}
