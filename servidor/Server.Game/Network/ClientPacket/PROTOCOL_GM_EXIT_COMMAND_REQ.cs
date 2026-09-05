// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_GM_EXIT_COMMAND_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_GM_EXIT_COMMAND_REQ : GameClientPacket
    {
        private byte Field0;

        public override void Read() => this.Field0 = this.ReadC();

        public override void Run()
        {
        }
    }
}