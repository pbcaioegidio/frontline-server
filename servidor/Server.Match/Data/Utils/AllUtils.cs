// Decompiled with JetBrains decompiler
// Type: Server.Match.Data.Utils.AllUtils
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.SharpDX;
using Plugin.Core.Utility;
using Server.Match.Data.Enums;
using Server.Match.Data.Models;
using Server.Match.Data.Models.Event;
using Server.Match.Data.XML;
using System;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;


namespace Server.Match.Data.Utils
{
    public static class AllUtils
    {
        public static float GetDuration(DateTime Date)
        {
            return (float)(DateTimeUtil.Now() - Date).TotalSeconds;
        }

        /// <summary>
        /// Anti-teleporte/speed: rejeita delta impossível e reverte para última posição válida.
        /// </summary>
        public static bool ValidatePlayerMovement(PlayerModel player, Half3 newPos, out Half3 acceptedPos)
        {
            acceptedPos = newPos;
            if (player == null)
                return true;

            DateTime now = DateTimeUtil.Now();
            if (player.LastPositionAt == default(DateTime) || player.Dead || player.NeverRespawn)
            {
                player.LastValidPosition = newPos;
                player.LastPositionAt = now;
                player.SpeedViolations = 0;
                return true;
            }

            double dt = (now - player.LastPositionAt).TotalSeconds;
            if (dt <= 0.001)
            {
                acceptedPos = player.LastValidPosition;
                return false;
            }

            if (dt > 2.5)
            {
                player.LastValidPosition = newPos;
                player.LastPositionAt = now;
                return true;
            }

            float dist = Vector3.Distance((Vector3)player.LastValidPosition, (Vector3)newPos);
            const float maxSpeed = 35f;
            float maxDist = (float)(maxSpeed * dt) + 2.5f;
            if (dist > maxDist)
            {
                player.SpeedViolations++;
                CLogger.Print(
                    $"[AntiSpeed] Slot={player.Slot} Dist={dist:F1} Max={maxDist:F1} Dt={dt:F3}s Violations={player.SpeedViolations}",
                    LoggerType.Hack);
                acceptedPos = player.LastValidPosition;
                player.LastPositionAt = now;

                int kickAt = Math.Max(3, ConfigLoader.SpeedKickViolations);
                int banAt = Math.Max(kickAt + 1, ConfigLoader.SpeedBanViolations);
                if (player.SpeedViolations == kickAt || player.SpeedViolations == banAt || player.SpeedViolations % 10 == 0)
                {
                    Plugin.Core.Security.SecurityDao.LogEvent(
                        Plugin.Core.Security.SecurityDao.SourceMatch, 0, "", "",
                        "flag", "SPEED",
                        $"Speed violations={player.SpeedViolations} slot={player.Slot} dist={dist:F1}",
                        "{\"slot\":" + player.Slot + ",\"violations\":" + player.SpeedViolations +
                        ",\"dist\":" + dist.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) + "}",
                        player.SpeedViolations >= banAt ? 5 : 3, "FG-120");
                }
                // Congela o jogador na partida (sem respawn) após limiar — Game aplica ban via TIMERSYNC/AutoBan
                if (player.SpeedViolations >= kickAt)
                {
                    player.Dead = true;
                    player.NeverRespawn = true;
                }

                return false;
            }

            player.LastValidPosition = newPos;
            player.LastPositionAt = now;
            if (player.SpeedViolations > 0)
                player.SpeedViolations = Math.Max(0, player.SpeedViolations - 1);
            return true;
        }

