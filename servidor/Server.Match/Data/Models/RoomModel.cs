// Decompiled with JetBrains decompiler
// Type: Server.Match.Data.Models.RoomModel
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SharpDX;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Match.Data.Managers;
using Server.Match.Data.Utils;
using Server.Match.Data.XML;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;

namespace Server.Match.Data.Models
{
    public class RoomModel
    {
        public PlayerModel[] Players = new PlayerModel[18];
        public ObjectInfo[] Objects = new ObjectInfo[200];
        public long LastStartTick;
        public uint UniqueRoomId;
        public uint RoomSeed;
        public int ObjsSyncRound;
        public int ServerRound;
        public int SourceToMap = -1;
        public int ServerId;
        public int RoomId;
        public int ChannelId;
        public int LastRound;
        public int Bar1 = 6000;
        public int Bar2 = 6000;
        public int Default1 = 6000;
        public int Default2 = 6000;
        public byte DropCounter;
        public bool BotMode;
        public bool HasC4;
        public bool IsTeamSwap;
        public MapIdEnum MapId;
        public MapRules Rule;
        public RoomCondition RoomType;
        public SChannelModel Server;
        public MapModel Map;
        public Half3 BombPosition;
        public DateTime StartTime;
        public DateTime LastObjsSync;
        public DateTime LastPlayersSync;
        private readonly object Field0 = new object();
        private readonly object Field1 = new object();

        public RoomModel(int A_1)
        {
            this.Server = SChannelXML.GetServer(A_1);
            CLogger.Print($"[DBG-ROOMCTOR] serverId={A_1} GetServer={(this.Server == null ? "NULL" : "OK")} loadedServers={SChannelXML.Servers.Count}", LoggerType.Warning);
            if (this.Server == null)
                return;
            this.ServerId = A_1;
            for (int A_1_1 = 0; A_1_1 < 18; ++A_1_1)
                this.Players[A_1_1] = new PlayerModel(A_1_1);
            for (int A_1_2 = 0; A_1_2 < 200; ++A_1_2)
                this.Objects[A_1_2] = new ObjectInfo(A_1_2);
        }

        public void SyncInfo(List<ObjectHitInfo> Objs, int Type)
        {
            lock (this.Field1)
            {
                if (this.BotMode || !this.ObjectsIsValid())
                    return;
                double duration1 = ComDiv.GetDuration(this.LastObjsSync);
                double duration2 = ComDiv.GetDuration(this.LastPlayersSync);
                if (duration1 >= 0.5 && (Type & 1) == 1)
                {
                    this.LastObjsSync = DateTimeUtil.Now();
                    foreach (ObjectInfo objectInfo in this.Objects)
                    {
                        ObjectModel model = objectInfo.Model;
                        if (model != null && (model.Destroyable && objectInfo.Life != model.Life || model.NeedSync))
                        {
                            float duration3 = AllUtils.GetDuration(objectInfo.UseDate);
                            AnimModel animation = objectInfo.Animation;
                            if (animation != null && (double)animation.Duration > 0.0 && (double)duration3 >= (double)animation.Duration)
                                model.GetAnim(animation.NextAnim, duration3, animation.Duration, objectInfo);
                            ObjectHitInfo objectHitInfo = new ObjectHitInfo(model.UpdateId)
                            {
                                ObjSyncId = model.NeedSync ? 1 : 0,
                                AnimId1 = model.Animation,
                                AnimId2 = objectInfo.Animation != null ? objectInfo.Animation.Id : (int)byte.MaxValue,
                                DestroyState = objectInfo.DestroyState,
                                ObjId = model.Id,
                                ObjLife = objectInfo.Life,
                                SpecialUse = duration3
                            };
                            Objs.Add(objectHitInfo);
                        }
                    }
                }
                if (duration2 < 0.5 || (Type & 2) != 2)
                    return;
                this.LastPlayersSync = DateTimeUtil.Now();
                foreach (PlayerModel player in this.Players)
                {
                    if (!player.Immortal && (player.MaxLife != player.Life || player.Dead))
                    {
                        ObjectHitInfo objectHitInfo = new ObjectHitInfo(4)
                        {
                            ObjId = player.Slot,
                            ObjLife = player.Life
                        };
                        Objs.Add(objectHitInfo);
                    }
                }
            }
        }

