using Microsoft.Win32.SafeHandles;
using Network.ClientPacket;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Logging;
using Plugin.Core.Security;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Data.Sync.Server;
using Server.Game.Data.Utils;
using Server.Game.Network;
using Server.Game.Network.ClientPacket;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;
using Utility;

namespace Server.Game
{
    public class GameClient : IDisposable
    {
        public int ServerId;
        public long PlayerId;
        public Socket Client;
        public Account Player;
        public DateTime SessionDate;
        public int SessionId;
        public ushort SessionSeed;
        private ushort NextSessionSeed;
        public int SessionShift, FirstPacketId;
        private bool Disposed = false;
        private bool connectionClosed = false;
        private int seedMismatchCount = 0;
        private DateTime lastSeedMismatch = DateTime.MinValue;
        private const int MaxSeedMismatches = 10;
        private readonly object syncLock = new object();
        private readonly object receiveLock = new object();
        private byte[] receiveRemainder = new byte[0];
        private readonly PacketRateLimiter PacketLimiter = new PacketRateLimiter(180);
        public readonly byte[] ServerKey = new byte[16];
        public readonly byte[] ClientKey = new byte[16];
        public GameClient(int ServerId, Socket Client)
        {
            this.ServerId = ServerId;
            this.Client = Client;
            this.SessionDate = DateTime.Now;
        }

        public void Close(int delay = 0)
        {
            lock (syncLock)
            {
                if (connectionClosed) return;

                try
                {
                    connectionClosed = true;
                    if (Client != null)
                    {
                        if (Client.Connected)
                        {
                            Client.Shutdown(SocketShutdown.Both);
                        }
                        Client.Close(delay);
                    }
                    Player = null;
                }
                catch (Exception ex)
                {
                    CLogger.Print($"Handled error closing socket: {ex.Message}", LoggerType.Debug);
                }
            }
        }

        public bool CheckSeedIntegrity()
        {
            if (seedMismatchCount >= MaxSeedMismatches)
            {
                return false;
            }
            return true;
        }

        public Account GetAccount()
        {
            try
            {
                Account localPlayer = new Account();
                if (Player != null)
                {
                    localPlayer = Player;
                }
                else
                {

                    if (ConfigLoader.DebugMode)
                        CLogger.Print($"GameClient.GetAccount: Account is null for IP: {GetIPAddress()}", LoggerType.Warning);

                    localPlayer = null;
                }
                return localPlayer;
            }
            catch (Exception Ex)
            {
                CLogger.Print($"GameClient.GetAccount Exception; {Ex.Message}", LoggerType.Error, Ex);
                return null;
            }
        }

        public void Dispose()
        {
            try
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (Disposed)
                {
                    return;
                }
                Player = null;
                if (Client != null)
                {
                    Client.Dispose();
                    Client = null;
                }
                PlayerId = 0;
                Disposed = true;
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }

        public void StartSession()
        {
            try
            {
                NextSessionSeed = SessionSeed;
                SessionShift = ((SessionId + Bitwise.CRYPTO[0]) % 7 + 1);
                // CONNECT_ACK síncrono — evita corrida QueueWork+BeginSend (FIN sem payload)
                Connect();
                CLogger.QueueWork(ServerKind.Game, ReadPacket);
                CLogger.QueueWork(ServerKind.Game, CheckConnectionTimeout);
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
                Close(0, true);
            }
        }

        private void CheckConnectionTimeout()
        {
            Thread.Sleep(30000);
            // Same rule as Auth: a peer that already dropped (panel port probe) is not a stall.
            if (connectionClosed || Client == null || FirstPacketId != 0)
                return;


            if (ConfigLoader.DebugMode)
            {
                CLogger.Print("Connection destroyed due to no responses. IPAddress: " + GetIPAddress(), LoggerType.Warning);
            }
            Close(0, true);
        }

        public string GetIPAddress()
        {
            try
            {
                if (Client != null && Client.RemoteEndPoint != null)
                {
                    return (Client.RemoteEndPoint as IPEndPoint).Address.ToString();
                }
                return "";
            }
            catch
            {
                return "";
            }
        }

