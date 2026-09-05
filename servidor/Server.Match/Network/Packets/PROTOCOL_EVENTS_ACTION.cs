// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Packets.PROTOCOL_EVENTS_ACTION
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core.Network;
using Server.Match.Data.Enums;
using Server.Match.Data.Models;
using System;
using System.Collections.Generic;


namespace Server.Match.Network.Packets
{
    public class PROTOCOL_EVENTS_ACTION
    {
        public static byte[] GET_CODE(List<ObjectHitInfo> Objs)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                foreach (ObjectHitInfo objectHitInfo in Objs)
                {
                    if (objectHitInfo.Type != 1)
                    {
                        if (objectHitInfo.Type == 2)
                        {
                            ushort num = 11;
                            UdpGameEvent udpGameEvent = UdpGameEvent.HpSync;
                            if (objectHitInfo.ObjLife == 0)
                            {
                                num += (ushort)13;
                                udpGameEvent |= UdpGameEvent.GetWeaponForHost;
                            }
                            syncServerPacket.WriteH(num);
                            syncServerPacket.WriteH((ushort)objectHitInfo.ObjId);
                            syncServerPacket.WriteC((byte)0);
                            syncServerPacket.WriteD((uint)udpGameEvent);
                            syncServerPacket.WriteH((ushort)objectHitInfo.ObjLife);
                            if (udpGameEvent.HasFlag((Enum)UdpGameEvent.GetWeaponForHost))
                            {
                                syncServerPacket.WriteC((byte)objectHitInfo.DeathType);
                                syncServerPacket.WriteC((byte)objectHitInfo.KillerSlot);
                                syncServerPacket.WriteHV(objectHitInfo.Position);
                                syncServerPacket.WriteD(0);
                                syncServerPacket.WriteC((byte)objectHitInfo.HitPart);
                            }
                        }
                        else if (objectHitInfo.Type == 3)
                        {
                            if (objectHitInfo.ObjSyncId != 0)
                            {
                                syncServerPacket.WriteH((short)14);
                                syncServerPacket.WriteH((ushort)objectHitInfo.ObjId);
                                syncServerPacket.WriteC((byte)12);
                                syncServerPacket.WriteC((byte)objectHitInfo.DestroyState);
                                syncServerPacket.WriteH((ushort)objectHitInfo.ObjLife);
                                syncServerPacket.WriteT(objectHitInfo.SpecialUse);
                                syncServerPacket.WriteC((byte)objectHitInfo.AnimId1);
                                syncServerPacket.WriteC((byte)objectHitInfo.AnimId2);
                            }
                            else
                            {
                                syncServerPacket.WriteH((short)8);
                                syncServerPacket.WriteH((ushort)objectHitInfo.ObjId);
                                syncServerPacket.WriteC((byte)9);
                                syncServerPacket.WriteH((short)1);
                                syncServerPacket.WriteC(objectHitInfo.ObjLife == 0 ? (byte)1 : (byte)0);
                            }
                        }
                        else if (objectHitInfo.Type == 4)
                        {
                            syncServerPacket.WriteH((short)11);
                            syncServerPacket.WriteH((ushort)objectHitInfo.ObjId);
                            syncServerPacket.WriteC((byte)8);
                            syncServerPacket.WriteD(8U);
                            syncServerPacket.WriteH((ushort)objectHitInfo.ObjLife);
                        }
                        else if (objectHitInfo.Type == 5)
                        {
                            syncServerPacket.WriteH((short)12);
                            syncServerPacket.WriteH((ushort)objectHitInfo.ObjId);
                            syncServerPacket.WriteC((byte)0);
                            syncServerPacket.WriteD(1073741824U /*0x40000000*/);
                            syncServerPacket.WriteC((byte)((uint)objectHitInfo.DeathType * 16U /*0x10*/));
                            syncServerPacket.WriteC((byte)objectHitInfo.HitPart);
                            syncServerPacket.WriteC((byte)objectHitInfo.KillerSlot);
                        }
                        else if (objectHitInfo.Type == 6)
                        {
                            syncServerPacket.WriteH((short)15);
                            syncServerPacket.WriteH((ushort)objectHitInfo.ObjId);
                            syncServerPacket.WriteC((byte)0);
                            syncServerPacket.WriteD(67108864U /*0x04000000*/);
                            syncServerPacket.WriteD(objectHitInfo.WeaponId);
                            syncServerPacket.WriteC(objectHitInfo.Accessory);
                            syncServerPacket.WriteC(objectHitInfo.Extensions);
                        }
                    }
                    else if (objectHitInfo.ObjSyncId != 0)
                    {
                        syncServerPacket.WriteH((short)13);
                        syncServerPacket.WriteH((ushort)objectHitInfo.ObjId);
                        syncServerPacket.WriteC((byte)6);
                        syncServerPacket.WriteH((ushort)objectHitInfo.ObjLife);
                        syncServerPacket.WriteC((byte)objectHitInfo.AnimId1);
                        syncServerPacket.WriteC((byte)objectHitInfo.AnimId2);
                        syncServerPacket.WriteT(objectHitInfo.SpecialUse);
                    }
                    else
                    {
                        syncServerPacket.WriteH((short)10);
                        syncServerPacket.WriteH((ushort)objectHitInfo.ObjId);
                        syncServerPacket.WriteC((byte)3);
                        syncServerPacket.WriteH((short)6);
                        syncServerPacket.WriteH((ushort)objectHitInfo.ObjLife);
                        syncServerPacket.WriteC((byte)objectHitInfo.KillerSlot);
                    }
                }
                return syncServerPacket.ToArray();
            }
        }
    }
}