using Microsoft.Win32.SafeHandles;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Logging;
using Plugin.Core.Models;
using Plugin.Core.Security;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Auth.Data.Models;
using Server.Auth.Data.Sync.Server;
using Server.Auth.Network;
using Server.Auth.Network.ClientPacket;
using Server.Auth.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using Utility;

namespace Server.Auth
{
    public class AuthClient : IDisposable
    {
        public int ServerId;
        public Socket Client;
        public Account Player;
        public DateTime SessionDate;
        public int SessionId;
        public ushort SessionSeed;
        public int FirstPacketId;
        public int SessionShift;
        private ushort _nextSessionSeed;
        public readonly byte[] ServerKey = new byte[16];
        public readonly byte[] ClientKey = new byte[16];
        private volatile bool _keysReady = false;
        private bool _connectionClosed = false;
        private bool _disposed = false;
        private bool _mapInfoSent = false;
        private readonly object _mapInfoLock = new object();
        public bool ItemGroupSent = false;
        private int _failedSeedAttempts = 0;
        private const int MaxFailedAttempts = 3;
        private DateTime _lastActivity = DateTime.Now;
        private readonly object _receiveLock = new object();
        private byte[] _receiveRemainder = new byte[0];
        private readonly PacketRateLimiter _packetLimiter = new PacketRateLimiter(120);

        public AuthClient(int ServerId, Socket Client)
        {
            this.ServerId = ServerId;
            this.Client = Client;
        }

        public void Dispose()
        {
            try { Dispose(true); GC.SuppressFinalize(this); }
            catch (Exception ex) { CLogger.Print(ex.Message, LoggerType.Error, ex); }
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (_disposed) return;
                Player = null;
                if (Client != null) { Client.Dispose(); Client = null; }
                _disposed = true;
            }
            catch (Exception ex) { CLogger.Print(ex.Message, LoggerType.Error, ex); }
        }

        public Account GetAccount()
        {
            try
            {
                if (Player != null) return Player;
                if (ConfigLoader.DebugMode)
                    CLogger.Print($"AuthClient.GetAccount: Account is null. IP: {GetIPAddress()}", LoggerType.Warning);
                return null;
            }
            catch (Exception ex) { CLogger.Print(ex.Message, LoggerType.Error, ex); return null; }
        }

        public string GetIPAddress()
        {
            try { return Client?.RemoteEndPoint is IPEndPoint ep ? ep.Address.ToString() : ""; }
            catch { return ""; }
        }

        public IPAddress GetAddress()
        {
            try { return Client?.RemoteEndPoint is IPEndPoint ep ? ep.Address : null; }
            catch { return null; }
        }

        public void StartSession()
        {
            try
            {
                _nextSessionSeed = SessionSeed;
                SessionShift = ((SessionId + Bitwise.CRYPTO[0]) % 7 + 1);

                // ACK tem que ir pro fio ANTES do receive/timeout em outra thread —
                // QueueWork+BeginSend perdia a corrida (logava CONNECT_ACK e mandava FIN sem payload).
                SendConnectAck();
                CLogger.QueueWork(ServerKind.Auth, StartReceive);
                CLogger.QueueWork(ServerKind.Auth, CheckConnectionTimeout);
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                Close(0, true);
            }
        }


        private void CheckConnectionTimeout()
        {
            Thread.Sleep(30000);
            // A peer that already hung up is not a stalled client: port probes (the panel status
            // poll) connect and drop instantly, and warning on those floods the console.
            if (_connectionClosed || Client == null) return;
            if (FirstPacketId == 0)
            {
                if (ConfigLoader.DebugMode)
                    CLogger.Print($"Conexión destruida por timeout inicial. IP: {GetIPAddress()}", LoggerType.Warning);
                Close(0, true);
            }
        }

        public void HeartBeatCounter()
        {
            CLogger.QueueWork(ServerKind.Auth, () =>
            {
                try
                {
                    while (!_connectionClosed)
                    {
                        Thread.Sleep(15000);
                        if (_connectionClosed) break;
                        if ((DateTime.Now - _lastActivity).TotalMinutes >= 10)
                        {
                            if (ConfigLoader.IsTestMode)
                                CLogger.Print($"Connection closed (idle timeout, 10 min). IP: {GetIPAddress()}", LoggerType.Debug);
                            Close(0, true);
                            break;
                        }
                    }
                }
                catch { Close(0, true); }
            });
        }

