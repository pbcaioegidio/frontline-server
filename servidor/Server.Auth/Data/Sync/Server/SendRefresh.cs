// Decompiled with JetBrains decompiler
// Type: Server.Auth.Data.Sync.Server.SendRefresh
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.XML;
using Server.Auth.Data.Managers;
using Server.Auth.Data.Models;
using System;
using System.Net;

namespace Server.Auth.Data.Sync.Server
{
    public static class SendRefresh
    {
        public static void RefreshAccount(Account Player, bool IsConnect)
        {
            try
            {
                UpdateServer.RefreshSChannel(0);
                AccountManager.GetFriendlyAccounts(Player.Friend);
                foreach (FriendModel friend in Player.Friend.Friends)
                {
                    PlayerInfo info = friend.Info;
                    if (info != null)
                    {
                        SChannelModel server = SChannelXML.GetServer((int)info.Status.ServerId);
                        if (server != null)
                            SendRefresh.SendRefreshPacket(0, Player.PlayerId, friend.PlayerId, IsConnect, server);
                    }
                }
                if (Player.ClanId <= 0)
                    return;
                foreach (Account clanPlayer in Player.ClanPlayers)
                {
                    if (clanPlayer != null && clanPlayer.IsOnline)
                    {
                        SChannelModel server = SChannelXML.GetServer((int)clanPlayer.Status.ServerId);
                        if (server != null)
                            SendRefresh.SendRefreshPacket(1, Player.PlayerId, clanPlayer.PlayerId, IsConnect, server);
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        public static void SendRefreshPacket(
          int Type,
          long PlayerId,
          long MemberId,
          bool IsConnect,
          SChannelModel Server)
        {
            IPEndPoint connection = SynchronizeXML.GetServer((int)Server.Port).Connection;
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                syncServerPacket.WriteH((short)11);
                syncServerPacket.WriteC((byte)Type);
                syncServerPacket.WriteC(IsConnect ? (byte)1 : (byte)0);
                syncServerPacket.WriteQ(PlayerId);
                syncServerPacket.WriteQ(MemberId);
                AuthXender.Sync.SendPacket(syncServerPacket.ToArray(), connection);
            }
        }
    }
}