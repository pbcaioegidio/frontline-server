using Executable.UDP.Client;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Executable.UDP
{
    public static class Communication
    {
        public static UdpClient Client;
        public static bool Start(IPEndPoint Conn)
        {
            try
            {
                Client = new UdpClient(Conn);
                uint IOC_IN = 0x80000000, IOC_VENDOR = 0x18000000, SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                Client.Client.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
                ThreadPool.QueueUserWorkItem(_ => Read());
                CLogger.Print($"Endereco Comunicacao {Conn.Address}:{Conn.Port}", LoggerType.Info);
                return true;
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
                return false;
            }
        }
        private static void Read()
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
        // The receive must be issued from a pool thread and re-armed in a finally, the same shape
        // GameSync uses: a dedicated thread that exits right after BeginReceive gets its pending
        // overlapped read cancelled, so the supervisor stops receiving after one datagram and
        // every later reload broadcast is silently dropped.
        private static void AcceptCallback(IAsyncResult Result)
        {
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
        private static void LoadPacket(byte[] buffer)
        {
            try
            {
                SyncClientPacket C = new SyncClientPacket(buffer);
                short Opcode = C.ReadH();
                switch (Opcode)
                {
                    case 26: ServiceControl.Load(C); break;
                    case 27: Server.SupervisorCommands.BroadcastReload(C.ReadC()); break;
                    case 7171: MessageStatus.Load(C); break;
                    default: CLogger.Print(Bitwise.ToHexData($"Communication - Opcode Not Found: [{Opcode}]", C.ToArray()), LoggerType.Opcode); break;
                }
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }
        public static void SendPacket(byte[] Data, IPEndPoint Address)
        {
            Client.Send(Data, Data.Length, Address);
        }
    }
}
