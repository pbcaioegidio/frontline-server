// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Sync.Server.SendClanInfo
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.XML;
using Server.Game.Data.Models;
using Server.Game.Data.Sync;
using System;
using System.Net;


namespace Server.Game.Data.Sync.Server
{
    public class SendClanInfo
    {
        public static void Load(Account Player, Account Member, int Type)
        {
            try
            {
                if (Player == null)
                    return;
                GameSync sync = GameXender.SyncOf(Player.ServerId);
                if (sync == null)
                    return;
                SChannelModel server = sync.GetServer(Player.Status);
                if (server == null)
                    return;
                IPEndPoint connection = SynchronizeXML.GetServer((int)server.Port).Connection;
                using (SyncServerPacket syncServerPacket = new SyncServerPacket())
                {
                    syncServerPacket.WriteH((short)16 /*0x10*/);
                    syncServerPacket.WriteQ(Player.PlayerId);
                    syncServerPacket.WriteC((byte)Type);
                    switch (Type)
                    {
                        case 1:
                            syncServerPacket.WriteQ(Member.PlayerId);
                            syncServerPacket.WriteC((byte)(Member.Nickname.Length + 1));
                            syncServerPacket.WriteS(Member.Nickname, Member.Nickname.Length + 1);
                            syncServerPacket.WriteB(Member.Status.Buffer);
                            syncServerPacket.WriteC((byte)Member.Rank);
                            break;
                        case 2:
                            syncServerPacket.WriteQ(Member.PlayerId);
                            break;
                        case 3:
                            syncServerPacket.WriteD(Player.ClanId);
                            syncServerPacket.WriteC((byte)Player.ClanAccess);
                            break;
                    }
                    sync.SendPacket(syncServerPacket.ToArray(), connection);
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        public static void Update(ClanModel Clan, int Type, int SourceServerId)
        {
            try
            {
                GameSync sync = GameXender.SyncOf(SourceServerId);
                if (sync == null)
                    return;
                foreach (SChannelModel server in SChannelXML.Servers)
                {
                    if (server.Id != 0 && server.Id != SourceServerId)
                    {
                        IPEndPoint connection = SynchronizeXML.GetServer((int)server.Port).Connection;
                        using (SyncServerPacket syncServerPacket = new SyncServerPacket())
                        {
                            syncServerPacket.WriteH((short)22);
                            syncServerPacket.WriteC((byte)Type);
                            switch (Type)
                            {
                                case 0:
                                    syncServerPacket.WriteQ(Clan.OwnerId);
                                    break;
                                case 1:
                                    syncServerPacket.WriteC((byte)(Clan.Name.Length + 1));
                                    syncServerPacket.WriteS(Clan.Name, Clan.Name.Length + 1);
                                    break;
                                case 2:
                                    syncServerPacket.WriteC((byte)Clan.NameColor);
                                    break;
                            }
                            sync.SendPacket(syncServerPacket.ToArray(), connection);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        public static void Load(ClanModel Clan, int Type, int SourceServerId)
        {
            try
            {
                GameSync sync = GameXender.SyncOf(SourceServerId);
                if (sync == null)
                    return;
                foreach (SChannelModel server in SChannelXML.Servers)
                {
                    if (server.Id != 0 && server.Id != SourceServerId)
                    {
                        IPEndPoint connection = SynchronizeXML.GetServer((int)server.Port).Connection;
                        using (SyncServerPacket syncServerPacket = new SyncServerPacket())
                        {
                            syncServerPacket.WriteH((short)21);
                            syncServerPacket.WriteC((byte)Type);
                            syncServerPacket.WriteD(Clan.Id);
                            if (Type == 0)
                            {
                                syncServerPacket.WriteQ(Clan.OwnerId);
                                syncServerPacket.WriteD(Clan.CreationDate);
                                syncServerPacket.WriteC((byte)(Clan.Name.Length + 1));
                                syncServerPacket.WriteS(Clan.Name, Clan.Name.Length + 1);
                                syncServerPacket.WriteC((byte)(Clan.Info.Length + 1));
                                syncServerPacket.WriteS(Clan.Info, Clan.Info.Length + 1);
                            }
                            sync.SendPacket(syncServerPacket.ToArray(), connection);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}