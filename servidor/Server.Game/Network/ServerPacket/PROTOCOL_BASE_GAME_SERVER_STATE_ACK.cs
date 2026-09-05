// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BASE_GAME_SERVER_STATE_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Game.Data.Models;
using Server.Game.Data.XML;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_GAME_SERVER_STATE_ACK : GameServerPacket
    {
        public override void Write()
        {
            this.WriteH((short)2400);
            this.WriteH((short)0);
            this.WriteB(this.Method0(SChannelXML.Servers));
            this.WriteC((byte)0);
        }

        
        private byte[] Method0(List<SChannelModel> A_1)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                syncServerPacket.WriteC((byte)A_1.Count);
                foreach (SChannelModel schannelModel in A_1)
                {
                    syncServerPacket.WriteD(schannelModel.State ? 1 : 0);
                    syncServerPacket.WriteB(ComDiv.AddressBytes(schannelModel.Host));
                    syncServerPacket.WriteB(ComDiv.AddressBytes(schannelModel.Host));
                    syncServerPacket.WriteH(schannelModel.Port);
                    syncServerPacket.WriteC((byte)schannelModel.Type);
                    syncServerPacket.WriteH((ushort)schannelModel.MaxPlayers);
                    syncServerPacket.WriteD(schannelModel.LastPlayers);
                    if (schannelModel.Id == 0)
                    {
                        syncServerPacket.WriteB(Bitwise.HexStringToByteArray("01 01 01 01 01 01 01 01 01 01 0E 00 00 00 00"));
                    }
                    else
                    {
                        foreach (ChannelModel channel in ChannelsXML.GetChannels(schannelModel.Id))
                            syncServerPacket.WriteC((byte)channel.Type);
                        syncServerPacket.WriteC((byte)schannelModel.Type);
                        syncServerPacket.WriteC((byte)0);
                        syncServerPacket.WriteH((short)0);
                    }
                }
                return syncServerPacket.ToArray();
            }
        }
    }
}