        public static ItemClass ItemClassified(ClassType ClassWeapon)
        {
            ItemClass itemClass = ItemClass.Unknown;
            switch (ClassWeapon)
            {
                case ClassType.Knife:
                case ClassType.DualKnife:
                case ClassType.Knuckle:
                    itemClass = ItemClass.Melee;
                    break;
                case ClassType.HandGun:
                case ClassType.CIC:
                case ClassType.DualHandGun:
                    itemClass = ItemClass.Secondary;
                    break;
                case ClassType.Assault:
                    itemClass = ItemClass.Primary;
                    break;
                case ClassType.SMG:
                case ClassType.DualSMG:
                    itemClass = ItemClass.Primary;
                    break;
                case ClassType.Sniper:
                    itemClass = ItemClass.Primary;
                    break;
                case ClassType.Shotgun:
                case ClassType.DualShotgun:
                    itemClass = ItemClass.Primary;
                    break;
                case ClassType.ThrowingGrenade:
                    itemClass = ItemClass.Explosive;
                    break;
                case ClassType.ThrowingSpecial:
                    itemClass = ItemClass.Special;
                    break;
                case ClassType.Machinegun:
                    itemClass = ItemClass.Primary;
                    break;
                case ClassType.Dino:
                    itemClass = ItemClass.Unknown;
                    break;
            }
            return itemClass;
        }

        public static ObjectType GetHitType(uint HitInfo) => (ObjectType)((int)HitInfo & 3);

        public static int GetHitWho(uint HitInfo) => (int)(HitInfo >> 2) & 511 /*0x01FF*/;

        public static CharaHitPart GetHitPart(uint HitInfo)
        {
            return (CharaHitPart)((int)(HitInfo >> 11) & 63 /*0x3F*/);
        }

        public static int GetHitDamageBot(uint HitInfo) => (int)(HitInfo >> 20);

        public static int GetHitDamageNormal(uint HitInfo) => (int)(HitInfo >> 21);

        public static int GetHitHelmet(uint info) => (int)(info >> 17) & 7;

        public static CharaDeath GetCharaDeath(uint HitInfo) => (CharaDeath)((int)HitInfo & 15);

        public static int GetKillerId(uint HitInfo) => (int)(HitInfo >> 11) & 511 /*0x01FF*/;

        public static int GetObjectType(uint HitInfo) => (int)(HitInfo >> 10) & 1;

        public static int GetRoomInfo(uint UniqueRoomId, int Type)
        {
            switch (Type)
            {
                case 0:
                    return (int)UniqueRoomId & 4095 /*0x0FFF*/;
                case 1:
                    return (int)(UniqueRoomId >> 12) & (int)byte.MaxValue;
                case 2:
                    return (int)(UniqueRoomId >> 20) & 4095 /*0x0FFF*/;
                default:
                    return 0;
            }
        }

        public static int GetSeedInfo(uint Seed, int Type)
        {
            switch (Type)
            {
                case 0:
                    return (int)Seed & 4095 /*0x0FFF*/;
                case 1:
                    return (int)(Seed >> 12) & (int)byte.MaxValue;
                case 2:
                    return (int)(Seed >> 20) & 4095 /*0x0FFF*/;
                default:
                    return 0;
            }
        }

        public static byte[] BaseWriteCode(
          int Opcode,
          byte[] Actions,
          int SlotId,
          float Time,
          int Round,
          int Respawn,
          int RoundNumber,
          int AccountId)
        {
            if (Actions == null || Actions.Length == 0)
                return new byte[0];

            byte[] key1 = BitConverter.GetBytes(Time);
            uint key2;
            byte[] blob = Udp121Cipher.EncryptServerToClient(Actions, key1, out key2); // as we update to 122 we maybe need to recheck this on IDA (mcp 13337 has dump 122)
            int length = 25 + blob.Length;
            byte[] encryptedBlob = Bitwise.Encrypt(blob, length % 6 + 1);

            using (SyncServerPacket packet = new SyncServerPacket())
            {
                packet.WriteC((byte)Opcode);
                packet.WriteC((byte)SlotId);
                packet.WriteT(Time);
                packet.WriteC((byte)Round);
                packet.WriteH((ushort)length);
                packet.WriteC((byte)Respawn);
                packet.WriteC((byte)RoundNumber);
                packet.WriteC((byte)AccountId);
                packet.WriteC(0);
                packet.WriteD(key2);
                packet.WriteD(0);
                packet.WriteD(0);
                packet.WriteB(encryptedBlob);
                return packet.ToArray();
            }
        }

        
        public static bool ValidateHitData(int RawDamage, HitDataInfo Hit, out int Damage)
        {
            return ValidateHitData(RawDamage, Hit, null, out Damage);
        }

