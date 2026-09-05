// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_SEASON_CHALLENGE_SEASON_CHANGE
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_SEASON_CHALLENGE_SEASON_CHANGE : GameServerPacket
    {
        public override void Write()
        {
            this.WriteH((short)8451);
            this.WriteH((short)0);
            this.WriteC((byte)1);
        }
    }
}