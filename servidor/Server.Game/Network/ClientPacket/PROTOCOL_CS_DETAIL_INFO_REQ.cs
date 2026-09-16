// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CS_DETAIL_INFO_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CS_DETAIL_INFO_REQ : GameClientPacket
    {
        private int Field0;
        private int Field1;

        public override void Read()
        {
            this.Field0 = this.ReadD();
            this.Field1 = (int)this.ReadC();
        }


        public override void Run()
        {
            try
            {
                Account player = this.Client.Player;
                if (player == null)
                    return;
                player.FindClanId = this.Field0;
                ClanModel clan = ClanManager.GetClan(player.FindClanId);
                // O client dispara este REQ tambem ao abrir a loja; se o blob de resposta
                // estiver errado o sintoma e Please Wait/crash na loja, nao na aba de cla.
                // Log para separar os dois casos ao investigar.
                CLogger.Print($"CS_DETAIL_INFO clanId={this.Field0} echo={this.Field1} found={(clan != null && clan.Id > 0)}", LoggerType.Info);
                // Sempre ACK: se clan invalido, manda blob vazio (Clan=null).
                // Antes retornava sem ACK → Please Wait no client.
                if (clan == null || clan.Id <= 0)
                    this.Client.SendPacket(new PROTOCOL_CS_DETAIL_INFO_ACK(this.Field1, null));
                else
                    this.Client.SendPacket(new PROTOCOL_CS_DETAIL_INFO_ACK(this.Field1, clan));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CS_DETAIL_INFO_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}