        public IPAddress GetAddress()
        {
            try
            {
                if (Client != null && Client.RemoteEndPoint != null)
                {
                    return (Client.RemoteEndPoint as IPEndPoint).Address;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private void Connect() => SendPacketPlain(new PROTOCOL_BASE_CONNECT_ACK(this));

        public void SendCompletePacket(byte[] Data, string PacketName)
        {
            try
            {
                if (Data.Length < 4)
                {
                    return;
                }
                byte[] Logical = new byte[5];
                byte[] Result = new byte[Data.Length + 1 + Logical.Length];
                Array.Copy(Data, 0, Result, 0, Data.Length);
                Array.Clear(Result, Data.Length, 1 + Logical.Length);
                if (Client != null)
                {
                    ushort Opcode = BitConverter.ToUInt16(Result, 2);
                    CLogger.Packet(ServerKind.Game, Direction.Out, Opcode, PacketName, ConnRegistry.IdFor(Client?.RemoteEndPoint?.ToString()), Result.Length, Result);
                }
                if (Result.Length > 0)
                {
                    byte[] FinalResult = Result;
                    Client.BeginSend(FinalResult, 0, FinalResult.Length, SocketFlags.None, new AsyncCallback(SendCallback), Client);
                }
                Result = null;
            }
            catch
            {
                Close(0, true);
            }
        }

        public void SendPacket(byte[] originalData, string PacketName)
        {
            try
            {
                if (originalData.Length < 2)
                {
                    return;
                }

                byte[] packetSize = BitConverter.GetBytes((ushort)(originalData.Length + 2));
                byte[] newData = new byte[originalData.Length + packetSize.Length];

                Array.Copy(packetSize, 0, newData, 0, packetSize.Length);
                Array.Copy(originalData, 0, newData, 2, originalData.Length);

                byte[] packetData = new byte[newData.Length + 5];
                Array.Copy(newData, 0, packetData, 0, newData.Length);

                if (Client != null)
                {
                    ushort Opcode = BitConverter.ToUInt16(packetData, 2);
                    int conn = ConnRegistry.IdFor(Client?.RemoteEndPoint?.ToString());
                    CLogger.Packet(ServerKind.Game, Direction.Out, Opcode, PacketName, conn, packetData.Length, packetData);
                }

                if (Client != null && packetData.Length > 0)
                {
                    Client.BeginSend(packetData, 0, packetData.Length, SocketFlags.None, new AsyncCallback(SendCallback), Client);
                }
                packetData = null;
            }
            catch
            {
                Close(0, true);
            }
        }


        public void SendPacket(GameServerPacket Packet)
        {
            try
            {
                byte[] originalData = Packet.GetBytes("GameClient.SendPacket");
                if (originalData.Length < 2)
                {
                    Packet.Dispose();
                    return;
                }

                byte[] packetSize = BitConverter.GetBytes((ushort)(originalData.Length + 2));
                byte[] newData = new byte[originalData.Length + packetSize.Length];

                Array.Copy(packetSize, 0, newData, 0, packetSize.Length);
                Array.Copy(originalData, 0, newData, 2, originalData.Length);

                byte[] packetData = new byte[newData.Length + 5];
                Array.Copy(newData, 0, packetData, 0, newData.Length);

                if (Client != null)
                {
                    ushort Opcode = BitConverter.ToUInt16(packetData, 2);
                    int conn = ConnRegistry.IdFor(Client?.RemoteEndPoint?.ToString());
                    CLogger.Packet(ServerKind.Game, Direction.Out, Opcode, Packet.GetType().Name, conn, packetData.Length, packetData, Packet.Schema);
                }

                if (Client != null && Client.Connected && packetData.Length > 0)
                {
                    Client.BeginSend(packetData, 0, packetData.Length, SocketFlags.None, new AsyncCallback(SendCallback), Client);
                }

                Packet.Dispose();
                packetData = null;
            }
            catch (SocketException)
            {
                try { Packet?.Dispose(); } catch { }
            }
            catch (Exception ex)
            {
                try { Packet?.Dispose(); } catch { }
                CLogger.Print($"SendPacket error: {ex.Message}", LoggerType.Error, ex);
                Close(0, true);
            }
        }
        private void SendPacketPlain(GameServerPacket Packet)
        {
            try
            {
                byte[] payload = Packet.GetBytes("AuthClient.SendPacketPlain");
                if (payload == null || payload.Length < 2) { Packet.Dispose(); return; }

                byte[] sizePrefix = BitConverter.GetBytes((ushort)(payload.Length + 2));
                byte[] frame = new byte[sizePrefix.Length + payload.Length + 5];
                Buffer.BlockCopy(sizePrefix, 0, frame, 0, sizePrefix.Length);
                Buffer.BlockCopy(payload, 0, frame, sizePrefix.Length, payload.Length);

                if (Client != null)
                {
                    ushort opcode = BitConverter.ToUInt16(payload, 0);
                    int conn = ConnRegistry.IdFor(Client?.RemoteEndPoint?.ToString());
                    CLogger.Packet(ServerKind.Game, Direction.Out, opcode, Packet.GetType().Name, conn, payload.Length, payload);
                }

                if (Client != null && Client.Connected && frame.Length > 0)
                {
                    int sent = 0;
                    while (sent < frame.Length)
                        sent += Client.Send(frame, sent, frame.Length - sent, SocketFlags.None);
                }
                Packet.Dispose();
            }
            catch (SocketException) { try { Packet?.Dispose(); } catch { } }
            catch (Exception ex)
            {
                try { Packet?.Dispose(); } catch { }
                CLogger.Print($"SendPacketPlain error: {ex.Message}", LoggerType.Error, ex);
                Close(0, true);
            }
        }
        private void BeginSend(byte[] frame)
        {
            if (Client == null || !Client.Connected || frame == null)
                return;
            Client.BeginSend(frame, 0, frame.Length, SocketFlags.None, new AsyncCallback(SendCallback), Client);
        }
        private void SendCallback(IAsyncResult Result)
        {
            try
            {
                if (Result.AsyncState is Socket Handler && Handler.Connected)
                {
                    Handler.EndSend(Result);
                }
            }
            catch
            {
                Close(0, true);
            }
        }

        private void ReadPacket()
        {
            try
            {
                StateObject State = new StateObject()
                {
                    WorkSocket = Client,
                    Buffer = new byte[StateObject.BufferSize]
                };
                Client.BeginReceive(State.Buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(OnReceiveCallback), State);
            }
            catch
            {
                Close(0, true);
            }
        }

        private void OnReceiveCallback(IAsyncResult Result)
        {
            CLogger.SetThreadKind(ServerKind.Game);
            StateObject state = Result.AsyncState as StateObject;
            try
            {
                int bytesCount = state.WorkSocket.EndReceive(Result);
                if (bytesCount <= 0) { Close(0, true); return; }

                byte[] rawBuffer;
                lock (receiveLock)
                {
                    rawBuffer = new byte[receiveRemainder.Length + bytesCount];
                    Buffer.BlockCopy(receiveRemainder, 0, rawBuffer, 0, receiveRemainder.Length);
                    Buffer.BlockCopy(state.Buffer, 0, rawBuffer, receiveRemainder.Length, bytesCount);
                    receiveRemainder = new byte[0];
                }
                bytesCount = rawBuffer.Length;

                int offset = 0;
                bool isFirstInBuffer = true;

                while (offset + 2 <= bytesCount)
                {
                    ushort hw = (ushort)(rawBuffer[offset] | (rawBuffer[offset + 1] << 8));
                    bool enc = PacketFraming.IsEncrypted(hw);
                    int payloadLen = PacketFraming.PayloadLength(hw);
                    int frameLength = PacketFraming.WireLength(hw);
                    if (offset + frameLength > bytesCount)
                    {
                        CLogger.Print(
                            $"Incomplete TCP frame. IP: {GetIPAddress()}; SessionId: {SessionId}; " +
                            $"Header: 0x{hw:X4}; Encrypted: {enc}; BufferOffset: {offset}; " +
                            $"BufferLength: {bytesCount}; PayloadLength: {payloadLen}; FrameLength: {frameLength}; " +
                            $"RemainingLength: {bytesCount - offset}; FirstInBuffer: {isFirstInBuffer}; " +
                            $"Data: {BitConverter.ToString(rawBuffer, offset, bytesCount - offset)}",
                            LoggerType.Warning);
                        break;
                    }

                    if (!enc)
                    {
                        int dumpLength = Math.Min(frameLength, 64);
                        CLogger.Print(
                            $"Ignored non-encrypted TCP frame. IP: {GetIPAddress()}; SessionId: {SessionId}; " +
                            $"Header: 0x{hw:X4}; BufferOffset: {offset}; BufferLength: {bytesCount}; " +
                            $"PayloadLength: {payloadLen}; FrameLength: {frameLength}; " +
                            $"FirstInBuffer: {isFirstInBuffer}; DataPrefix: {BitConverter.ToString(rawBuffer, offset, dumpLength)}",
                            LoggerType.Warning);
                    }

                    if (enc)
                    {
                        byte[] cmess = new byte[payloadLen];
                        Array.Copy(rawBuffer, offset + 2, cmess, 0, payloadLen);

                        var (decrypted, ok) = CMessCipher.Decrypt(ClientKey, cmess, CMessCipher.MODE_AUTH);

                        if (!ok || decrypted == null || decrypted.Length < 4)
                        {
                            Close(0, true);
                            return;
                        }
                        ushort packetId = BitConverter.ToUInt16(decrypted, 0);
                        ushort packetSeed = BitConverter.ToUInt16(decrypted, 2);
                        FirstPacketCheck(packetId);
                        if (connectionClosed) return;
                        ushort nextSessionSeedBefore = NextSessionSeed;
                        bool seedAccepted = CheckSeed(packetSeed, isFirstInBuffer);
                        if (!seedAccepted)
                        {
                            CLogger.Print(
                                $"Rejected encrypted packet due to seed mismatch. IP: {GetIPAddress()}; " +
                                $"SessionId: {SessionId}; Opcode: {packetId}; PacketSeed: {packetSeed}; " +
                                $"NextSeedBefore: {nextSessionSeedBefore}; NextSeedAfter: {NextSessionSeed}; " +
                                $"PrimarySeed: {SessionSeed}; BufferOffset: {offset}; BufferLength: {bytesCount}; " +
                                $"PayloadLength: {payloadLen}; FirstInBuffer: {isFirstInBuffer}; Data: {BitConverter.ToString(decrypted)}",
                                LoggerType.Warning);
                            if (connectionClosed) return;
                            offset += frameLength;
                            isFirstInBuffer = false;
                            continue;
                        }
                        CLogger.Print(
                            $"Accepted encrypted packet seed. IP: {GetIPAddress()}; SessionId: {SessionId}; " +
                            $"Opcode: {packetId}; PacketSeed: {packetSeed}; NextSeedBefore: {nextSessionSeedBefore}; " +
                            $"NextSeedAfter: {NextSessionSeed}; BufferOffset: {offset}; BufferLength: {bytesCount}; " +
                            $"PayloadLength: {payloadLen}; FirstInBuffer: {isFirstInBuffer}",
                            LoggerType.Debug);

                        ProcessPacket(packetId, decrypted, "REQ");
                    }

                    offset += frameLength;
                    isFirstInBuffer = false;
                }

                if (offset < bytesCount)
                {
                    lock (receiveLock)
                    {
                        receiveRemainder = new byte[bytesCount - offset]; // fix respawn: retain a TCP fragment for the next receive
                        Buffer.BlockCopy(rawBuffer, offset, receiveRemainder, 0, receiveRemainder.Length);
                    }
                }
            }
            catch { Close(0, true); }
            finally
            {
                if (!connectionClosed)
                {
                    CLogger.QueueWork(ServerKind.Game, () =>
                    {
                        try { ReadPacket(); }
                        catch (Exception ex)
                        {
                            CLogger.Print($"Failed to resume receive: {ex.Message}", LoggerType.Error, ex);
                            Close(0, true);
                        }
                    });
                }
            }
        }

        public void Close(int TimeMS, bool DestroyConnection, bool Kicked = false)
        {
            if (connectionClosed)
            {
                return;
            }
            try
            {
                connectionClosed = true;
                string Ip = GetIPAddress();
                GameManager Manager = GameXender.Get(ServerId);
                if (Manager != null)
                {
                    Manager.RemoveSession(this);
                    Manager.RemoveConnection(Ip);
                }
                Account Cache = Player;
                if (DestroyConnection)
                {
                    if (PlayerId > 0 && Cache != null)
                    {
                        Cache.SetOnlineStatus(false);
                        RoomModel Room = Cache.Room;
                        if (Room != null)
                        {
                            Room.RemovePlayer(Cache, false, Kicked ? 1 : 0);
                        }
                        MatchModel Match = Cache.Match;
                        if (Match != null)
                        {
                            Match.RemovePlayer(Cache);
                        }
                        ChannelModel Channel = Cache.GetChannel();
                        if (Channel != null)
                        {
                            Channel.RemovePlayer(Cache);
                        }
                        Cache.Status.ResetData(PlayerId);
                        AllUtils.SyncPlayerToFriends(Cache, false);
                        AllUtils.SyncPlayerToClanMembers(Cache);
                        Cache.SimpleClear();
                        Cache.UpdateCacheInfo();
                        Player = null;
                    }
                    PlayerId = 0;
                    if (Client != null)
                    {
                        Client.Close(TimeMS);
                    }
                    Thread.Sleep(TimeMS);
                    Dispose();
                }
                else if (Cache != null)
                {
                    Cache.SimpleClear();
                    Cache.UpdateCacheInfo();
                    Player = null;
                }
                UpdateServer.RefreshSChannel(ServerId);
            }
            catch (Exception ex)
            {
                CLogger.Print($"GameClient.Close: {ex.Message}", LoggerType.Error, ex);
            }
        }

        private void FirstPacketCheck(ushort PacketId)
        {
            if (PacketId != 0)
            {
                this.FirstPacketId = PacketId;
                return;
            }
            if (PacketId != 1281 && PacketId != 2309)
            {
                CLogger.Print($"Connection destroyed due to unknown first packet. Opcode: {PacketId}; IPAddress: {GetIPAddress()}", LoggerType.Warning);
                Close(0, true);
            }
            else
            {
                this.FirstPacketId = PacketId;
            }
        }

        public bool CheckSeed(ushort PacketSeed, bool IsTheFirstPacket)
        {
            const int MAX_SEEK = 16;
            for (int i = 0; i < MAX_SEEK; i++)
            {
                if (GetNextSessionSeed() == PacketSeed)
                    return true;
            }
            CLogger.Print(
                $"Connection blocked. IP: {GetIPAddress()}; Date: {DateTimeUtil.Now()}; " +
                $"SessionId: {SessionId}; PacketSeed: {PacketSeed} / NextSessionSeed: {NextSessionSeed}; PrimarySeed: {SessionSeed}",
                LoggerType.Warning);
            return false;
        }

        private ushort GetNextSessionSeed()
        {
            NextSessionSeed = (ushort)((((NextSessionSeed * 214013) + 2531011) >> 16) & short.MaxValue);
            return NextSessionSeed;
        }

        private void ProcessPacket(ushort opcode, byte[] packetData, string direction)
        {
            try
            {
                if (!PacketLimiter.TryAllow())
                {
                    CLogger.Print(
                        $"[RateLimit] Game flood IP={GetIPAddress()} SessionId={SessionId} Opcode={opcode}",
                        LoggerType.Warning);
                    Close(0, true);
                    return;
                }

                GameClientPacket packet = null;

                switch (opcode)
                {
                    // Clan System packets
                    case 769: packet = new PROTOCOL_CS_CLIENT_ENTER_REQ(); break;
                    case 771: packet = new PROTOCOL_CS_CLIENT_LEAVE_REQ(); break;
                    case 800: packet = new PROTOCOL_CS_DETAIL_INFO_REQ(); break;
                    case 802: packet = new PROTOCOL_CS_MEMBER_CONTEXT_REQ(); break;
                    case 804: packet = new PROTOCOL_CS_MEMBER_LIST_REQ(); break;
                    case 806: packet = new PROTOCOL_CS_CREATE_CLAN_REQ(); break;
                    case 808: packet = new PROTOCOL_CS_CLOSE_CLAN_REQ(); break;
                    case 810: packet = new PROTOCOL_CS_CHECK_JOIN_AUTHORITY_ERQ(); break;
                    case 812: packet = new PROTOCOL_CS_JOIN_REQUEST_REQ(); break;
                    case 814: packet = new PROTOCOL_CS_CANCEL_REQUEST_REQ(); break;
                    case 816: packet = new PROTOCOL_CS_REQUEST_CONTEXT_REQ(); break;
                    case 818: packet = new PROTOCOL_CS_REQUEST_LIST_REQ(); break;
                    case 820: packet = new PROTOCOL_CS_REQUEST_INFO_REQ(); break;
                    case 822: packet = new PROTOCOL_CS_ACCEPT_REQUEST_REQ(); break;
                    case 825: packet = new PROTOCOL_CS_DENIAL_REQUEST_REQ(); break;
                    case 828: packet = new PROTOCOL_CS_SECESSION_CLAN_REQ(); break;
                    case 830: packet = new PROTOCOL_CS_DEPORTATION_REQ(); break;
                    case 833: packet = new PROTOCOL_CS_COMMISSION_MASTER_REQ(); break;
                    case 836: packet = new PROTOCOL_CS_COMMISSION_STAFF_REQ(); break;
                    case 839: packet = new PROTOCOL_CS_COMMISSION_REGULAR_REQ(); break;
                    case 854: packet = new PROTOCOL_CS_CHATTING_REQ(); break;
                    case 856: packet = new PROTOCOL_CS_CHECK_MARK_REQ(); break;
                    case 858: packet = new PROTOCOL_CS_REPLACE_NOTICE_REQ(); break;
                    case 860: packet = new PROTOCOL_CS_REPLACE_INTRO_REQ(); break;
                    case 868: packet = new PROTOCOL_CS_REPLACE_MANAGEMENT_REQ(); break;
                    case 877: packet = new PROTOCOL_CS_ROOM_INVITED_REQ(); break;
                    case 886: packet = new PROTOCOL_CS_PAGE_CHATTING_REQ(); break;
                    case 888: packet = new PROTOCOL_CS_INVITE_REQ(); break;
                    case 890: packet = new PROTOCOL_CS_INVITE_ACCEPT_REQ(); break;
                    case 892: packet = new PROTOCOL_CS_NOTE_REQ(); break;
                    case 916: packet = new PROTOCOL_CS_CREATE_CLAN_CONDITION_REQ(); break;
                    case 918: packet = new PROTOCOL_CS_CHECK_DUPLICATE_REQ(); break;
                    case 997: packet = new PROTOCOL_CS_CLAN_LIST_FILTER_REQ(); break;
                    case 999: packet = new PROTOCOL_CS_CLAN_LIST_FILTER_REQ(); break;
                    case 1001: packet = new PROTOCOL_CS_SIMPLE_CLAN_INFO_REQ(); break;

                    // Shop System
                    case 1025: packet = new PROTOCOL_SHOP_ENTER_REQ(); break;
                    case 1027: packet = new PROTOCOL_SHOP_LEAVE_REQ(); break;
                    case 1029: packet = new PROTOCOL_SHOP_GET_SAILLIST_REQ(); break;
                    case 1041: packet = new PROTOCOL_AUTH_SHOP_GET_GIFTLIST_REQ(); break;
                    case 1043: packet = new PROTOCOL_AUTH_SHOP_GET_GIFTLIST_REQ(); break;
                    case 1045: packet = new PROTOCOL_AUTH_SHOP_GOODS_BUY_REQ(); break;
                    case 1047: packet = new PROTOCOL_AUTH_SHOP_ITEM_AUTH_REQ(); break;
                    case 1048: packet = new PROTOCOL_AUTH_SHOP_GOODS_GIFT_REQ(); break;
                    case 1052: packet = new PROTOCOL_INVENTORY_USE_ITEM_REQ(); break;
                    case 1053:
                    case 1056: packet = new PROTOCOL_AUTH_SHOP_AUTH_GIFT_REQ(); break;
                    // 1055 = builds antigos; o client 122 envia Excluir Item como 1058 (0x422).
                    case 1055:
                    case 1058: packet = new PROTOCOL_AUTH_SHOP_DELETE_ITEM_REQ(); break;
                    case 1060: packet = new PROTOCOL_AUTH_GET_POINT_CASH_REQ(); break;
                    case 1061: packet = new PROTOCOL_AUTH_USE_ITEM_CHECK_NICK_REQ(); break;
                    case 1063: packet = new PROTOCOL_BASE_CHECK_NICK_REQ(); break;
                    case 1064: packet = new PROTOCOL_BASE_CHECK_NICK_REQ(); break;
                    case 1076: packet = new PROTOCOL_SHOP_REPAIR_REQ(); break;
                    // 1082 is what the 122 client actually sends for the shop "extend goods"
                    // purchase (CGameEventHandler__evtShop_BuyExtendGoods 0xD0E751 writes
                    // opcode 0x43A). 1075 is kept for older clients that used it.
                    case 1075:
                    case 1082: packet = new PROTOCOL_AUTH_SHOP_EXTEND_REQ(); break;
                    case 1091: packet = new PROTOCOL_AUTH_SHOP_USE_GIFTCOUPON_REQ(); break;
                    case 1087:
                    case 1094: packet = new PROTOCOL_AUTH_SHOP_ITEM_CHANGE_DATA_REQ(); break;
                    case 1097: packet = new PROTOCOL_SHOP_LIMITED_SALE_SYNC_REQ(); break;
                    case 1121: packet = new PROTOCOL_AUTH_SHOP_NAME_CARD_NICK_OUTLINE_COLOR_REQ(); break;
                    

                    // Friend System
                    case 1811: packet = new PROTOCOL_AUTH_FRIEND_INVITED_REQ(); break;
                    case 1816: packet = new PROTOCOL_AUTH_FRIEND_ACCEPT_REQ(); break;
                    case 1818: packet = new PROTOCOL_AUTH_FRIEND_INSERT_REQ(); break;
                    case 1820: packet = new PROTOCOL_AUTH_FRIEND_DELETE_REQ(); break;
                    case 1826: packet = new PROTOCOL_AUTH_RECV_WHISPER_REQ(); break;
                    case 1828: packet = new PROTOCOL_AUTH_SEND_WHISPER_REQ(); break;

                    // Messenger System
                    case 1921: packet = new PROTOCOL_MESSENGER_NOTE_SEND_REQ(); break;
                    case 1926: packet = new PROTOCOL_MESSENGER_NOTE_CHECK_READED_REQ(); break;
                    case 1928: packet = new PROTOCOL_MESSENGER_NOTE_DELETE_REQ(); break;
                    case 1930: packet = new PROTOCOL_MESSENGER_NOTE_RECEIVE_REQ(); break;

                    // Base System
                    case 2307: packet = new PROTOCOL_BASE_LOGOUT_REQ(); break;
                    case 2309: packet = new PROTOCOL_BASE_KEEP_ALIVE_REQ(); break;
                    case 2312: packet = new PROTOCOL_BASE_GAMEGUARD_REQ(); break;
                    case 2322: packet = new PROTOCOL_BASE_OPTION_SAVE_REQ(); break;
                    case 2326: packet = new PROTOCOL_BASE_CREATE_NICK_REQ(); break;
                    case 2328: packet = new PROTOCOL_BASE_USER_LEAVE_REQ(); break;
                    case 2330: packet = new PROTOCOL_BASE_USER_ENTER_REQ(); break;
                    case 2332: packet = new PROTOCOL_BASE_GET_CHANNELLIST_REQ(); break;
                    case 2334: packet = new PROTOCOL_BASE_SELECT_CHANNEL_REQ(); break;
                    case 2336: packet = new PROTOCOL_BASE_ATTENDANCE_REQ(); break;
                    case 2338: packet = new PROTOCOL_BASE_ATTENDANCE_CLEAR_ITEM_REQ(); break;
                    case 2350: packet = new PROTOCOL_BASE_GET_RECORD_INFO_DB_REQ(); break;
                    case 2360: packet = new PROTOCOL_BASE_QUEST_ACTIVE_IDX_CHANGE_REQ(); break;
                    case 2364: packet = new PROTOCOL_BASE_QUEST_BUY_CARD_SET_REQ(); break;
                    case 2366: packet = new PROTOCOL_BASE_QUEST_DELETE_CARD_SET_REQ(); break;
                    case 8709: packet = new PROTOCOL_BASE_QUEST_LIST_REQ(); break;
                    case 8705: packet = new PROTOCOL_BASE_QUEST_ACCEPT_REQ(); break;
                    case 8707: packet = new PROTOCOL_BASE_QUEST_ABANDON_REQ(); break;
                    case 2376: packet = new PROTOCOL_BASE_USER_TITLE_CHANGE_REQ(); break;
                    case 2378: packet = new PROTOCOL_BASE_USER_TITLE_EQUIP_REQ(); break;
                    case 2380: packet = new PROTOCOL_BASE_USER_TITLE_RELEASE_REQ(); break;
                    case 2384: packet = new PROTOCOL_BASE_CHATTING_REQ(); break;
                    case 2399: packet = new PROTOCOL_BASE_GAME_SERVER_STATE_REQ(); break;
                    case 2401: packet = new PROTOCOL_BASE_ENTER_PASS_REQ(); break;
                    case 2414: packet = new PROTOCOL_BASE_DAILY_RECORD_REQ(); break;
                    case 2422: packet = new PROTOCOL_BASE_GET_USER_DETAIL_INFO_REQ(); break;
                    case 2424: packet = new PROTOCOL_BASE_GET_ROOM_USER_DETAIL_INFO_REQ(); break;
                    case 2425: packet = new PROTOCOL_BASE_GET_USER_BASIC_INFO_REQ(); break;
                    case 2426: packet = new PROTOCOL_AUTH_FIND_USER_REQ(); break;
                    case 2447: packet = new PROTOCOL_BASE_GET_USER_SUBTASK_REQ(); break;
                    case 2465: packet = new PROTOCOL_BASE_URL_LIST_REQ(); break;
                    case 2489: packet = new PROTOCOL_BASE_CHANNELTYPE_CHANGE_CONDITION_REQ(); break;
                    case 2491: packet = new PROTOCOL_LOBBY_NEW_MYINFO_REQ(); break;
                    case 2500: packet = new PROTOCOL_BASE_RANDOMBOX_LIST_REQ(); break;
                    case 2502: packet = new PROTOCOL_BASE_WEBTOOL_SETBOX_LIST_REQ(); break;
                    case 2508: packet = new PROTOCOL_BASE_TICKET_UPDATE_REQ(); break;
                    case 2514: packet = new PROTOCOL_BASE_EVENT_PORTAL_REQ(); break;

                    // Lobby System
                    case 2561: packet = new PROTOCOL_LOBBY_LEAVE_REQ(); break;
                    case 2565: packet = new PROTOCOL_LOBBY_STAGE_RULE_REQ(); break;
                    case 2567: packet = new PROTOCOL_LOBBY_GET_ROOMINFOADD_REQ(); break;
                    case 2583: packet = new PROTOCOL_LOBBY_ENTER_REQ(); break;
                    case 2587: packet = new PROTOCOL_LOBBY_GET_ROOMLIST_REQ(); break;
                    case 3083: packet = new PROTOCOL_LOBBY_GET_ROOMINFOADD_REQ(); break;
                    case 3329: packet = new PROTOCOL_INVENTORY_ENTER_REQ(); break;
                    case 3331: packet = new PROTOCOL_INVENTORY_LEAVE_REQ(); break;
                    case 3333: packet = new PROTOCOL_INVENTORY_UPGRADE_REQ(); break;
                    case 3585: packet = new PROTOCOL_ROOM_JOIN_REQ(); break;
                    case 3592: packet = new PROTOCOL_ROOM_CREATE_REQ(); break;
                    case 3596: packet = new PROTOCOL_ROOM_GET_PLAYERINFO_REQ(); break;
                    case 3602: packet = new PROTOCOL_ROOM_CHANGE_PASSWD_REQ(); break;
                    case 3604: packet = new PROTOCOL_ROOM_CHANGE_SLOT_REQ(); break;
                    case 3609: packet = new PROTOCOL_ROOM_PERSONAL_TEAM_CHANGE_REQ(); break;
                    case 3611: packet = new PROTOCOL_ROOM_REQUEST_MAIN_REQ(); break;
                    case 3613: packet = new RandomHostChangePacket(); break;
                    case 3615: packet = new PROTOCOL_ROOM_REQUEST_MAIN_CHANGE_WHO_REQ(); break;
                    case 3617: packet = new CheckRandomHostPacket(); break;
                    case 3619: packet = new PROTOCOL_ROOM_TOTAL_TEAM_CHANGE_REQ(); break;
                    case 3631: packet = new PROTOCOL_ROOM_GET_LOBBY_USER_LIST_REQ(); break;
                    case 3636: packet = new PROTOCOL_ROOM_CHANGE_ROOM_OPTIONINFO_REQ(); break;
                    case 3639:
                    case 3640: packet = new PROTOCOL_ROOM_CHANGE_ROOMINFO_REQ(); break;
                    case 3643: packet = new PROTOCOL_GM_KICK_COMMAND_REQ(); break;
                    case 3647: packet = new PROTOCOL_GM_EXIT_COMMAND_REQ(); break;
                    case 3657:
                    case 3658: packet = new PROTOCOL_ROOM_LOADING_START_REQ(); break;
                    case 3665:
                    case 3666: packet = new PROTOCOL_ROOM_GET_USER_EQUIPMENT_REQ(); break;
                    case 3672: packet = new PROTOCOL_ROOM_INFO_ENTER_REQ(); break;
                    case 3674: packet = new PROTOCOL_ROOM_INFO_LEAVE_REQ(); break;
                    case 3675:
                    case 3676: packet = new PROTOCOL_ROOM_INVITE_LOBBY_USER_LIST_REQ(); break;
                    case 3677:
                    case 3678: packet = new PROTOCOL_ROOM_CHANGE_COSTUME_REQ(); break;
                    case 3679:
                    case 3683: packet = new PROTOCOL_ROOM_SELECT_SLOT_CHANGE_REQ(); break;
                    case 3680:
                    case 3681: packet = new PROTOCOL_ROOM_GET_ACEMODE_PLAYERINFO_REQ(); break;
                    case 3682: packet = new PROTOCOL_ROOM_SELECT_SLOT_CHANGE_REQ(); break;
                    case 3650:
                    case 3686: packet = new PROTOCOL_ROOM_CHANGE_OBSERVER_SLOT_REQ(); break;
                    case 3850: packet = new PROTOCOL_COMMUNITY_USER_REPORT_REQ(); break;
                    case 3852: packet = new PROTOCOL_COMMUNITY_USER_REPORT_CONDITION_CHECK_REQ(); break;
                    case 3854: packet = new PROTOCOL_COMMUNITY_USER_LIST_REQ(); break;
                    case 3856: packet = new PROTOCOL_COMMUNITY_USER_LIST_EX_REQ(); break;

                    // Battle System
                    case 5123: packet = new PROTOCOL_BATTLE_READYBATTLE_REQ(); break;
                    case 5129: packet = new PROTOCOL_BATTLE_PRESTARTBATTLE_REQ(); break;
                    case 5131: packet = new PROTOCOL_BATTLE_STARTBATTLE_REQ(); break;
                    case 5133: packet = new PROTOCOL_BATTLE_GIVEUPBATTLE_REQ(); break;
                    case 5135: packet = new PROTOCOL_BATTLE_DEATH_REQ(); break;
                    case 5137: packet = new PROTOCOL_BATTLE_RESPAWN_REQ(); break;
                    case 5146: packet = new PROTOCOL_BATTLE_SENDPING_REQ(); break;
                    case 5156: packet = new PROTOCOL_BATTLE_MISSION_BOMB_INSTALL_REQ(); break;
                    case 5158: packet = new PROTOCOL_BATTLE_MISSION_BOMB_UNINSTALL_REQ(); break;
                    case 5166: packet = new PROTOCOL_BATTLE_MISSION_GENERATOR_INFO_REQ(); break;
                    case 5168: packet = new PROTOCOL_BATTLE_TIMERSYNC_REQ(); break;
                    case 5172: packet = new PROTOCOL_BATTLE_CHANGE_DIFFICULTY_LEVEL_REQ(); break;
                    case 5174: packet = new PROTOCOL_BATTLE_RESPAWN_FOR_AI_REQ(); break;
                    case 5180: packet = new PROTOCOL_BATTLE_MISSION_DEFENCE_INFO_REQ(); break;
                    case 5182: packet = new PROTOCOL_BATTLE_MISSION_TOUCHDOWN_COUNT_REQ(); break;
                    case 5184: packet = new PROTOCOL_BATTLE_MISSION_TOUCHDOWN_COUNT_REQ(); break;
                    case 5188: packet = new PROTOCOL_BATTLE_MISSION_TUTORIAL_ROUND_END_REQ(); break;
                    case 5262: packet = new PROTOCOL_BATTLE_NEW_JOIN_ROOM_SCORE_REQ(); break;
                    case 5276: packet = new PROTOCOL_BATTLE_USER_SOPETYPE_REQ(); break;
                    case 5377: packet = new PROTOCOL_LOBBY_QUICKJOIN_ROOM_REQ(); break;

                    // Character System
                    case 6145: packet = new PROTOCOL_CHAR_CREATE_CHARA_REQ(); break;
                    case 6149: packet = new PROTOCOL_CHAR_CHANGE_EQUIP_REQ(); break;
                    case 1050: packet = new PROTOCOL_CHAR_CHANGE_EQUIP_SINGLE_REQ(); break;
                    case 6151: packet = new PROTOCOL_CHAR_DELETE_CHARA_REQ(); break;

                    case 6657: packet = new PROTOCOL_GMCHAT_START_CHAT_REQ(); break;
                    case 6661: packet = new PROTOCOL_GMCHAT_END_CHAT_REQ(); break;
                    case 6663: packet = new PROTOCOL_GMCHAT_APPLY_PENALTY_REQ(); break;
                    case 6667: packet = new PROTOCOL_GMCHAT_APPLY_PENALTY_MULTI_REQ(); break;

                    // Clan War (category 0x1B). These are the C2S opcodes the 122
                    // client can send, per the UI packet-prototype factory at
                    // 0xD14CF4 — see docs/re/CLANWAR_122_CONTRACT.md.
                    // Provably unsendable by the 122 client (no prototype anywhere):
                    // 6914 (MATCH_TEAM_COUNT_REQ) and 6928 (TEAM_CHATTING_REQ).
                    // Deliberately not served: 6950 (MERCENARY_DETAIL_INFO_REQ) —
                    // its ACK 6951 embeds S2_USER_DETAIL_INFO, 2319 bytes of layout
                    // we have not mapped, and a wrong body would corrupt the read;
                    // and 6974 (MERCENARY_RECORD_REQ) — its ACK 6975 is parsed at
                    // 0xEF10F8 and discarded without any UI event, so answering it
                    // changes nothing on screen.
                    case 6916: packet = new PROTOCOL_CLAN_WAR_MATCH_TEAM_LIST_REQ(); break;
                    case 6918: packet = new PROTOCOL_CLAN_WAR_CREATE_TEAM_REQ(); break;
                    case 6920: packet = new PROTOCOL_CLAN_WAR_JOIN_TEAM_REQ(); break;
                    case 6922: packet = new PROTOCOL_CLAN_WAR_LEAVE_TEAM_REQ(); break;
                    case 6924: packet = new PROTOCOL_CLAN_WAR_CHANGE_OPERATION_REQ(); break;
                    case 6926: packet = new PROTOCOL_CLAN_WAR_CHANGE_MAX_PER_REQ(); break;
                    case 6932: packet = new PROTOCOL_CLAN_WAR_MATCHMAKING_REQ(); break;
                    case 6934: packet = new PROTOCOL_CLAN_WAR_CANCEL_MATCHMAKING_REQ(); break;
                    case 6936: packet = new PROTOCOL_CLAN_WAR_MERCENARY_LIST_REQ(); break;
                    case 6938: packet = new PROTOCOL_CLAN_WAR_REGIST_MERCENARY_REQ(); break;
                    case 6940: packet = new PROTOCOL_CLAN_WAR_REMOVE_MERCENARY_REQ(); break;
                    case 6942: packet = new PROTOCOL_CLAN_WAR_INVITE_MERCENARY_REQ(); break;
                    case 6946: packet = new PROTOCOL_CLAN_WAR_INVITE_ACCEPT_REQ(); break;
                    case 6948: packet = new PROTOCOL_CLAN_WAR_INVITE_DENIAL_REQ(); break;
                    case 6965: packet = new PROTOCOL_CLAN_WAR_RESULT_REQ(); break;
                    case 7429: packet = new PROTOCOL_BATTLEBOX_AUTH_REQ(); break;
                    case 7681: packet = new PROTOCOL_MATCH_SERVER_IDX_REQ(); break;
                    case 7699: packet = new PROTOCOL_MATCH_CLAN_SEASON_REQ(); break;

                    // Season System
                    case 8449: packet = new PROTOCOL_SEASON_CHALLENGE_INFO_REQ(); break;
                    case 8453:
                    case 8454: packet = new PROTOCOL_SEASON_CHALLENGE_BUY_SEASON_PASS_REQ(); break;
                    case 2392: packet = new PROTOCOL_2392_UNKNOWN_PACKET_REQ(); break;
                    case 2518: packet = new PROTOCOL_2518_UNKNOWN_PACKET_REQ(); break;
                    case 2478: packet = new PROTOCOL_2478_UNKNOWN_PACKET_REQ(); break;
                    case 3635: packet = new PROTOCOL_BASE_UNKNOWN_3635_REQ(); break;
                    case 5145: break;
                    case 5291: packet = new PROTOCOL_REPORT_MACHINE_SPEC_REQ(); break;
                    case 5292: packet = new PROTOCOL_BASE_UNKNOWN_PACKET_REQ(); break;

                    default:
                        CLogger.Emit(new LogEvent { Level = LogLevel.Info, Cat = LogCat.Opcode, Srv = ServerKind.Game, Dir = Direction.In, Op = opcode, Len = packetData?.Length ?? 0, Conn = ConnRegistry.IdFor(Client?.RemoteEndPoint?.ToString()), Hex = packetData != null ? BitConverter.ToString(packetData) : null });
                        break;
                }

                if (packet == null)
                    return;

                using (packet)
                {
                    CLogger.Packet(ServerKind.Game, Direction.In, opcode, packet.GetType().Name, ConnRegistry.IdFor(Client?.RemoteEndPoint?.ToString()), packetData?.Length ?? 0, packetData);

                    packet.Makeme(this, packetData);
                    CLogger.QueueWork(ServerKind.Game, () =>
                    {
                        try
                        {
                            packet.Run();
                        }
                        catch (Exception ex)
                        {
                            CLogger.Print($"Error running packet {packet.GetType().Name}: {ex.Message}", LoggerType.Error, ex);
                        }
                    });
                    packet.Dispose();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
