// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_SHOP_GET_SAILLIST_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Utility;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_SHOP_GET_SAILLIST_ACK : GameServerPacket
    {
        private readonly bool Field0;

        public PROTOCOL_SHOP_GET_SAILLIST_ACK(bool A_1) => this.Field0 = A_1;

        
        public override void Write()
        {
            this.WriteH((short)1030);
            this.WriteC(this.Field0 ? (byte)1 : (byte)0);
            // Client (122) handler sub_ED69DC reads [u8 flag][u32 date YYMMDDHHMM] and stores it as
            // the server clock (mgr dword_15E8E20). Flash sale et al derive minute-of-day from this;
            // a hardcoded 2359 pins it at the daily-expiry boundary and kills the card. Live sends the
            // real current time -- match it.
            this.WriteD(uint.Parse(System.DateTime.Now.ToString("yyMMddHHmm")));
        }
    }
}