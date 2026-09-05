using Plugin.Core.Enums;
using Plugin.Core.Utility;
using System;
using System.Collections.Generic;

namespace Plugin.Core.Models
{
    public class SlotModel
    {
        public int Id;
        public int Flag;
        public int AiLevel;
        public int Latency;
        public int FailLatencyTimes;
        public int Ping;
        public int PassSequence;
        public int IsPlaying;
        public int EarnedEXP;
        public int SpawnsCount;
        public int AllHeadshots;
        public int LastKillState;
        public int KillsOnLife;
        public int Exp;
        public int Cash;
        public int Gold;
        public int Score;
        public int AllKills;
        public int AllDeaths;
        public int AllAssists;
        public int Objects;
        public int BonusItemExp;
        public int BonusItemPoint;
        public int BonusEventExp;
        public int BonusEventPoint;
        public int BonusCafePoint;
        public int BonusCafeExp;
        public int BonusBattlePass;
        public int UnkItem;

        public int SeasonPoint;

        public int BonusSeasonPoint;

        public int Tags;

        public ushort DamageBar1;
        public ushort DamageBar2;
        public long PlayerId;
        public bool Check;
        public bool SpecGM;
        public bool WithHost;
        public bool Spectator;
        public bool RepeatLastState;
        public bool MissionsCompleted;
        public bool FirstRespawn = true;
        public bool FistInactivityOff;
        public bool FirstInactivityOff;
        public bool IsMercenary;
        public TeamEnum Team;
        public TeamEnum CostumeTeam;
        public SlotState State;
        public ViewerType ViewType;
        public ResultIcon BonusFlags;
        public DeadEnum DeathState = DeadEnum.Alive;
        public DateTime NextVoteDate;
        public DateTime StartTime;
        public DateTime PreStartDate;
        public DateTime PreLoadDate;
        /// <summary>Último sync de morte Match→Game (anti-flood).</summary>
        public DateTime LastDeathSync;
        /// <summary>Anti-burst (modo bot / desafio): janela de kills.</summary>
        public DateTime KillWindowStart;
        public int KillsInWindow;
        public int HeadshotsInWindow;
        public int KillBurstViolations;
        public PlayerEquipment Equipment;
        public PlayerMissions Missions;
        public SlotModel NewSlot;
        public SlotModel OldSlot;
        public int[] AR = new int[2] { 0, 0 };
        public int[] SMG = new int[2] { 0, 0 };
        public int[] SR = new int[2] { 0, 0 };
        public int[] MG = new int[2] { 0, 0 };
        public int[] SHD = new int[2] { 0, 0 };
        public int[] SG = new int[2] { 0, 0 };
        public List<int> ItemUsages = new List<int>();
        public TimerState Timing = new TimerState();


        public SlotModel(int slotIdx)
        {
            SetSlotId(slotIdx);
        }
        public SlotModel(SlotModel NewSlot, SlotModel OldSlot)
        {
            this.NewSlot = ObjectCopier.Copy(NewSlot);
            this.OldSlot = ObjectCopier.Copy(OldSlot);
        }
        public void SetSlotId(int SlotIdx)
        {
            Id = SlotIdx;
            Team = (TeamEnum)(SlotIdx % 2);
            Flag = 1 << SlotIdx;
        }
        public void StopTiming()
        {
            if (Timing != null)
            {
                Timing.StopJob();
            }
        }
        public void ResetSlot()
        {
            StopTiming();
            DeathState = DeadEnum.Alive;
            FailLatencyTimes = 0;
            Latency = 0;
            Ping = 5;
            PassSequence = 0;
            AllKills = 0;
            AllHeadshots = 0;
            AllDeaths = 0;
            AllAssists = 0;
            BonusFlags = 0;
            KillsOnLife = 0;
            LastKillState = 0;
            Score = 0;
            Gold = 0;
            Exp = 0;
            Cash = 0;
            Objects = 0;
            SeasonPoint = 0;
            BonusItemExp = 0;
            BonusItemPoint = 0;
            BonusCafeExp = 0;
            BonusCafePoint = 0;
            BonusEventExp = 0;
            BonusEventPoint = 0;
            BonusSeasonPoint = 0;
            SpawnsCount = 0;
            DamageBar1 = 0;
            DamageBar2 = 0;
            EarnedEXP = 0;
            IsPlaying = 0;
            AiLevel = 0;
            NextVoteDate = new DateTime();
            Check = false;
            SpecGM = false;
            WithHost = false;
            Spectator = false;
            FirstRespawn = true;
            RepeatLastState = false;
            MissionsCompleted = false;
            FistInactivityOff = false;
            Missions = null;
            ItemUsages.Clear();
            Array.Clear(AR, 0, AR.Length);
            Array.Clear(SMG, 0, SMG.Length);
            Array.Clear(SR, 0, SR.Length);
            Array.Clear(SG, 0, SG.Length);
            Array.Clear(MG, 0, MG.Length);
        }
        public void SetMissionsClone(PlayerMissions Missions)
        {
            MissionsCompleted = false;
            this.Missions = null;
            this.Missions = ObjectCopier.Copy(Missions);
        }
        public double InBattleTime(DateTime Date)
        {
            if (StartTime == new DateTime())// || startTime > date)
            {
                return 0;
            }
            return (Date - StartTime).TotalSeconds;
        }
        public byte GetClientState()
        {
            if (State == SlotState.PLAYING)
                return 18;
            if (State == SlotState.BATTLE)
                // Client TAB/scoreboard gate (CHUD::ShouldShowChallengeScore -> sub_D31AF6) only
                // renders a non-AI slot when m_Slots[slot].state == 19. For a battle-playing player
                // (BATTLE + IsPlaying==2) emit 19; emitting 18 (BATTLE_READY) hid the local player
                // from the scoreboard in solo AI DeathMatch.
                return (byte)(IsPlaying == 2 ? 19 : 16);
            if (State >= SlotState.NORMAL)
                return (byte)((byte)State + 3);
            return (byte)State;
        }
    }
}