        private void SendConnectAck()
        {
            SendPacketPlain(new PROTOCOL_BASE_CONNECT_ACK(this));

        }

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
                ushort Opcode = BitConverter.ToUInt16(Result, 2);
                CLogger.Packet(ServerKind.Auth, Direction.Out, Opcode, PacketName,
                    ConnRegistry.IdFor(Client?.RemoteEndPoint?.ToString()), Result.Length, Result);
                if (Result.Length > 0)
                {
                    byte[] FinalResult = Result;
                    Client.BeginSend(FinalResult, 0, FinalResult.Length, SocketFlags.None, new AsyncCallback(SendCallback), Client);
                }
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

                ushort Opcode = BitConverter.ToUInt16(packetData, 2);
                CLogger.Packet(ServerKind.Auth, Direction.Out, Opcode, PacketName,
                    ConnRegistry.IdFor(Client?.RemoteEndPoint?.ToString()), originalData.Length, originalData);

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

        public void SendPacket(AuthServerPacket Packet)
        {
            try
            {
                byte[] originalData = Packet.GetBytes("AuthClient.SendPacket");
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

                ushort Opcode = BitConverter.ToUInt16(packetData, 2);
                CLogger.Packet(ServerKind.Auth, Direction.Out, Opcode, Packet.GetType().Name,
                    ConnRegistry.IdFor(Client?.RemoteEndPoint?.ToString()), originalData.Length, originalData, Packet.Schema);

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

        public void SendMapInfoOnce()
        {
            lock (_mapInfoLock)
            {
                if (_mapInfoSent) return;
                _mapInfoSent = true;
            }
            try
            {
                SendPacket(new PROTOCOL_BASE_MAP_RULELIST_ACK());
                foreach (IEnumerable<MapMatch> source in SystemMapXML.Matches.Split<MapMatch>(100))
                {
                    List<MapMatch> list = source.ToList<MapMatch>();
                    if (list.Count > 0)
                        SendPacket(new PROTOCOL_BASE_MAP_MATCHINGLIST_ACK(list, list.Count));
                }
            }
            catch (Exception ex)
            {
                _mapInfoSent = false;
                CLogger.Print("SendMapInfoOnce: " + ex.Message, LoggerType.Error, ex);
            }
        }
        private void SendPacketPlain(AuthServerPacket Packet)
        {
            try
            {
                byte[] payload = Packet.GetBytes("AuthClient.SendPacketPlain");
                if (payload == null || payload.Length < 2) { Packet.Dispose(); return; }

                byte[] sizePrefix = BitConverter.GetBytes((ushort)(payload.Length + 2));
                byte[] frame = new byte[sizePrefix.Length + payload.Length + 5];
                Buffer.BlockCopy(sizePrefix, 0, frame, 0, sizePrefix.Length);
                Buffer.BlockCopy(payload, 0, frame, sizePrefix.Length, payload.Length);

                ushort opcode = BitConverter.ToUInt16(payload, 0);
                CLogger.Packet(ServerKind.Auth, Direction.Out, opcode, Packet.GetType().Name, ConnRegistry.IdFor(Client?.RemoteEndPoint?.ToString()), payload.Length, payload, Packet.Schema);

                // CONNECT_ACK (e demais plain): Send bloqueante pra garantir bytes no TCP
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
                if (Result.AsyncState is Socket s && s.Connected)
                    s.EndSend(Result);
            }
            catch { Close(0, true); }
        }

        private void StartReceive()
        {
            try
            {
                StateObject state = new StateObject
                {
                    WorkSocket = Client,
                    Buffer = new byte[StateObject.BufferSize]
                };
                Client.BeginReceive(state.Buffer, 0, StateObject.BufferSize,
                    SocketFlags.None, new AsyncCallback(OnReceiveCallback), state);
            }
            catch { Close(0, true); }
        }

        private void OnReceiveCallback(IAsyncResult Result)
        {
            CLogger.SetThreadKind(ServerKind.Auth);
            StateObject state = Result.AsyncState as StateObject;
            try
            {
                _lastActivity = DateTime.Now;
                int bytesCount = state.WorkSocket.EndReceive(Result);
                if (bytesCount <= 0)
                {
                    if (FirstPacketId == 0)
                        CLogger.Print($"[DIAG] client fechou antes do 1o pacote. IP: {GetIPAddress()}", LoggerType.Warning);
                    Close(0, true);
                    return;
                }
                if (FirstPacketId == 0)
                    CLogger.Print($"[DIAG] 1o receive: {bytesCount}B hw=0x{(bytesCount >= 2 ? (state.Buffer[0] | (state.Buffer[1] << 8)) : 0):X4} IP: {GetIPAddress()}", LoggerType.Warning);
                byte[] rawBuffer;
                lock (_receiveLock)
                {
                    rawBuffer = new byte[_receiveRemainder.Length + bytesCount];
                    Buffer.BlockCopy(_receiveRemainder, 0, rawBuffer, 0, _receiveRemainder.Length);
                    Buffer.BlockCopy(state.Buffer, 0, rawBuffer, _receiveRemainder.Length, bytesCount);
                    _receiveRemainder = new byte[0];
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
                    if (offset + frameLength > bytesCount) break;
                    if (!enc && FirstPacketId == 0)
                        CLogger.Print($"[DIAG] frame NAO cifrado hw=0x{hw:X4} payloadLen={payloadLen} IP: {GetIPAddress()}", LoggerType.Warning);
                    if (enc)
                    {
                        byte[] cmess = new byte[payloadLen];
                        Array.Copy(rawBuffer, offset + 2, cmess, 0, payloadLen);
                        var (decrypted, ok) = CMessCipher.Decrypt(ClientKey, cmess, CMessCipher.MODE_AUTH);
                        if (!ok || decrypted == null || decrypted.Length < 4)
                        {
                            CLogger.Print($"[DIAG] decrypt FALHOU payloadLen={payloadLen} ok={ok} out={(decrypted == null ? -1 : decrypted.Length)} IP: {GetIPAddress()}", LoggerType.Warning);
                            Close(0, true);
                            return;
                        }
                        ushort packetId = BitConverter.ToUInt16(decrypted, 0);
                        ushort packetSeed = BitConverter.ToUInt16(decrypted, 2);
                        FirstPacketCheck(packetId);
                        if (_connectionClosed) return;
                        if (!CheckSeed(packetSeed, isFirstInBuffer))
                        {
                            if (_connectionClosed) return;
                            offset += frameLength;
                            isFirstInBuffer = false;
                            continue;
                        }

                        RunPacket(packetId, decrypted, "REQ");
                    }

                    offset += frameLength;
                    isFirstInBuffer = false;
                }
                if (offset < bytesCount)
                {
                    lock (_receiveLock)
                    {
                        _receiveRemainder = new byte[bytesCount - offset]; // fix respawn: retain a TCP fragment for the next receive
                        Buffer.BlockCopy(rawBuffer, offset, _receiveRemainder, 0, _receiveRemainder.Length);
                    }
                }
            }
            catch { Close(0, true); }
            finally
            {
                if (!_connectionClosed)
                {
                    CLogger.QueueWork(ServerKind.Auth, () =>
                    {
                        try { StartReceive(); }
                        catch (Exception ex)
                        {
                            CLogger.Print($"Failed to resume receive: {ex.Message}", LoggerType.Error, ex);
                            Close(0, true);
                        }
                    });
                }
            }
        }


        private void FirstPacketCheck(ushort packetId)
        {
            if (FirstPacketId != 0) return;
            if (packetId != 1281 && packetId != 2309)
            {
                if (ConfigLoader.DebugMode)
                    CLogger.Print($"Invalid first packet. Opcode: {packetId}; IP: {GetIPAddress()}", LoggerType.Warning);
                Close(0, true);
            }
            else
            {
                FirstPacketId = packetId;
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
                $"SessionId: {SessionId}; PacketSeed: {PacketSeed} / NextSessionSeed: {_nextSessionSeed}; PrimarySeed: {SessionSeed}",
                LoggerType.Warning);
            return false;
        }

        private ushort GetNextSessionSeed()
        {
            _nextSessionSeed = (ushort)((((_nextSessionSeed * 214013) + 2531011) >> 16) & 0x7FFF);
            return _nextSessionSeed;
        }


        private void RunPacket(ushort Opcode, byte[] Buffer, string Value)
        {
            try
            {
                if (!_packetLimiter.TryAllow())
                {
                    CLogger.Print(
                        $"[RateLimit] Auth flood IP={GetIPAddress()} SessionId={SessionId} Opcode={Opcode}",
                        LoggerType.Warning);
                    Close(0, true);
                    return;
                }

                AuthClientPacket packet = null;
                switch (Opcode)
                {
                    case 1060:
                        packet = new PROTOCOL_AUTH_GET_POINT_CASH_REQ();
                        break;

                    case 1127:
                        packet = new PROTOCOL_SHOP_ITEMGROUP_INFO_REQ();
                        break;

                    case 1059:
                        packet = new PROTOCOL_BASE_GAME_SERVER_STATE_REQ();
                        break;

                    case 1281:
                        packet = new PROTOCOL_BASE_LOGIN_REQ();
                        break;

                    case 2307:
                        packet = new PROTOCOL_BASE_LOGOUT_REQ();
                        break;

                    case 2309:
                        packet = new PROTOCOL_BASE_KEEP_ALIVE_REQ();
                        break;

                    case 2312:
                        packet = new PROTOCOL_BASE_GAMEGUARD_REQ();
                        break;

                    case 2314:
                        packet = new PROTOCOL_BASE_GET_SYSTEM_INFO_REQ();
                        break;

                    case 2316:
                        packet = new PROTOCOL_BASE_GET_USER_INFO_REQ();
                        break;

                    case 2318:
                        packet = new PROTOCOL_BASE_GET_INVEN_INFO_REQ();
                        break;

                    case 2320:
                        packet = new PROTOCOL_BASE_GET_OPTION_REQ();
                        break;

                    case 2322:
                        packet = new PROTOCOL_BASE_OPTION_SAVE_REQ();
                        break;

                    case 2328:
                        packet = new PROTOCOL_BASE_USER_LEAVE_REQ();
                        break;

                    case 2332:
                        packet = new PROTOCOL_BASE_GET_CHANNELLIST_REQ();
                        break;

                    case 2399:
                        packet = new PROTOCOL_BASE_GAME_SERVER_STATE_REQ();
                        break;

                    case 2414:
                        packet = new PROTOCOL_BASE_DAILY_RECORD_REQ();
                        break;

                    case 2459:
                        packet = new PROTOCOL_BASE_GET_MAP_INFO_REQ();
                        break;

                    case 2489:
                        packet = new PROTOCOL_BASE_CHANNELTYPE_CHANGE_CONDITION_REQ();
                        break;

                    case 2520:
                        packet = new PROTOCOL_BASE_MISSION_CARD_INFO_STREAM_REQ();
                        break;

                    case 8709:
                        packet = new PROTOCOL_BASE_QUEST_LIST_REQ();
                        break;

                    case 7681:
                        packet = new PROTOCOL_MATCH_SERVER_IDX_REQ();
                        break;

                    case 7697:
                        packet = new PROTOCOL_MATCH_SEASON_REQ();
                        break;

                    case 7699:
                        packet = new PROTOCOL_MATCH_CLAN_SEASON_REQ();
                        break;

                    default:
                        CLogger.Emit(new LogEvent {
                            Level = LogLevel.Info, Cat = LogCat.Opcode,
                            Srv = ServerKind.Auth, Dir = Direction.In,
                            Op = Opcode, Len = Buffer?.Length ?? 0,
                            Conn = ConnRegistry.IdFor(Client?.RemoteEndPoint?.ToString()),
                            Hex = Buffer != null ? BitConverter.ToString(Buffer) : null });
                        break;
                }

                if (packet == null) return;

                using (packet)
                {
                    CLogger.Packet(ServerKind.Auth, Direction.In, Opcode, packet.GetType().Name,
                        ConnRegistry.IdFor(Client?.RemoteEndPoint?.ToString()), Buffer?.Length ?? 0, Buffer);

                    packet.Makeme(this, Buffer);

                    CLogger.QueueWork(ServerKind.Auth, () =>
                    {
                        try { packet.Run(); }
                        catch (Exception ex) { CLogger.Print(ex.Message, LoggerType.Error, ex); Close(50, true); }
                    });

                    packet.Dispose();
                    Buffer = null;
                }
            }
            catch (Exception ex) { CLogger.Print(ex.Message, LoggerType.Error, ex); }
        }

        public void Close(int TimeMS, bool DestroyConnection)
        {
            if (_connectionClosed) return;
            try
            {
                _connectionClosed = true;
                string ip = GetIPAddress();
                AuthXender.Client.RemoveSession(this);
                AuthXender.Client.RemoveConnection(ip);

                Account cache = Player;

                if (DestroyConnection)
                {
                    if (cache != null)
                    {
                        cache.SetOnlineStatus(false);
                        // FL GUARD: sessão acabou, token do launcher morre junto
                        if (ConfigLoader.RequireOtpToken)
                            Plugin.Core.Security.SecurityDao.InvalidateToken(cache.PlayerId);
                        if (cache.Status.ServerId == 0)
                            SendRefresh.RefreshAccount(cache, false);
                        cache.Status.ResetData(cache.PlayerId);
                        cache.SimpleClear();
                        cache.UpdateCacheInfo();
                        Player = null;
                    }
                    Client?.Close(TimeMS);
                    Thread.Sleep(TimeMS);
                    Dispose();
                }
                else if (cache != null)
                {
                    cache.SimpleClear();
                    cache.UpdateCacheInfo();
                    Player = null;
                }

                UpdateServer.RefreshSChannel(ServerId);
            }
            catch (Exception ex) { CLogger.Print($"AuthClient.Close: {ex.Message}", LoggerType.Error, ex); }
        }
    }
}
