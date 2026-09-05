//v3.80

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Plugin.Core.SharpDX;
using Plugin.Core.Utility;
using Server.Match.Data.Enums;
using Server.Match.Data.Managers;
using Server.Match.Data.Models;
using Server.Match.Data.Models.Event;
using Server.Match.Data.Models.Event.Event;
using Server.Match.Data.Models.SubHead;
using Server.Match.Data.Sync.Server;
using Server.Match.Data.Utils;
using Server.Match.Network.Actions.Event;
using Server.Match.Network.Actions.SubHead;
using Server.Match.Network.Packets;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Server.Match
{
    public class MatchClient
    {
        private readonly Socket MainSocket;
        private readonly IPEndPoint RemoteAddress;

        public MatchClient(Socket socket, IPEndPoint endPoint)
        {
            MainSocket = socket;
            RemoteAddress = endPoint;
        }

        public void BeginReceive(byte[] Buffer, DateTime Date)
        {
            // VALIDACIÓN 1: Buffer no puede ser nulo o muy pequeño
            if (Buffer == null)
            {
                CLogger.Print("BeginReceive: Buffer is null", LoggerType.Warning);
                return;
            }

            if (Buffer.Length < 22)
            {
                CLogger.Print($"BeginReceive: Buffer too short ({Buffer.Length} bytes)", LoggerType.Warning);
                return;
            }

            PacketModel Packet = new PacketModel()
            {
                Data = Buffer,
                ReceiveDate = Date
            };

            try
            {
                SyncClientPacket syncClientPacket = new SyncClientPacket(Packet.Data);
                Packet.Opcode = (int)syncClientPacket.ReadC();
                Packet.Slot = (int)syncClientPacket.ReadC();
                Packet.Time = syncClientPacket.ReadT();
                Packet.Round = (int)syncClientPacket.ReadC();
                Packet.Length = (int)syncClientPacket.ReadUH();
                Packet.Respawn = (int)syncClientPacket.ReadC();
                Packet.RoundNumber = (int)syncClientPacket.ReadC();
                Packet.AccountId = (int)syncClientPacket.ReadC();
                Packet.Unk1 = (int)syncClientPacket.ReadC();
                Packet.Unk2 = syncClientPacket.ReadD();

                // VALIDACIÓN 2: Verificar longitud del paquete
                if (Packet.Length <= 0 || Packet.Length > Packet.Data.Length)
                {
                    CLogger.Print($"BeginReceive: Invalid packet length. Length: {Packet.Length}, DataLength: {Packet.Data.Length}", LoggerType.Warning);
                    return;
                }

                // Desencriptar datos
                AllUtils.GetDecryptedData(Packet);

                // VALIDACIÓN 3: Verificar que la desencriptación funcionó
                if (Packet.WithEndData == null || Packet.WithoutEndData == null)
                {
                    CLogger.Print($"BeginReceive: Decryption failed. WithEndData={Packet.WithEndData != null}, " + $"WithoutEndData={Packet.WithoutEndData != null}", LoggerType.Warning);
                    return;
                }

                if (ConfigLoader.IsTestMode && Packet.Unk1 > 0)
                {
                    CLogger.Print(Bitwise.ToHexData($"[N] Test Mode, Packet Unk: {Packet.Unk1}", Packet.Data), LoggerType.Debug);
                    CLogger.Print(Bitwise.ToHexData($"[D] Test Mode, Packet Unk: {Packet.Unk1}", Packet.WithoutEndData), LoggerType.Debug);
                }

                if (ConfigLoader.EnableLog && Packet.Opcode != 131 && Packet.Opcode != 132 && Packet.Opcode != 3)
                {
                    int opcode = Packet.Opcode;
                }

                ReadPacket(Packet);
            }
            catch (IndexOutOfRangeException ex)
            {
                CLogger.Print($"BeginReceive: Buffer read error - {ex.Message}", LoggerType.Error, ex);
            }
            catch (Exception ex)
            {
                CLogger.Print($"BeginReceive: Unexpected error - {ex.Message}", LoggerType.Error, ex);
            }
        }

        public void ReadPacket(PacketModel Packet)
        {
            // VALIDACIÓN CRÍTICA: Verificar que el paquete y sus datos no sean nulos
            if (Packet == null)
            {
                CLogger.Print("ReadPacket: Packet is null", LoggerType.Error);
                return;
            }

            if (Packet.WithEndData == null)
            {
                CLogger.Print($"ReadPacket: WithEndData is null (Opcode: {Packet.Opcode})", LoggerType.Error);
                return;
            }

            if (Packet.WithoutEndData == null)
            {
                CLogger.Print($"ReadPacket: WithoutEndData is null (Opcode: {Packet.Opcode})", LoggerType.Error);
                return;
            }

            byte[] withEndData = Packet.WithEndData;
            byte[] withoutEndData = Packet.WithoutEndData;
            int BasicBufferLength = withoutEndData.Length;

            // VALIDACIÓN: Verificar longitudes mínimas
            if (withEndData.Length < 4)
            {
                CLogger.Print($"ReadPacket: WithEndData too short ({withEndData.Length} bytes)", LoggerType.Warning);
                return;
            }

            try
            {
                SyncClientPacket syncClientPacket = new SyncClientPacket(withEndData);

                switch (Packet.Opcode)
                {
                    case 3:
                        ProcessPVPPacket(syncClientPacket, Packet, BasicBufferLength);
                        break;

                    case 4:
                        ProcessPVEPacket(syncClientPacket, Packet, BasicBufferLength);
                        break;

                    case 65:
                        ProcessUnregister_65(syncClientPacket, Packet);
                        break;

                    case 67:
                        ProcessUnregister_67(syncClientPacket, Packet);
                        break;

                    case 97:
                        ProcessPingPacket(syncClientPacket, Packet);
                        break;

                    case 131: //AI BY PLAYER
                        ProcessBotPlayerPacket(syncClientPacket, Packet, BasicBufferLength);
                        break;

                    case 132: ////AI BY HOST
                        ProcessBotHostPacket(syncClientPacket, Packet, BasicBufferLength);
                        break;

                    default:
                        CLogger.Print(Bitwise.ToHexData($"MATCH Opcode Not Found: [{Packet.Opcode}]", withEndData),
                                     LoggerType.Opcode);
                        break;
                }
            }
            catch (NullReferenceException ex)
            {
                CLogger.Print($"ReadPacket NullRef: Opcode={Packet.Opcode}, Slot={Packet.Slot}, " +
                             $"WithEndData.Length={withEndData?.Length ?? 0}, " +
                             $"WithoutEndData.Length={withoutEndData?.Length ?? 0}\n{ex.Message}",
                             LoggerType.Error, ex);
            }
            catch (Exception ex)
            {
                CLogger.Print($"ReadPacket Exception: Opcode={Packet.Opcode} - {ex.Message}",
                             LoggerType.Error, ex);
            }
        }

        private void ProcessUnregister_67(SyncClientPacket c, PacketModel packet)
        {
            try
            {
                string Version = $"{c.ReadH()}.{c.ReadH()}";
                c.Advance(packet.WithoutEndData.Length - 4);
                uint UniqueRoomId = c.ReadUD();
                uint RoomSeed = c.ReadUD();
                byte DedicationSlot = c.ReadC();

                RoomModel roomModel = RoomsManager.GetRoom(UniqueRoomId, (uint)RoomSeed);
                if (roomModel != null)
                {
                    if (roomModel.RemovePlayer(RemoteAddress, packet, Version))
                    {
                        if (ConfigLoader.IsTestMode)
                        {
                            CLogger.Print($"Player Disconnected. [{RemoteAddress.Address}:{RemoteAddress.Port}]", LoggerType.Warning);
                        }
                    }
                    else
                    {
                        CLogger.Print($"Player Disconnected. [{this.RemoteAddress.Address}:{this.RemoteAddress.Port}]", LoggerType.Warning);
                    }

                    if (roomModel.GetPlayersCount() == 0)
                    {
                        RoomsManager.RemoveRoom(roomModel.UniqueRoomId, roomModel.RoomSeed);
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error processing unregister packet: {ex.Message}", LoggerType.Error, ex);
            }
        }

        private void ProcessUnregister_65(SyncClientPacket c, PacketModel packet)
        {
            try
            {
                string Version = $"{c.ReadH()}.{c.ReadH()}";
                c.Advance(packet.WithoutEndData.Length - 4);
                uint UniqueRoomId = c.ReadUD();
                uint RoomSeed = c.ReadUD();
                byte DedicationSlot = c.ReadC();

                CLogger.Print($"[DBG65] Version={Version} UniqueRoomId=0x{UniqueRoomId:X8} RoomSeed=0x{RoomSeed:X8} DedicationSlot={DedicationSlot} srvIdExtracted={Server.Match.Data.Utils.AllUtils.GetRoomInfo(UniqueRoomId, 2)}", LoggerType.Warning);

                RoomModel roomModel = RoomsManager.CreateOrGetRoom(UniqueRoomId, RoomSeed);
                if (roomModel != null)
                {
                    PlayerModel playerModel = roomModel.AddPlayer(RemoteAddress, packet, Version);
                    if (playerModel != null)
                    {
                        if (!playerModel.Integrity)
                        {
                            playerModel.ResetBattleInfos();
                        }
                        SendPacket(PROTOCOL_CONNECT.GET_CODE(), playerModel.Client);
                        if (ConfigLoader.IsTestMode)
                        {
                            CLogger.Print($"Player Connected. [{playerModel.Client.Address}:{playerModel.Client.Port}]", LoggerType.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error processing unregister packet: {ex.Message}", LoggerType.Error, ex);
            }
        }

        private void ProcessPVEPacket(SyncClientPacket c, PacketModel packet, int BasicBufferLength)
        {
            try
            {
                c.Advance(BasicBufferLength);
                uint UniqueRoomId = c.ReadUD();
                byte DedicationSlot = c.ReadC();
                uint RoomSeed = c.ReadUD();

                RoomModel room = RoomsManager.GetRoom(UniqueRoomId, RoomSeed);
                if (room != null)
                {
                    PlayerModel playerModel = room.GetPlayer(packet.Slot, this.RemoteAddress);
                    if (playerModel != null && playerModel.PlayerIdByServer == packet.AccountId)
                    {
                        playerModel.RespawnByUser = packet.Respawn;
                        room.BotMode = true;
                        if (room.StartTime == new DateTime())
                        {
                            return;
                        }
                        byte[] Actions = WriteBotActionData(packet.WithoutEndData, room);
                        byte[] Code = AllUtils.BaseWriteCode(4, Actions, packet.Slot, AllUtils.GetDuration(playerModel.StartTime), packet.Round, packet.Respawn, packet.RoundNumber, packet.AccountId);
                        foreach (PlayerModel _playerModel in room.Players)
                        {
                            bool flag = _playerModel.Slot != packet.Slot;
                            if (_playerModel.Client != null && playerModel.AccountIdIsValid() && DedicationSlot == (int)byte.MaxValue & flag)
                            {
                                SendPacket(Code, _playerModel.Client);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error processing PVE packet: {ex.Message}", LoggerType.Error, ex);
            }
        }

        private void ProcessPVPPacket(SyncClientPacket c, PacketModel packet, int BasicBufferLength)
        {
            try
            {
                c.Advance(BasicBufferLength);
                uint UniqueRoomId = c.ReadUD();
                byte DedicationSlot = c.ReadC();
                uint RoomSeed = c.ReadUD();
                RoomModel room = RoomsManager.GetRoom(UniqueRoomId, RoomSeed);
                if (room != null)
                {
                    PlayerModel player1 = room.GetPlayer(packet.Slot, RemoteAddress);
                    if (player1 != null && player1.PlayerIdByServer == packet.AccountId)
                    {
                        player1.RespawnByUser = packet.Respawn;
                        if (room.StartTime == new DateTime())
                        {
                            return;
                        }
                        byte[] ActionData = packet.WithoutEndData;
                        byte[] Actions = room.BotMode
                            ? WriteBotActionData(ActionData, room)
                            : WritePlayerActionData(ActionData, room, packet.Time, packet);

                        byte[] Code = AllUtils.BaseWriteCode(4, Actions, (room.BotMode ? packet.Slot : byte.MaxValue), AllUtils.GetDuration(room.StartTime), packet.Round, packet.Respawn, packet.RoundNumber, packet.AccountId);
                        bool flag1 = !room.BotMode && DedicationSlot != (int)byte.MaxValue;

                        foreach (PlayerModel player2 in room.Players)
                        {
                            bool flag2 = player2.Slot != packet.Slot;
                            if (player2.Client != null && player1.AccountIdIsValid() && ((DedicationSlot == (int)byte.MaxValue & flag2 ? 1 : (room.BotMode & flag2 ? 1 : 0)) | (flag1 ? 1 : 0)) != 0)
                            {
                                SendPacket(Code, player2.Client);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error processing PVE packet: {ex.Message}", LoggerType.Error, ex);
            }
        }

        private void ProcessBotPlayerPacket(SyncClientPacket c, PacketModel packet, int BasicBufferLength)
        {
            try
            {
                c.Advance(BasicBufferLength);
                uint UniqueRoomId = c.ReadUD();
                byte DedicationSlot = c.ReadC();
                uint RoomSeed = c.ReadUD();
                RoomModel roomModel = RoomsManager.GetRoom(UniqueRoomId, RoomSeed);
                if (roomModel != null)
                {
                    PlayerModel player5 = roomModel.GetPlayer(packet.Slot, this.RemoteAddress);
                    if (player5 != null && player5.PlayerIdByServer == packet.AccountId)
                    {
                        roomModel.BotMode = true;
                        PlayerModel player6 = roomModel.GetPlayer(DedicationSlot, false);
                        byte[] Actions = PROTOCOL_BOTS_ACTION.GET_CODE(packet.WithoutEndData);
                        byte[] Code = AllUtils.BaseWriteCode(132, Actions, DedicationSlot, AllUtils.GetDuration(player6.StartTime), packet.Round, packet.Respawn, packet.RoundNumber, packet.AccountId);
                        foreach (PlayerModel playerModel in roomModel.Players)
                        {
                            if (playerModel.Client != null && player5.AccountIdIsValid() && playerModel.Slot != packet.Slot)
                            {
                                SendPacket(Code, playerModel.Client);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error processing bot player packet: {ex.Message}", LoggerType.Error, ex);
            }
        }

        private void ProcessBotHostPacket(SyncClientPacket c, PacketModel packet, int BasicBufferLength)
        {
            try
            {
                c.Advance(BasicBufferLength);
                uint UniqueRoomId = c.ReadUD();
                byte DedicationSlot = c.ReadC();
                uint RoomSeed = c.ReadUD();

                RoomModel roomModel = RoomsManager.GetRoom(UniqueRoomId, RoomSeed);
                if (roomModel != null)
                {
                    PlayerModel playerModel = roomModel.GetPlayer(packet.Slot, this.RemoteAddress);
                    if (playerModel != null && playerModel.PlayerIdByServer == packet.AccountId)
                    {
                        roomModel.BotMode = true;
                        byte[] Actions = PROTOCOL_BOTS_ACTION.GET_CODE(packet.WithoutEndData);
                        byte[] Code = AllUtils.BaseWriteCode(132, Actions, packet.Slot, AllUtils.GetDuration(playerModel.StartTime), packet.Round, packet.Respawn, packet.RoundNumber, packet.AccountId);
                        foreach (PlayerModel player9 in roomModel.Players)
                        {
                            if (player9.Client != null && playerModel.AccountIdIsValid() && player9.Slot != packet.Slot)
                            {
                                SendPacket(Code, player9.Client);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error processing bot host packet: {ex.Message}", LoggerType.Error, ex);
            }
        }

        private void ProcessPingPacket(SyncClientPacket c, PacketModel packet)
        {
            try
            {
                uint UniqueRoomId4 = c.ReadUD();
                byte DedicationSlot = c.ReadC();
                uint RoomSeed = c.ReadUD();

                RoomModel roomModel = RoomsManager.GetRoom(UniqueRoomId4, RoomSeed);
                byte[] data = packet.Data;

                if (roomModel != null)
                {
                    PlayerModel playerModel = roomModel.GetPlayer(packet.Slot, RemoteAddress);
                    if (playerModel != null)
                    {
                        playerModel.LastPing = packet.ReceiveDate;
                        SendPacket(data, RemoteAddress);
                        if (ConfigLoader.SendPingSync)
                        {
                            playerModel.Latency = AllUtils.PingTime($"{RemoteAddress.Address}", data, (int)MainSocket.Ttl, 120, false, out int PlayerPing);
                            playerModel.Ping = PlayerPing;
                            SendMatchInfo.SendPingSync(roomModel, playerModel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error processing ping packet: {ex.Message}", LoggerType.Error, ex);
            }
        }

        private List<ObjectHitInfo> Method0(ActionModel A_1, RoomModel A_2, float A_3, out byte[] A_4)
        {
            A_4 = new byte[0];
            if (A_2 == null)
                return (List<ObjectHitInfo>)null;
            if (A_1.Data.Length == 0)
                return new List<ObjectHitInfo>();
            byte[] data = A_1.Data;
            List<ObjectHitInfo> Objs = new List<ObjectHitInfo>();
            SyncClientPacket C = new SyncClientPacket(data);
            using (SyncServerPacket S = new SyncServerPacket())
            {
                uint num1 = 0;
                PlayerModel player = A_2.GetPlayer((int)A_1.Slot, false);
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.ActionState))
                {
                    num1 += 256U /*0x0100*/;
                    ActionStateInfo Info = ActionState.ReadInfo(A_1, C, false);
                    if (!A_2.BotMode && player != null)
                    {
                        Equipment equip = player.Equip;
                        if (equip != null)
                        {
                            int num2 = 0;
                            byte num3 = 0;
                            byte num4 = 0;
                            if (Info.Flag.HasFlag((System.Enum)WeaponSyncType.Primary))
                            {
                                num2 = equip.WpnPrimary;
                                int idStatics1 = ComDiv.GetIdStatics(equip.Accessory, 1);
                                int idStatics2 = ComDiv.GetIdStatics(equip.Accessory, 3);
                                num3 = idStatics1 == 30 ? (byte)idStatics2 : num3;
                            }
                            if (Info.Flag.HasFlag((System.Enum)WeaponSyncType.Secondary))
                                num2 = equip.WpnSecondary;
                            if (Info.Flag.HasFlag((System.Enum)WeaponSyncType.Melee))
                                num2 = equip.WpnMelee;
                            if (Info.Flag.HasFlag((System.Enum)WeaponSyncType.Explosive))
                                num2 = equip.WpnExplosive;
                            if (Info.Flag.HasFlag((System.Enum)WeaponSyncType.Special))
                                num2 = equip.WpnSpecial;
                            if (Info.Flag.HasFlag((System.Enum)WeaponSyncType.Mission) && A_2.RoomType == RoomCondition.Bomb)
                                num2 = 5009001;
                            if (Info.Flag.HasFlag((System.Enum)WeaponSyncType.Dual))
                            {
                                num4 = (byte)16 /*0x10*/;
                                if (Info.Action.HasFlag((System.Enum)ActionFlag.Unk2048))
                                    num2 = equip.WpnPrimary;
                                if (Info.Action.HasFlag((System.Enum)ActionFlag.Unk4096))
                                    num2 = equip.WpnPrimary;
                            }
                            if (Info.Flag.HasFlag((System.Enum)WeaponSyncType.Ext))
                            {
                                num4 = (byte)16 /*0x10*/;
                                if (Info.Action.HasFlag((System.Enum)ActionFlag.Unk2048))
                                    num2 = equip.WpnSecondary;
                                if (Info.Action.HasFlag((System.Enum)ActionFlag.Unk4096))
                                    num2 = equip.WpnSecondary;
                            }
                            ObjectHitInfo objectHitInfo = new ObjectHitInfo(6)
                            {
                                ObjId = player.Slot,
                                WeaponId = num2,
                                Accessory = num3,
                                Extensions = num4
                            };
                            Objs.Add(objectHitInfo);
                        }
                    }
                    ActionState.WriteInfo(S, Info);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.Animation))
                {
                    num1 += 2U;
                    AnimationInfo Info = Animation.ReadInfo(A_1, C, false);
                    Animation.WriteInfo(S, Info);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.PosRotation))
                {
                    num1 += 134217728U /*0x08000000*/;
                    PosRotationInfo Info = PosRotation.ReadInfo(A_1, C, false);
                    if (player != null)
                    {
                        Half3 rawPos = new Half3(Info.RotationX, Info.RotationY, Info.RotationZ);
                        Half3 accepted;
                        AllUtils.ValidatePlayerMovement(player, rawPos, out accepted);
                        player.Position = accepted;
                        Info.RotationX = accepted.X.RawValue;
                        Info.RotationY = accepted.Y.RawValue;
                        Info.RotationZ = accepted.Z.RawValue;
                    }
                    A_1.Flag |= UdpGameEvent.SoundPosRotation;
                    PosRotation.WriteInfo(S, Info);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.SoundPosRotation))
                {
                    num1 += 8388608U /*0x800000*/;
                    SoundPosRotationInfo Info = SoundPosRotation.ReadInfo(A_1, C, A_3, false);
                    SoundPosRotation.WriteInfo(S, Info);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.UseObject))
                {
                    num1 += 4U;
                    List<UseObjectInfo> Infos = UseObject.ReadInfo(A_1, C, false);
                    for (int index = 0; index < Infos.Count; ++index)
                    {
                        UseObjectInfo useObjectInfo = Infos[index];
                        if (!A_2.BotMode && useObjectInfo.ObjectId != ushort.MaxValue)
                        {
                            ObjectInfo objectInfo = A_2.GetObject((int)useObjectInfo.ObjectId);
                            if (objectInfo != null)
                            {
                                bool flag = false;
                                if (useObjectInfo.SpaceFlags.HasFlag((System.Enum)CharaMoves.HeliInMove) && objectInfo.UseDate.ToString("yyMMddHHmm").Equals("0101010000"))
                                    flag = true;
                                useObjectInfo.SpaceFlags.HasFlag((System.Enum)CharaMoves.HeliUnknown);
                                useObjectInfo.SpaceFlags.HasFlag((System.Enum)CharaMoves.HeliLeave);
                                if (useObjectInfo.SpaceFlags.HasFlag((System.Enum)CharaMoves.HeliStopped))
                                {
                                    AnimModel animation = objectInfo.Animation;
                                    if (animation != null && animation.Id == 0)
                                        objectInfo.Model.GetAnim(animation.NextAnim, 0.0f, 0.0f, objectInfo);
                                }
                                if (!flag)
                                {
                                    ObjectHitInfo objectHitInfo = new ObjectHitInfo(3)
                                    {
                                        ObjSyncId = 1,
                                        ObjId = objectInfo.Id,
                                        ObjLife = objectInfo.Life,
                                        AnimId1 = (int)byte.MaxValue,
                                        AnimId2 = objectInfo.Animation != null ? objectInfo.Animation.Id : (int)byte.MaxValue,
                                        SpecialUse = AllUtils.GetDuration(objectInfo.UseDate)
                                    };
                                    Objs.Add(objectHitInfo);
                                }
                            }
                        }
                        else
                            Infos.RemoveAt(index--);
                    }
                    UseObject.WriteInfo(S, Infos);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.ActionForObjectSync))
                {
                    num1 += 16U /*0x10*/;
                    ActionObjectInfo Info = ActionForObjectSync.ReadInfo(A_1, C, false);
                    if (player != null)
                        A_2.SyncInfo(Objs, 1);
                    ActionForObjectSync.WriteInfo(S, Info);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.RadioChat))
                {
                    num1 += 32U /*0x20*/;
                    RadioChatInfo Info = RadioChat.ReadInfo(A_1, C, false);
                    RadioChat.WriteInfo(S, Info);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.WeaponSync))
                {
                    num1 += 67108864U /*0x04000000*/;
                    WeaponSyncInfo Info = WeaponSync.ReadInfo(A_1, C, false);
                    if (player != null)
                    {
                        player.WeaponId = Info.WeaponId;
                        player.Accessory = Info.Accessory;
                        player.Extensions = Info.Extensions;
                        player.WeaponClass = Info.WeaponClass;
                        A_2.SyncInfo(Objs, 2);
                    }
                    WeaponSync.WriteInfo(S, Info);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.WeaponRecoil))
                {
                    num1 += 128U /*0x80*/;
                    WeaponRecoilInfo Info = WeaponRecoil.ReadInfo(A_1, C, false);
                    WeaponRecoil.WriteInfo(S, Info);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.HpSync))
                {
                    num1 += 8U;
                    HPSyncInfo Info = HpSync.ReadInfo(A_1, C, false);
                    HpSync.WriteInfo(S, Info);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.Suicide))
                {
                    num1 += 1048576U /*0x100000*/;
                    List<SuicideInfo> Hits = Suicide.ReadInfo(A_1, C, false);
                    List<DeathServerData> Deaths = new List<DeathServerData>();
                    if (player != null)
                    {
                        int ObjectId = -1;
                        int WeaponId = 0;
                        for (int index = 0; index < Hits.Count; ++index)
                        {
                            SuicideInfo suicideInfo = Hits[index];
                            if (!player.Dead && player.Life > 0)
                            {
                                int hitDamageBot = AllUtils.GetHitDamageBot(suicideInfo.HitInfo);
                                int killerId = AllUtils.GetKillerId(suicideInfo.HitInfo);
                                int objectType = AllUtils.GetObjectType(suicideInfo.HitInfo);
                                CharaHitPart HitPart = (CharaHitPart)((int)(suicideInfo.HitInfo >> 4) & 63 /*0x3F*/);
                                CharaDeath charaDeath = AllUtils.GetCharaDeath(suicideInfo.HitInfo);
                                if (objectType == 1 || objectType == 0)
                                    ObjectId = killerId;
                                WeaponId = suicideInfo.WeaponId;
                                DamageManager.SimpleDeath(A_2, Deaths, Objs, player, player, hitDamageBot, WeaponId, HitPart, charaDeath);
                                if (hitDamageBot > 0)
                                {
                                    if (ConfigLoader.UseHitMarker)
                                        SendMatchInfo.SendHitMarkerSync(A_2, player, charaDeath, HitType.Normal, hitDamageBot);
                                    ObjectHitInfo objectHitInfo = new ObjectHitInfo(2)
                                    {
                                        ObjId = player.Slot,
                                        ObjLife = player.Life,
                                        HitPart = HitPart,
                                        KillerSlot = ObjectId,
                                        Position = suicideInfo.PlayerPos,
                                        DeathType = charaDeath,
                                        WeaponId = WeaponId
                                    };
                                    Objs.Add(objectHitInfo);
                                }
                            }
                            else
                                Hits.RemoveAt(index--);
                        }
                        if (Deaths.Count > 0)
                            SendMatchInfo.SendDeathSync(A_2, player, ObjectId, WeaponId, Deaths);
                    }
                    else
                        Hits = new List<SuicideInfo>();
                    Suicide.WriteInfo(S, Hits);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.MissionData))
                {
                    num1 += 2048U /*0x0800*/;
                    MissionDataInfo Info = MissionData.ReadInfo(A_1, C, A_3, false);
                    if (A_2.Map != null && player != null && !player.Dead && (double)Info.PlantTime > 0.0 && !Info.BombEnum.HasFlag((System.Enum)BombFlag.Stop))
                    {
                        BombPosition bomb = A_2.Map.GetBomb(Info.BombId);
                        if (bomb != null)
                        {
                            bool flag;
                            Vector3 vector3 = (Vector3)((flag = Info.BombEnum.HasFlag((System.Enum)BombFlag.Defuse)) ? A_2.BombPosition : (Info.BombEnum.HasFlag((System.Enum)BombFlag.Start) ? bomb.Position : new Half3((ushort)0, (ushort)0, (ushort)0)));
                            double num5 = (double)Vector3.Distance((Vector3)player.Position, vector3);
                            TeamEnum swappedTeam = AllUtils.GetSwappedTeam(player, A_2);
                            if ((bomb.EveryWhere || num5 <= 2.0) && (swappedTeam == TeamEnum.CT_TEAM & flag || swappedTeam == TeamEnum.FR_TEAM && !flag))
                            {
                                if ((double)player.C4Time != (double)Info.PlantTime)
                                {
                                    player.C4First = DateTimeUtil.Now();
                                    player.C4Time = Info.PlantTime;
                                }
                                double duration = ComDiv.GetDuration(player.C4First);
                                float num6 = flag ? player.DefuseDuration : player.PlantDuration;
                                if (((double)A_3 >= (double)Info.PlantTime + (double)num6 || duration >= (double)num6) && (!A_2.HasC4 && Info.BombEnum.HasFlag((System.Enum)BombFlag.Start) || A_2.HasC4 & flag))
                                {
                                    A_2.HasC4 = !A_2.HasC4;
                                    Info.Bomb |= 2;
                                    SendMatchInfo.SendBombSync(A_2, player, Info.BombEnum.HasFlag((System.Enum)BombFlag.Defuse) ? 1 : 0, Info.BombId);
                                }
                            }
                        }
                    }
                    MissionData.WriteInfo(S, Info);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.DropWeapon))
                {
                    num1 += 4194304U /*0x400000*/;
                    DropWeaponInfo Info = DropWeapon.ReadInfo(A_1, C, false);
                    if (!A_2.BotMode)
                    {
                        ++A_2.DropCounter;
                        if ((int)A_2.DropCounter > (int)ConfigLoader.MaxDropWpnCount)
                            A_2.DropCounter = (byte)0;
                        Info.Counter = A_2.DropCounter;
                        Equipment equip = player.Equip;
                        if (player != null && equip != null)
                        {
                            int idStatics = ComDiv.GetIdStatics(Info.WeaponId, 1);
                            if (idStatics == 1)
                                equip.WpnPrimary = 0;
                            if (idStatics == 2)
                                equip.WpnSecondary = 0;
                        }
                    }
                    DropWeapon.WriteInfo(S, Info);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.GetWeaponForClient))
                {
                    num1 += 16777216U /*0x01000000*/;
                    WeaponClient Info = GetWeaponForClient.ReadInfo(A_1, C, false);
                    if (!A_2.BotMode)
                    {
                        Equipment equip = player.Equip;
                        if (player != null && equip != null)
                        {
                            int idStatics = ComDiv.GetIdStatics(Info.WeaponId, 1);
                            if (idStatics == 1)
                                equip.WpnPrimary = Info.WeaponId;
                            if (idStatics == 2)
                                equip.WpnSecondary = Info.WeaponId;
                        }
                        if (player != null)
                            AllUtils.ValidateWeaponAmmo(player, Info);
                    }
                    GetWeaponForClient.WriteInfo(S, Info);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.FireData))
                {
                    num1 += 33554432U /*0x02000000*/;
                    FireDataInfo Info = FireData.ReadInfo(A_1, C, false);
                    if (player != null && !A_2.BotMode)
                        AllUtils.TrackFireShot(player, Info.WeaponId);
                    FireData.WriteInfo(S, Info);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.CharaFireNHitData))
                {
                    num1 += 1024U /*0x0400*/;
                    List<CharaFireNHitDataInfo> Hits = CharaFireNHitData.ReadInfo(A_1, C, false);
                    CharaFireNHitData.WriteInfo(S, Hits);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.HitData))
                {
                    num1 += 131072U /*0x020000*/;
                    List<HitDataInfo> Hits = HitData.ReadInfo(A_1, C, false);
                    List<DeathServerData> Deaths = new List<DeathServerData>();
                    if (player == null)
                    {
                        Hits = new List<HitDataInfo>();
                    }
                    else
                    {
                        int WeaponId = 0;
                        for (int index = 0; index < Hits.Count; ++index)
                        {
                            HitDataInfo Hit = Hits[index];
                            if (Hit.HitEnum != HitType.HelmetProtection && Hit.HitEnum != HitType.HeadshotProtection)
                            {
                                int Damage;
                                if (AllUtils.ValidateHitData(AllUtils.GetHitDamageNormal(Hit.HitIndex), Hit, player, out Damage))
                                {
                                    int objectId = (int)Hit.ObjectId;
                                    CharaHitPart hitPart = AllUtils.GetHitPart(Hit.HitIndex);
                                    CharaDeath DeathType = CharaDeath.DEFAULT;
                                    WeaponId = Hit.WeaponId;
                                    ObjectType hitType = AllUtils.GetHitType(Hit.HitIndex);
                                    switch (hitType)
                                    {
                                        case ObjectType.User:
                                            PlayerModel Player;
                                            if (A_2.GetPlayer(objectId, out Player) && player.RespawnIsValid() && !player.Dead && !Player.Dead && !Player.Immortal)
                                            {
                                                if (hitPart == CharaHitPart.HEAD)
                                                    DeathType = CharaDeath.HEADSHOT;
                                                if (A_2.RoomType == RoomCondition.DeathMatch && A_2.Rule == MapRules.HeadHunter && DeathType != CharaDeath.HEADSHOT)
                                                    Damage = 1;
                                                else if (A_2.RoomType != RoomCondition.Boss || DeathType != CharaDeath.HEADSHOT)
                                                {
                                                    if (A_2.RoomType == RoomCondition.DeathMatch && A_2.Rule == MapRules.Chaos)
                                                        Damage = 200;
                                                }
                                                else if (A_2.LastRound == 1 && Player.Team == TeamEnum.FR_TEAM || A_2.LastRound == 2 && Player.Team == TeamEnum.CT_TEAM)
                                                    Damage /= 10;
                                                DamageManager.SimpleDeath(A_2, Deaths, Objs, player, Player, Damage, WeaponId, hitPart, DeathType);
                                                if (Damage > 0)
                                                {
                                                    if (ConfigLoader.UseHitMarker)
                                                        SendMatchInfo.SendHitMarkerSync(A_2, player, DeathType, Hit.HitEnum, Damage);
                                                    ObjectHitInfo objectHitInfo = new ObjectHitInfo(2)
                                                    {
                                                        ObjId = Player.Slot,
                                                        ObjLife = Player.Life,
                                                        HitPart = hitPart,
                                                        KillerSlot = player.Slot,
                                                        Position = (Half3)((Vector3)Player.Position - (Vector3)player.Position),
                                                        DeathType = DeathType,
                                                        WeaponId = WeaponId
                                                    };
                                                    Objs.Add(objectHitInfo);
                                                    continue;
                                                }
                                                continue;
                                            }
                                            Hits.RemoveAt(index--);
                                            continue;
                                        case ObjectType.UserObject:
                                            continue;
                                        case ObjectType.Object:
                                            ObjectInfo ObjI = A_2.GetObject(objectId);
                                            ObjectModel model = ObjI?.Model;
                                            if (model != null && model.Destroyable)
                                            {
                                                if (ObjI.Life > 0)
                                                {
                                                    ObjI.Life -= Damage;
                                                    if (ObjI.Life <= 0)
                                                    {
                                                        ObjI.Life = 0;
                                                        DamageManager.BoomDeath(A_2, player, Damage, WeaponId, Deaths, Objs, Hit.BoomPlayers, CharaHitPart.ALL, CharaDeath.OBJECT_EXPLOSION);
                                                    }
                                                    ObjI.DestroyState = model.CheckDestroyState(ObjI.Life);
                                                    DamageManager.SabotageDestroy(A_2, player, model, ObjI, Damage);
                                                    float duration = AllUtils.GetDuration(ObjI.UseDate);
                                                    if (ObjI.Animation != null && (double)ObjI.Animation.Duration > 0.0 && (double)duration >= (double)ObjI.Animation.Duration)
                                                        ObjI.Model.GetAnim(ObjI.Animation.NextAnim, duration, ObjI.Animation.Duration, ObjI);
                                                    ObjectHitInfo objectHitInfo = new ObjectHitInfo(model.UpdateId)
                                                    {
                                                        ObjId = ObjI.Id,
                                                        ObjLife = ObjI.Life,
                                                        KillerSlot = (int)A_1.Slot,
                                                        ObjSyncId = model.NeedSync ? 1 : 0,
                                                        SpecialUse = duration,
                                                        AnimId1 = model.Animation,
                                                        AnimId2 = ObjI.Animation != null ? ObjI.Animation.Id : (int)byte.MaxValue,
                                                        DestroyState = ObjI.DestroyState
                                                    };
                                                    Objs.Add(objectHitInfo);
                                                    continue;
                                                }
                                                continue;
                                            }
                                            if (ConfigLoader.SendFailMsg && model == null)
                                            {
                                                CLogger.Print($"Fire Obj: {objectId} Map: {A_2.MapId} Invalid Object.", LoggerType.Debug);
                                                //player.LogPlayerPos(Hit.EndBullet);
                                                continue;
                                            }
                                            continue;
                                        default:
                                            CLogger.Print($"HitType: ({hitType}/{(int)hitType}) Slot: {A_1.Slot}", LoggerType.Debug);
                                            CLogger.Print($"BoomPlayers: {Hit.BoomInfo} {Hit.BoomPlayers.Count}", LoggerType.Debug);
                                            continue;
                                    }
                                }
                                else
                                    Hits.RemoveAt(index--);
                            }
                        }
                        if (Deaths.Count > 0)
                            SendMatchInfo.SendDeathSync(A_2, player, (int)byte.MaxValue, WeaponId, Deaths);
                    }
                    HitData.WriteInfo(S, Hits);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.GrenadeHit))
                {
                    num1 += 268435456U /*0x10000000*/;
                    List<GrenadeHitInfo> Hits = GrenadeHit.ReadInfo(A_1, C, false);
                    List<DeathServerData> Deaths = new List<DeathServerData>();
                    if (player != null)
                    {
                        int num7 = -1;
                        int WeaponId = 0;
                        for (int index = 0; index < Hits.Count; ++index)
                        {
                            GrenadeHitInfo Hit = Hits[index];
                            int Damage;
                            if (!AllUtils.ValidateGrenadeHit(AllUtils.GetHitDamageNormal(Hit.HitInfo), Hit, out Damage))
                            {
                                Hits.RemoveAt(index--);
                            }
                            else
                            {
                                int objectId = (int)Hit.ObjectId;
                                CharaHitPart hitPart = AllUtils.GetHitPart(Hit.HitInfo);
                                WeaponId = Hit.WeaponId;
                                ObjectType hitType = AllUtils.GetHitType(Hit.HitInfo);
                                switch (hitType)
                                {
                                    case ObjectType.User:
                                        ++num7;
                                        PlayerModel Player;
                                        if (Damage > 0 && A_2.GetPlayer(objectId, out Player) && player.RespawnIsValid() && !Player.Dead && !Player.Immortal)
                                        {
                                            TeamEnum teamEnum = num7 % 2 == 0 ? TeamEnum.FR_TEAM : TeamEnum.CT_TEAM;
                                            if (Hit.DeathType == CharaDeath.MEDICAL_KIT)
                                            {
                                                Player.Life += Damage;
                                                Player.CheckLifeValue();
                                            }
                                            else if (Hit.DeathType == CharaDeath.BOOM && ClassType.Dino != Hit.WeaponClass && (teamEnum == TeamEnum.FR_TEAM || teamEnum == TeamEnum.CT_TEAM))
                                            {
                                                Damage = (int)Math.Ceiling((double)Damage / 2.7);
                                                DamageManager.SimpleDeath(A_2, Deaths, Objs, player, Player, Damage, WeaponId, hitPart, Hit.DeathType);
                                            }
                                            else
                                                DamageManager.SimpleDeath(A_2, Deaths, Objs, player, Player, Damage, WeaponId, hitPart, Hit.DeathType);
                                            if (Damage > 0)
                                            {
                                                if (ConfigLoader.UseHitMarker)
                                                    SendMatchInfo.SendHitMarkerSync(A_2, player, Hit.DeathType, Hit.HitEnum, Damage);
                                                ObjectHitInfo objectHitInfo = new ObjectHitInfo(2)
                                                {
                                                    ObjId = Player.Slot,
                                                    ObjLife = Player.Life,
                                                    HitPart = hitPart,
                                                    KillerSlot = player.Slot,
                                                    Position = (Half3)((Vector3)Player.Position - (Vector3)player.Position),
                                                    DeathType = Hit.DeathType,
                                                    WeaponId = WeaponId
                                                };
                                                Objs.Add(objectHitInfo);
                                                continue;
                                            }
                                            continue;
                                        }
                                        Hits.RemoveAt(index--);
                                        continue;
                                    case ObjectType.UserObject:
                                        continue;
                                    case ObjectType.Object:
                                        ObjectInfo ObjI = A_2.GetObject(objectId);
                                        ObjectModel model = ObjI?.Model;
                                        if (model != null && model.Destroyable && ObjI.Life > 0)
                                        {
                                            ObjI.Life -= Damage;
                                            if (ObjI.Life <= 0)
                                            {
                                                ObjI.Life = 0;
                                                DamageManager.BoomDeath(A_2, player, Damage, WeaponId, Deaths, Objs, Hit.BoomPlayers, CharaHitPart.ALL, CharaDeath.OBJECT_EXPLOSION);
                                            }
                                            ObjI.DestroyState = model.CheckDestroyState(ObjI.Life);
                                            DamageManager.SabotageDestroy(A_2, player, model, ObjI, Damage);
                                            if (Damage > 0)
                                            {
                                                ObjectHitInfo objectHitInfo = new ObjectHitInfo(model.UpdateId)
                                                {
                                                    ObjId = ObjI.Id,
                                                    ObjLife = ObjI.Life,
                                                    KillerSlot = (int)A_1.Slot,
                                                    ObjSyncId = model.NeedSync ? 1 : 0,
                                                    AnimId1 = model.Animation,
                                                    AnimId2 = ObjI.Animation != null ? ObjI.Animation.Id : (int)byte.MaxValue,
                                                    DestroyState = ObjI.DestroyState,
                                                    SpecialUse = AllUtils.GetDuration(ObjI.UseDate)
                                                };
                                                Objs.Add(objectHitInfo);
                                                continue;
                                            }
                                            continue;
                                        }
                                        if (ConfigLoader.SendFailMsg && model == null)
                                        {
                                            //CLogger.Print($"Boom Obj: {objectId} Map: {A_2.MapId} Invalid Object.", LoggerType.Warning);
                                            //player.LogPlayerPos(Hit.HitPos);
                                            continue;
                                        }
                                        continue;
                                    default:
                                        // CLogger.Print($"Grenade Boom, HitType: ({hitType}/{(int)hitType})", LoggerType.Warning);
                                        continue;
                                }
                            }
                        }
                        if (Deaths.Count > 0)
                            SendMatchInfo.SendDeathSync(A_2, player, (int)byte.MaxValue, WeaponId, Deaths);
                    }
                    else
                        Hits = new List<GrenadeHitInfo>();
                    GrenadeHit.WriteInfo(S, Hits);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.GetWeaponForHost))
                {
                    num1 += 512U /*0x0200*/;
                    WeaponHost Info = GetWeaponForHost.ReadInfo(A_1, C, false);
                    GetWeaponForHost.WriteInfo(S, Info);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.FireDataOnObject))
                {
                    num1 += 1073741824U /*0x40000000*/;
                    FireDataObjectInfo Info = FireDataOnObject.ReadInfo(A_1, C, false);
                    FireDataOnObject.WriteInfo(S, Info);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.FireNHitDataOnObject))
                {
                    num1 += 8192U /*0x2000*/;
                    FireNHitDataObjectInfo Info = FireNHitDataOnObject.ReadInfo(A_1, C, false);
                    if (player != null && !player.Dead)
                        SendMatchInfo.SendPortalPass(A_2, player, (int)Info.Portal);
                    FireNHitDataOnObject.WriteInfo(S, Info);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.SeizeDataForClient))
                {
                    num1 += 32768U /*0x8000*/;
                    SeizeDataForClientInfo Info = SeizeDataForClient.ReadInfo(A_1, C, true);
                    if (player != null && !player.Dead)
                    {
                        //TODO HERE
                    }
                    SeizeDataForClient.WriteInfo(S, Info);
                }
                A_4 = S.ToArray();
                if ((UdpGameEvent)num1 != A_1.Flag)
                    CLogger.Print(Bitwise.ToHexData($"PVP - Missing Flag Events: '{(ValueType)(uint)A_1.Flag}' | '{(ValueType)(uint)(A_1.Flag - num1)}'", data), LoggerType.Opcode);
                return Objs;
            }
        }

        public byte[] WritePlayerActionData(byte[] Data, RoomModel Room, float Time, PacketModel Packet)
        {
            SyncClientPacket C = new SyncClientPacket(Data);
            List<ObjectHitInfo> Objs = new List<ObjectHitInfo>();
            using (SyncServerPacket S = new SyncServerPacket())
            {
                for (int index = 0; index < 18; ++index)
                {
                    ActionModel actionModel = new ActionModel();
                    try
                    {
                        bool flag = false;
                        bool Exception;
                        actionModel.Length = C.ReadUH(out Exception);
                        if (!Exception)
                        {
                            actionModel.Slot = C.ReadUH();
                            actionModel.SubHead = (UdpSubHead)C.ReadC();
                            if (actionModel.SubHead != (UdpSubHead)255 /*0xFF*/)
                            {
                                S.WriteH(actionModel.Length);
                                S.WriteH(actionModel.Slot);
                                S.WriteC((byte)actionModel.SubHead);
                                switch (actionModel.SubHead)
                                {
                                    case UdpSubHead.User:
                                    case UdpSubHead.StageInfoChara:
                                        actionModel.Flag = (UdpGameEvent)C.ReadUD();
                                        actionModel.Data = C.ReadB((int)actionModel.Length - 9);
                                        AllUtils.CheckDataFlags(actionModel, Packet);
                                        byte[] A_4;
                                        Objs.AddRange((IEnumerable<ObjectHitInfo>)this.Method0(actionModel, Room, Time, out A_4));
                                        S.GoBack(5);
                                        S.WriteH((ushort)(A_4.Length + 9));
                                        S.WriteH(actionModel.Slot);
                                        S.WriteC((byte)actionModel.SubHead);
                                        S.WriteD((uint)actionModel.Flag);
                                        S.WriteB(A_4);
                                        if (actionModel.Data.Length == 0 && (int)actionModel.Length - 9 != 0)
                                        {
                                            flag = true;
                                            break;
                                        }
                                        break;

                                    case UdpSubHead.Grenade:
                                        GrenadeInfo Info1 = GrenadeSync.ReadInfo(C, false);
                                        GrenadeSync.WriteInfo(S, Info1);
                                        break;

                                    case UdpSubHead.DroppedWeapon:
                                        DropedWeaponInfo Info2 = DropedWeapon.ReadInfo(C, false);
                                        DropedWeapon.WriteInfo(S, Info2);
                                        break;

                                    case UdpSubHead.ObjectStatic:
                                        ObjectStaticInfo Info3 = ObjectStatic.ReadInfo(C, false);
                                        ObjectStatic.WriteInfo(S, Info3);
                                        break;

                                    case UdpSubHead.ObjectMove:
                                        ObjectMoveInfo Info4 = ObjectMove.ReadInfo(C, false);
                                        ObjectMove.WriteInfo(S, Info4);
                                        break;

                                    case UdpSubHead.ObjectAnim:
                                        ObjectAnimInfo Info5 = ObjectAnim.ReadInfo(C, false);
                                        ObjectAnim.WriteInfo(S, Info5);
                                        break;

                                    case UdpSubHead.StageInfoObjectStatic:
                                        StageStaticInfo Info6 = StageInfoObjStatic.ReadInfo(C, false);
                                        StageInfoObjStatic.WriteInfo(S, Info6);
                                        break;

                                    case UdpSubHead.StageInfoObjectAnim:
                                        StageAnimInfo Info7 = StageInfoObjAnim.ReadInfo(C, false);
                                        StageInfoObjAnim.WriteInfo(S, Info7);
                                        break;

                                    case UdpSubHead.StageInfoObjectControl:
                                        StageControlInfo Info8 = StageInfoObjControl.ReadInfo(C, false);
                                        StageInfoObjControl.WriteInfo(S, Info8);
                                        break;

                                    default:
                                        CLogger.Print(Bitwise.ToHexData($"PVP Sub Head: '{actionModel.SubHead}' or '{(int)actionModel.SubHead}'", Data), LoggerType.Opcode);
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
                        CLogger.Print($"PVP Action Data - Buffer (Length: {Data.Length}): | {ex.Message}", LoggerType.Error, ex);
                        Objs = new List<ObjectHitInfo>();
                        break;
                    }
                }
                if (Objs.Count > 0)
                    S.WriteB(PROTOCOL_EVENTS_ACTION.GET_CODE(Objs));
                return S.ToArray();
            }
        }

        private List<ObjectHitInfo> Method1(ActionModel A_1, RoomModel A_2, out byte[] A_3)
        {
            A_3 = new byte[0];
            if (A_2 == null)
                return (List<ObjectHitInfo>)null;
            if (A_1.Data.Length == 0)
                return new List<ObjectHitInfo>();
            byte[] data = A_1.Data;
            List<ObjectHitInfo> Objs = new List<ObjectHitInfo>();
            SyncClientPacket syncClientPacket = new SyncClientPacket(data);
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                uint num1 = 0;
                PlayerModel player = A_2.GetPlayer((int)A_1.Slot, false);
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.ActionState))
                {
                    num1 += 256U /*0x0100*/;
                    ActionFlag actionFlag = (ActionFlag)syncClientPacket.ReadUH();
                    byte num2 = syncClientPacket.ReadC();
                    WeaponSyncType weaponSyncType = (WeaponSyncType)syncClientPacket.ReadC();
                    syncServerPacket.WriteH((ushort)actionFlag);
                    syncServerPacket.WriteC(num2);
                    syncServerPacket.WriteC((byte)weaponSyncType);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.Animation))
                {
                    num1 += 2U;
                    AnimationInfo info = Animation.ReadInfo(A_1, syncClientPacket, false);
                    Animation.WriteInfo(syncServerPacket, info);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.PosRotation))
                {
                    num1 += 134217728U /*0x08000000*/;
                    ushort num4 = syncClientPacket.ReadUH();
                    ushort num5 = syncClientPacket.ReadUH();
                    ushort num6 = syncClientPacket.ReadUH();
                    ushort num7 = syncClientPacket.ReadUH();
                    ushort num8 = syncClientPacket.ReadUH();
                    ushort num9 = syncClientPacket.ReadUH();
                    Half3 rawPos = new Half3(num8, num9, num7);
                    if (player != null)
                    {
                        Half3 accepted;
                        if (AllUtils.ValidatePlayerMovement(player, rawPos, out accepted))
                            player.Position = accepted;
                        else
                        {
                            player.Position = accepted;
                            // Reescreve coordenadas clampadas no relay
                            num8 = accepted.X.RawValue;
                            num9 = accepted.Y.RawValue;
                            num7 = accepted.Z.RawValue;
                        }
                    }
                    syncServerPacket.WriteH(num4);
                    syncServerPacket.WriteH(num5);
                    syncServerPacket.WriteH(num6);
                    syncServerPacket.WriteH(num7);
                    syncServerPacket.WriteH(num8);
                    syncServerPacket.WriteH(num9);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.SoundPosRotation))
                {
                    num1 += 8388608U /*0x800000*/;
                    float num10 = syncClientPacket.ReadT();
                    syncServerPacket.WriteT(num10);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.UseObject))
                {
                    num1 += 4U;
                    byte num11 = syncClientPacket.ReadC();
                    syncServerPacket.WriteC(num11);
                    for (int index = 0; index < (int)num11; ++index)
                    {
                        ushort num12 = syncClientPacket.ReadUH();
                        byte num13 = syncClientPacket.ReadC();
                        CharaMoves charaMoves = (CharaMoves)syncClientPacket.ReadC();
                        syncServerPacket.WriteH(num12);
                        syncServerPacket.WriteC(num13);
                        syncServerPacket.WriteC((byte)charaMoves);
                    }
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.ActionForObjectSync))
                {
                    num1 += 16U /*0x10*/;
                    byte num14 = syncClientPacket.ReadC();
                    byte num15 = syncClientPacket.ReadC();
                    if (player != null)
                        A_2.SyncInfo(Objs, 1);
                    syncServerPacket.WriteC(num14);
                    syncServerPacket.WriteC(num15);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.RadioChat))
                {
                    num1 += 32U /*0x20*/;
                    byte num16 = syncClientPacket.ReadC();
                    byte num17 = syncClientPacket.ReadC();
                    syncServerPacket.WriteC(num16);
                    syncServerPacket.WriteC(num17);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.WeaponSync))
                {
                    num1 += 67108864U /*0x04000000*/;
                    int Id = syncClientPacket.ReadD();
                    byte num18 = syncClientPacket.ReadC();
                    byte num19 = syncClientPacket.ReadC();
                    if (player != null)
                    {
                        player.WeaponId = Id;
                        player.Accessory = num18;
                        player.Extensions = num19;
                        player.WeaponClass = (ClassType)ComDiv.GetIdStatics(Id, 2);
                        A_2.SyncInfo(Objs, 2);
                    }
                    syncServerPacket.WriteD(Id);
                    syncServerPacket.WriteC(num18);
                    syncServerPacket.WriteC(num19);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.WeaponRecoil))
                {
                    num1 += 128U /*0x80*/;
                    float num20 = syncClientPacket.ReadT();
                    float num21 = syncClientPacket.ReadT();
                    float num22 = syncClientPacket.ReadT();
                    float num23 = syncClientPacket.ReadT();
                    float num24 = syncClientPacket.ReadT();
                    byte num25 = syncClientPacket.ReadC();
                    int num26 = syncClientPacket.ReadD();
                    byte num27 = syncClientPacket.ReadC();
                    byte num28 = syncClientPacket.ReadC();
                    CLogger.Print($"PVE (WeaponRecoil); Slot: {player.Slot}; Weapon Id: {num26}; Extensions: {num28}; Unknowns: {num27}", LoggerType.Warning);
                    syncServerPacket.WriteT(num20);
                    syncServerPacket.WriteT(num21);
                    syncServerPacket.WriteT(num22);
                    syncServerPacket.WriteT(num23);
                    syncServerPacket.WriteT(num24);
                    syncServerPacket.WriteC(num25);
                    syncServerPacket.WriteD(num26);
                    syncServerPacket.WriteC(num27);
                    syncServerPacket.WriteC(num28);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.HpSync))
                {
                    num1 += 8U;
                    ushort num29 = syncClientPacket.ReadUH();
                    syncServerPacket.WriteH(num29);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.Suicide))
                {
                    num1 += 1048576U /*0x100000*/;
                    byte num30 = syncClientPacket.ReadC();
                    syncServerPacket.WriteC(num30);
                    for (int index = 0; index < (int)num30; ++index)
                    {
                        Half3 Half = syncClientPacket.ReadUHV();
                        int num31 = syncClientPacket.ReadD();
                        byte num32 = syncClientPacket.ReadC();
                        byte num33 = syncClientPacket.ReadC();
                        uint num34 = syncClientPacket.ReadUD();
                        syncServerPacket.WriteHV(Half);
                        syncServerPacket.WriteD(num31);
                        syncServerPacket.WriteC(num32);
                        syncServerPacket.WriteC(num33);
                        syncServerPacket.WriteD(num34);
                    }
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.DropWeapon))
                {
                    num1 += 4194304U /*0x400000*/;
                    ushort num35 = syncClientPacket.ReadUH();
                    ushort num36 = syncClientPacket.ReadUH();
                    ushort num37 = syncClientPacket.ReadUH();
                    ushort num38 = syncClientPacket.ReadUH();
                    ushort num39 = syncClientPacket.ReadUH();
                    ushort num40 = syncClientPacket.ReadUH();
                    byte num41 = syncClientPacket.ReadC();
                    int num42 = syncClientPacket.ReadD();
                    byte num43 = syncClientPacket.ReadC();
                    byte num44 = syncClientPacket.ReadC();
                    if (!ConfigLoader.UseMaxAmmoInDrop)
                    {
                        syncServerPacket.WriteH(num35);
                        syncServerPacket.WriteH(num36);
                        syncServerPacket.WriteH(num37);
                    }
                    else
                    {
                        syncServerPacket.WriteH(ushort.MaxValue);
                        syncServerPacket.WriteH(num36);
                        syncServerPacket.WriteH((short)10000);
                    }
                    syncServerPacket.WriteH(num38);
                    syncServerPacket.WriteH(num39);
                    syncServerPacket.WriteH(num40);
                    syncServerPacket.WriteC(num41);
                    syncServerPacket.WriteD(num42);
                    syncServerPacket.WriteC(num43);
                    syncServerPacket.WriteC(num44);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.GetWeaponForClient))
                {
                    num1 += 16777216U /*0x01000000*/;
                    ushort num45 = syncClientPacket.ReadUH();
                    ushort num46 = syncClientPacket.ReadUH();
                    ushort num47 = syncClientPacket.ReadUH();
                    ushort num48 = syncClientPacket.ReadUH();
                    ushort num49 = syncClientPacket.ReadUH();
                    ushort num50 = syncClientPacket.ReadUH();
                    byte num51 = syncClientPacket.ReadC();
                    int num52 = syncClientPacket.ReadD();
                    byte num53 = syncClientPacket.ReadC();
                    byte num54 = syncClientPacket.ReadC();
                    if (ConfigLoader.UseMaxAmmoInDrop)
                    {
                        syncServerPacket.WriteH(ushort.MaxValue);
                        syncServerPacket.WriteH(num46);
                        syncServerPacket.WriteH((short)10000);
                    }
                    else
                    {
                        syncServerPacket.WriteH(num45);
                        syncServerPacket.WriteH(num46);
                        syncServerPacket.WriteH(num47);
                    }
                    syncServerPacket.WriteH(num48);
                    syncServerPacket.WriteH(num49);
                    syncServerPacket.WriteH(num50);
                    syncServerPacket.WriteC(num51);
                    syncServerPacket.WriteD(num52);
                    syncServerPacket.WriteC(num53);
                    syncServerPacket.WriteC(num54);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.FireData))
                {
                    num1 += 33554432U /*0x02000000*/;
                    byte num55 = syncClientPacket.ReadC();
                    byte num56 = syncClientPacket.ReadC();
                    short num57 = syncClientPacket.ReadH();
                    int num58 = syncClientPacket.ReadD();
                    byte num59 = syncClientPacket.ReadC();
                    byte num60 = syncClientPacket.ReadC();
                    ushort num61 = syncClientPacket.ReadUH();
                    ushort num62 = syncClientPacket.ReadUH();
                    ushort num63 = syncClientPacket.ReadUH();
                    syncServerPacket.WriteC(num55);
                    syncServerPacket.WriteC(num56);
                    syncServerPacket.WriteH(num57);
                    syncServerPacket.WriteD(num58);
                    syncServerPacket.WriteC(num59);
                    syncServerPacket.WriteC(num60);
                    syncServerPacket.WriteH(num61);
                    syncServerPacket.WriteH(num62);
                    syncServerPacket.WriteH(num63);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.CharaFireNHitData))
                {
                    num1 += 1024U /*0x0400*/;
                    byte num64 = syncClientPacket.ReadC();
                    syncServerPacket.WriteC(num64);
                    for (int index = 0; index < (int)num64; ++index)
                    {
                        int num65 = syncClientPacket.ReadD();
                        byte num66 = syncClientPacket.ReadC();
                        byte num67 = syncClientPacket.ReadC();
                        ushort num68 = syncClientPacket.ReadUH();
                        uint num69 = syncClientPacket.ReadUD();
                        ushort num70 = syncClientPacket.ReadUH();
                        ushort num71 = syncClientPacket.ReadUH();
                        ushort num72 = syncClientPacket.ReadUH();
                        syncServerPacket.WriteD(num65);
                        syncServerPacket.WriteC(num66);
                        syncServerPacket.WriteC(num67);
                        syncServerPacket.WriteH(num68);
                        syncServerPacket.WriteD(num69);
                        syncServerPacket.WriteH(num70);
                        syncServerPacket.WriteH(num71);
                        syncServerPacket.WriteH(num72);
                    }
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.GetWeaponForHost))
                {
                    num1 += 512U /*0x0200*/;
                    CharaDeath charaDeath = (CharaDeath)syncClientPacket.ReadC();
                    byte num73 = syncClientPacket.ReadC();
                    ushort num74 = syncClientPacket.ReadUH();
                    ushort num75 = syncClientPacket.ReadUH();
                    ushort num76 = syncClientPacket.ReadUH();
                    int num77 = syncClientPacket.ReadD();
                    CharaHitPart charaHitPart = (CharaHitPart)syncClientPacket.ReadC();
                    syncServerPacket.WriteC((byte)charaDeath);
                    syncServerPacket.WriteC(num73);
                    syncServerPacket.WriteH(num74);
                    syncServerPacket.WriteH(num75);
                    syncServerPacket.WriteH(num76);
                    syncServerPacket.WriteD(num77);
                    syncServerPacket.WriteC((byte)charaHitPart);
                }
                if (A_1.Flag.HasFlag((System.Enum)UdpGameEvent.FireDataOnObject))
                {
                    num1 += 1073741824U /*0x40000000*/;
                    byte num78 = syncClientPacket.ReadC();
                    CharaHitPart charaHitPart = (CharaHitPart)syncClientPacket.ReadC();
                    byte num79 = syncClientPacket.ReadC();
                    syncServerPacket.WriteC(num78);
                    syncServerPacket.WriteC((byte)charaHitPart);
                    syncServerPacket.WriteC(num79);
                }
                A_3 = syncServerPacket.ToArray();
                if ((UdpGameEvent)num1 != A_1.Flag)
                    CLogger.Print(Bitwise.ToHexData($"PVE - Missing Flag Events: '{(ValueType)(uint)A_1.Flag}' | '{(ValueType)(uint)(A_1.Flag - num1)}'", data), LoggerType.Opcode);
                return Objs;
            }
        }

        public byte[] WriteBotActionData(byte[] Data, RoomModel Room)
        {
            SyncClientPacket syncClientPacket = new SyncClientPacket(Data);
            List<ObjectHitInfo> Objs = new List<ObjectHitInfo>();
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                for (int index = 0; index < 18; ++index)
                {
                    ActionModel A_1 = new ActionModel();
                    try
                    {
                        bool flag = false;
                        bool Exception;
                        A_1.Length = syncClientPacket.ReadUH(out Exception);
                        if (!Exception)
                        {
                            A_1.Slot = syncClientPacket.ReadUH();
                            A_1.SubHead = (UdpSubHead)syncClientPacket.ReadC();
                            if (A_1.SubHead != (UdpSubHead)255 /*0xFF*/)
                            {
                                syncServerPacket.WriteH(A_1.Length);
                                syncServerPacket.WriteH(A_1.Slot);
                                syncServerPacket.WriteC((byte)A_1.SubHead);
                                switch (A_1.SubHead)
                                {
                                    case UdpSubHead.User:
                                    case UdpSubHead.StageInfoChara:
                                        A_1.Flag = (UdpGameEvent)syncClientPacket.ReadUD();
                                        A_1.Data = syncClientPacket.ReadB((int)A_1.Length - 9);
                                        byte[] A_3;
                                        Objs.AddRange((IEnumerable<ObjectHitInfo>)this.Method1(A_1, Room, out A_3));
                                        syncServerPacket.GoBack(5);
                                        syncServerPacket.WriteH((ushort)(A_3.Length + 9));
                                        syncServerPacket.WriteH(A_1.Slot);
                                        syncServerPacket.WriteC((byte)A_1.SubHead);
                                        syncServerPacket.WriteD((uint)A_1.Flag);
                                        syncServerPacket.WriteB(A_3);
                                        if (A_1.Data.Length == 0 && (int)A_1.Length - 9 != 0)
                                        {
                                            flag = true;
                                            break;
                                        }
                                        break;

                                    case UdpSubHead.Grenade:
                                        byte num1 = syncClientPacket.ReadC();
                                        byte num2 = syncClientPacket.ReadC();
                                        byte num3 = syncClientPacket.ReadC();
                                        byte num4 = syncClientPacket.ReadC();
                                        ushort num5 = syncClientPacket.ReadUH();
                                        int num6 = syncClientPacket.ReadD();
                                        byte num7 = syncClientPacket.ReadC();
                                        byte num8 = syncClientPacket.ReadC();
                                        ushort num9 = syncClientPacket.ReadUH();
                                        ushort num10 = syncClientPacket.ReadUH();
                                        ushort num11 = syncClientPacket.ReadUH();
                                        int num12 = syncClientPacket.ReadD();
                                        int num13 = syncClientPacket.ReadD();
                                        int num14 = syncClientPacket.ReadD();
                                        syncServerPacket.WriteC(num1);
                                        syncServerPacket.WriteC(num2);
                                        syncServerPacket.WriteC(num3);
                                        syncServerPacket.WriteC(num4);
                                        syncServerPacket.WriteH(num5);
                                        syncServerPacket.WriteD(num6);
                                        syncServerPacket.WriteC(num7);
                                        syncServerPacket.WriteC(num8);
                                        syncServerPacket.WriteH(num9);
                                        syncServerPacket.WriteH(num10);
                                        syncServerPacket.WriteH(num11);
                                        syncServerPacket.WriteD(num12);
                                        syncServerPacket.WriteD(num13);
                                        syncServerPacket.WriteD(num14);
                                        break;

                                    case UdpSubHead.DroppedWeapon:
                                        byte[] numArray1 = syncClientPacket.ReadB(31 /*0x1F*/);
                                        syncServerPacket.WriteB(numArray1);
                                        break;

                                    case UdpSubHead.ObjectStatic:
                                        byte[] numArray2 = syncClientPacket.ReadB(10);
                                        syncServerPacket.WriteB(numArray2);
                                        break;

                                    case UdpSubHead.ObjectMove:
                                        byte[] numArray3 = syncClientPacket.ReadB(16 /*0x10*/);
                                        syncServerPacket.WriteB(numArray3);
                                        break;

                                    case UdpSubHead.ObjectAnim:
                                        byte[] numArray4 = syncClientPacket.ReadB(8);
                                        syncServerPacket.WriteB(numArray4);
                                        break;

                                    case UdpSubHead.StageInfoObjectStatic:
                                        byte[] numArray5 = syncClientPacket.ReadB(1);
                                        syncServerPacket.WriteB(numArray5);
                                        break;

                                    case UdpSubHead.StageInfoObjectAnim:
                                        byte[] numArray6 = syncClientPacket.ReadB(9);
                                        syncServerPacket.WriteB(numArray6);
                                        break;

                                    case UdpSubHead.StageInfoObjectControl:
                                        byte[] numArray7 = syncClientPacket.ReadB(9);
                                        syncServerPacket.WriteB(numArray7);
                                        break;

                                    default:
                                        CLogger.Print(Bitwise.ToHexData($"PVP Sub Head: '{A_1.SubHead}' or '{(int)A_1.SubHead}'", Data), LoggerType.Opcode);
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
                        CLogger.Print($"PVE Action Data - Buffer (Length: {Data.Length}): | {ex.Message}", LoggerType.Error, ex);
                        Objs = new List<ObjectHitInfo>();
                        break;
                    }
                }
                if (Objs.Count > 0)
                    syncServerPacket.WriteB(PROTOCOL_EVENTS_ACTION.GET_CODE(Objs));
                return syncServerPacket.ToArray();
            }
        }

        public void SendPacket(byte[] Data, IPEndPoint Address)
        {
            try
            {
                this.MainSocket.BeginSendTo(Data, 0, Data.Length, SocketFlags.None, (EndPoint)Address, new AsyncCallback(this.Method2), (object)this.MainSocket);
            }
            catch (Exception ex)
            {
                CLogger.Print($"Failed to send package to {Address}: {ex.Message}", LoggerType.Error, ex);
            }
        }

        private void Method2(IAsyncResult A_1)
        {
            try
            {
                if (!(A_1.AsyncState is Socket asyncState))
                    return;
                asyncState.EndSendTo(A_1);
            }
            catch (SocketException ex)
            {
                CLogger.Print($"Socket Error on Send: {ex.SocketErrorCode}", LoggerType.Warning, ex);
            }
            catch (ObjectDisposedException ex)
            {
                CLogger.Print("Socket was closed while sending.", LoggerType.Warning, ex);
            }
            catch (Exception ex)
            {
                CLogger.Print("Error during EndSendCallback: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}