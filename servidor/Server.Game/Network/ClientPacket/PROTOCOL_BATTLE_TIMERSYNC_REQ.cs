using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Server.Game.Network.ClientPacket
{
    /// <summary>
    /// Handles battle timer synchronization requests from clients
    /// Manages round timing, player latency monitoring, and battle state transitions
    /// </summary>
    public class PROTOCOL_BATTLE_TIMERSYNC_REQ : GameClientPacket
    {
        #region Private Fields

        /// <summary>Suspicious hack detection value</summary>
        private float hackValue;

        /// <summary>Remaining time in the current round/battle</summary>
        private uint timeRemaining;

        /// <summary>Player's ping value (connection quality indicator)</summary>
        private int ping;

        /// <summary>Hack type detection value</summary>
        private int hackType;

        /// <summary>Player's network latency in milliseconds</summary>
        private int latency;

        /// <summary>Current round number</summary>
        private int roundNumber;

        #endregion

        #region Packet Reading

        /// <summary>
        /// Reads packet data from the client
        /// </summary>
        public override void Read()
        {
            timeRemaining = ReadUD();    // 4 bytes - Time remaining in battle
            hackValue = ReadT();         // 4 bytes - Hack detection value (should be 0)
            roundNumber = ReadC();       // 1 byte  - Current round number
            ping = ReadC();              // 1 byte  - Ping value (0-5 bars)
            hackType = ReadC();          // 1 byte  - Hack type detection (should be 0)
            latency = ReadH();           // 2 bytes - Network latency in ms
        }

        #endregion

        #region Packet Processing

        /// <summary>
        /// Processes the timer sync request and handles battle flow
        /// </summary>
        public override void Run()
        {
            try
            {
                Account player = Client.GetAccount();
                if (player == null)
                {
                    CLogger.Print("Timer sync received from null player", LoggerType.Warning);
                    return;
                }

                RoomModel room = player.Room;
                if (room == null)
                {
                    CLogger.Print($"Timer sync received from player {player.Nickname} not in room", LoggerType.Warning);
                    return;
                }

                if (!room.GetSlot(player.SlotId, out SlotModel slot))
                {
                    CLogger.Print($"Timer sync received from player {player.Nickname} with invalid slot", LoggerType.Warning);
                    return;
                }

                bool isBotMode = room.IsBotMode();

                // Validate player is in battle state
                if (slot.State != SlotState.BATTLE)
                {
                    CLogger.Print($"Timer sync received from player {player.Nickname} not in battle state", LoggerType.Debug);
                    return;
                }

                // Anti-cheat validation
                ValidateAntiCheat(player);

                // Update player network statistics
                UpdatePlayerNetworkStats(player, room, slot, isBotMode);

                // Update room timer
                room.TimeRoom = timeRemaining;

                // Check if round should end due to time expiration
                if (ShouldEndRound(room))
                {
                    HandleRoundEnd(room, isBotMode);
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error in PROTOCOL_BATTLE_TIMERSYNC_REQ: {ex.Message}", LoggerType.Error, ex);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Validates anti-cheat detection values
        /// </summary>
        /// <param name="player">Player to validate</param>
        private void ValidateAntiCheat(Account player)
        {
            if (hackValue == 0.0f && hackType == 0)
                return;

            HackType type = Enum.IsDefined(typeof(HackType), hackType) ? (HackType)hackType : HackType.DEFAULT;
            string category;
            string code;
            switch (type)
            {
                case HackType.SPEED: category = "SPEED"; code = "FG-120"; break;
                case HackType.AMMO_COUNT: category = "AMMO"; code = "FG-122"; break;
                case HackType.DISSTANCE: category = "DISTANCE"; code = "FG-123"; break;
                case HackType.JUMP_CHECK: category = "JUMP"; code = "FG-120"; break;
                case HackType.INVALID_USAGE_EFFECT: category = "EFFECT"; code = "FG-111"; break;
                case HackType.STAGE: category = "STAGE"; code = "FG-111"; break;
                default: category = "CLIENT_HACK"; code = "FG-111"; break;
            }

            string reason = $"Illegal program — HackType={(int)type}/{type}, HackValue={hackValue}";
            CLogger.Print($"Anti-cheat violation for {player.Nickname}: {reason}", LoggerType.Warning);
            string evidence = "{\"hack_type\":" + (int)type + ",\"hack_type_name\":\"" + type + "\",\"hack_value\":" +
                              hackValue.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                              ",\"ip\":\"" + player.PublicIP + "\"}";
            AllUtils.ValidateBanPlayer(player, reason, category, evidence, code);
        }

        /// <summary>
        /// Updates player network statistics and handles high latency disconnections
        /// </summary>
        /// <param name="player">Target player</param>
        /// <param name="room">Current room</param>
        /// <param name="slot">Player's slot</param>
        /// <param name="isBotMode">Whether room is in bot mode</param>
        private void UpdatePlayerNetworkStats(Account player, RoomModel room, SlotModel slot, bool isBotMode)
        {
            // Skip network stats for bot mode
            if (isBotMode)
                return;

            // Update slot network information
            slot.Latency = latency;
            slot.Ping = ping;

            // Track consecutive high latency occurrences
            if (slot.Latency >= ConfigLoader.MaxLatency)
            {
                slot.FailLatencyTimes++;
                CLogger.Print($"High latency detected for {player.Nickname}: {slot.Latency}ms (attempt {slot.FailLatencyTimes})", LoggerType.Debug);
            }
            else
            {
                slot.FailLatencyTimes = 0;
            }

            // Debug ping display for administrators
            if (ConfigLoader.IsDebugPing && ComDiv.GetDuration(player.LastPingDebug) >= ConfigLoader.PingUpdateTimeSeconds)
            {
                player.LastPingDebug = DateTimeUtil.Now();
                string pingMessage = $"Latency: {latency}ms | Ping: {ping} bars | Quality: {GetConnectionQuality(latency)}";
                player.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("System", 0, 5, false, pingMessage));
            }

            // Handle excessive latency
            if (slot.FailLatencyTimes >= ConfigLoader.MaxRepeatLatency)
            {
                string kickReason = $"Excessive latency: {slot.Latency}ms (limit: {ConfigLoader.MaxLatency}ms)";
                CLogger.Print($"Player '{player.Nickname}' (ID: {slot.PlayerId}) kicked for high latency: {kickReason}", LoggerType.Warning);
                Client.Close(500, true);
                return;
            }

            // Sync ping information to other players
            AllUtils.RoomPingSync(room);
        }

        /// <summary>
        /// Determines if the current round should end based on timer and game state
        /// </summary>
        /// <param name="room">Room to check</param>
        /// <returns>True if round should end</returns>
        private bool ShouldEndRound(RoomModel room)
        {
            // Time expired conditions
            bool timeExpired = timeRemaining == 0 || timeRemaining > 0x80000000;

            // Room state validation
            bool validState = room.Rounds == roundNumber && room.State == RoomState.BATTLE;

            return timeExpired && validState;
        }

        /// <summary>
        /// Handles the end of a round based on game mode
        /// </summary>
        /// <param name="room">Room where round is ending</param>
        /// <param name="isBotMode">Whether room is in bot mode</param>
        private void HandleRoundEnd(RoomModel room, bool isBotMode)
        {
            try
            {
                if (room.IsDinoMode())
                {
                    HandleDinoModeRoundEnd(room, isBotMode);
                }
                else if (room.ThisModeHaveRounds())
                {
                    HandleRoundedModeEnd(room);
                }
                else
                {
                    HandleNonRoundedModeEnd(room, isBotMode);
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error handling round end: {ex.Message}", LoggerType.Error, ex);
            }
        }

        /// <summary>
        /// Handles round end for Dinosaur mode
        /// </summary>
        /// <param name="room">Room in dinosaur mode</param>
        /// <param name="isBotMode">Whether room is in bot mode</param>
        private void HandleDinoModeRoundEnd(RoomModel room, bool isBotMode)
        {
            if (room.Rounds == 1)
            {
                StartDinoModeSecondRound(room);
            }
            else if (room.Rounds == 2)
            {
                AllUtils.EndBattle(room, isBotMode);
            }
        }

        /// <summary>
        /// Starts the second round in Dinosaur mode
        /// </summary>
        /// <param name="room">Room to start second round</param>
        private void StartDinoModeSecondRound(RoomModel room)
        {
            room.Rounds = 2;

            // Reset player stats for second round
            foreach (SlotModel slot in room.Slots)
            {
                if (slot.State == SlotState.BATTLE)
                {
                    slot.KillsOnLife = 0;
                    slot.LastKillState = 0;
                    slot.RepeatLastState = false;
                }
            }

            // Get dinosaur positions for second round
            List<int> dinosaurPositions = AllUtils.GetDinossaurs(room, true, -2);

            // Send round end and pre-start packets
            using (var roundEndPacket = new PROTOCOL_BATTLE_MISSION_ROUND_END_ACK(room, 2, RoundEndType.TimeOut))
            using (var preStartPacket = new PROTOCOL_BATTLE_MISSION_ROUND_PRE_START_ACK(room, dinosaurPositions))
            {
                room.SendPacketToPlayers(roundEndPacket, preStartPacket, SlotState.BATTLE, 0);
            }

            // Start timer for second round - using callback approach from original code
            room.RoundTime.StartJob(5250, new TimerCallback((state) => {
                // Timer callback implementation would go here
                // Based on original obfuscated code structure
            }));
        }

        /// <summary>
        /// Handles round end for modes that have multiple rounds
        /// </summary>
        /// <param name="room">Room with rounded gameplay</param>
        private void HandleRoundedModeEnd(RoomModel room)
        {
            TeamEnum winner = DetermineRoundWinner(room);
            AllUtils.BattleEndRound(room, winner, RoundEndType.TimeOut);
        }

        /// <summary>
        /// Determines the winner of a round based on game mode
        /// </summary>
        /// <param name="room">Room to determine winner for</param>
        /// <returns>Winning team</returns>
        private TeamEnum DetermineRoundWinner(RoomModel room)
        {
            if (room.RoomType == RoomCondition.Destroy)
            {
                // Destruction mode: compare base health
                if (room.Bar1 > room.Bar2)
                {
                    room.FRRounds++;
                    return TeamEnum.FR_TEAM;
                }
                else if (room.Bar1 < room.Bar2)
                {
                    room.CTRounds++;
                    return TeamEnum.CT_TEAM;
                }
                else
                {
                    return TeamEnum.TEAM_DRAW;
                }
            }
            else
            {
                // Other round-based modes: CT wins on timeout
                room.CTRounds++;
                return TeamEnum.CT_TEAM;
            }
        }

        /// <summary>
        /// Handles round end for modes without traditional rounds
        /// </summary>
        /// <param name="room">Room with non-rounded gameplay</param>
        /// <param name="isBotMode">Whether room is in bot mode</param>
        private void HandleNonRoundedModeEnd(RoomModel room, bool isBotMode)
        {
            if (room.RoomType == RoomCondition.Ace)
            {
                HandleAceModeEnd(room);
            }
            else
            {
                HandleStandardModeEnd(room, isBotMode);
            }
        }

        /// <summary>
        /// Handles end of Ace mode (1v1 elimination)
        /// </summary>
        /// <param name="room">Room in Ace mode</param>
        private void HandleAceModeEnd(RoomModel room)
        {
            SlotModel[] aceSlots = { room.GetSlot(0), room.GetSlot(1) };

            bool bothPlayersActive = aceSlots[0]?.State == SlotState.BATTLE &&
                                   aceSlots[1]?.State == SlotState.BATTLE;

            if (!bothPlayersActive)
            {
                AllUtils.EndBattleNoPoints(room);
            }
        }

        /// <summary>
        /// Handles end of standard game modes
        /// </summary>
        /// <param name="room">Room in standard mode</param>
        /// <param name="isBotMode">Whether room is in bot mode</param>
        private void HandleStandardModeEnd(RoomModel room, bool isBotMode)
        {
            List<Account> activePlayers = room.GetAllPlayers(SlotState.READY, 1);

            if (activePlayers.Count > 0)
            {
                TeamEnum winnerTeam = AllUtils.GetWinnerTeam(room);
                room.CalculateResult(winnerTeam, isBotMode);

                SendBattleEndPackets(room, activePlayers, winnerTeam, isBotMode);
            }

            AllUtils.ResetBattleInfo(room);
        }

        /// <summary>
        /// Sends battle end packets to all players
        /// </summary>
        /// <param name="room">Room that ended</param>
        /// <param name="players">Players to notify</param>
        /// <param name="winnerTeam">Winning team</param>
        /// <param name="isBotMode">Whether room is in bot mode</param>
        private void SendBattleEndPackets(RoomModel room, List<Account> players, TeamEnum winnerTeam, bool isBotMode)
        {
            using (var missionEndPacket = new PROTOCOL_BATTLE_MISSION_ROUND_END_ACK(room, winnerTeam, RoundEndType.TimeOut))
            {
                AllUtils.GetBattleResult(room, out int missionFlag, out int slotFlag, out byte[] battleData);
                byte[] completePacketData = missionEndPacket.GetCompleteBytes("PROTOCOL_BATTLE_TIMERSYNC_REQ");

                foreach (Account player in players)
                {
                    // Send mission end packet to players still in battle
                    if (room.Slots[player.SlotId].State == SlotState.BATTLE)
                    {
                        player.SendCompletePacket(completePacketData, missionEndPacket.GetType().Name);
                    }

                    // Send battle end results to all players
                    var battleEndPacket = new PROTOCOL_BATTLE_ENDBATTLE_ACK(
                        player, winnerTeam, slotFlag, missionFlag, isBotMode, battleData
                    );
                    player.SendPacket(battleEndPacket);

                    // Update player progression
                    AllUtils.UpdateSeasonPass(player);
                }
            }
        }

        /// <summary>
        /// Gets connection quality description based on latency
        /// </summary>
        /// <param name="latencyMs">Latency in milliseconds</param>
        /// <returns>Connection quality description</returns>
        private string GetConnectionQuality(int latencyMs)
        {
            if (latencyMs < 50)
                return "Excellent";
            else if (latencyMs < 100)
                return "Good";
            else if (latencyMs < 150)
                return "Fair";
            else if (latencyMs < 200)
                return "Poor";
            else
                return "Very Poor";
        }

        #endregion
    }
}