        public bool ObjectsIsValid() => this.ServerRound == this.ObjsSyncRound;

        public void ResyncTick(long StartTick, uint Seed)
        {
            if (StartTick <= this.LastStartTick)
                return;
            this.StartTime = new DateTime(StartTick);
            if (this.LastStartTick > 0L)
                this.ResetRoomInfo(Seed);
            this.LastStartTick = StartTick;
            if (!ConfigLoader.IsTestMode)
                return;
            CLogger.Print($"New tick is defined. [{this.LastStartTick}]", LoggerType.Warning);
        }

        public void ResetRoomInfo(uint Seed)
        {
            for (int A_1 = 0; A_1 < 200; ++A_1)
                this.Objects[A_1] = new ObjectInfo(A_1);
            this.MapId = (MapIdEnum)AllUtils.GetSeedInfo(Seed, 2);
            this.RoomType = (RoomCondition)AllUtils.GetSeedInfo(Seed, 0);
            this.Rule = (MapRules)AllUtils.GetSeedInfo(Seed, 1);
            this.SourceToMap = -1;
            this.Map = (MapModel)null;
            this.LastRound = 0;
            this.DropCounter = (byte)0;
            this.BotMode = false;
            this.HasC4 = false;
            this.IsTeamSwap = false;
            this.ServerRound = 0;
            this.ObjsSyncRound = 0;
            this.LastObjsSync = new DateTime();
            this.LastPlayersSync = new DateTime();
            this.BombPosition = new Half3();
            if (!ConfigLoader.IsTestMode)
                return;
            CLogger.Print("A room has been reseted by server.", LoggerType.Warning);
        }

        public bool RoundResetRoomF1(int Round)
        {
            lock (this.Field0)
            {
                if (this.LastRound != Round)
                {
                    if (ConfigLoader.IsTestMode)
                        CLogger.Print($"Reseting room. [Last: {this.LastRound}; New: {Round}]", LoggerType.Warning);
                    DateTime dateTime = DateTimeUtil.Now();
                    this.LastRound = Round;
                    this.HasC4 = false;
                    this.BombPosition = new Half3();
                    this.DropCounter = (byte)0;
                    this.ObjsSyncRound = 0;
                    this.SourceToMap = (int)this.MapId;
                    if (!this.BotMode)
                    {
                        for (int index = 0; index < 18; ++index)
                        {
                            PlayerModel player = this.Players[index];
                            player.Life = player.MaxLife;
                        }
                        this.LastPlayersSync = dateTime;
                        this.Map = MapStructureXML.GetMapId((int)this.MapId);
                        List<ObjectModel> objects = this.Map?.Objects;
                        if (objects != null)
                        {
                            foreach (ObjectModel objectModel in objects)
                            {
                                if (objectModel.Id < 0 || objectModel.Id >= this.Objects.Length)
                                {
                                    CLogger.Print($"RoundResetRoomF1: object Id {objectModel.Id} out of range [0,{this.Objects.Length}) on Map {this.MapId}; skipping.", LoggerType.Error);
                                    continue;
                                }
                                ObjectInfo objectInfo = this.Objects[objectModel.Id];
                                objectInfo.Life = objectModel.Life;
                                if (objectModel.NoInstaSync)
                                {
                                    objectInfo.Animation = new AnimModel()
                                    {
                                        NextAnim = 1
                                    };
                                    objectInfo.UseDate = dateTime;
                                }
                                else
                                    objectModel.GetRandomAnimation(this, objectInfo);
                                objectInfo.Model = objectModel;
                                objectInfo.DestroyState = 0;
                                MapStructureXML.SetObjectives(objectModel, this);
                            }
                        }
                        this.LastObjsSync = dateTime;
                        this.ObjsSyncRound = Round;
                    }
                    return true;
                }
            }
            return false;
        }

