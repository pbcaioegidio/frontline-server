// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BASE_EVENT_PORTAL_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.XML;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_EVENT_PORTAL_ACK : GameServerPacket
    {
        private readonly bool Field0;

        public PROTOCOL_BASE_EVENT_PORTAL_ACK(bool A_1) => this.Field0 = A_1;


        // El cliente (Base__ParseEventPortal, 0xEBEC01) lee este DWORD como la cantidad de
        // bytes del blob y los copia a un buffer fijo de 0x1FFF; aborta con
        // "buffersize < maxsize" si lo declarado lo alcanza. Mandar la constante 8192 hacía
        // que leyera 8192 bytes de un paquete mucho más corto.
        private const int MaxBlobSize = 0x1FFE;

        public override void Write()
        {
            List<byte[]> blocks = new List<byte[]>();
            int blobSize = 1; // el primer byte del blob es el contador de eventos
            foreach (KeyValuePair<string, PortalEvents> allEvent in PortalManager.Build())
            {
                byte[] block = BuildBlock(allEvent.Key, allEvent.Value);
                if (block == null)
                    continue;
                if (blobSize + block.Length > MaxBlobSize)
                {
                    CLogger.Print($"[EventPortal] {allEvent.Key} omitted: the blob would exceed {MaxBlobSize} bytes.", LoggerType.Warning);
                    break;
                }
                blocks.Add(block);
                blobSize += block.Length;
            }

            this.WriteH((short)2515);
            this.WriteC(this.Field0 ? (byte)1 : (byte)0);
            this.WriteC((byte)1);
            this.WriteD(blobSize);
            this.WriteC((byte)blocks.Count);
            foreach (byte[] block in blocks)
                this.WriteB(block);
        }

        private static byte[] BuildBlock(string key, PortalEvents portal)
        {
            int id = PortalManager.GetInitialId(key);
            if (key.Contains("Boost") && portal == PortalEvents.BoostEvent)
            {
                EventBoostModel Boost = EventBoostXML.GetEvent(id);
                if (Boost == null)
                    return null;
                uint[] DateTime = new uint[2] { Boost.BeginDate, Boost.EndedDate };
                string[] Info = new string[3] { Boost.Name, Boost.Subtitle, Boost.Description };
                byte[] Type = new byte[2] { Boost.Period ? (byte)0 : (byte)1, Boost.Priority ? (byte)1 : (byte)0 };
                return Concat(PortalManager.InitEventData(portal, Boost.Id, DateTime, Info, Type, Boost.Image),
                              PortalManager.InitBoostData(Boost));
            }
            if (key.Contains("RankUp") && portal == PortalEvents.RankUpEvent)
            {
                EventRankUpModel RankUp = EventRankUpXML.GetEvent(id);
                if (RankUp == null)
                    return null;
                uint[] DateTime = new uint[2] { RankUp.BeginDate, RankUp.EndedDate };
                string[] Info = new string[3] { RankUp.Name, RankUp.Subtitle, RankUp.Description };
                byte[] Type = new byte[2] { RankUp.Period ? (byte)0 : (byte)1, RankUp.Priority ? (byte)1 : (byte)0 };
                return Concat(PortalManager.InitEventData(portal, RankUp.Id, DateTime, Info, Type, RankUp.Image),
                              PortalManager.InitRankUpData(RankUp));
            }
            if (key.Contains("Login") && portal == PortalEvents.LoginEvent)
            {
                EventLoginModel Login = EventLoginXML.GetEvent(id);
                if (Login == null)
                    return null;
                uint[] DateTime = new uint[2] { Login.BeginDate, Login.EndedDate };
                string[] Info = new string[3] { Login.Name, Login.Subtitle, Login.Description };
                byte[] Type = new byte[2] { Login.Period ? (byte)0 : (byte)1, Login.Priority ? (byte)1 : (byte)0 };
                return Concat(PortalManager.InitEventData(portal, Login.Id, DateTime, Info, Type, Login.Image),
                              PortalManager.InitLoginData(Login));
            }
            if (key.Contains("Playtime") && portal == PortalEvents.PlaytimeEvent)
            {
                EventPlaytimeModel Playtime = EventPlaytimeJSON.GetEvent(id);
                if (Playtime == null)
                    return null;
                uint[] DateTime = new uint[2] { Playtime.BeginDate, Playtime.EndedDate };
                string[] Info = new string[3] { Playtime.Name, Playtime.Subtitle, Playtime.Description };
                byte[] Type = new byte[2] { Playtime.Period ? (byte)0 : (byte)1, Playtime.Priority ? (byte)1 : (byte)0 };
                return Concat(PortalManager.InitEventData(portal, Playtime.Id, DateTime, Info, Type, Playtime.Image),
                              PortalManager.InitPlaytimeData(Playtime));
            }
            return null;
        }

        private static byte[] Concat(byte[] head, byte[] tail)
        {
            byte[] block = new byte[head.Length + tail.Length];
            Buffer.BlockCopy(head, 0, block, 0, head.Length);
            Buffer.BlockCopy(tail, 0, block, head.Length, tail.Length);
            return block;
        }
    }
}
