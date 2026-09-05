// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_GM_KICK_COMMAND_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_GM_KICK_COMMAND_REQ : GameClientPacket
    {
        private byte Field0;

        public override void Read() => this.Field0 = this.ReadC();

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                RoomModel room = player.Room;
                if (room == null)
                    return;
                Account playerBySlot = room.GetPlayerBySlot((int)this.Field0);
                if (playerBySlot == null || playerBySlot.IsGM())
                    return;
                room.RemovePlayer(playerBySlot, true);
            }
            catch (Exception ex)
            {
                CLogger.Print($"{this.GetType().Name}: {ex.Message}", LoggerType.Error, ex);
            }
        }
    }
}