        public bool RoundResetRoomS1(int Round)
        {
            lock (this.Field0)
            {
                if (this.LastRound != Round)
                {
                    if (ConfigLoader.IsTestMode)
                        CLogger.Print($"Reseting room. [Last: {this.LastRound}; New: {Round}]", LoggerType.Warning);
                    this.LastRound = Round;
                    this.HasC4 = false;
                    this.DropCounter = (byte)0;
                    this.BombPosition = new Half3();
                    if (!this.BotMode)
                    {
                        for (int index = 0; index < 18; ++index)
                        {
                            PlayerModel player = this.Players[index];
                            player.Life = player.MaxLife;
                        }
                        DateTime dateTime = DateTimeUtil.Now();
                        this.LastPlayersSync = dateTime;
                        foreach (ObjectInfo objectInfo in this.Objects)
                        {
                            ObjectModel model = objectInfo.Model;
                            if (model != null)
                            {
                                objectInfo.Life = model.Life;
                                if (!model.NoInstaSync)
                                {
                                    model.GetRandomAnimation(this, objectInfo);
                                }
                                else
                                {
                                    objectInfo.Animation = new AnimModel()
                                    {
                                        NextAnim = 1
                                    };
                                    objectInfo.UseDate = dateTime;
                                }
                                objectInfo.DestroyState = 0;
                            }
                        }
                        this.LastObjsSync = dateTime;
                        this.ObjsSyncRound = Round;
                        if (this.RoomType == RoomCondition.Destroy || this.RoomType == RoomCondition.Defense)
                        {
                            this.Bar1 = this.Default1;
                            this.Bar2 = this.Default2;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public PlayerModel AddPlayer(IPEndPoint Client, PacketModel Packet, string Udp)
        {
            if (ConfigLoader.UdpVersion != Udp)
            {
                if (ConfigLoader.IsTestMode)
                    CLogger.Print($"Wrong UDP Version ({Udp}); Player can't be connected into it!", LoggerType.Warning);
                return (PlayerModel)null;
            }
            try
            {
                PlayerModel player = this.Players[Packet.Slot];
                if (!player.CompareIp(Client))
                {
                    player.Client = Client;
                    player.StartTime = Packet.ReceiveDate;
                    player.PlayerIdByUser = Packet.AccountId;
                    return player;
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return (PlayerModel)null;
        }

        public bool GetPlayer(int Slot, out PlayerModel Player)
        {
            try
            {
                Player = this.Players[Slot];
            }
            catch
            {
                Player = (PlayerModel)null;
            }
            return Player != null;
        }

        public PlayerModel GetPlayer(int Slot, bool Active)
        {
            PlayerModel playerModel;
            try
            {
                playerModel = this.Players[Slot];
            }
            catch
            {
                playerModel = (PlayerModel)null;
            }
            return playerModel != null && (!Active || playerModel.Client != null) ? playerModel : (PlayerModel)null;
        }

        public PlayerModel GetPlayer(int Slot, IPEndPoint Client)
        {
            PlayerModel playerModel;
            try
            {
                playerModel = this.Players[Slot];
            }
            catch
            {
                playerModel = (PlayerModel)null;
            }
            return playerModel != null && playerModel.CompareIp(Client) ? playerModel : (PlayerModel)null;
        }

        public ObjectInfo GetObject(int Id)
        {
            try
            {
                return this.Objects[Id];
            }
            catch
            {
                return (ObjectInfo)null;
            }
        }

        public bool RemovePlayer(IPEndPoint Client, PacketModel Packet, string Udp)
        {
            if (ConfigLoader.UdpVersion != Udp)
            {
                if (ConfigLoader.IsTestMode)
                    CLogger.Print($"Wrong UDP Version ({Udp}); Player can't be disconnected on it!", LoggerType.Warning);
                return false;
            }
            try
            {
                PlayerModel player = this.Players[Packet.Slot];
                if (player.CompareIp(Client))
                {
                    player.ResetAllInfos();
                    return true;
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return false;
        }

        public int GetPlayersCount()
        {
            int playersCount = 0;
            for (int index = 0; index < 18; ++index)
            {
                if (this.Players[index].Client != null)
                    ++playersCount;
            }
            return playersCount;
        }
    }
}