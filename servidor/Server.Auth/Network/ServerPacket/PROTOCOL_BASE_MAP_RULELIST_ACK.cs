// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ServerPacket.PROTOCOL_BASE_MAP_RULELIST_ACK
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core.Models.Map;
using Plugin.Core.XML;


namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_BASE_MAP_RULELIST_ACK : AuthServerPacket
    {
        public override void Write()
        {
            this.WriteH((short)2462);
            this.WriteH((short)0);
            this.WriteH((ushort)SystemMapXML.Rules.Count);
            foreach (MapRule rule in SystemMapXML.Rules)
            {
                this.WriteD(rule.Id);
                this.WriteC((byte)0);
                this.WriteC((byte)rule.Rule);
                this.WriteC((byte)rule.StageOptions);
                this.WriteC((byte)rule.Conditions);
                this.WriteC((byte)0);
                this.WriteS(rule.Name, 67);
            }
        }
    }
}