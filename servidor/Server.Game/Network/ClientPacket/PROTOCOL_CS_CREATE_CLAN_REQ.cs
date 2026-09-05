// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CS_CREATE_CLAN_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Data.Sync.Server;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CS_CREATE_CLAN_REQ : GameClientPacket
    {
        private uint Field0;
        private string Field1;
        private string Field2;

        public override void Read()
        {
            this.ReadD();
            this.Field1 = this.ReadU(34);
            this.Field2 = this.ReadU(510);
            this.ReadD();
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                ClanModel clanModel = new ClanModel()
                {
                    Name = this.Field1,
                    Info = this.Field2,
                    Logo = 0,
                    OwnerId = player.PlayerId,
                    CreationDate = uint.Parse(DateTimeUtil.Now("yyyyMMdd"))
                };
                if (player.ClanId <= 0 && DaoManagerSQL.GetRequestClanId(player.PlayerId) <= 0)
                {
                    if (0 <= player.Gold - ConfigLoader.MinCreateGold && ConfigLoader.MinCreateRank <= player.Rank)
                    {
                        if (ClanManager.IsClanNameExist(clanModel.Name))
                        {
                            this.Field0 = 2147487834U;
                            return;
                        }
                        if (ClanManager.Clans.Count <= ConfigLoader.MaxActiveClans)
                        {
                            if (DaoManagerSQL.CreateClan(out clanModel.Id, clanModel.Name, clanModel.OwnerId, clanModel.Info, clanModel.CreationDate) && DaoManagerSQL.UpdateAccountGold(player.PlayerId, player.Gold - ConfigLoader.MinCreateGold))
                            {
                                clanModel.BestPlayers.SetDefault();
                                player.ClanDate = clanModel.CreationDate;
                                if (ComDiv.UpdateDB("accounts", "player_id", (object)player.PlayerId, new string[3]
                                {
                "clan_access",
                "clan_date",
                "clan_id"
                                }, (object)1, (object)(long)clanModel.CreationDate, (object)clanModel.Id))
                                {
                                    if (clanModel.Id <= 0)
                                    {
                                        this.Field0 = 2147487819U;
                                    }
                                    else
                                    {
                                        player.ClanId = clanModel.Id;
                                        player.ClanAccess = 1;
                                        ClanManager.AddClan(clanModel);
                                        SendClanInfo.Load(clanModel, 0, player.ServerId);
                                        player.Gold -= ConfigLoader.MinCreateGold;
                                    }
                                }
                                else
                                    this.Field0 = 2147487816U;
                            }
                            else
                                this.Field0 = 2147487816U;
                        }
                        else
                            this.Field0 = 2147487829U;
                    }
                    else
                        this.Field0 = 2147487818U;
                }
                else
                    this.Field0 = 2147487836U;
                this.Client.SendPacket(new PROTOCOL_CS_CREATE_CLAN_ACK(this.Field0, clanModel, player));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CS_CREATE_CLAN_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}