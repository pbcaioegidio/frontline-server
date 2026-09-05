// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BATTLE_MISSION_ROUND_END_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BATTLE_MISSION_ROUND_END_ACK : GameServerPacket
    {
        private readonly RoomModel Field0;
        private readonly int Field1;
        private readonly RoundEndType Field2;

        public PROTOCOL_BATTLE_MISSION_ROUND_END_ACK(RoomModel A_1, int A_2, RoundEndType A_3)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
            this.Field2 = A_3;
        }

        public PROTOCOL_BATTLE_MISSION_ROUND_END_ACK(RoomModel A_1, TeamEnum A_2, RoundEndType A_3)
        {
            this.Field0 = A_1;
            this.Field1 = (int)A_2;
            this.Field2 = A_3;
        }

        
        public override void Write()
        {
            this.WriteH((short)5155);
            this.WriteC((byte)this.Field1);
            this.WriteC((byte)this.Field2);
            if (this.Field0.IsDinoMode("DE"))
            {
                this.WriteH((ushort)this.Field0.FRDino);
                this.WriteH((ushort)this.Field0.CTDino);
            }
            else if (this.Field0.RoomType != RoomCondition.DeathMatch && this.Field0.RoomType != RoomCondition.FreeForAll)
            {
                this.WriteH((ushort)this.Field0.FRRounds);
                this.WriteH((ushort)this.Field0.CTRounds);
            }
            else
            {
                this.WriteH((ushort)this.Field0.FRKills);
                this.WriteH((ushort)this.Field0.CTKills);
            }
            
            // Reset FirstRespawn for observers at round end to allow proper respawn in next round
            foreach (var slot in this.Field0.Slots)
            {
                if (slot != null && slot.SpecGM && !slot.FirstRespawn)
                {
                    slot.FirstRespawn = true;
                    CLogger.Print($"[RESPAWN DEBUG] Reset FirstRespawn for Observer in slot {slot.Id} at round end", LoggerType.Debug);
                }
            }
        }
    }
}