        public static bool ValidateHitData(int RawDamage, HitDataInfo Hit, PlayerModel shooter, out int Damage)
        {
            if (!ConfigLoader.AntiScript)
            {
                Damage = RawDamage;
                return true;
            }
            ItemsStatistic itemStats = ItemStatisticXML.GetItemStats(Hit.WeaponId);
            if (itemStats == null)
            {
                CLogger.Print($"The Item Statistic was not found. Please add: {Hit.WeaponId} to config!", LoggerType.Warning);
                Damage = 0;
                return false;
            }
            ItemClass itemClass = AllUtils.ItemClassified(Hit.WeaponClass);
            float num1 = Vector3.Distance((Vector3)Hit.StartBullet, (Vector3)Hit.EndBullet);
            if (itemClass == ItemClass.Melee || (double)num1 <= (double)itemStats.Range)
            {
                if (itemClass == ItemClass.Melee && (double)num1 > (double)itemStats.Range)
                {
                    Damage = 0;
                    return false;
                }
                if (AllUtils.GetHitPart(Hit.HitIndex) != CharaHitPart.HEAD)
                {
                    int num2 = itemStats.Damage + itemStats.Damage * 30 / 100;
                    if (itemClass != ItemClass.Melee && RawDamage > num2)
                    {
                        Damage = 0;
                        return false;
                    }
                    if (itemClass == ItemClass.Melee && RawDamage > itemStats.Damage)
                    {
                        Damage = 0;
                        return false;
                    }
                }
                else
                {
                    // Headshot: só corta dano claramente impossível (cheat).
                    // Armas fortes / sniper / HelmetPenetrate podem passar bem acima do Damage base.
                    int maxHead = itemStats.Damage * 8 + Math.Max(0, itemStats.HelmetPenetrate) * 2;
                    if (maxHead < 1500)
                        maxHead = 1500;
                    if (itemStats.Damage >= 500)
                        maxHead = Math.Max(maxHead, itemStats.Damage * 4); // RPG / explosivo
                    if (RawDamage > maxHead)
                    {
                        Damage = 0;
                        if (shooter != null)
                        {
                            CLogger.Print($"[AntiHit] Slot={shooter.Slot} HEAD dmg={RawDamage} max={maxHead} weapon={Hit.WeaponId}", LoggerType.Hack);
                            Plugin.Core.Security.SecurityDao.LogEvent(
                                Plugin.Core.Security.SecurityDao.SourceMatch, Math.Max(0, shooter.PlayerIdByServer), "", "",
                                "flag", "HIT_HEAD",
                                $"Dano headshot inválido weapon={Hit.WeaponId} dmg={RawDamage}",
                                "{\"slot\":" + shooter.Slot + ",\"weapon\":" + Hit.WeaponId + ",\"dmg\":" + RawDamage + ",\"max\":" + maxHead + "}",
                                4, "FG-123");
                        }
                        return false;
                    }
                }

                // Cadência: FireDelay em ms (ItemStatistic). Folga 35% para lag.
                if (shooter != null && itemStats.FireDelay > 0 && itemClass != ItemClass.Melee)
                {
                    DateTime now = DateTimeUtil.Now();
                    if (shooter.LastFireAt != default(DateTime))
                    {
                        double minMs = itemStats.FireDelay * 0.65;
                        double elapsed = (now - shooter.LastFireAt).TotalMilliseconds;
                        if (elapsed < minMs)
                        {
                            shooter.FireRateViolations++;
                            if (shooter.FireRateViolations >= 5)
                            {
                                CLogger.Print($"[AntiFireRate] Slot={shooter.Slot} Weapon={Hit.WeaponId} Elapsed={elapsed:F0}ms Min={minMs:F0} Viol={shooter.FireRateViolations}", LoggerType.Hack);
                                Plugin.Core.Security.SecurityDao.LogEvent(
                                    Plugin.Core.Security.SecurityDao.SourceMatch, 0, "", "", "flag", "HIT_RATE",
                                    $"Fire rate inválido weapon={Hit.WeaponId} viol={shooter.FireRateViolations}",
                                    "{\"slot\":" + shooter.Slot + ",\"weapon\":" + Hit.WeaponId + ",\"elapsed_ms\":" + (int)elapsed + "}",
                                    3, "FG-121");
                            }
                            Damage = 0;
                            return false;
                        }
                    }
                    shooter.LastFireAt = now;
                    if (shooter.FireRateViolations > 0)
                        shooter.FireRateViolations = Math.Max(0, shooter.FireRateViolations - 1);
                }

                Damage = RawDamage;
                return true;
            }
            Damage = 0;
            return false;
        }

