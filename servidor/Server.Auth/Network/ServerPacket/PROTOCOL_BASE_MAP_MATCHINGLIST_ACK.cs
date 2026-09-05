// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ServerPacket.PROTOCOL_BASE_MAP_MATCHINGLIST_ACK
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using Plugin.Core.XML;
using System.Collections.Generic;


namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_BASE_MAP_MATCHINGLIST_ACK : AuthServerPacket
    {
        private readonly List<MapMatch> Field0;
        private readonly int Field1;

        public PROTOCOL_BASE_MAP_MATCHINGLIST_ACK(List<MapMatch> A_1, int A_2)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
        }

        public override void Write()
        {
            this.WriteH((short)2464);
            this.WriteH((short)0);
            this.WriteC((byte)this.Field0.Count);
            foreach (MapMatch mapMatch in this.Field0)
            {
                this.WriteD(mapMatch.Mode);
                this.WriteC((byte)mapMatch.Id);
                this.WriteC((byte)SystemMapXML.GetMapRule(mapMatch.Mode).Rule);
                this.WriteC((byte)SystemMapXML.GetMapRule(mapMatch.Mode).StageOptions);
                this.WriteC((byte)SystemMapXML.GetMapRule(mapMatch.Mode).Conditions);
                this.WriteC((byte)mapMatch.Limit);
                this.WriteC((byte)mapMatch.Tag);
                this.WriteC(mapMatch.Tag == 2 || mapMatch.Tag == 3 ? (byte)1 : (byte)0);
                this.WriteC((byte)0);
            }
            this.WriteD(this.Field0.Count != 100 ? 1 : 0);
            this.WriteH((ushort)(this.Field1 - 100));
            this.WriteH((ushort)SystemMapXML.Matches.Count);
        }
    }
}
