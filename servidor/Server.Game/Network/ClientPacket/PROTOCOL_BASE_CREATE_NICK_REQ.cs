// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BASE_CREATE_NICK_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Logging;
using Plugin.Core.Filters;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_CREATE_NICK_REQ : GameClientPacket
    {
        private string Field0;

        public override void Read() => Field0 = ReadU((int)ReadC() * 2);

        public override void Run()
        {
            try
            {
                Account player = Client.GetAccount();
                if (player == null)
                    return;
                CLogger.Event(LogCat.Nick, new { reqNick = Field0, existingNick = player.Nickname, existingLen = (player.Nickname == null ? -2 : player.Nickname.Length), pid = player.PlayerId });
                if (player.Nickname.Length == 0 && !string.IsNullOrEmpty(Field0) && Field0.Length >= ConfigLoader.MinNickSize && Field0.Length <= ConfigLoader.MaxNickSize)
                {
                    foreach (string filter in NickFilter.Filters)
                    {
                        if (Field0.Contains(filter))
                        {
                            Client.SendPacket(new PROTOCOL_BASE_CREATE_NICK_ACK(0x80001013, null));
                            break;
                        }
                    }
                    if (!DaoManagerSQL.IsPlayerNameExist(Field0))
                    {
                        if (AccountManager.UpdatePlayerName(Field0, player.PlayerId))
                        {
                            DaoManagerSQL.CreatePlayerNickHistory(player.PlayerId, player.Nickname, Field0, "First nick choosed");
                            player.Nickname = Field0;
                            List<ItemsModel> basics = TemplatePackXML.Basics;
                            if (basics.Count > 0)
                            {
                                foreach (ItemsModel itemsModel in basics)
                                {
                                    if (ComDiv.GetIdStatics(itemsModel.Id, 1) == 6 && player.Character.GetCharacter(itemsModel.Id) == null)
                                        AllUtils.CreateCharacter(player, itemsModel);
                                }
                            }
                            List<ItemsModel> awards = TemplatePackXML.Awards;
                            if (awards.Count > 0)
                            {
                                foreach (ItemsModel A_3 in awards)
                                {
                                    if (ComDiv.GetIdStatics(A_3.Id, 1) == 6 && player.Character.GetCharacter(A_3.Id) == null)
                                        AllUtils.CreateCharacter(player, A_3);
                                    else
                                        Client.SendPacket(new PROTOCOL_INVENTORY_GET_INFO_ACK(0, player, A_3));
                                }
                            }
                            Client.SendPacket(new PROTOCOL_BASE_CREATE_NICK_ACK(0U, player));
                            Client.SendPacket(new PROTOCOL_SHOP_PLUS_POINT_ACK(player.Gold, player.Gold, 4));
                            Client.SendPacket(new PROTOCOL_BASE_QUEST_GET_INFO_ACK(player));
                            Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INFO_ACK(player.Friend.Friends));
                        }
                        else
                            Client.SendPacket(new PROTOCOL_BASE_CREATE_NICK_ACK(0x80001013, null));
                    }
                    else
                        Client.SendPacket(new PROTOCOL_BASE_CREATE_NICK_ACK(0x80000113, null));
                }
                else
                    Client.SendPacket(new PROTOCOL_BASE_CREATE_NICK_ACK(0x80000113, null));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_LOBBY_CREATE_NICK_NAME_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}