        /// <summary>
        /// Valida munição reportada no GetWeaponForClient contra ItemStatisticXML.
        /// AmmoPrin &gt; BulletLoaded ou AmmoTotal &gt; BulletTotal → flag FG-122.
        /// </summary>
        public static bool ValidateWeaponAmmo(PlayerModel player, WeaponClient info)
        {
            if (player == null || info == null)
                return true;

            ItemsStatistic stats = ItemStatisticXML.GetItemStats(info.WeaponId);
            if (stats == null || (stats.BulletLoaded <= 0 && stats.BulletTotal <= 0))
            {
                // troca de arma / reload — zera contador de tiros do magazine
                player.MagWeaponId = info.WeaponId;
                player.MagShots = 0;
                return true;
            }

            bool bad = false;
            // folga pequena (+2 / +10%) para lag / dual
            int maxMag = Math.Max(stats.BulletLoaded + 2, (int)(stats.BulletLoaded * 1.1) + 1);
            int maxTotal = Math.Max(stats.BulletTotal + 5, (int)(stats.BulletTotal * 1.1) + 1);

            if (stats.BulletLoaded > 0 && info.AmmoPrin > maxMag)
                bad = true;
            if (stats.BulletTotal > 0 && info.AmmoTotal > maxTotal)
                bad = true;

            // reload/troca → reseta contador
            player.MagWeaponId = info.WeaponId;
            player.MagShots = 0;

            if (!bad) return true;

            player.AmmoViolations++;
            CLogger.Print(
                $"[AntiAmmo] Slot={player.Slot} Weapon={info.WeaponId} Mag={info.AmmoPrin}/{stats.BulletLoaded} Total={info.AmmoTotal}/{stats.BulletTotal} Viol={player.AmmoViolations}",
                LoggerType.Hack);

            if (player.AmmoViolations >= 3)
            {
                Plugin.Core.Security.SecurityDao.LogEvent(
                    Plugin.Core.Security.SecurityDao.SourceMatch, 0, "", "", "flag", "AMMO",
                    $"Munição inválida weapon={info.WeaponId} mag={info.AmmoPrin}/{stats.BulletLoaded}",
                    "{\"slot\":" + player.Slot + ",\"weapon\":" + info.WeaponId +
                    ",\"ammo_prin\":" + info.AmmoPrin + ",\"ammo_total\":" + info.AmmoTotal +
                    ",\"max_mag\":" + stats.BulletLoaded + ",\"max_total\":" + stats.BulletTotal + "}",
                    4, "FG-122");
            }

            // clamp enviado ao peer
            if (stats.BulletLoaded > 0 && info.AmmoPrin > maxMag)
                info.AmmoPrin = (ushort)Math.Min(maxMag, ushort.MaxValue);
            if (stats.BulletTotal > 0 && info.AmmoTotal > maxTotal)
                info.AmmoTotal = (ushort)Math.Min(maxTotal, ushort.MaxValue);
            return false;
        }

