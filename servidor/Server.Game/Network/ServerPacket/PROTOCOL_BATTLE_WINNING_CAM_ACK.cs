using Plugin.Core.Models;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BATTLE_WINNING_CAM_ACK : GameServerPacket
    {
        private readonly FragInfos fragInfos;
        private readonly SlotModel slotModel;

        public PROTOCOL_BATTLE_WINNING_CAM_ACK(FragInfos fragInfos, SlotModel slotModel)
        {
            this.fragInfos = fragInfos;
            this.slotModel = slotModel;
        }

        public override void Write()
        {
            WriteH((short)5279);

            if (fragInfos == null)
            {
                WriteC(0); // KillingType default (0)
                WriteC(0); // KillsCount default (0)
                WriteC(0); // KillerSlot default (0)
                WriteD(0); // WeaponId default (0)
                WriteT(0.0f); // X default (0)
                WriteT(0.0f); // Y default (0)
                WriteT(0.0f); // Z default (0)
                WriteC(0); // Flag default (0)
                WriteC(0); // Unk default (0)
                WriteC(1);
                WriteC(1);
                WriteB(new byte[17]);

                return;
            }

            WriteC((byte)fragInfos.KillingType);
            WriteC(fragInfos.KillsCount);
            WriteC(fragInfos.KillerSlot);
            WriteD(fragInfos.WeaponId);
            WriteT(fragInfos.X);
            WriteT(fragInfos.Y);
            WriteT(fragInfos.Z);
            WriteC(fragInfos.Flag);
            WriteC(fragInfos.Unk);
            WriteC(1);
            WriteC(1);
            WriteB(new byte[17]);
        }
    }
}