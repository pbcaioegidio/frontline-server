using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Utility;
using Server.Game;
using Server.Game.Network;
using System;
using System.Collections.Generic;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_CONNECT_ACK : GameServerPacket
    {
        private readonly ushort SessionId;
        private readonly ushort SessionSeed;
        private readonly List<byte[]> RSAKey;
        public PROTOCOL_BASE_CONNECT_ACK(GameClient client)
        {
            SessionId = (ushort)client.SessionId;
            SessionSeed = client.SessionSeed;
            RSAKey = Bitwise.GenerateRSAKeyPair(SessionId, SECURITY_KEY, SEED_LENGTH);

            Buffer.BlockCopy(RSAKey[0], 0, client.ServerKey, 0, 16);
            Buffer.BlockCopy(RSAKey[0], 16, client.ClientKey, 0, 16);
        }

        public override void Write()
        {
            WriteH(2306);
            WriteH(0);
            WriteC(11);
            WriteB(new byte[11]);
            WriteH((ushort)(RSAKey[0].Length + RSAKey[1].Length + 2));
            WriteH((ushort)RSAKey[0].Length);
            WriteB(RSAKey[0]);
            WriteB(RSAKey[1]);
            WriteC(118);
            WriteH(0);
            WriteH(SessionSeed);
            WriteD(SessionId);
        }
    }
}