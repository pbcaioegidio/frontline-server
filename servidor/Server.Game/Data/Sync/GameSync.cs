using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Game.Data.Sync.Client;
using Server.Game.Network;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server.Game.Data.Sync
{
    public class GameSync
    {
        protected UdpClient Client;

        public readonly GameManager Owner;

        public GameSync(IPEndPoint Conn, GameManager Owner)
        {
            Client = new UdpClient(Conn);
            this.Owner = Owner;
        }

        public bool Start()
        {
            try
            {
                IPEndPoint EP = Client.Client.LocalEndPoint as IPEndPoint;
                uint IOC_IN = 0x80000000, IOC_VENDOR = 0x18000000, SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                if (OperatingSystem.IsWindows())
                    Client.Client.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null); // fix linux: SIO_UDP_CONNRESET only exists on Winsock
                ThreadPool.QueueUserWorkItem(_ => Read());
                CLogger.Print($"Endereco Sync Game {EP.Address}:{EP.Port}", LoggerType.Info);
                return true;
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
                return false;
            }
        }

        private void Read()
        {
            try
            {
                Client.BeginReceive(new AsyncCallback(AcceptCallback), null);
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }

        // The receive must be issued from a pool thread and re-armed in a finally: a dedicated
        // thread that exits right after BeginReceive gets its pending overlapped read cancelled,
        // so the next datagram fails with "the I/O operation has been aborted" and the loop dies.
        private void AcceptCallback(IAsyncResult Result)
        {
            if (Owner.ServerIsClosed)
            {
                return;
            }
            try
            {
                IPEndPoint Remote = new IPEndPoint(IPAddress.Any, 8000);
                byte[] Buffer = Client.EndReceive(Result, ref Remote);
                if (Buffer.Length >= 2)
                {
                    LoadPacket(Buffer);
                }
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
            finally
            {
                Read();
            }
        }

        private void LoadPacket(byte[] Buffer)
        {
            try
            {
                SyncClientPacket C = new SyncClientPacket(Buffer);

                short Opcode = C.ReadH();
                switch (Opcode)
                {
                    case 1: RoomPassPortal.Load(C); break;
                    case 2: RoomBombC4.Load(C); break;
                    case 3: RoomDeath.Load(C); break;
                    case 4: RoomHitMarker.Load(C); break;
                    case 5: RoomSabotageSync.Load(C); break;
                    case 6: RoomPing.Load(C); break;
                    case 10: AuthLogin.Load(C); break;
                    case 11: FriendInfo.Load(C); break;
                    case 33: LoginSessionSync.Load(C); break;
                    case 13: AccountInfo.Load(C); break;
                    case 15: ServerCache.Load(C); break;
                    case 16: ClanSync.Load(C); break;
                    case 17: FriendSync.Load(C); break;
                    case 18: InventorySync.Load(C); break;
                    case 19: PlayerSync.Load(C); break;
                    case 20: ServerWarning.LoadGMWarning(C, Owner); break;
                    case 21: ClanServersSync.Load(C); break;
                    case 22: ServerWarning.LoadShopRestart(C); break;
                    case 23: ServerWarning.LoadServerUpdate(C); break;
                    case 24: ServerWarning.LoadShutdown(C, Owner); break;
                    case 31: EventInfo.LoadEventInfo(C); break;
                    case 25: SupervisorReload.Load(C, Owner); break;
                    case 32: ReloadConfig.Load(C); break;
                    case 7171: ServerMessage.Load(C, Owner); break;

                    default: CLogger.Print(Bitwise.ToHexData($"Game - Opcode Not Found: [{Opcode}]", C.ToArray()), LoggerType.Opcode); break;
                }
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }

        public SChannelModel GetServer(AccountStatus status)
        {
            return GetServer(status.ServerId);
        }

        public SChannelModel GetServer(int serverId)
        {
            if (serverId == 255 || serverId == Owner.ServerId)
            {
                return null;
            }
            return SChannelXML.GetServer(serverId);
        }

        public void SendBytes(long PlayerId, GameServerPacket Packet, int ServerId)
        {
            try
            {
                if (Packet == null)
                {
                    return;
                }
                SChannelModel Server = GetServer(ServerId);
                if (Server == null)
                {
                    return;
                }
                string PacketName = Packet.GetType().Name;
                byte[] Data = Packet.GetBytes("GameSync.SendBytes");
                IPEndPoint Sync = SynchronizeXML.GetServer(Server.Port).Connection;
                using (SyncServerPacket S = new SyncServerPacket())
                {
                    S.WriteH(13);
                    S.WriteQ(PlayerId);
                    S.WriteC(0);
                    S.WriteC((byte)(PacketName.Length + 1));
                    S.WriteS(PacketName, PacketName.Length + 1);
                    S.WriteH((ushort)Data.Length);
                    S.WriteB(Data);
                    SendPacket(S.ToArray(), Sync);
                }
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }

        public void SendBytes(long PlayerId, string PacketName, byte[] Data, int ServerId)
        {
            try
            {
                if (Data.Length == 0)
                {
                    return;
                }
                SChannelModel Server = GetServer(ServerId);
                if (Server == null)
                {
                    return;
                }
                IPEndPoint Sync = SynchronizeXML.GetServer(Server.Port).Connection;
                using (SyncServerPacket S = new SyncServerPacket())
                {
                    S.WriteH(13);
                    S.WriteQ(PlayerId);
                    S.WriteC(0);
                    S.WriteC((byte)(PacketName.Length + 1));
                    S.WriteS(PacketName, PacketName.Length + 1);
                    S.WriteH((ushort)Data.Length);
                    S.WriteB(Data);
                    SendPacket(S.ToArray(), Sync);
                }
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }

        public void SendCompleteBytes(long PlayerId, string PacketName, byte[] Data, int ServerId)
        {
            try
            {
                if (Data.Length == 0)
                {
                    return;
                }
                SChannelModel Server = GetServer(ServerId);
                if (Server == null)
                {
                    return;
                }
                IPEndPoint Sync = SynchronizeXML.GetServer(Server.Port).Connection;
                using (SyncServerPacket S = new SyncServerPacket())
                {
                    S.WriteH(13);
                    S.WriteQ(PlayerId);
                    S.WriteC(1);
                    S.WriteC((byte)(PacketName.Length + 1));
                    S.WriteS(PacketName, PacketName.Length + 1);
                    S.WriteH((ushort)Data.Length);
                    S.WriteB(Data);
                    SendPacket(S.ToArray(), Sync);
                }
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }

        public void SendPacket(byte[] Data, IPEndPoint Address)
        {
            Client.Send(Data, Data.Length, Address);
        }

        public void Close() => Client.Close();
    }
}
