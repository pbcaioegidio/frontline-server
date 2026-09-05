// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_SERVER_MESSAGE_DISCONNECTED_HACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Utility;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_SERVER_MESSAGE_DISCONNECTED_HACK : GameServerPacket
    {
        private readonly uint Field0;

        public PROTOCOL_SERVER_MESSAGE_DISCONNECTED_HACK(uint A_1) => this.Field0 = A_1;

        
        public override void Write()
        {
            this.WriteH((short)3074);
            this.WriteD(this.Field0);
        }
    }
}
