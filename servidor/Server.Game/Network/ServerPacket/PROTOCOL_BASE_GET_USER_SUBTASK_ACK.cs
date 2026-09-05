// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BASE_GET_USER_SUBTASK_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_GET_USER_SUBTASK_ACK : GameServerPacket
    {
        private readonly Account Field0;
        private readonly int Field1;

        public PROTOCOL_BASE_GET_USER_SUBTASK_ACK(PlayerSession A_1)
        {
            this.Field0 = AccountManager.GetAccount(A_1.PlayerId, true);
            this.Field1 = A_1.SessionId;
        }

        public override void Write()
        {
            this.WriteH((short)2448);
            this.WriteH((short)0);
            this.WriteD(0);
            this.WriteC((byte)0);
            this.WriteD(this.Field1);
        }
    }
}