        /// <summary>Conta tiros desde o último GetWeaponForClient; estoura mag → FG-122.</summary>
        public static void TrackFireShot(PlayerModel player, int weaponId)
        {
            if (player == null || weaponId <= 0) return;
            if (player.MagWeaponId != weaponId)
            {
                player.MagWeaponId = weaponId;
                player.MagShots = 0;
            }
            player.MagShots++;

            ItemsStatistic stats = ItemStatisticXML.GetItemStats(weaponId);
            if (stats == null || stats.BulletLoaded <= 0) return;

            int limit = stats.BulletLoaded + 3; // folga
            if (player.MagShots > limit)
            {
                player.AmmoViolations++;
                if (player.AmmoViolations == 1 || player.AmmoViolations % 5 == 0)
                {
                    CLogger.Print($"[AntiAmmo] Slot={player.Slot} shots={player.MagShots} > mag={stats.BulletLoaded} weapon={weaponId}", LoggerType.Hack);
                    Plugin.Core.Security.SecurityDao.LogEvent(
                        Plugin.Core.Security.SecurityDao.SourceMatch, 0, "", "", "flag", "AMMO",
                        $"Tiros além da mag ({player.MagShots}/{stats.BulletLoaded})",
                        "{\"slot\":" + player.Slot + ",\"weapon\":" + weaponId + ",\"shots\":" + player.MagShots + "}",
                        4, "FG-122");
                }
            }
        }

