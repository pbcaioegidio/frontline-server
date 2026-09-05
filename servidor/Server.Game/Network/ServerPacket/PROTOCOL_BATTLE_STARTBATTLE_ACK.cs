using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BATTLE_STARTBATTLE_ACK : GameServerPacket
    {
        private readonly RoomModel Room;
        private readonly SlotModel Slot;
        private readonly bool Type;
        private readonly List<int> Dinos;

        public PROTOCOL_BATTLE_STARTBATTLE_ACK(SlotModel Slot, Account Player, List<int> Dinos, bool Type)
        {
            this.Slot = Slot;
            Room = Player.Room;
            this.Type = Type;
            this.Dinos = Dinos;
            if (Room != null && Player != null && Slot != null && !Room.IsBotMode() && Room.RoomType != RoomCondition.Tutorial)
            {
                AllUtils.CompleteMission(Room, Slot, Type ? MissionType.STAGE_ENTER : MissionType.STAGE_INTERCEPT, 0);
            }
        }

        public PROTOCOL_BATTLE_STARTBATTLE_ACK()
        {
        }

        public override void Write()
        {
            try
            {
                if (Room == null)
                {
                    CLogger.Print("PROTOCOL_BATTLE_STARTBATTLE_ACK: Room is null", LoggerType.Error);
                    return;
                }

                WriteH(5132);
                WriteH(0);
                WriteD(0);
                WriteC(0);
                WriteC(0);
                WriteB(DinoData(Room, Dinos));
                WriteC(0);
                WriteD(0);
                WriteC(2);
                WriteH((ushort)Room.FRRounds);
                WriteH((ushort)Room.CTRounds);
                WriteC(2);
                WriteH((ushort)Room.FRRounds);
                WriteH((ushort)Room.CTRounds);
                WriteD(0);
                WriteC(0);
                WriteC((byte)Slot.Id);
            }
            catch (Exception ex)
            {
                CLogger.Print($"PROTOCOL_BATTLE_STARTBATTLE_ACK: {ex.Message}", LoggerType.Error, ex);
            }
        }

        private byte[] DinoData(RoomModel Slot, List<int> Player)
        {
            if (Room == null || Dinos == null)
            {
                return new byte[10]; // Devolver array de bytes por defecto
            }

            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                if (Slot.IsDinoMode())
                {
                    int TRex = Player.Count == 1 || Slot.IsDinoMode("CC") ? (int)byte.MaxValue : Slot.TRex;
                    syncServerPacket.WriteC((byte)TRex);
                    syncServerPacket.WriteC(10);
                    for (int index = 0; index < Player.Count; ++index)
                    {
                        int num2 = Player[index];
                        if (num2 != Slot.TRex && Slot.IsDinoMode("DE") || Slot.IsDinoMode("CC"))
                        {
                            syncServerPacket.WriteC((byte)num2);
                        }
                    }
                    int num3 = 8 - Player.Count - (TRex == (int)byte.MaxValue ? 1 : 0);
                    for (int index = 0; index < num3; ++index)
                    {
                        syncServerPacket.WriteC(byte.MaxValue);
                    }
                    syncServerPacket.WriteC(byte.MaxValue);
                }
                else
                {
                    syncServerPacket.WriteB(new byte[10]);
                }
                return syncServerPacket.ToArray();
            }
        }
    }
}