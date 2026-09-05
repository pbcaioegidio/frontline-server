// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Packets.PROTOCOL_BOTS_ACTION
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Match.Data.Enums;
using Server.Match.Data.Models;
using System;
using System.IO;
using System.Runtime.CompilerServices;


namespace Server.Match.Network.Packets
{
    public class PROTOCOL_BOTS_ACTION
    {

        public static byte[] GET_CODE(byte[] Data)
        {
            SyncClientPacket syncClientPacket = new SyncClientPacket(Data);
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                syncServerPacket.WriteT(syncClientPacket.ReadT());
                for (int index = 0; index < 18; ++index)
                {
                    ActionModel actionModel = new ActionModel();
                    try
                    {
                        bool flag = false;
                        bool Exception;
                        actionModel.Length = syncClientPacket.ReadUH(out Exception);
                        if (!Exception)
                        {
                            actionModel.Slot = syncClientPacket.ReadUH();
                            actionModel.SubHead = (UdpSubHead)syncClientPacket.ReadC();
                            if (actionModel.SubHead != (UdpSubHead)255 /*0xFF*/)
                            {
                                syncServerPacket.WriteH(actionModel.Length);
                                syncServerPacket.WriteH(actionModel.Slot);
                                syncServerPacket.WriteC((byte)actionModel.SubHead);
                                switch (actionModel.SubHead)
                                {
                                    case UdpSubHead.User:
                                    case UdpSubHead.StageInfoChara:
                                        actionModel.Flag = (UdpGameEvent)syncClientPacket.ReadUD();
                                        actionModel.Data = syncClientPacket.ReadB((int)actionModel.Length - 9);
                                        syncServerPacket.WriteD((uint)actionModel.Flag);
                                        syncServerPacket.WriteB(actionModel.Data);
                                        if (actionModel.Data.Length == 0 && actionModel.Flag != (UdpGameEvent)0)
                                        {
                                            flag = true;
                                            break;
                                        }
                                        break;
                                    case UdpSubHead.Grenade:
                                        byte[] numArray1 = syncClientPacket.ReadB(30);
                                        syncServerPacket.WriteB(numArray1);
                                        break;
                                    case UdpSubHead.DroppedWeapon:
                                        byte[] numArray2 = syncClientPacket.ReadB(31 /*0x1F*/);
                                        syncServerPacket.WriteB(numArray2);
                                        break;
                                    case UdpSubHead.ObjectStatic:
                                        byte[] numArray3 = syncClientPacket.ReadB(10);
                                        syncServerPacket.WriteB(numArray3);
                                        break;
                                    case UdpSubHead.ObjectMove:
                                        byte[] numArray4 = syncClientPacket.ReadB(16 /*0x10*/);
                                        syncServerPacket.WriteB(numArray4);
                                        break;
                                    case UdpSubHead.ObjectAnim:
                                        byte[] numArray5 = syncClientPacket.ReadB(8);
                                        syncServerPacket.WriteB(numArray5);
                                        break;
                                    case UdpSubHead.StageInfoObjectStatic:
                                        byte[] numArray6 = syncClientPacket.ReadB(1);
                                        syncServerPacket.WriteB(numArray6);
                                        break;
                                    case UdpSubHead.StageInfoObjectAnim:
                                        byte[] numArray7 = syncClientPacket.ReadB(9);
                                        syncServerPacket.WriteB(numArray7);
                                        break;
                                    case UdpSubHead.StageInfoObjectControl:
                                        byte[] numArray8 = syncClientPacket.ReadB(9);
                                        syncServerPacket.WriteB(numArray8);
                                        break;
                                    default:
                                        CLogger.Print(Bitwise.ToHexData($"BOT SUB HEAD: '{actionModel.SubHead}' or '{(int)actionModel.SubHead}'", Data), LoggerType.Opcode);
                                        break;
                                }
                                if (flag)
                                    break;
                            }
                            else
                                break;
                        }
                        else
                            break;
                    }
                    catch (Exception ex)
                    {
                        CLogger.Print($"BOTS ACTION DATA - Buffer (Length: {Data.Length}): | {ex.Message}", LoggerType.Error, ex);
                        syncServerPacket.SetMStream(new MemoryStream());
                        break;
                    }
                }
                return syncServerPacket.ToArray();
            }
        }
    }
}