        /// <summary>
        /// Conta kills em janela curta. Burst impossível → FG-124 + clip + congela na partida.
        /// </summary>
        public static void TrackKillBurst(PlayerModel killer, bool wasHeadshot)
        {
            if (killer == null || !ConfigLoader.AntiScript)
                return;

            DateTime now = DateTimeUtil.Now();
            int windowSec = Math.Max(3, ConfigLoader.KillBurstWindowSeconds);
            int maxKills = Math.Max(3, ConfigLoader.KillBurstMaxKills);

            if (killer.KillWindowStart == default(DateTime) ||
                (now - killer.KillWindowStart).TotalSeconds > windowSec)
            {
                killer.KillWindowStart = now;
                killer.KillsInWindow = 0;
                killer.HeadshotsInWindow = 0;
            }

            killer.KillsInWindow++;
            if (wasHeadshot)
                killer.HeadshotsInWindow++;

            bool burst = killer.KillsInWindow >= maxKills;
            bool headSpam = killer.HeadshotsInWindow >= Math.Max(3, maxKills - 1)
                            && (now - killer.KillWindowStart).TotalSeconds <= windowSec;

            if (!burst && !headSpam)
                return;

            killer.KillBurstViolations++;
            long pid = killer.PlayerIdByServer > 0 ? killer.PlayerIdByServer : 0;
            string why = burst
                ? $"Burst {killer.KillsInWindow} kills / {windowSec}s"
                : $"Headspam {killer.HeadshotsInWindow} HS / {windowSec}s";

            CLogger.Print($"[AntiBurst] Slot={killer.Slot} {why} Viol={killer.KillBurstViolations}", LoggerType.Hack);
            Plugin.Core.Security.SecurityDao.LogEvent(
                Plugin.Core.Security.SecurityDao.SourceMatch, pid, "", "",
                "flag", "KILL_BURST", why,
                "{\"slot\":" + killer.Slot + ",\"kills\":" + killer.KillsInWindow +
                ",\"hs\":" + killer.HeadshotsInWindow + ",\"window_s\":" + windowSec + "}",
                5, "FG-124");

            if (pid > 0)
            {
                Plugin.Core.Security.SecurityDao.RequestCapture(
                    pid, "clip", "auto:KILL_BURST — " + why,
                    "match:antiburst", 0, Plugin.Core.Security.SecurityDao.SourceMatch);
            }

            killer.Dead = true;
            killer.NeverRespawn = true;
            killer.KillsInWindow = 0;
            killer.HeadshotsInWindow = 0;
            killer.KillWindowStart = now;
        }

        
        public static bool ValidateGrenadeHit(int RawDamage, GrenadeHitInfo Hit, out int Damage)
        {
            if (!ConfigLoader.AntiScript)
            {
                Damage = RawDamage * 2;
                return true;
            }
            ItemsStatistic itemStats = ItemStatisticXML.GetItemStats(Hit.WeaponId);
            if (itemStats != null)
            {
                int num1 = (int)AllUtils.ItemClassified(Hit.WeaponClass);
                float num2 = Vector3.Distance((Vector3)Hit.FirePos, (Vector3)Hit.HitPos);
                if (num1 == 4)
                {
                    if ((double)num2 <= (double)itemStats.Range)
                    {
                        if (RawDamage > itemStats.Damage)
                        {
                            Damage = 0;
                            return false;
                        }
                    }
                    else
                    {
                        Damage = 0;
                        return false;
                    }
                }
                Damage = RawDamage * 2;
                return true;
            }
            CLogger.Print($"The Item Statistic was not found. Please add: {Hit.WeaponId} to config!", LoggerType.Warning);
            Damage = 0;
            return false;
        }

        
        public static void GetDecryptedData(PacketModel Packet)
        {
            try
            {
                if (IsUdp121Handshake(Packet))
                {
                    Packet.WithEndData = BuildUdp121HandshakeBody(Packet);
                    Packet.WithoutEndData = new byte[0];
                    return;
                }
                if (IsUdp121GameData(Packet) && TryDecryptUdp121GameData(Packet))
                    return;
                if (Packet.Data.Length >= Packet.Length)
                {
                    byte[] numArray = new byte[Packet.Length - 17];
                    Array.Copy((Array)Packet.Data, 17, (Array)numArray, 0, numArray.Length);
                    byte[] sourceArray = Bitwise.Decrypt(numArray, Packet.Length % 6 + 1);
                    byte[] destinationArray = new byte[sourceArray.Length - 9];
                    Array.Copy((Array)sourceArray, (Array)destinationArray, destinationArray.Length);
                    Packet.WithEndData = sourceArray;
                    Packet.WithoutEndData = destinationArray;
                }
                else
                    CLogger.Print($"Invalid packet size. (Packet.Data.Length >= Packet.Length): [ {Packet.Data.Length} | {Packet.Length} ]", LoggerType.Warning);
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        private static bool IsUdp121GameData(PacketModel Packet)
        {
            int op = Packet.Opcode;
            return op == 3 || op == 4 || op == 131 || op == 132;
        }

        // 121 game-data wire: [25B plaintext header][ rotate( cipherBlob ++ 9B trailer ) ].
        // Client rotates the [25..Length] window (sub_92530C, shift = Length%6+1) and encrypts
        // only the payload with the keyed feedback-XOR (sub_F09D67), keyed by the header time
        // field (off 2) and unk2 field (off 13). Un-rotate the same window, then decrypt the
        // blob to recover the legacy Length/Slot/SubHead action stream.
        private static bool TryDecryptUdp121GameData(PacketModel Packet)
        {
            try
            {
                if (Packet.Data == null || Packet.Data.Length < Packet.Length || Packet.Length < 38)
                    return false;

                int regionLen = Packet.Length - 25;          // cipher blob + 9B trailer (still rotated)
                if (regionLen < 13)
                    return false;

                byte[] region = new byte[regionLen];
                Array.Copy((Array)Packet.Data, 25, (Array)region, 0, regionLen);
                byte[] unrot = Bitwise.Decrypt(region, Packet.Length % 6 + 1);

                int blobLen = regionLen - 9;
                if (!Udp121Cipher.LooksLikeBlob(unrot, 0, blobLen)) // as we update to 122 we maybe need to recheck this on IDA (mcp 13337 has dump 122)
                    return false;

                byte[] key1 = new byte[4];
                byte[] key2 = new byte[4];
                Array.Copy((Array)Packet.Data, 2, (Array)key1, 0, 4);    // header time field
                Array.Copy((Array)Packet.Data, 13, (Array)key2, 0, 4);   // header unk2 field

                byte[] plain = Udp121Cipher.Decrypt(unrot, 0, blobLen, key1, key2); // as we update to 122 we maybe need to recheck this on IDA (mcp 13337 has dump 122)
                if (plain == null)
                    return false;

                byte[] withEnd = new byte[plain.Length + 9];
                Array.Copy((Array)plain, 0, (Array)withEnd, 0, plain.Length);
                Array.Copy((Array)unrot, blobLen, (Array)withEnd, plain.Length, 9);   // raw room trailer

                Packet.WithEndData = withEnd;
                Packet.WithoutEndData = plain;

                // if (ConfigLoader.IsTestMode)
                //     CLogger.Print(Bitwise.ToHexData($"[UDP121 DEC op={Packet.Opcode}] plain", plain), LoggerType.Debug);

                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print($"TryDecryptUdp121GameData error: {ex.Message}", LoggerType.Error, ex);
                return false;
            }
        }

        private static bool IsUdp121Handshake(PacketModel Packet)
        {
            return Packet != null &&
                   (Packet.Opcode == 65 || Packet.Opcode == 67) &&
                   Packet.Length == 38 &&
                   Packet.Data != null &&
                   Packet.Data.Length >= 38 &&
                   ReadUInt32(Packet.Data, 17) == 0x34282E8C &&
                   ReadUInt32(Packet.Data, 21) == 0x2B3C3014 &&
                   ReadUInt32(Packet.Data, 25) == 0x000705E4;
        }

        private static byte[] BuildUdp121HandshakeBody(PacketModel Packet)
        {
            byte[] data = new byte[13];
            WriteInt16(data, 0, -31247);
            WriteInt16(data, 2, 1733);
            WriteUInt32(data, 4, ReadUInt32(Packet.Data, 29));
            WriteUInt32(data, 8, ReadUInt32(Packet.Data, 33));
            data[12] = Packet.Data[37];
            return data;
        }

        private static uint ReadUInt32(byte[] data, int offset)
        {
            return (uint)(data[offset] |
                          data[offset + 1] << 8 |
                          data[offset + 2] << 16 |
                          data[offset + 3] << 24);
        }

        private static void WriteInt16(byte[] data, int offset, short value)
        {
            data[offset] = (byte)value;
            data[offset + 1] = (byte)(value >> 8);
        }

        private static void WriteUInt32(byte[] data, int offset, uint value)
        {
            data[offset] = (byte)value;
            data[offset + 1] = (byte)(value >> 8);
            data[offset + 2] = (byte)(value >> 16);
            data[offset + 3] = (byte)(value >> 24);
        }

        public static void CheckDataFlags(ActionModel Action, PacketModel Packet)
        {
            UdpGameEvent flag = Action.Flag;
            if (!flag.HasFlag((Enum)UdpGameEvent.WeaponSync) || Packet.Opcode == 4 || (flag & (UdpGameEvent.DropWeapon | UdpGameEvent.GetWeaponForClient)) <= (UdpGameEvent)0)
                return;
            Action.Flag -= UdpGameEvent.WeaponSync;
        }

        public static int PingTime(
          string Address,
          byte[] Buffer,
          int TTL,
          int TimeOut,
          bool IsFragmented,
          out int Ping)
        {
            int A_0 = 0;
            try
            {
                PingOptions options = new PingOptions()
                {
                    Ttl = TTL,
                    DontFragment = IsFragmented
                };
                using (Ping ping = new Ping())
                {
                    PingReply pingReply = ping.Send(Address, TimeOut, Buffer, options);
                    if (pingReply.Status == IPStatus.Success)
                        A_0 = Convert.ToInt32(pingReply.RoundtripTime);
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            Ping = (int)AllUtils.StaticMethod0(A_0);
            return A_0;
        }

        private static byte StaticMethod0(int A_0)
        {
            if (A_0 <= 100)
                return 5;
            if (A_0 >= 100 && A_0 <= 200)
                return 4;
            if (A_0 >= 200 && A_0 <= 300)
                return 3;
            if (A_0 >= 300 && A_0 <= 400)
                return 2;
            return A_0 >= 400 && A_0 <= 500 ? (byte)1 : (byte)0;
        }

        public static TeamEnum GetSwappedTeam(PlayerModel Player, RoomModel Room)
        {
            if (Player == null || Room == null)
                return TeamEnum.TEAM_DRAW;
            TeamEnum swappedTeam = Player.Team;
            if (Room.IsTeamSwap)
                swappedTeam = swappedTeam == TeamEnum.FR_TEAM ? TeamEnum.CT_TEAM : TeamEnum.FR_TEAM;
            return swappedTeam;
        }
    }
}
