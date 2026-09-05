// Decompiled with JetBrains decompiler

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.SharpDX;
using Server.Match.Data.Enums;
using System;
using System.Net;
using System.Runtime.CompilerServices;

namespace Server.Match.Data.Models
{
    public class PlayerModel
    {
        public int Slot = -1;
        public TeamEnum Team;
        public int Life = 100;
        public int MaxLife = 100;
        public int PlayerIdByUser = -2;
        public int PlayerIdByServer = -1;
        public int RespawnByUser = -2;
        public int RespawnByServer = -1;
        public int Ping = 5;
        public int Latency;
        public int WeaponId;
        public byte Accessory;
        public byte Extensions;
        public float PlantDuration;
        public float DefuseDuration;
        public float C4Time;
        public Half3 Position;
        public DateTime LastPositionAt;
        public Half3 LastValidPosition;
        public int SpeedViolations;
        public DateTime LastFireAt;
        public int FireRateViolations;
        public int AmmoViolations;
        public int MagShots;
        public int MagWeaponId;
        public DateTime KillWindowStart;
        public int KillsInWindow;
        public int HeadshotsInWindow;
        public int KillBurstViolations;
        public IPEndPoint Client;
        public DateTime StartTime;
        public DateTime LastPing;
        public DateTime LastDie;
        public DateTime C4First;
        public Equipment Equip;
        public ClassType WeaponClass;
        public CharaResId CharaRes;
        public bool Dead = true;
        public bool NeverRespawn = true;
        public bool Integrity = true;
        public bool Immortal;

        public PlayerModel(int A_1)
        {
            this.Slot = A_1;
            this.Team = (TeamEnum)(A_1 % 2);
        }

        public bool CompareIp(IPEndPoint Address)
        {
            return this.Client != null && Address != null && this.Client.Address.Equals((object)Address.Address) && this.Client.Port == Address.Port;
        }

        public bool RespawnIsValid() => this.RespawnByServer == this.RespawnByUser;

        public bool AccountIdIsValid() => this.PlayerIdByServer == this.PlayerIdByUser;

        public void CheckLifeValue()
        {
            if (this.Life <= this.MaxLife)
                return;
            this.Life = this.MaxLife;
        }

        public void ResetAllInfos()
        {
            this.Client = (IPEndPoint)null;
            this.StartTime = new DateTime();
            this.PlayerIdByUser = -2;
            this.PlayerIdByServer = -1;
            this.Integrity = true;
            this.ResetBattleInfos();
        }

        public void ResetBattleInfos()
        {
            this.RespawnByUser = -2;
            this.RespawnByServer = -1;
            this.Immortal = false;
            this.Dead = true;
            this.NeverRespawn = true;
            this.WeaponId = 0;
            this.Accessory = (byte)0;
            this.Extensions = (byte)0;
            this.WeaponClass = ClassType.Unknown;
            this.LastPing = new DateTime();
            this.LastDie = new DateTime();
            this.C4First = new DateTime();
            this.C4Time = 0.0f;
            this.Position = new Half3();
            this.LastValidPosition = new Half3();
            this.LastPositionAt = new DateTime();
            this.SpeedViolations = 0;
            this.FireRateViolations = 0;
            this.AmmoViolations = 0;
            this.MagShots = 0;
            this.MagWeaponId = 0;
            this.LastFireAt = new DateTime();
            this.KillWindowStart = new DateTime();
            this.KillsInWindow = 0;
            this.HeadshotsInWindow = 0;
            this.KillBurstViolations = 0;
            this.Life = 100;
            this.MaxLife = 100;
            this.Ping = 5;
            this.Latency = 0;
            this.PlantDuration = ConfigLoader.PlantDuration;
            this.DefuseDuration = ConfigLoader.DefuseDuration;
        }

        public void ResetLife() => this.Life = this.MaxLife;

        public void LogPlayerPos(Half3 EndBullet)
        {
            CLogger.Print($"Player Position X: {this.Position.X} Y: {this.Position.Y} Z: {this.Position.Z}", LoggerType.Debug);
            CLogger.Print($"End Bullet Position X: {EndBullet.X} Y: {EndBullet.Y} Z: {EndBullet.Z}", LoggerType.Debug);
        }
    }
}