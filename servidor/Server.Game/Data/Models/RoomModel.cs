//v3.80

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Game.Data.Managers;
using Server.Game.Data.Sync.Server;
using Server.Game.Data.Utils;
using Server.Game.Data.XML;
using Server.Game.Network;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Server.Game.Data.Models
{
    public class RoomModel
    {
        public SlotModel[] Slots = new SlotModel[18];
        public int Rounds;
        public int TRex;
        public int CTRounds;
        public int CTDino;
        public int FRRounds;
        public int FRDino;
        public int Bar1;
        public int Bar2;
        public int Ping;
        public int FRKills;
        public int FRDeaths;
        public int FRAssists;
        public int CTKills;
        public int CTDeaths;
        public int CTAssists;
        public int SpawnsCount;
        public int KillTime;
        public int RoomId;
        public int ChannelId;
        public int ServerId;
        public int Leader;
        public int CountPlayers;
        public int CountMaxSlots;
        public int NewInt;
        public byte Limit;
        public byte WatchRuleFlag;
        public byte AiCount;
        public byte IngameAiLevel;
        public byte AiLevel;
        public byte AiType;
        public byte CountdownIG = 5;
        public byte KillCam;
        public uint TimeRoom;
        public uint StartDate;
        public uint UniqueRoomId;
        public uint Seed;
        public long StartTick;
        public string Name;
        public string Password;
        public string MapName;
        public string LeaderName;
        public bool ActiveC4;
        public bool ChangingSlots;
        public bool BlockedClan;
        public bool PreMatchCD;
        public bool SwapRound;
        public bool Competitive;

        public bool RoundEnding;

        public readonly int[] TIMES = new int[11]
        {
            3,
            3,
            3,
            5,
            7,
            5,
            10,
            15,
            20,
            25,
            30
        };

        public readonly int[] KILLS = new int[9]
        {
            15,
            30,
            50,
            60,
            80 /*0x50*/,
            100,
            120,
            140,
            160 /*0xA0*/
        };

        public readonly int[] ROUNDS = new int[6]
        {
            1,
            2,
            3,
            5,
            7,
            9
        };

        public readonly int[] SWAP_ROUNDS = new int[6]
        {
            1,
            2,
            2,
            4,
            6,
            8
        };

        public readonly int[] FR_TEAM = new int[9]
        {
            0,
            2,
            4,
            6,
            8,
            10,
            12,
            14,
            16
        };

        public readonly int[] CT_TEAM = new int[9]
        {
            1,
            3,
            5,
            7,
            9,
            11,
            13,
            15,
            17
        };

        public readonly int[] ALL_TEAM = new int[18]
        {
            0,
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            10,
            11,
            12,
            13,
            14,
            15,
            16 /*0x10*/,
            17
        };

        public readonly int[] INVERT_FR_TEAM = new int[9]
        {
            16 /*0x10*/,
            14,
            12,
            10,
            8,
            6,
            4,
            2,
            0
        };

        public readonly int[] INVERT_CT_TEAM = new int[9]
        {
            17,
            15,
            13,
            11,
            9,
            7,
            5,
            3,
            1
        };

        public byte[] RandomMaps = new byte[24];
        public byte[] LeaderAddr = new byte[4];

        public byte[] HitParts = new byte[35]
        {
            (byte) 0,
            (byte) 1,
            (byte) 2,
            (byte) 3,
            (byte) 4,
            (byte) 5,
            (byte) 6,
            (byte) 7,
            (byte) 8,
            (byte) 9,
            (byte) 10,
            (byte) 11,
            (byte) 12,
            (byte) 13,
            (byte) 14,
            (byte) 15,
            (byte) 16 /*0x10*/,
            (byte) 17,
            (byte) 18,
            (byte) 19,
            (byte) 20,
            (byte) 21,
            (byte) 22,
            (byte) 23,
            (byte) 24,
            (byte) 25,
            (byte) 26,
            (byte) 27,
            (byte) 28,
            (byte) 29,
            (byte) 30,
            (byte) 31 /*0x1F*/,
            (byte) 32 /*0x20*/,
            (byte) 33,
            (byte) 34
        };

        public (byte[], int[]) SlotRewards;
        public ChannelType ChannelType;
        public MapIdEnum MapId;
        public RoomCondition RoomType;
        public RoomState State;
        public MapRules Rule;
        public StageOptions Stage;
        public TeamBalance BalanceType;
        public RoomStageFlag Flag;
        public RoomWeaponsFlag WeaponsFlag;
        public VoteKickModel VoteKick;
        public Synchronize UdpServer;
        public DateTime BattleStart;
        public DateTime LastPingSync;
        public DateTime LastChangeTeam;
        public TimerState BombTime = new TimerState();
        public TimerState CountdownTime = new TimerState();
        public TimerState RoundTime = new TimerState();
        public TimerState RoundTeamSwap = new TimerState();
        public TimerState VoteTime = new TimerState();
        public TimerState PreMatchTime = new TimerState();
        public SafeList<long> KickedPlayersVote = new SafeList<long>();
        public SafeList<long> RequestRoomMaster = new SafeList<long>();
        public SortedList<long, DateTime> KickedPlayersHost = new SortedList<long, DateTime>();

        public RoomModel(int roomId, ChannelModel channel)
        {
            this.RoomId = roomId;
            for (int index = 0; index < this.Slots.Length; ++index)
            {
                this.Slots[index] = new SlotModel(index);
            }
            this.ChannelId = channel.Id;
            this.ChannelType = channel.Type;
            this.ServerId = channel.ServerId;
            this.Rounds = 1;
            this.AiCount = (byte)1;
            this.AiLevel = (byte)1;
            this.TRex = -1;
            this.Ping = 5;
            this.Name = "";
            this.Password = "";
            this.MapName = "";
            this.LeaderName = "";
            this.GenerateUniqueRoomId();
            DateTime dateTime = DateTimeUtil.Now();
            this.LastChangeTeam = dateTime;
            this.LastPingSync = dateTime;
            this.RoundEnding = false;
        }

        public RoomModel GetRoom()
        {
            RoomModel roomModel = this;

            try
            {
                if (roomModel != null)
                {
                    roomModel = this;
                }
                else
                {
                    CLogger.Print($"RoomModel.GetRoom: RoomModel is null for RoomId: {this.RoomId}", LoggerType.Warning);
                    roomModel = null;
                }
                return roomModel;
            }
            catch (Exception Ex)
            {
                CLogger.Print($"RoomModel.GetRoom Exception: {Ex.Message}", LoggerType.Error, Ex);
                return null;
            }
        }

        private void GenerateUniqueRoomId()
        {
            this.UniqueRoomId = (uint)((this.ServerId & (int)byte.MaxValue) << 20 | (this.ChannelId & (int)byte.MaxValue) << 12 | this.RoomId & 4095 /*0x0FFF*/);
        }

        public void GenerateSeed()
        {
            this.Seed = (uint)((RoomCondition)((int)(this.MapId & (MapIdEnum)255 /*0xFF*/) << 20 | (int)(this.Rule & ~MapRules.None) << 12) | this.RoomType & (RoomCondition)4095 /*0x0FFF*/);
        }

        public bool ThisModeHaveCD()
        {
            return this.RoomType == RoomCondition.Bomb || this.RoomType == RoomCondition.Annihilation || this.RoomType == RoomCondition.Boss || this.RoomType == RoomCondition.CrossCounter || this.RoomType == RoomCondition.Convoy || this.RoomType == RoomCondition.Ace;
        }

        public bool ThisModeHaveRounds()
        {
            return this.RoomType == RoomCondition.Bomb || this.RoomType == RoomCondition.Destroy || this.RoomType == RoomCondition.Annihilation || this.RoomType == RoomCondition.Defense || this.RoomType == RoomCondition.Convoy || this.RoomType == RoomCondition.Ace;
        }

        public int GetFlag()
        {
            int flag = 0;
            if (this.Flag.HasFlag((Enum)RoomStageFlag.TEAM_SWAP))
                ++flag;
            if (this.Flag.HasFlag((Enum)RoomStageFlag.RANDOM_MAP))
                flag += 2;
            if (this.Flag.HasFlag((Enum)RoomStageFlag.PASSWORD) || this.Password.Length > 0)
                flag += 4;
            if (this.Flag.HasFlag((Enum)RoomStageFlag.OBSERVER_MODE))
                flag += 8;
            if (this.Flag.HasFlag((Enum)RoomStageFlag.REAL_IP))
                flag += 16 /*0x10*/;
            if (this.Flag.HasFlag((Enum)RoomStageFlag.TEAM_BALANCE) || this.BalanceType != TeamBalance.None)
                flag += 32 /*0x20*/;
            if (this.Flag.HasFlag((Enum)RoomStageFlag.OBSERVER))
                flag += 64 /*0x40*/;
            if (this.Flag.HasFlag((Enum)RoomStageFlag.INTER_ENTER) || this.Limit > (byte)0 && this.IsStartingMatch())
                flag += 128 /*0x80*/;
            this.Flag = (RoomStageFlag)flag;
            return flag;
        }

        public bool IsBotMode()
        {
            return this.Stage == StageOptions.AI || this.Stage == StageOptions.DieHard || this.Stage == StageOptions.Infection;
        }

        public void SetBotLevel()
        {
            if (!this.IsBotMode())
                return;
            this.IngameAiLevel = this.AiLevel;
            for (int index = 0; index < 18; ++index)
                this.Slots[index].AiLevel = (int)this.IngameAiLevel;
        }

        private void SetGameModeSpecificBars()
        {
            if (this.RoomType == RoomCondition.Defense)
            {
                if (this.MapId != MapIdEnum.BlackPanther)
                    return;
                this.Bar1 = 6000;
                this.Bar2 = 9000;
            }
            else
            {
                if (this.RoomType != RoomCondition.Destroy)
                    return;
                if (this.MapId != MapIdEnum.Hospital)
                {
                    if (this.MapId != MapIdEnum.BreakDown)
                        return;
                    this.Bar1 = 6000;
                    this.Bar2 = 6000;
                }
                else
                {
                    this.Bar1 = 12000;
                    this.Bar2 = 12000;
                }
            }
        }

        public int GetInBattleTime()
        {
            int inBattleTime = 0;
            if (this.BattleStart != new DateTime() && (this.State == RoomState.BATTLE || this.State == RoomState.PRE_BATTLE))
            {
                inBattleTime = (int)ComDiv.GetDuration(this.BattleStart);
                if (inBattleTime < 0)
                    inBattleTime = 0;
            }
            return inBattleTime;
        }

        public int GetInBattleTimeLeft() => this.GetTimeByMask() * 60 - this.GetInBattleTime();

        public ChannelModel GetChannel() => ChannelsXML.GetChannel(this.ServerId, this.ChannelId);

        public bool GetChannel(out ChannelModel Channel)
        {
            Channel = ChannelsXML.GetChannel(this.ServerId, this.ChannelId);
            return Channel != null;
        }

        public bool GetSlot(int SlotId, out SlotModel Slot)
        {
            Slot = (SlotModel)null;
            lock (this.Slots)
            {
                if (SlotId >= 0 && SlotId <= 17)
                    Slot = this.Slots[SlotId];
                return Slot != null;
            }
        }

        public SlotModel GetSlot(int SlotIdx)
        {
            lock (this.Slots)
                return SlotIdx >= 0 && SlotIdx <= 17 ? this.Slots[SlotIdx] : (SlotModel)null;
        }

        public void StartCounter(int Type, Account Player, SlotModel Slot)
        {
            InactivityHandler handler = new InactivityHandler();
            handler.slot = Slot;
            handler.room = this;
            handler.player = Player;
            int Period = 0;
            handler.errorType = EventErrorEnum.SUCCESS;
            switch (Type)
            {
                case 0:
                    handler.errorType = EventErrorEnum.BATTLE_FIRST_MAINLOAD;
                    Period = 90000;
                    break;

                case 1:
                    handler.errorType = EventErrorEnum.BATTLE_FIRST_HOLE;
                    Period = 30000;
                    break;
            }
            if (Period > 0)
            {
                handler.slot.FirstInactivityOff = true;
            }
            handler.slot.Timing.StartJob(Period, new TimerCallback(handler.HandleInactivity));
        }

        public void KickInactivePlayer(EventErrorEnum errorType, Account player, SlotModel slot)
        {
            player.SendPacket(new PROTOCOL_SERVER_MESSAGE_KICK_BATTLE_PLAYER_ACK(errorType));
            player.SendPacket(new PROTOCOL_BATTLE_GIVEUPBATTLE_ACK(player, 0));
            this.ChangeSlotState(slot.Id, SlotState.NORMAL, true);
            AllUtils.BattleEndPlayersCount(this, this.IsBotMode());
        }

        public void StartBomb()
        {
            this.BombTime.StartJob(42000, (TimerCallback)(timer =>
            {
                // Verificar que la sala sigue activa y el C4 sigue plantado
                if (this != null && this.ActiveC4 && !this.RoundEnding)
                {
                    // Marcar que la ronda está terminando
                    this.RoundEnding = true;

                    // Determinar qué equipo gana según el swap
                    TeamEnum winnerTeam;
                    if (this.IsTeamSwap() && this.SwapRound)
                    {
                        // Después del swap: CT planta, si explota gana CT
                        ++this.CTRounds;
                        winnerTeam = TeamEnum.CT_TEAM;
                    }
                    else
                    {
                        // Normal: FR planta, si explota gana FR
                        ++this.FRRounds;
                        winnerTeam = TeamEnum.FR_TEAM;
                    }

                    this.ActiveC4 = false;

                    CLogger.Print($"C4 EXPLOTÓ - Winner:{winnerTeam}, " +
                                 $"FR:{this.FRRounds}, CT:{this.CTRounds}, " +
                                 $"SwapRound:{this.SwapRound}",
                                 LoggerType.Warning);

                    AllUtils.BattleEndRound(this, winnerTeam, RoundEndType.BombFire);
                }

                lock (timer)
                    this.BombTime.StopJob();
            }));
        }

        public void StartVote()
        {
            if (this.VoteKick == null)
                return;
            this.VoteTime.StartJob(20000, (TimerCallback)(timer =>
            {
                AllUtils.VotekickResult(this);
                lock (timer)
                    this.VoteTime.StopJob();
            }));
        }

        public void RoundRestart()
        {
            // Resetear flag de ronda terminando
            this.RoundEnding = false;

            this.StopBomb();
            foreach (SlotModel slot in this.Slots)
            {
                if (slot.PlayerId > 0L && slot.State == SlotState.BATTLE)
                {
                    if (!slot.DeathState.HasFlag((Enum)DeadEnum.UseChat))
                        slot.DeathState |= DeadEnum.UseChat;
                    if (slot.Spectator)
                        slot.Spectator = false;
                    if (slot.KillsOnLife >= 3 &&
                        (this.RoomType == RoomCondition.Annihilation ||
                         this.RoomType == RoomCondition.Ace))
                        ++slot.Objects;
                    slot.KillsOnLife = 0;
                    slot.LastKillState = 0;
                    slot.RepeatLastState = false;
                    slot.DamageBar1 = (ushort)0;
                    slot.DamageBar2 = (ushort)0;
                }
            }

            this.RoundTime.StartJob(8000, (TimerCallback)(timer =>
            {
                this.StartNewRound();
                lock (timer)
                    this.RoundTime.StopJob();
            }));
        }

        private void StartNewRound()
        {
            foreach (SlotModel slot in this.Slots)
            {
                if (slot.PlayerId > 0L)
                {
                    if (!slot.DeathState.HasFlag((Enum)DeadEnum.UseChat))
                        slot.DeathState |= DeadEnum.UseChat;
                    if (slot.Spectator)
                        slot.Spectator = false;
                }
            }

            this.StopBomb();
            DateTime dateTime = DateTimeUtil.Now();
            if (this.State == RoomState.BATTLE)
                this.BattleStart = this.IsDinoMode() ? dateTime.AddSeconds(5.0) : dateTime;

            using (PROTOCOL_BATTLE_MISSION_ROUND_PRE_START_ACK Packet =
                new PROTOCOL_BATTLE_MISSION_ROUND_PRE_START_ACK(this, AllUtils.GetDinossaurs(this, false, -1)))
                this.SendPacketToPlayers(Packet, SlotState.BATTLE, 0);

            // Verificar si es momento de hacer swap
            if (this.Flag.HasFlag(RoomStageFlag.TEAM_SWAP) &&
                !this.SwapRound &&
                this.FRRounds + this.CTRounds == this.GetSwapRoundByMask())
            {
                this.SwapRound = true;

                //CLogger.Print($"ACTIVANDO SWAP en ronda {this.Rounds}. FR:{this.FRRounds}, CT:{this.CTRounds}", LoggerType.Debug);

                this.RoundTeamSwap.StartJob(5250, (TimerCallback)(timer =>
                {
                    this.SendRoundStartPacket();
                    lock (timer)
                        this.RoundTeamSwap.StopJob();
                }));
            }
            else
            {
                this.SendRoundStartPacket();
            }

            this.StopBomb();
        }

        private void SendRoundStartPacket()
        {
            using (PROTOCOL_BATTLE_MISSION_ROUND_START_ACK Packet = new PROTOCOL_BATTLE_MISSION_ROUND_START_ACK(this))
                this.SendPacketToPlayers(Packet, SlotState.BATTLE, 0);
        }

        public void StopBomb()
        {
            if (!this.ActiveC4)
                return;
            this.ActiveC4 = false;
            if (this.BombTime == null)
                return;
            this.BombTime.StopJob();
        }

        public void StartBattle(bool UpdateInfo)
        {
            lock (this.Slots)
            {
                this.State = RoomState.LOADING;
                this.RequestRoomMaster.Clear();
                this.SetBotLevel();
                AllUtils.CheckClanMatchRestrict(this);
                this.StartTick = DateTimeUtil.Now().Ticks;
                this.StartDate = uint.Parse(DateTimeUtil.Now("yyMMddHHmm"));
                using (PROTOCOL_BATTLE_START_GAME_ACK battleStartGameAck = new PROTOCOL_BATTLE_START_GAME_ACK(this))
                {
                    byte[] completeBytes = battleStartGameAck.GetCompleteBytes("Room.StartBattle");
                    foreach (Account allPlayer in this.GetAllPlayers(SlotState.READY, 0))
                    {
                        SlotModel slot = this.GetSlot(allPlayer.SlotId);
                        if (slot != null)
                        {
                            slot.WithHost = true;
                            slot.State = SlotState.LOAD;
                            slot.IsMercenary = this.ChannelType == ChannelType.Clan
                                && allPlayer.Match != null && allPlayer.Match.Clan != null
                                && allPlayer.ClanId != allPlayer.Match.Clan.Id;
                            slot.SetMissionsClone(allPlayer.Mission);
                            allPlayer.SendCompletePacket(completeBytes, battleStartGameAck.GetType().Name);
                        }
                    }
                }
                if (UpdateInfo)
                    this.UpdateSlotsInfo();
                this.UpdateRoomInfo();
            }
        }

        public void StartCountDown()
        {
            using (PROTOCOL_BATTLE_START_COUNTDOWN_ACK Packet = new PROTOCOL_BATTLE_START_COUNTDOWN_ACK(CountDownEnum.Start))
                this.SendPacketToPlayers(Packet);
            this.CountdownTime.StartJob(5250, (TimerCallback)(timer =>
            {
                this.OnCountdownComplete();
                lock (timer)
                    this.CountdownTime.StopJob();
            }));
        }

        private void OnCountdownComplete()
        {
            try
            {
                // Periksa apakah room masih dalam keadaan countdown
                if (this.State != RoomState.COUNTDOWN)
                {
                    CLogger.Print($"OnCountdownComplete: Room is not in Countdown state. Current state: {this.State}", LoggerType.Debug);
                    return;
                }

                // Periksa apakah leader masih siap
                if (this.Slots[this.Leader].State != SlotState.READY)
                {
                    CLogger.Print("OnCountdownComplete: Leader is not ready, stopping countdown", LoggerType.Debug);
                    this.StopCountDown(CountDownEnum.StopByHost);
                    return;
                }

                // PERBAIKAN: Periksa jumlah pemain minimum
                int totalPlayers = this.GetCountPlayers();
                int readyPlayers = this.GetReadyPlayers();

                // Pastikan ada minimal 2 pemain total
                if (totalPlayers < 2)
                {
                    CLogger.Print($"OnCountdownComplete: Not enough players to start battle. Total players: {totalPlayers}, Required: 2", LoggerType.Debug);
                    this.StopCountDown(CountDownEnum.StopByPlayer);
                    return;
                }

                // Pastikan ada minimal 2 pemain yang siap
                if (readyPlayers < 2)
                {
                    CLogger.Print($"OnCountdownComplete: Not enough ready players. Ready players: {readyPlayers}, Required: 2", LoggerType.Debug);
                    this.StopCountDown(CountDownEnum.StopByPlayer);
                    return;
                }

                // Untuk mode tim, pastikan kedua tim memiliki pemain
                if (this.RoomType != RoomCondition.FreeForAll)
                {
                    int frPlayers = this.GetPlayingPlayers(TeamEnum.FR_TEAM, SlotState.READY, 0);
                    int ctPlayers = this.GetPlayingPlayers(TeamEnum.CT_TEAM, SlotState.READY, 0);

                    if (frPlayers < 1 || ctPlayers < 1)
                    {
                        CLogger.Print($"OnCountdownComplete: Teams are not balanced. FR players: {frPlayers}, CT players: {ctPlayers}", LoggerType.Debug);
                        this.StopCountDown(CountDownEnum.StopByPlayer);
                        return;
                    }
                }

                // Jika semua kondisi terpenuhi, mulai pertempuran
                CLogger.Print("OnCountdownComplete: Starting battle", LoggerType.Debug);
                this.StartBattle(true);
            }
            catch (Exception ex)
            {
                CLogger.Print($"OnCountdownComplete: {ex.Message}", LoggerType.Error, ex);
            }
        }

        public void StopCountDown(CountDownEnum Type, bool RefreshRoom = true)
        {
            // Hanya hentikan countdown jika room sedang dalam keadaan countdown
            if (this.State != RoomState.COUNTDOWN)
                return;

            // Kembalikan state room ke READY
            this.State = RoomState.READY;

            // Perbarui info room jika diperlukan
            if (RefreshRoom)
                this.UpdateRoomInfo();

            // Hentikan timer countdown
            this.CountdownTime.StopJob();

            // Kirim paket penghentian countdown ke semua pemain
            using (PROTOCOL_BATTLE_START_COUNTDOWN_ACK Packet = new PROTOCOL_BATTLE_START_COUNTDOWN_ACK(Type))
                this.SendPacketToPlayers(Packet);
        }

        public void StopCountDown(int SlotId)
        {
            // Hanya hentikan countdown jika room sedang dalam keadaan countdown
            if (this.State != RoomState.COUNTDOWN)
                return;

            // Jika yang meninggalkan adalah host
            if (SlotId == this.Leader)
            {
                CLogger.Print("StopCountDown: Leader left, stopping countdown", LoggerType.Debug);
                this.StopCountDown(CountDownEnum.StopByHost);
            }
            else
            {
                // Periksa apakah masih ada cukup pemain di tim yang sama dengan host
                TeamEnum leaderTeam = this.Slots[this.Leader].Team;
                if (this.GetPlayingPlayers(leaderTeam, SlotState.READY, 0) == 0)
                {
                    CLogger.Print("StopCountDown: Not enough players in leader's team, stopping countdown", LoggerType.Debug);
                    // Jika tidak ada pemain lain di tim host, kembalikan host ke keadaan normal
                    this.ChangeSlotState(this.Leader, SlotState.NORMAL, false);
                    this.StopCountDown(CountDownEnum.StopByPlayer);
                }
            }
        }

        public void CalculateResult()
        {
            lock (this.Slots)
                this.CalculateBattleResults(AllUtils.GetWinnerTeam(this), this.IsBotMode());
        }

        public void CalculateResult(TeamEnum resultType)
        {
            lock (this.Slots)
                this.CalculateBattleResults(resultType, this.IsBotMode());
        }

        public void CalculateResult(TeamEnum resultType, bool isBotMode)
        {
            lock (this.Slots)
                this.CalculateBattleResults(resultType, isBotMode);
        }

        public void CalculateResultFreeForAll(int SlotWin)
        {
            lock (this.Slots)
                this.CalculateBattleResults((TeamEnum)SlotWin, false);
        }

        private void CalculateBattleResults(TeamEnum winnerTeam, bool isBotMode)
        {
            ServerConfig config = GameXender.Get(this.ServerId)?.Config;
            EventRankUpModel runningEvent1 = EventRankUpXML.GetRunningEvent();
            EventBoostModel runningEvent2 = EventBoostXML.GetRunningEvent();
            EventPlaytimeModel runningEvent3 = EventPlaytimeJSON.GetRunningEvent();
            BattlePassSeason bPass = BattlePassManager.GetActiveSeason();
            DateTime Date = DateTimeUtil.Now();
            int[] numArray = new int[18];
            int index1 = 0;
            if (config != null)
            {
                List<SlotModel> slotModelList = new List<SlotModel>();
                for (int index2 = 0; index2 < 18; ++index2)
                {
                    SlotModel slot = this.Slots[index2];
                    numArray[index2] = slot.PlayerId != 0L ? slot.AllKills : 0;
                    if (numArray[index2] > numArray[index1])
                        index1 = index2;
                    Account Player;
                    if (!slot.Check && slot.State == SlotState.BATTLE && this.GetPlayerBySlot(slot, out Player))
                    {
                        StatisticTotal basic = Player.Statistic.Basic;
                        StatisticSeason season = Player.Statistic.Season;
                        StatisticDaily daily = Player.Statistic.Daily;
                        StatisticWeapon weapon = Player.Statistic.Weapon;
                        DBQuery query = new DBQuery();
                        DBQuery basicStatQuery = new DBQuery();
                        DBQuery seasonStatQuery = new DBQuery();
                        DBQuery dailyStatQuery = new DBQuery();
                        DBQuery weaponStatQuery = new DBQuery();
                        slot.Check = true;
                        double PlayedTime = slot.InBattleTime(Date);
                        int gold = Player.Gold;
                        int exp = Player.Exp;
                        int cash = Player.Cash;
                        if (!isBotMode)
                        {
                            if (config.Missions)
                            {
                                AllUtils.EndMatchMission(this, Player, slot, winnerTeam);
                                if (slot.MissionsCompleted)
                                {
                                    Player.Mission = slot.Missions;
                                    DaoManagerSQL.UpdatePlayerMissionSlotProgress(Player.PlayerId, Player.Mission.ActualMission, Player.Mission.Current.Progress);
                                }
                                AllUtils.GenerateMissionAwards(Player, query);
                            }
                            int num = slot.AllKills != 0 || slot.AllDeaths != 0 ? (int)PlayedTime : (int)(PlayedTime / 3.0);

                            // ==========================================
                            // ✅ CÁLCULO CORREGIDO DE EXPERIENCIA
                            // Aquí está la corrección principal del bug
                            // ==========================================

                            // Paso 1: Calcular score mejorado
                            double calculatedScore = Math.Max(
                                slot.Score * 15,  // Multiplicar Score existente por 15
                                (slot.AllKills * 100) + (slot.AllHeadshots * 50) + (slot.AllAssists * 25)  // Score basado en estadísticas
                            );

                            // Paso 2: Calcular componentes en DOUBLE (evita overflow)
                            double baseScore = calculatedScore;
                            double timeBonus = (double)num / 2.5;
                            double objectBonus = (double)(slot.Objects * 20);
                            double deathBonus;
                            double totalExpDouble;
                            double totalGoldDouble;
                            double totalCashDouble;

                            if (this.RoomType != RoomCondition.Bomb && this.RoomType != RoomCondition.Annihilation && this.RoomType != RoomCondition.Ace)
                            {
                                // Modos normales
                                deathBonus = (double)slot.AllDeaths * 1.8;
                                totalExpDouble = baseScore + timeBonus + deathBonus + objectBonus;
                                totalGoldDouble = calculatedScore + (double)num / 3.0 + deathBonus + objectBonus;
                                totalCashDouble = calculatedScore / 1.5 + (double)num / 4.5 + (double)slot.AllDeaths * 1.1 + objectBonus;
                            }
                            else
                            {
                                // Modos Bomb, Annihilation, Ace
                                deathBonus = (double)slot.AllDeaths * 2.2;
                                totalExpDouble = baseScore + timeBonus + deathBonus + objectBonus;
                                totalGoldDouble = calculatedScore + (double)num / 3.0 + deathBonus + objectBonus;
                                totalCashDouble = (double)(slot.Score / 2) + (double)num / 6.5 + (double)slot.AllDeaths * 1.5 + (double)(slot.Objects * 10);
                            }

                            // ==========================================
                            // ⚠️ VALIDACIÓN CRÍTICA ANTES DEL CAST
                            // Esta es la parte MÁS IMPORTANTE que previene el overflow
                            // ==========================================

                            // Validar EXP
                            if (totalExpDouble > int.MaxValue)
                            {
                                slot.Exp = int.MaxValue;
                            }
                            else if (totalExpDouble < 0)
                            {
                                slot.Exp = Math.Max(1000, (int)(slot.Score * 0.2));
                            }
                            else
                            {
                                slot.Exp = (int)totalExpDouble;
                            }

                            // Validar GOLD
                            if (totalGoldDouble > int.MaxValue)
                            {
                                slot.Gold = int.MaxValue;
                            }
                            else if (totalGoldDouble < 0)
                            {
                                slot.Gold = 2;
                            }
                            else
                            {
                                slot.Gold = (int)totalGoldDouble;
                            }

                            // Validar CASH
                            if (totalCashDouble > int.MaxValue)
                            {
                                slot.Cash = int.MaxValue;
                            }
                            else if (totalCashDouble < 0)
                            {
                                slot.Cash = 2;
                            }
                            else
                            {
                                slot.Cash = (int)totalCashDouble;
                            }

                            // Logs opcionales para debug
                            if (ConfigLoader.IsTestMode)
                            {
                                CLogger.Print($"📊 Player:{Player.PlayerId} | Score:{slot.Score}→{calculatedScore} | K:{slot.AllKills} H:{slot.AllHeadshots} A:{slot.AllAssists}", LoggerType.Warning);
                                CLogger.Print($"📊 CALCULO | BaseScore:{baseScore} | TimeBonus:{timeBonus} | DeathBonus:{deathBonus} | ObjectBonus:{objectBonus} | Total:{totalExpDouble}", LoggerType.Warning);
                                CLogger.Print($"📈 RESULTADO | Exp:{slot.Exp} | Gold:{slot.Gold} | Cash:{slot.Cash}", LoggerType.Warning);
                            }

                            // Continua con el resto del código original
                            bool flag = slot.Team == winnerTeam;
                            if (this.Rule != MapRules.Chaos && this.Rule != MapRules.HeadHunter)
                            {
                                if (basic != null && season != null)
                                    this.UpdatePlayerStatistics(Player, basic, season, slot, basicStatQuery, seasonStatQuery, index1, flag, (int)winnerTeam);
                                if (daily != null)
                                    this.UpdateDailyStatistics(Player, daily, slot, dailyStatQuery, index1, flag, (int)winnerTeam);
                                if (weapon != null)
                                    this.UpdateWeaponStatistics(Player, weapon, slot, weaponStatQuery);
                            }

                            // Bono de victoria
                            if (flag || this.RoomType == RoomCondition.FreeForAll && winnerTeam == (TeamEnum)index1)
                            {
                                slot.Gold += ComDiv.Percentage(slot.Gold, 15);
                                slot.Exp += ComDiv.Percentage(slot.Exp, 20);

                                if (ConfigLoader.IsTestMode)
                                {
                                    CLogger.Print($"🏆 BONO VICTORIA | Player:{Player.PlayerId} | Exp +20%", LoggerType.Warning);
                                }
                            }

                            // Bono EarnedEXP
                            if (slot.EarnedEXP > 0)
                            {
                                slot.Exp += slot.EarnedEXP * 5;

                                if (ConfigLoader.IsTestMode)
                                {
                                    CLogger.Print($"⭐ EARNED EXP | Player:{Player.PlayerId} | Bonus:{slot.EarnedEXP * 5}", LoggerType.Warning);
                                }
                            }
                        }
                        else
                        {
                            // Modo Bot
                            int num1 = (int)this.IngameAiLevel * (150 + slot.AllDeaths);
                            if (num1 == 0)
                                ++num1;
                            int num2 = slot.Score / num1;
                            slot.Gold += num2;
                            slot.Exp += num2;
                        }

                        // Aplicar límites máximos
                        slot.Exp = slot.Exp > ConfigLoader.MaxExpReward ? ConfigLoader.MaxExpReward : slot.Exp;
                        slot.Gold = slot.Gold > ConfigLoader.MaxGoldReward ? ConfigLoader.MaxGoldReward : slot.Gold;
                        slot.Cash = slot.Cash > ConfigLoader.MaxCashReward ? ConfigLoader.MaxCashReward : slot.Cash;

                        // ==========================================
                        // ✅ VALIDACIÓN FINAL MEJORADA
                        // Red de seguridad adicional
                        // ==========================================
                        if (this.RoomType != RoomCondition.Ace)
                        {
                            if (slot.Exp < 0 || slot.Gold < 0 || slot.Cash < 0)
                            {
                                if (slot.Exp < 0)
                                {
                                    slot.Exp = Math.Max(1000, (int)(slot.Score * 0.2));
                                }
                                if (slot.Gold < 0)
                                {
                                    slot.Gold = 2;
                                }
                                if (slot.Cash < 0)
                                {
                                    slot.Cash = 2;
                                }
                            }
                        }
                        else if (Player.SlotId < 0 || Player.SlotId > 1)
                        {
                            slot.Exp = 0;
                            slot.Gold = 0;
                            slot.Cash = 0;
                        }

                        // Resto del código original (bonos, eventos, etc.)
                        int num3 = 0;
                        int num4 = 0;
                        int num5 = 0;
                        int num6 = 0;
                        int num7 = 0;
                        int num8 = 0;
                        int num9 = 0;

                        if (runningEvent1 != null || runningEvent2 != null)
                        {
                            if (runningEvent1 != null)
                            {
                                int[] bonuses = runningEvent1.GetBonuses(Player.Rank);
                                if (bonuses != null)
                                {
                                    num7 += ComDiv.Percentage(bonuses[0], bonuses[2]);
                                    num8 += ComDiv.Percentage(bonuses[1], bonuses[2]);
                                }
                            }
                            if (runningEvent2 != null && runningEvent2.BoostType == PortalBoostEvent.Mode)
                            {
                                // Misma escala que PortalManager.InitBoostData: el cliente
                                // anuncia bonus*percent/100, así que se aplica ese valor.
                                num7 += ComDiv.Percentage(runningEvent2.BonusExp, runningEvent2.Percent);
                                num8 += ComDiv.Percentage(runningEvent2.BonusGold, runningEvent2.Percent);
                            }
                            if (!slot.BonusFlags.HasFlag((Enum)ResultIcon.Event))
                                slot.BonusFlags |= ResultIcon.Event;
                            slot.BonusEventExp += num7;
                            slot.BonusEventPoint += num8;
                        }
                        PlayerBonus bonus = Player.Bonus;
                        if (bonus != null && bonus.Bonuses > 0)
                        {
                            if ((bonus.Bonuses & 8) == 8)
                                num3 += 100;
                            if ((bonus.Bonuses & 128) == 128)
                                num4 += 100;
                            if ((bonus.Bonuses & 4) == 4)
                                num3 += 50;
                            if ((bonus.Bonuses & 64) == 64)
                                num4 += 50;
                            if ((bonus.Bonuses & 2) == 2)
                                num3 += 30;
                            if ((bonus.Bonuses & 32) == 32)
                                num4 += 30;
                            if ((bonus.Bonuses & 1) == 1)
                                num3 += 10;
                            if ((bonus.Bonuses & 16) == 16)
                                num4 += 10;
                            if ((bonus.Bonuses & 512) == 512)
                                num9 += 20;
                            if ((bonus.Bonuses & 1024) == 1024)
                                num9 += 30;
                            if ((bonus.Bonuses & 2048) == 2048)
                                num9 += 50;
                            if ((bonus.Bonuses & 4096) == 4096)
                                num9 += 100;
                            if (!slot.BonusFlags.HasFlag((Enum)ResultIcon.Item))
                                slot.BonusFlags |= ResultIcon.Item;
                            slot.BonusItemExp += num3;
                            slot.BonusItemPoint += num4;
                            slot.BonusBattlePass += num9;
                        }
                        PCCafeModel pcCafe = TemplatePackXML.GetPCCafe(Player.CafePC);
                        if (pcCafe != null)
                        {
                            PlayerVip playerVip = DaoManagerSQL.GetPlayerVIP(Player.PlayerId);
                            if (playerVip != null && InternetCafeXML.IsValidAddress(DaoManagerSQL.GetPlayerIP4Address(Player.PlayerId), playerVip.Address))
                            {
                                InternetCafe icafe = InternetCafeXML.GetICafe(ConfigLoader.InternetCafeId);
                                if (icafe != null && (Player.CafePC == CafeEnum.Gold || Player.CafePC == CafeEnum.Silver))
                                {
                                    num5 += Player.CafePC == CafeEnum.Gold ? icafe.PremiumExp : (Player.CafePC == CafeEnum.Silver ? icafe.BasicExp : 0);
                                    num6 += Player.CafePC == CafeEnum.Gold ? icafe.PremiumGold : (Player.CafePC == CafeEnum.Silver ? icafe.BasicGold : 0);
                                }
                            }
                            int num10 = num5 + pcCafe.ExpUp;
                            int num11 = num6 + pcCafe.PointUp;
                            if (Player.CafePC == CafeEnum.Silver && !slot.BonusFlags.HasFlag((Enum)ResultIcon.Pc))
                                slot.BonusFlags |= ResultIcon.Pc;
                            else if (Player.CafePC == CafeEnum.Gold && !slot.BonusFlags.HasFlag((Enum)ResultIcon.PcPlus))
                                slot.BonusFlags |= ResultIcon.PcPlus;
                            slot.BonusCafeExp += num10;
                            slot.BonusCafePoint += num11;
                        }
                        if (isBotMode)
                        {
                            if (slot.BonusItemExp > 300)
                                slot.BonusItemExp = 300;
                            if (slot.BonusItemPoint > 300)
                                slot.BonusItemPoint = 300;
                            if (slot.BonusCafeExp > 300)
                                slot.BonusCafeExp = 300;
                            if (slot.BonusCafePoint > 300)
                                slot.BonusCafePoint = 300;
                            if (slot.BonusEventExp > 300)
                                slot.BonusEventExp = 300;
                            if (slot.BonusEventPoint > 300)
                                slot.BonusEventPoint = 300;
                        }
                        int Percent1 = slot.BonusEventExp + slot.BonusCafeExp + slot.BonusItemExp;
                        int Percent2 = slot.BonusEventPoint + slot.BonusCafePoint + slot.BonusItemPoint;
                        Player.Exp += slot.Exp + ComDiv.Percentage(slot.Exp, Percent1);
                        Player.Gold += slot.Gold + ComDiv.Percentage(slot.Gold, Percent2);
                        if (daily != null)
                        {
                            daily.ExpGained += slot.Exp + ComDiv.Percentage(slot.Exp, Percent1);
                            daily.PointGained += slot.Gold + ComDiv.Percentage(slot.Gold, Percent2);
                            daily.Playtime += (uint)PlayedTime;
                            dailyStatQuery.AddQuery("exp_gained", (object)daily.ExpGained);
                            dailyStatQuery.AddQuery("point_gained", (object)daily.PointGained);
                            dailyStatQuery.AddQuery("playtime", (object)(long)daily.Playtime);
                        }

                        AllUtils.CalculateSeasonExpPoints(Player, bonus, slot, bPass);

                        if (ConfigLoader.WinCashPerBattle)
                            Player.Cash += slot.Cash;
                        RankModel rank = PlayerRankXML.GetRank(Player.Rank);
                        if (rank != null && Player.Exp >= rank.OnNextLevel + rank.OnAllExp && Player.Rank <= 50)
                        {
                            AllUtils.GrantRankRewards(Player, Player.Rank);
                            Player.Gold += rank.OnGoldUp;
                            Player.LastRankUpDate = uint.Parse(Date.ToString("yyMMddHHmm"));
                            int previousRank = Player.Rank;
                            Player.Rank = previousRank + 1;
                            Player.SendPacket(new PROTOCOL_BASE_RANK_UP_ACK(Player.Rank, previousRank, rank.OnGoldUp));
                            query.AddQuery("last_rank_update", (object)(long)Player.LastRankUpDate);
                            query.AddQuery("rank", (object)Player.Rank);
                        }
                        if (runningEvent3 != null)
                            AllUtils.PlayTimeEvent(Player, runningEvent3, isBotMode, slot, (long)PlayedTime);
                        if (bPass.SeasonEnabled != 0)
                        {
                            AllUtils.UpdateSeasonPass(Player);
                        }
                        if (this.Competitive)
                            AllUtils.CalculateCompetitive(this, Player, slot, winnerTeam == slot.Team);
                        AllUtils.DiscountPlayerItems(slot, Player);
                        if (exp != Player.Exp)
                            query.AddQuery("experience", (object)Player.Exp);
                        if (gold != Player.Gold)
                            query.AddQuery("gold", (object)Player.Gold);
                        if (cash != Player.Cash)
                            query.AddQuery("cash", (object)Player.Cash);
                        ComDiv.UpdateDB("accounts", "player_id", (object)Player.PlayerId, query.GetTables(), query.GetValues());
                        ComDiv.UpdateDB("player_stat_basics", "owner_id", (object)Player.PlayerId, basicStatQuery.GetTables(), basicStatQuery.GetValues());
                        ComDiv.UpdateDB("player_stat_seasons", "owner_id", (object)Player.PlayerId, seasonStatQuery.GetTables(), seasonStatQuery.GetValues());
                        ComDiv.UpdateDB("player_stat_dailies", "owner_id", (object)Player.PlayerId, dailyStatQuery.GetTables(), dailyStatQuery.GetValues());
                        ComDiv.UpdateDB("player_stat_weapons", "owner_id", (object)Player.PlayerId, weaponStatQuery.GetTables(), weaponStatQuery.GetValues());
                        if (ConfigLoader.WinCashPerBattle && ConfigLoader.ShowCashReceiveWarn)
                            Player.SendPacket(new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(Translation.GetLabel("CashReceived", (object)slot.Cash)));
                        slotModelList.Add(slot);
                    }
                }
                if (slotModelList.Count > 0)
                {
                    this.SlotRewards = AllUtils.GetRewardData(this, slotModelList);
                    this.UpdateMVPInformation(slotModelList, isBotMode);
                }
                this.UpdateSlotsInfo();
                if (this.RoomType == RoomCondition.FreeForAll)
                    return;
                this.UpdateClanInformation(winnerTeam);
            }
            else
                CLogger.Print("Server Config Null. RoomResult canceled.", LoggerType.Warning);
        }

        private void UpdateMVPInformation(List<SlotModel> slotList, bool isBotMode)
        {
            SlotModel Slot = slotList.OrderByDescending<SlotModel, int>((Func<SlotModel, int>)(slot => slot.Score)).FirstOrDefault<SlotModel>();
            Account Player;
            if (Slot == null || !Slot.Check || Slot.State != SlotState.BATTLE || !this.GetPlayerBySlot(Slot, out Player))
                return;
            StatisticTotal basic = Player.Statistic.Basic;
            StatisticSeason season = Player.Statistic.Season;
            if (isBotMode || basic == null || season == null)
                return;
            ++basic.MvpCount;
            ++season.MvpCount;
            ComDiv.UpdateDB("player_stat_basics", "mvp_count", (object)basic.MvpCount, "owner_id", (object)Player.PlayerId);
            ComDiv.UpdateDB("player_stat_seasons", "mvp_count", (object)season.MvpCount, "owner_id", (object)Player.PlayerId);
        }

        private void UpdatePlayerStatistics(
          Account player,
          StatisticTotal basic,
          StatisticSeason season,
          SlotModel slot,
          DBQuery basicQuery,
          DBQuery seasonQuery,
          int mvpSlotIndex,
          bool wonMatch,
          int winnerTeamId)
        {
            basic.HeadshotsCount += slot.AllHeadshots;
            basic.KillsCount += slot.AllKills;
            basic.TotalKillsCount += slot.AllKills;
            basic.DeathsCount += slot.AllDeaths;
            basic.AssistsCount += slot.AllAssists;
            season.HeadshotsCount += slot.AllHeadshots;
            season.KillsCount += slot.AllKills;
            season.TotalKillsCount += slot.AllKills;
            season.DeathsCount += slot.AllDeaths;
            season.AssistsCount += slot.AllAssists;
            this.UpdateStatisticQueries(slot, player.Statistic, basicQuery, seasonQuery);
            if (this.RoomType != RoomCondition.FreeForAll)
                AllUtils.UpdateMatchCount(wonMatch, player, winnerTeamId, basicQuery, seasonQuery);
            else
                AllUtils.UpdateMatchCountFFA(this, player, mvpSlotIndex, basicQuery, seasonQuery);
        }

        private void UpdateDailyStatistics(
          Account player,
          StatisticDaily daily,
          SlotModel slot,
          DBQuery dailyQuery,
          int mvpSlotIndex,
          bool wonMatch,
          int winnerTeamId)
        {
            daily.KillsCount += slot.AllKills;
            daily.DeathsCount += slot.AllDeaths;
            daily.HeadshotsCount += slot.AllHeadshots;
            this.UpdateDailyStatisticQueries(slot, player.Statistic, dailyQuery);
            if (this.RoomType == RoomCondition.FreeForAll)
                AllUtils.UpdateMatchDailyRecordFFA(this, player, mvpSlotIndex, dailyQuery);
            else
                AllUtils.UpdateDailyRecord(wonMatch, player, winnerTeamId, dailyQuery);
        }

        private void UpdateWeaponStatistics(Account player, StatisticWeapon weapon, SlotModel slot, DBQuery weaponQuery)
        {
            weapon.AssaultKills += slot.AR[0];
            weapon.AssaultDeaths += slot.AR[1];
            weapon.SmgKills += slot.SMG[0];
            weapon.SmgDeaths += slot.SMG[1];
            weapon.SniperKills += slot.SR[0];
            weapon.SniperDeaths += slot.SR[1];
            weapon.ShotgunKills += slot.SG[0];
            weapon.ShotgunDeaths += slot.SG[1];
            weapon.MachinegunKills += slot.MG[0];
            weapon.MachinegunDeaths += slot.MG[1];
            weapon.ShieldKills += slot.SHD[0];
            weapon.ShieldDeaths += slot.SHD[1];
            AllUtils.UpdateWeaponRecord(player, slot, weaponQuery);
        }

        private void UpdateStatisticQueries(SlotModel slot, PlayerStatistic statistic, DBQuery basicQuery, DBQuery seasonQuery)
        {
            if (statistic == null)
                return;
            if (slot.AllKills > 0)
            {
                basicQuery.AddQuery("kills_count", (object)statistic.Basic.KillsCount);
                basicQuery.AddQuery("total_kills", (object)statistic.Basic.TotalKillsCount);
                seasonQuery.AddQuery("kills_count", (object)statistic.Season.KillsCount);
                seasonQuery.AddQuery("total_kills", (object)statistic.Season.TotalKillsCount);
            }
            if (slot.AllAssists > 0)
            {
                basicQuery.AddQuery("assists_count", (object)statistic.Basic.AssistsCount);
                seasonQuery.AddQuery("assists_count", (object)statistic.Season.AssistsCount);
            }
            if (slot.AllDeaths > 0)
            {
                basicQuery.AddQuery("deaths_count", (object)statistic.Basic.DeathsCount);
                seasonQuery.AddQuery("deaths_count", (object)statistic.Season.DeathsCount);
            }
            if (slot.AllHeadshots <= 0)
                return;
            basicQuery.AddQuery("headshots_count", (object)statistic.Basic.HeadshotsCount);
            seasonQuery.AddQuery("headshots_count", (object)statistic.Season.HeadshotsCount);
        }

        private void UpdateDailyStatisticQueries(SlotModel slot, PlayerStatistic statistic, DBQuery dailyQuery)
        {
            if (statistic == null)
                return;
            if (slot.AllKills > 0)
                dailyQuery.AddQuery("kills_count", (object)statistic.Daily.KillsCount);
            if (slot.AllDeaths > 0)
                dailyQuery.AddQuery("deaths_count", (object)statistic.Daily.DeathsCount);
            if (slot.AllHeadshots <= 0)
                return;
            dailyQuery.AddQuery("headshots_count", (object)statistic.Daily.HeadshotsCount);
        }

        private void UpdateMercenaryStatistics(Account player, SlotModel slot, TeamEnum winnerTeam)
        {
            StatisticMercenary merc = player.Statistic.Mercenary;
            if (merc == null)
                return;
            ++merc.Matches;
            if (winnerTeam != TeamEnum.TEAM_DRAW)
            {
                if (slot.Team == winnerTeam)
                    ++merc.MatchWins;
                else
                    ++merc.MatchLoses;
            }
            merc.KillsCount += slot.AllKills;
            merc.DeathsCount += slot.AllDeaths;
            merc.HeadshotsCount += slot.AllHeadshots;
            merc.AssistsCount += slot.AllAssists;
            DBQuery query = new DBQuery();
            query.AddQuery("matches", (object)merc.Matches);
            query.AddQuery("match_wins", (object)merc.MatchWins);
            query.AddQuery("match_loses", (object)merc.MatchLoses);
            query.AddQuery("kills_count", (object)merc.KillsCount);
            query.AddQuery("deaths_count", (object)merc.DeathsCount);
            query.AddQuery("headshots_count", (object)merc.HeadshotsCount);
            query.AddQuery("assists_count", (object)merc.AssistsCount);
            ComDiv.UpdateDB("player_stat_mercenaries", "owner_id", (object)player.PlayerId, query.GetTables(), query.GetValues());
        }

        private void UpdateClanInformation(TeamEnum winnerTeam)
        {
            if (this.ChannelType != ChannelType.Clan || this.BlockedClan)
                return;
            SortedList<int, ClanModel> sortedList = new SortedList<int, ClanModel>();
            foreach (SlotModel slot in this.Slots)
            {
                Account Player;
                if (slot.State == SlotState.BATTLE && this.GetPlayerBySlot(slot, out Player))
                {
                    if (slot.IsMercenary)
                    {
                        this.UpdateMercenaryStatistics(Player, slot, winnerTeam);
                        continue;
                    }
                    ClanModel clan = ClanManager.GetClan(Player.ClanId);
                    if (clan.Id != 0)
                    {
                        bool WonTheMatch = slot.Team == winnerTeam;
                        clan.Exp += (Player.Bonus != null && (Player.Bonus.Bonuses & 256) == 256) ? slot.Exp * 3 / 2 : slot.Exp;
                        clan.BestPlayers.SetBestExp(slot);
                        clan.BestPlayers.SetBestKills(slot);
                        clan.BestPlayers.SetBestHeadshot(slot);
                        clan.BestPlayers.SetBestWins(Player.Statistic, slot, WonTheMatch);
                        clan.BestPlayers.SetBestParticipation(Player.Statistic, slot);
                        if (!sortedList.ContainsKey(Player.ClanId))
                        {
                            sortedList.Add(Player.ClanId, clan);
                            if (winnerTeam != TeamEnum.TEAM_DRAW)
                            {
                                this.UpdateClanPointsAndMatches(clan, winnerTeam, slot.Team);
                                if (WonTheMatch)
                                    ++clan.MatchWins;
                                else
                                    ++clan.MatchLoses;
                            }
                            ++clan.Matches;
                            DaoManagerSQL.UpdateClanBattles(clan.Id, clan.Matches, clan.MatchWins, clan.MatchLoses);
                        }
                    }
                }
            }
            foreach (ClanModel Clan in (IEnumerable<ClanModel>)sortedList.Values)
            {
                DaoManagerSQL.UpdateClanExp(Clan.Id, Clan.Exp);
                DaoManagerSQL.UpdateClanPoints(Clan.Id, Clan.Points);
                DaoManagerSQL.UpdateClanBestPlayers(Clan);
                RankModel rank = ClanRankXML.GetRank(Clan.Rank);
                if (rank != null && Clan.Exp >= rank.OnNextLevel + rank.OnAllExp)
                    DaoManagerSQL.UpdateClanRank(Clan.Id, ++Clan.Rank);
            }
        }

        private void UpdateClanPointsAndMatches(ClanModel clan, TeamEnum winnerTeam, TeamEnum playerTeam)
        {
            if (winnerTeam == TeamEnum.TEAM_DRAW)
                return;
            if (winnerTeam != playerTeam)
            {
                if ((double)clan.Points == 0.0)
                    return;
                float num = 40f - (this.RoomType != RoomCondition.DeathMatch ? (playerTeam == TeamEnum.FR_TEAM ? (float)this.FRRounds : (float)this.CTRounds) : (float)((playerTeam == TeamEnum.FR_TEAM ? this.FRKills : this.CTKills) / 20));
                clan.Points -= num;
            }
            else
            {
                float num = 25f + (this.RoomType != RoomCondition.DeathMatch ? (playerTeam == TeamEnum.FR_TEAM ? (float)this.FRRounds : (float)this.CTRounds) : (float)((playerTeam == TeamEnum.FR_TEAM ? this.FRKills : this.CTKills) / 20));
                clan.Points += num;
            }
        }

        public bool IsStartingMatch() => this.State > RoomState.READY;

        public bool IsPreparing() => this.State >= RoomState.LOADING;

        public void UpdateRoomInfo()
        {
            this.GenerateSeed();
            using (PROTOCOL_ROOM_CHANGE_ROOMINFO_ACK Packet = new PROTOCOL_ROOM_CHANGE_ROOMINFO_ACK(this))
                this.SendPacketToPlayers(Packet);
        }

        public void SetSlotCount(int Count, bool IsCreateRoom, bool IsUpdateRoom)
        {
            MapMatch mapLimit = SystemMapXML.GetMapLimit((int)MapId, (int)Rule);
            if (mapLimit != null && Count > mapLimit.Limit)
            {
                Count = mapLimit.Limit;
            }
            if (RoomType == RoomCondition.Tutorial)
            {
                Count = 1;
            }
            if (IsBotMode())
            {
                Count = 8;
            }
            if (Count <= 0)
            {
                Count = 1;
            }
            if (Count > 16)
            {
                Count = 16;
            }
            // Criação e alteração pela engrenagem: abre/fecha slots vazios.
            // Antes só rodava em create — quantidade na UI nunca mudava no update.
            lock (Slots)
            {
                foreach (SlotModel item in Slots.Where((SlotModel s) => s.Id != 16 && s.Id != 17))
                {
                    if (item.Id >= Count)
                    {
                        if (item.PlayerId == 0L)
                            item.State = SlotState.CLOSE;
                    }
                    else if (item.State == SlotState.CLOSE)
                    {
                        item.State = SlotState.EMPTY;
                    }
                }
            }
            CountMaxSlots = Count;
            if (IsUpdateRoom)
            {
                UpdateSlotsInfo();
            }
        }

        public int GetSlotCount()
        {
            lock (Slots)
            {
                int num = 0;
                foreach (SlotModel item in Slots.Where((SlotModel slotModel_0) => slotModel_0.Id != 16 && slotModel_0.Id != 17))
                {
                    if (item.State != SlotState.CLOSE)
                    {
                        num++;
                    }
                }
                return num;
            }
        }

        public void SwitchNewSlot(List<SlotModel> SlotChanges, Account Player, SlotModel OldSlot, TeamEnum TeamIdx, int SlotIdx)
        {
            bool IsValidSlot = GetSlot(SlotIdx, out SlotModel NewSlot);
            if (IsValidSlot && NewSlot.Team == TeamIdx && NewSlot.PlayerId == 0 && NewSlot.State == SlotState.EMPTY)
            {
                NewSlot.ResetSlot();
                NewSlot.State = SlotState.NORMAL;
                NewSlot.PlayerId = Player.PlayerId;
                NewSlot.Equipment = Player.Equipment;

                if (NewSlot.Id == 16 || NewSlot.Id == 17)
                {
                    NewSlot.SpecGM = true;
                    NewSlot.ViewType = ViewerType.SpecGM;
                }

                OldSlot.ResetSlot();
                OldSlot.State = SlotState.EMPTY;
                OldSlot.PlayerId = 0;
                OldSlot.Equipment = null;
                OldSlot.SpecGM = false;
                OldSlot.ViewType = ViewerType.Normal;
                if (Player.SlotId == Leader)
                {
                    LeaderName = Player.Nickname;
                    Leader = SlotIdx;
                }
                Player.SlotId = SlotIdx;
                SlotChanges.Add(new SlotModel(NewSlot, OldSlot));
            }
        }

        public void SwitchSlots(List<SlotModel> SlotChanges, int NewSlotId, int OldSlotId, bool ChangeReady)
        {
            SlotModel slot1 = this.Slots[NewSlotId];
            SlotModel slot2 = this.Slots[OldSlotId];
            if (ChangeReady)
            {
                if (slot1.State == SlotState.READY)
                {
                    slot1.State = SlotState.NORMAL;
                }
                if (slot2.State == SlotState.READY)
                {
                    slot2.State = SlotState.NORMAL;
                }
            }
            slot1.SetSlotId(OldSlotId);
            slot2.SetSlotId(NewSlotId);
            this.Slots[NewSlotId] = slot2;
            this.Slots[OldSlotId] = slot1;
            SlotChanges.Add(new SlotModel(slot2, slot1));
        }

        public void ChangeSlotState(int SlotId, SlotState State, bool SendInfo)
        {
            this.ChangeSlotState(this.GetSlot(SlotId), State, SendInfo);
        }

        public void ChangeSlotState(SlotModel Slot, SlotState State, bool SendInfo)
        {
            if (Slot == null || Slot.State == State)
                return;
            Slot.State = State;
            if (State == SlotState.EMPTY || State == SlotState.CLOSE)
            {
                AllUtils.ResetSlotInfo(this, Slot, false);
                Slot.PlayerId = 0L;
            }
            if (!SendInfo)
                return;
            this.UpdateSlotsInfo();
        }

        public Account GetPlayerBySlot(SlotModel Slot)
        {
            try
            {
                long playerId = Slot.PlayerId;
                return playerId > 0L ? AccountManager.GetAccount(playerId, true) : (Account)null;
            }
            catch
            {
                return (Account)null;
            }
        }

        public Account GetPlayerBySlot(int SlotId)
        {
            try
            {
                long playerId = this.Slots[SlotId].PlayerId;
                return playerId > 0L ? AccountManager.GetAccount(playerId, true) : (Account)null;
            }
            catch
            {
                return (Account)null;
            }
        }

        public bool GetPlayerBySlot(int SlotId, out Account Player)
        {
            try
            {
                long playerId = this.Slots[SlotId].PlayerId;
                Player = playerId > 0L ? AccountManager.GetAccount(playerId, true) : (Account)null;
                return Player != null;
            }
            catch
            {
                Player = (Account)null;
                return false;
            }
        }

        public bool GetPlayerBySlot(SlotModel Slot, out Account Player)
        {
            try
            {
                long playerId = Slot.PlayerId;
                Player = playerId > 0L ? AccountManager.GetAccount(playerId, true) : (Account)null;
                return Player != null;
            }
            catch
            {
                Player = (Account)null;
                return false;
            }
        }

        public int GetTimeByMask() => this.TIMES[this.KillTime >> 4];

        public int GetRoundsByMask() => this.ROUNDS[this.KillTime & 15];

        public int GetSwapRoundByMask() => this.SWAP_ROUNDS[this.KillTime & 15];

        public int GetKillsByMask() => this.KILLS[this.KillTime & 15];

        public void UpdateSlotsInfo()
        {
            using (PROTOCOL_ROOM_GET_SLOTINFO_ACK Packet = new PROTOCOL_ROOM_GET_SLOTINFO_ACK(this))
                this.SendPacketToPlayers(Packet);
        }

        public bool GetLeader(out Account Player)
        {
            Player = (Account)null;
            if (this.GetCountPlayers() <= 0)
                return false;
            if (this.Leader == -1)
                this.SetNewLeader(-1, SlotState.EMPTY, -1, false);
            if (this.Leader >= 0)
                Player = AccountManager.GetAccount(this.Slots[this.Leader].PlayerId, true);
            return Player != null;
        }

        public Account GetLeader()
        {
            if (this.GetCountPlayers() <= 0)
                return (Account)null;
            if (this.Leader == -1)
                this.SetNewLeader(-1, SlotState.EMPTY, -1, false);
            return this.Leader != -1 ? AccountManager.GetAccount(this.Slots[this.Leader].PlayerId, true) : (Account)null;
        }

        public void SetNewLeader(int LeaderSlot, SlotState State, int OldLeader, bool UpdateInfo)
        {
            lock (this.Slots)
            {
                if (LeaderSlot != -1)
                {
                    this.Leader = LeaderSlot;
                }
                else
                {
                    foreach (SlotModel slot in this.Slots)
                    {
                        if (slot.Id != OldLeader && slot.PlayerId > 0L && slot.State > State)
                        {
                            this.Leader = slot.Id;
                            break;
                        }
                    }
                }
                if (this.Leader == -1)
                    return;
                SlotModel slot1 = this.Slots[this.Leader];
                if (slot1.State == SlotState.READY)
                    slot1.State = SlotState.NORMAL;
                if (!UpdateInfo)
                    return;
                this.UpdateSlotsInfo();
            }
        }

        public void SendPacketToPlayers(GameServerPacket Packet)
        {
            List<Account> allPlayers = this.GetAllPlayers();
            if (allPlayers.Count == 0)
                return;
            byte[] completeBytes = Packet.GetCompleteBytes("Room.SendPacketToPlayers(SendPacket)");
            foreach (Account account in allPlayers)
                account.SendCompletePacket(completeBytes, Packet.GetType().Name);
        }

        public void SendPacketToPlayers(GameServerPacket Packet, long PlayerId)
        {
            List<Account> allPlayers = this.GetAllPlayers(PlayerId);
            if (allPlayers.Count == 0)
                return;
            byte[] completeBytes = Packet.GetCompleteBytes("Room.SendPacketToPlayers(SendPacket,long)");
            foreach (Account account in allPlayers)
                account.SendCompletePacket(completeBytes, Packet.GetType().Name);
        }

        public void SendPacketToPlayers(GameServerPacket Packet, SlotState State, int Type)
        {
            List<Account> allPlayers = this.GetAllPlayers(State, Type);
            if (allPlayers.Count == 0)
                return;
            byte[] completeBytes = Packet.GetCompleteBytes("Room.SendPacketToPlayers(SendPacket,SLOT_STATE,int)");
            foreach (Account account in allPlayers)
                account.SendCompletePacket(completeBytes, Packet.GetType().Name);
        }

        public void SendPacketToPlayers(GameServerPacket Packet, GameServerPacket Packet2, SlotState State, int Type)
        {
            List<Account> allPlayers = this.GetAllPlayers(State, Type);
            if (allPlayers.Count == 0)
                return;
            byte[] completeBytes1 = Packet.GetCompleteBytes("Room.SendPacketToPlayers(SendPacket,SendPacket,SLOT_STATE,int)-1");
            byte[] completeBytes2 = Packet2.GetCompleteBytes("Room.SendPacketToPlayers(SendPacket,SendPacket,SLOT_STATE,int)-2");
            foreach (Account account in allPlayers)
            {
                account.SendCompletePacket(completeBytes1, Packet.GetType().Name);
                account.SendCompletePacket(completeBytes2, Packet2.GetType().Name);
            }
        }

        public void SendPacketToPlayers(GameServerPacket Packet, SlotState State, int Type, int Exception)
        {
            List<Account> allPlayers = this.GetAllPlayers(State, Type, Exception);
            if (allPlayers.Count == 0)
                return;
            byte[] completeBytes = Packet.GetCompleteBytes("Room.SendPacketToPlayers(SendPacket,SLOT_STATE,int,int)");
            foreach (Account account in allPlayers)
                account.SendCompletePacket(completeBytes, Packet.GetType().Name);
        }

        public void SendPacketToPlayers(GameServerPacket Packet, SlotState State, int Type, int Exception, int Exception2)
        {
            List<Account> allPlayers = this.GetAllPlayers(State, Type, Exception, Exception2);
            if (allPlayers.Count == 0)
                return;
            byte[] completeBytes = Packet.GetCompleteBytes("Room.SendPacketToPlayers(SendPacket,SLOT_STATE,int,int,int)");
            foreach (Account account in allPlayers)
                account.SendCompletePacket(completeBytes, Packet.GetType().Name);
        }

        public void RemovePlayer(Account Player, bool WarnAllPlayers, int QuitMotive = 0)
        {
            SlotModel Slot;
            if (Player == null || !this.GetSlot(Player.SlotId, out Slot))
                return;
            this.InternalRemovePlayer(Player, Slot, WarnAllPlayers, QuitMotive);
        }

        public void RemovePlayer(Account Player, SlotModel Slot, bool WarnAllPlayers, int QuitMotive = 0)
        {
            if (Player == null || Slot == null)
                return;
            this.InternalRemovePlayer(Player, Slot, WarnAllPlayers, QuitMotive);
        }

        private void InternalRemovePlayer(Account player, SlotModel slot, bool warnAllPlayers, int quitMotive)
        {
            lock (this.Slots)
            {
                bool flag = false;
                bool host = false;
                if (player != null && slot != null)
                {
                    if (slot.State >= SlotState.LOAD)
                    {
                        if (this.Leader == slot.Id)
                        {
                            int leader = this.Leader;
                            SlotState State = SlotState.CLOSE;
                            if (this.State != RoomState.BATTLE)
                            {
                                if (this.State >= RoomState.LOADING)
                                    State = SlotState.READY;
                            }
                            else
                                State = SlotState.BATTLE_READY;
                            if (this.GetAllPlayers(slot.Id).Count >= 1)
                                this.SetNewLeader(-1, State, leader, false);
                            if (this.GetPlayingPlayers(TeamEnum.TEAM_DRAW, SlotState.READY, 1) >= 2)
                            {
                                using (PROTOCOL_BATTLE_LEAVEP2PSERVER_ACK Packet = new PROTOCOL_BATTLE_LEAVEP2PSERVER_ACK(this))
                                    this.SendPacketToPlayers(Packet, SlotState.RENDEZVOUS, 1, slot.Id);
                            }
                            host = true;
                        }
                        using (PROTOCOL_BATTLE_GIVEUPBATTLE_ACK Packet = new PROTOCOL_BATTLE_GIVEUPBATTLE_ACK(player, quitMotive))
                            this.SendPacketToPlayers(Packet, SlotState.READY, 1, !warnAllPlayers ? slot.Id : -1);
                        BattleLeaveSync.SendUDPPlayerLeave(this, slot.Id);
                        slot.ResetSlot();
                        if (this.VoteKick != null)
                            this.VoteKick.TotalArray[slot.Id] = false;
                    }
                    slot.PlayerId = 0L;
                    slot.Equipment = (PlayerEquipment)null;
                    slot.State = SlotState.EMPTY;
                    if (this.State == RoomState.COUNTDOWN)
                    {
                        if (slot.Id == this.Leader)
                        {
                            this.State = RoomState.READY;
                            flag = true;
                            this.CountdownTime.StopJob();
                            using (PROTOCOL_BATTLE_START_COUNTDOWN_ACK Packet = new PROTOCOL_BATTLE_START_COUNTDOWN_ACK(CountDownEnum.StopByHost))
                                this.SendPacketToPlayers(Packet);
                        }
                        else if (this.GetPlayingPlayers(slot.Team, SlotState.READY, 0) == 0)
                        {
                            if (slot.Id != this.Leader)
                                this.ChangeSlotState(this.Leader, SlotState.NORMAL, false);
                            this.StopCountDown(CountDownEnum.StopByPlayer, false);
                            flag = true;
                        }
                    }
                    else if (this.IsPreparing())
                    {
                        AllUtils.BattleEndPlayersCount(this, this.IsBotMode());
                        if (this.State == RoomState.BATTLE)
                            AllUtils.BattleEndRoundPlayersCount(this);
                    }
                    this.CheckToEndWaitingBattle(host);
                    this.RequestRoomMaster.Remove(player.PlayerId);
                    if (this.VoteTime.IsTimer() && this.VoteKick != null && this.VoteKick.VictimIdx == player.SlotId && quitMotive != 2)
                    {
                        this.VoteTime.StopJob();
                        this.VoteKick = (VoteKickModel)null;
                        using (PROTOCOL_BATTLE_NOTIFY_KICKVOTE_CANCEL_ACK Packet = new PROTOCOL_BATTLE_NOTIFY_KICKVOTE_CANCEL_ACK())
                            this.SendPacketToPlayers(Packet, SlotState.BATTLE, 0);
                    }
                    MatchModel match = player.Match;
                    if (match != null && player.MatchSlot >= 0)
                    {
                        match.Slots[player.MatchSlot].State = SlotMatchState.Normal;
                        match.BroadcastRoster();
                    }
                    player.Room = (RoomModel)null;
                    player.SlotId = -1;
                    player.Status.UpdateRoom(byte.MaxValue);

                    ChannelModel channel = this.GetChannel();
                    if (channel != null)
                    {
                        lock (channel.LobbyPlayers)
                        {
                            if (!channel.LobbyPlayers.Contains(player))
                                channel.LobbyPlayers.Add(player);
                        }
                    }

                    AllUtils.SyncPlayerToClanMembers(player);
                    AllUtils.SyncPlayerToFriends(player, false);
                    player.UpdateCacheInfo();
                }
                this.UpdateSlotsInfo();
                if (!flag)
                    return;
                this.UpdateRoomInfo();
            }
        }

        public int AddPlayer(Account Player)
        {
            lock (this.Slots)
            {
                foreach (SlotModel slot in this.Slots)
                {
                    if (slot.PlayerId == 0L && slot.State == SlotState.EMPTY)
                    {
                        slot.PlayerId = Player.PlayerId;
                        slot.State = SlotState.NORMAL;
                        Player.Room = this;
                        Player.SlotId = slot.Id;
                        slot.Equipment = Player.Equipment;
                        Player.Status.UpdateRoom((byte)this.RoomId);

                        ChannelModel channel = this.GetChannel();
                        if (channel != null)
                        {
                            lock (channel.LobbyPlayers)
                                channel.LobbyPlayers.Remove(Player);
                        }

                        AllUtils.SyncPlayerToClanMembers(Player);
                        AllUtils.SyncPlayerToFriends(Player, false);
                        Player.UpdateCacheInfo();
                        return slot.Id;
                    }
                }
            }
            return -1;
        }

        public int AddPlayer(Account Player, TeamEnum TeamIdx)
        {
            lock (this.Slots)
            {
                foreach (int team in this.GetTeamArray(TeamIdx))
                {
                    SlotModel slot = this.Slots[team];
                    if (slot.PlayerId == 0L && slot.State == SlotState.EMPTY)
                    {
                        slot.PlayerId = Player.PlayerId;
                        slot.State = SlotState.NORMAL;
                        Player.Room = this;
                        Player.SlotId = slot.Id;
                        slot.Equipment = Player.Equipment;
                        Player.Status.UpdateRoom((byte)this.RoomId);

                        ChannelModel channel = this.GetChannel();
                        if (channel != null)
                        {
                            lock (channel.LobbyPlayers)
                                channel.LobbyPlayers.Remove(Player);
                        }

                        AllUtils.SyncPlayerToClanMembers(Player);
                        AllUtils.SyncPlayerToFriends(Player, false);
                        Player.UpdateCacheInfo();
                        return slot.Id;
                    }
                }
            }
            return -1;
        }

        public int[] GetTeamArray(TeamEnum Team)
        {
            if (Team == TeamEnum.FR_TEAM)
                return this.FR_TEAM;
            return Team == TeamEnum.CT_TEAM ? this.CT_TEAM : this.ALL_TEAM;
        }

        public List<Account> GetAllPlayers(SlotState State, int Type)
        {
            List<Account> allPlayers = new List<Account>();
            lock (this.Slots)
            {
                foreach (SlotModel slot in this.Slots)
                {
                    if (slot.PlayerId > 0L && (Type == 0 && slot.State == State || Type == 1 && slot.State > State))
                    {
                        Account account = AccountManager.GetAccount(slot.PlayerId, true);
                        if (account != null && account.SlotId != -1)
                            allPlayers.Add(account);
                    }
                }
            }
            return allPlayers;
        }

        public List<Account> GetAllPlayers(SlotState State, int Type, int Exception)
        {
            List<Account> allPlayers = new List<Account>();
            lock (this.Slots)
            {
                foreach (SlotModel slot in this.Slots)
                {
                    if (slot.PlayerId > 0L && slot.Id != Exception && (Type == 0 && slot.State == State || Type == 1 && slot.State > State))
                    {
                        Account account = AccountManager.GetAccount(slot.PlayerId, true);
                        if (account != null && account.SlotId != -1)
                            allPlayers.Add(account);
                    }
                }
            }
            return allPlayers;
        }

        public List<Account> GetAllPlayers(SlotState State, int Type, int Exception, int Exception2)
        {
            List<Account> allPlayers = new List<Account>();
            lock (this.Slots)
            {
                foreach (SlotModel slot in this.Slots)
                {
                    if (slot.PlayerId > 0L && slot.Id != Exception && slot.Id != Exception2 && (Type == 0 && slot.State == State || Type == 1 && slot.State > State))
                    {
                        Account account = AccountManager.GetAccount(slot.PlayerId, true);
                        if (account != null && account.SlotId != -1)
                            allPlayers.Add(account);
                    }
                }
            }
            return allPlayers;
        }

        public List<Account> GetAllPlayers(int Exception)
        {
            List<Account> allPlayers = new List<Account>();
            lock (this.Slots)
            {
                foreach (SlotModel slot in this.Slots)
                {
                    long playerId = slot.PlayerId;
                    if (playerId > 0L && slot.Id != Exception)
                    {
                        Account account = AccountManager.GetAccount(playerId, true);
                        if (account != null && account.SlotId != -1)
                            allPlayers.Add(account);
                    }
                }
            }
            return allPlayers;
        }

        public List<Account> GetAllPlayers(long Exception)
        {
            List<Account> allPlayers = new List<Account>();
            lock (this.Slots)
            {
                foreach (SlotModel slot in this.Slots)
                {
                    if (slot.PlayerId > 0L && slot.PlayerId != Exception)
                    {
                        Account account = AccountManager.GetAccount(slot.PlayerId, true);
                        if (account != null && account.SlotId != -1)
                            allPlayers.Add(account);
                    }
                }
            }
            return allPlayers;
        }

        public List<Account> GetAllPlayers()
        {
            List<Account> allPlayers = new List<Account>();
            lock (this.Slots)
            {
                foreach (SlotModel slot in this.Slots)
                {
                    if (slot.PlayerId > 0L)
                    {
                        Account account = AccountManager.GetAccount(slot.PlayerId, true);
                        if (account != null && account.SlotId != -1)
                            allPlayers.Add(account);
                    }
                }
            }
            return allPlayers;
        }

        public int GetCountPlayers()
        {
            int countPlayers = 0;
            lock (this.Slots)
            {
                foreach (SlotModel slot in this.Slots)
                {
                    if (slot.PlayerId > 0L)
                    {
                        Account account = AccountManager.GetAccount(slot.PlayerId, true);
                        if (account != null && account.SlotId != -1)
                            ++countPlayers;
                    }
                }
            }
            return countPlayers;
        }

        public int GetPlayingPlayers(TeamEnum Team, bool InBattle)
        {
            int playingPlayers = 0;
            lock (this.Slots)
            {
                foreach (SlotModel slot in this.Slots)
                {
                    if (slot.PlayerId > 0L && !slot.SpecGM && (slot.Team == Team || Team == TeamEnum.TEAM_DRAW) && (InBattle && slot.State == SlotState.BATTLE_LOAD && !slot.Spectator || !InBattle && slot.State >= SlotState.LOAD))
                        ++playingPlayers;
                }
            }
            return playingPlayers;
        }

        public int GetPlayingPlayers(TeamEnum Team, SlotState State, int Type)
        {
            int playingPlayers = 0;
            lock (this.Slots)
            {
                foreach (SlotModel slot in this.Slots)
                {
                    if (slot.PlayerId > 0L && !slot.SpecGM && (Type == 0 && slot.State == State || Type == 1 && slot.State > State) && (Team == TeamEnum.TEAM_DRAW || slot.Team == Team))
                        ++playingPlayers;
                }
            }
            return playingPlayers;
        }

        public int GetPlayingPlayers(TeamEnum Team, SlotState State, int Type, int Exception)
        {
            int playingPlayers = 0;
            lock (this.Slots)
            {
                foreach (SlotModel slot in this.Slots)
                {
                    if (slot.Id != Exception && slot.PlayerId > 0L && !slot.SpecGM && (Type == 0 && slot.State == State || Type == 1 && slot.State > State) && (Team == TeamEnum.TEAM_DRAW || slot.Team == Team))
                        ++playingPlayers;
                }
            }
            return playingPlayers;
        }

        public void GetPlayingPlayers(bool InBattle, out int PlayerFR, out int PlayerCT)
        {
            PlayerFR = 0;
            PlayerCT = 0;
            lock (this.Slots)
            {
                foreach (SlotModel slot in this.Slots)
                {
                    if (slot.PlayerId > 0L && !slot.SpecGM && (InBattle && slot.State == SlotState.BATTLE && !slot.Spectator || !InBattle && slot.State >= SlotState.RENDEZVOUS))
                    {
                        if (slot.Team != TeamEnum.FR_TEAM)
                            ++PlayerCT;
                        else
                            ++PlayerFR;
                    }
                }
            }
        }

        public void GetPlayingPlayers(
          bool InBattle,
          out int PlayerFR,
          out int PlayerCT,
          out int DeathFR,
          out int DeathCT)
        {
            PlayerFR = 0;
            PlayerCT = 0;
            DeathFR = 0;
            DeathCT = 0;
            lock (this.Slots)
            {
                foreach (SlotModel slot in this.Slots)
                {
                    if (slot.DeathState.HasFlag((Enum)DeadEnum.Dead) && !slot.SpecGM)
                    {
                        if (slot.Team == TeamEnum.FR_TEAM)
                            ++DeathFR;
                        else
                            ++DeathCT;
                    }
                    if (slot.PlayerId > 0L && !slot.SpecGM && (InBattle && slot.State == SlotState.BATTLE && !slot.Spectator || !InBattle && slot.State >= SlotState.RENDEZVOUS))
                    {
                        if (slot.Team != TeamEnum.FR_TEAM)
                            ++PlayerCT;
                        else
                            ++PlayerFR;
                    }
                }
            }
        }

        public void CheckToEndWaitingBattle(bool host)
        {
            if (this.State != RoomState.COUNTDOWN && this.State != RoomState.LOADING && this.State != RoomState.RENDEZVOUS || !host && this.Slots[this.Leader].State != SlotState.BATTLE_READY)
                return;
            AllUtils.EndBattleNoPoints(this);
        }

        public void SpawnReadyPlayers()
        {
            lock (this.Slots)
            {
                bool flag = this.ThisModeHaveRounds() && (this.CountdownIG == (byte)3 || this.CountdownIG == (byte)5 || this.CountdownIG == (byte)7 || this.CountdownIG == (byte)9);
                if (((this.State != RoomState.PRE_BATTLE ? 0 : (!this.PreMatchCD ? 1 : 0)) & (flag ? 1 : 0)) != 0 && !this.IsBotMode())
                {
                    this.PreMatchCD = true;
                    using (PROTOCOL_BATTLE_COUNT_DOWN_ACK Packet = new PROTOCOL_BATTLE_COUNT_DOWN_ACK((int)this.CountdownIG))
                        this.SendPacketToPlayers(Packet);
                }
                this.PreMatchTime.StartJob(this.CountdownIG == (byte)0 ? 0 : (int)this.CountdownIG * 1000 + 250, (TimerCallback)(timer =>
                {
                    this.ExecuteSpawnReadyPlayers();
                    lock (timer)
                        this.PreMatchTime.StopJob();
                }));
            }
        }

        private void ExecuteSpawnReadyPlayers()
        {
            DateTime dateTime = DateTimeUtil.Now();
            foreach (SlotModel slot in this.Slots)
            {
                if (slot.State == SlotState.BATTLE_READY && slot.IsPlaying == 0 && slot.PlayerId > 0L)
                {
                    slot.IsPlaying = 1;
                    slot.StartTime = dateTime;
                    slot.State = SlotState.BATTLE;
                    if (this.State == RoomState.BATTLE && (this.RoomType == RoomCondition.Bomb || this.RoomType == RoomCondition.Annihilation || this.RoomType == RoomCondition.Convoy || this.RoomType == RoomCondition.Ace || this.RoomType == RoomCondition.Glass))
                        slot.Spectator = true;
                }
            }
            this.UpdateSlotsInfo();
            List<int> dinossaurs = AllUtils.GetDinossaurs(this, false, -1);
            if (this.State == RoomState.PRE_BATTLE)
            {
                this.BattleStart = this.IsDinoMode() ? dateTime.AddMinutes(5.0) : dateTime;
                this.SetGameModeSpecificBars();
            }
            bool flag = false;
            using (PROTOCOL_BATTLE_MISSION_ROUND_PRE_START_ACK roundPreStartAck = new PROTOCOL_BATTLE_MISSION_ROUND_PRE_START_ACK(this, dinossaurs))
            {
                using (PROTOCOL_BATTLE_MISSION_ROUND_START_ACK missionRoundStartAck = new PROTOCOL_BATTLE_MISSION_ROUND_START_ACK(this))
                {
                    using (PROTOCOL_BATTLE_RECORD_ACK protocolBattleRecordAck = new PROTOCOL_BATTLE_RECORD_ACK(this))
                    {
                        byte[] completeBytes1 = roundPreStartAck.GetCompleteBytes("Room.BaseSpawnReadyPlayers-1");
                        byte[] completeBytes2 = missionRoundStartAck.GetCompleteBytes("Room.BaseSpawnReadyPlayers-2");
                        byte[] completeBytes3 = protocolBattleRecordAck.GetCompleteBytes("Room.BaseSpawnReadyPlayers-3");
                        foreach (SlotModel slot in this.Slots)
                        {
                            Account Player;
                            if (slot.State == SlotState.BATTLE && slot.IsPlaying == 1 && this.GetPlayerBySlot(slot, out Player))
                            {
                                slot.FirstInactivityOff = true;
                                slot.IsPlaying = 2;
                                if (this.State == RoomState.PRE_BATTLE)
                                {
                                    using (PROTOCOL_BATTLE_STARTBATTLE_ACK Packet = new PROTOCOL_BATTLE_STARTBATTLE_ACK(slot, Player, dinossaurs, true))
                                    {
                                        this.SendPacketToPlayers(Packet, SlotState.READY, 1);
                                    }
                                    Player.SendCompletePacket(completeBytes1, roundPreStartAck.GetType().Name);
                                    if (!this.IsDinoMode())
                                    {
                                        Player.SendCompletePacket(completeBytes2, missionRoundStartAck.GetType().Name);
                                    }
                                    else
                                    {
                                        flag = true;
                                    }
                                }
                                else if (this.State == RoomState.BATTLE)
                                {
                                    using (PROTOCOL_BATTLE_STARTBATTLE_ACK Packet = new PROTOCOL_BATTLE_STARTBATTLE_ACK(slot, Player, dinossaurs, false))
                                    {
                                        this.SendPacketToPlayers(Packet, SlotState.READY, 1);
                                    }
                                    if (this.RoomType != RoomCondition.Bomb && this.RoomType != RoomCondition.Annihilation && this.RoomType != RoomCondition.Convoy && this.RoomType != RoomCondition.Ace && this.RoomType != RoomCondition.Glass)
                                    {
                                        Player.SendCompletePacket(completeBytes1, roundPreStartAck.GetType().Name);
                                    }
                                    else
                                    {
                                        EquipmentSync.SendUDPPlayerSync(this, slot, CouponEffects.None, 1);
                                    }
                                    Player.SendCompletePacket(completeBytes2, missionRoundStartAck.GetType().Name);
                                    Player.SendCompletePacket(completeBytes3, protocolBattleRecordAck.GetType().Name);
                                }
                            }
                        }
                    }
                }
                if (this.State == RoomState.PRE_BATTLE)
                {
                    this.State = RoomState.BATTLE;
                    this.UpdateRoomInfo();
                }
                this.UpdateSlotsInfo();
                if (!flag)
                    return;
                this.StartDinoModeRound();
            }
        }

        private void StartDinoModeRound()
        {
            this.RoundTime.StartJob(5250, (TimerCallback)(timer =>
            {
                if (this.State == RoomState.BATTLE)
                {
                    using (PROTOCOL_BATTLE_MISSION_ROUND_START_ACK Packet = new PROTOCOL_BATTLE_MISSION_ROUND_START_ACK(this))
                        this.SendPacketToPlayers(Packet, SlotState.BATTLE, 0);
                }
                lock (timer)
                    this.RoundTime.StopJob();
            }));
        }

        public bool IsDinoMode(string Dino = "")
        {
            switch (Dino)
            {
                case "DE":
                    return this.RoomType == RoomCondition.Boss;

                case "CC":
                    return this.RoomType == RoomCondition.CrossCounter;

                default:
                    return this.RoomType == RoomCondition.Boss || this.RoomType == RoomCondition.CrossCounter;
            }
        }

        public int GetReadyPlayers()
        {
            int Players = 0;
            foreach (SlotModel Slot in Slots)
            {
                if (Slot != null && Slot.State >= SlotState.READY && Slot.Equipment != null)
                {
                    Account Player = GetPlayerBySlot(Slot);
                    if (Player != null && Player.SlotId == Slot.Id)
                    {
                        Players += 1;
                    }
                }
            }
            return Players;
        }

        public int ResetReadyPlayers()
        {
            int num = 0;
            foreach (SlotModel slot in this.Slots)
            {
                if (slot.State == SlotState.READY)
                {
                    slot.State = SlotState.NORMAL;
                    ++num;
                }
            }
            return num;
        }

        public TeamEnum CheckTeam(int SlotIdx)
        {
            if (Array.Exists(FR_TEAM, V => V == SlotIdx))
            {
                return TeamEnum.FR_TEAM;
            }
            else if (Array.Exists(CT_TEAM, V => V == SlotIdx))
            {
                return TeamEnum.CT_TEAM;
            }
            else
            {
                return TeamEnum.ALL_TEAM;
            }
        }

        public bool IsSwappedBombMode()
        {
            if (this.SwapRound && ThisModeHaveRounds())
            {
                return true;
            }
            return false;
        }

        public TeamEnum ValidateTeam(TeamEnum Team, TeamEnum Costume)
        {
            if (RoomType == RoomCondition.FreeForAll)
                return Costume;

            // Si ya se hizo swap, invertir equipos
            if (SwapRound && Flag.HasFlag(RoomStageFlag.TEAM_SWAP))
                return Team == TeamEnum.FR_TEAM ? TeamEnum.CT_TEAM : TeamEnum.FR_TEAM;

            return Team;
        }

        public bool IsTeamSwap()
        {
            // Si no tiene flag de swap, retornar false
            if (!Flag.HasFlag(RoomStageFlag.TEAM_SWAP))
                return false;

            // Retornar true si:
            // 1. Es el momento exacto del swap (suma == punto de swap)
            // 2. O ya se hizo el swap (SwapRound == true)
            return FRRounds + CTRounds == GetSwapRoundByMask() || SwapRound;
        }

        public string RandomName(int Random)
        {
            switch (Random)
            {
                case 1:
                    return "Feel the Headshots!!";

                case 2:
                    return "Land of Dead!";

                case 3:
                    return "Kill! or be Killed!!";

                case 4:
                    return "Show Me Your Skills!!";

                default:
                    return "Point Blank!!";
            }
        }

        public void CheckGhostSlot(SlotModel Slot)
        {
            if (Slot.State == SlotState.EMPTY || Slot.State == SlotState.CLOSE || Slot.PlayerId != 0L || this.IsBotMode())
                return;
            Slot.ResetSlot();
            Slot.State = SlotState.EMPTY;
        }
    }

    // Helper classes to replace compiler-generated classes
    internal class InactivityHandler
    {
        public SlotModel slot;
        public RoomModel room;
        public Account player;
        public EventErrorEnum errorType;

        public void HandleInactivity(object state)
        {
            room.KickInactivePlayer(errorType, player, slot);
        }
    }

    internal class TeamChecker
    {
        public int slotIndex;

        public bool IsFRTeam(int teamSlot)
        {
            return teamSlot == slotIndex;
        }

        public bool IsCTTeam(int teamSlot)
        {
            return teamSlot == slotIndex;
        }
    }
}