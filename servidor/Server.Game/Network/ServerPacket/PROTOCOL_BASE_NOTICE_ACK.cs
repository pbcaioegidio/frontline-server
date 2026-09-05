// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BASE_NOTICE_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Models;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_NOTICE_ACK : GameServerPacket
    {
        private readonly ServerConfig Field0;
        private readonly string Field1;
        private readonly string Field2;

        
        public PROTOCOL_BASE_NOTICE_ACK(string A_1)
        {
            this.Field0 = GameXender.Config;
            if (this.Field0 == null)
                return;
            this.Field1 = Translation.GetLabel(this.Field0.ChannelAnnounce);
            this.Field2 = $"{Translation.GetLabel(this.Field0.ChatAnnounce)} \n\r{A_1}";
        }

        
        public override void Write()
        {
            this.WriteH((short)2455);
            this.WriteH((short)0);
            this.WriteD(this.Field0.ChatAnnounceColor);
            this.WriteD(this.Field0.ChannelAnnounceColor);
            this.WriteH((short)0);
            this.WriteH((ushort)this.Field2.Length);
            this.WriteN(this.Field2, this.Field2.Length, "UTF-16LE");
            this.WriteH((ushort)this.Field1.Length);
            this.WriteN(this.Field1, this.Field1.Length, "UTF-16LE");
        }
    }
}