// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_MATCH_SERVER_IDX_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_MATCH_SERVER_IDX_REQ : GameClientPacket
    {
        private short Field0;

        public override void Read()
        {
            this.Field0 = this.ReadH();
            int num = (int)this.ReadC();
        }

        
        public override void Run()
        {
            try
            {
                this.Client.SendPacket(new PROTOCOL_MATCH_SERVER_IDX_ACK(this.Field0));
                this.Client.Close(0, false);
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_MATCH_SERVER_IDX_REQ: " + ex.Message, LoggerType.Warning);
            }
        }
    }
}