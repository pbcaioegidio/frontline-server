// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BATTLE_NEW_JOIN_ROOM_SCORE_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Network;
using Server.Game.Data.Models;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BATTLE_NEW_JOIN_ROOM_SCORE_ACK : GameServerPacket
    {
        private readonly RoomModel Field0;

        public PROTOCOL_BATTLE_NEW_JOIN_ROOM_SCORE_ACK(RoomModel A_1) => this.Field0 = A_1;

        public override void Write()
        {
            this.WriteH((short)5263);
            this.WriteD(this.Method1());
            this.WriteD(this.Field0.GetTimeByMask() * 60 - this.Field0.GetInBattleTime());
            this.WriteB(this.Method0(this.Field0));
        }

        
        private byte[] Method0(RoomModel A_1)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                if (A_1.IsDinoMode("DE"))
                {
                    syncServerPacket.WriteD(A_1.FRDino);
                    syncServerPacket.WriteD(A_1.CTDino);
                }
                else if (A_1.RoomType == RoomCondition.DeathMatch && !A_1.IsBotMode())
                {
                    syncServerPacket.WriteD(A_1.FRKills);
                    syncServerPacket.WriteD(A_1.CTKills);
                }
                else if (A_1.RoomType != RoomCondition.FreeForAll)
                {
                    if (!A_1.IsBotMode())
                    {
                        syncServerPacket.WriteD(A_1.FRRounds);
                        syncServerPacket.WriteD(A_1.CTRounds);
                    }
                    else
                    {
                        syncServerPacket.WriteD((int)A_1.IngameAiLevel);
                        syncServerPacket.WriteD(0);
                    }
                }
                else
                {
                    syncServerPacket.WriteD(this.GetSlotKill());
                    syncServerPacket.WriteD(0);
                }
                return syncServerPacket.ToArray();
            }
        }

        private int Method1()
        {
            if (this.Field0.IsBotMode())
                return 3;
            if (this.Field0.RoomType == RoomCondition.DeathMatch && !this.Field0.IsBotMode())
                return 1;
            if (this.Field0.IsDinoMode())
                return 4;
            return this.Field0.RoomType == RoomCondition.FreeForAll ? 5 : 2;
        }

        public int GetSlotKill()
        {
            int[] numArray = new int[18];
            for (int index = 0; index < numArray.Length; ++index)
                numArray[index] = this.Field0.Slots[index].AllKills;
            int slotKill = 0;
            for (int index = 0; index < numArray.Length; ++index)
            {
                if (numArray[index] > numArray[slotKill])
                    slotKill = index;
            }
            return slotKill;
        }
    }
}