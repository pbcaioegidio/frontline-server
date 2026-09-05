//using Plugin.Core.Models;
//using System.Collections.Generic;

//namespace Server.Auth.Network.ServerPacket
//{
//    public class PROTOCOL_BASE_GET_INVEN_INFO_ACK : AuthServerPacket
//    {
//        private readonly uint Field0;
//        private readonly int Field1;
//        private readonly List<ItemsModel> Field2;
//        private readonly int TotalItemCount;
//        private readonly int Offset;
//        public PROTOCOL_BASE_GET_INVEN_INFO_ACK(uint A_1, List<ItemsModel> A_2, int A_3, int totalCount = -1, int offset = 0)
//        {
//            this.Field0 = A_1;
//            this.Field2 = A_2;
//            this.Field1 = A_3;
//            this.TotalItemCount = totalCount >= 0 ? totalCount : A_2.Count;
//            this.Offset = offset;
//        }

//        public override void Write()
//        {
//            this.WriteH(2319);
//            this.WriteH(0);
//            this.WriteD(this.Field0);
//            if (this.Field0 != 0U)
//                return;

//            this.WriteH((ushort)this.Field2.Count); // Items in this specific packet
//            foreach (ItemsModel itemsModel in this.Field2)
//            {
//                this.WriteD((uint)itemsModel.ObjectId);
//                this.WriteD(itemsModel.Id);
//                this.WriteC((byte)itemsModel.Equip);
//                this.WriteD(itemsModel.Count);
//            }

//            this.WriteH((ushort)this.Field1); // Basics
//            this.WriteH((ushort)this.TotalItemCount); // Total Items
//            this.WriteH((ushort)this.Offset); // Offset
//            this.WriteH((ushort)this.TotalItemCount); // Total Items
//            this.WriteH(0);
//        }
//    }
//}

// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ServerPacket.PROTOCOL_BASE_GET_INVEN_INFO_ACK
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using System.Collections.Generic;


namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_BASE_GET_INVEN_INFO_ACK : AuthServerPacket
    {
        private readonly uint Field0;
        private readonly int Field1;
        private readonly List<ItemsModel> Field2;

        public PROTOCOL_BASE_GET_INVEN_INFO_ACK(uint A_1, List<ItemsModel> A_2, int A_3)
        {
            Field0 = A_1;
            Field2 = A_2;
            Field1 = A_3;
        }

        public override void Write()
        {
            WriteH((short)2319);
            WriteH((short)0);
            WriteD(Field0);
            if (Field0 != 0U)
                return;
            WriteH((ushort)Field2.Count);
            foreach (ItemsModel itemsModel in Field2)
            {
                WriteD((uint)itemsModel.ObjectId);
                WriteD(itemsModel.Id);
                WriteC((byte)itemsModel.Equip);
                WriteD(itemsModel.Count);
            }
            WriteH((ushort)Field1);
            WriteH((ushort)Field2.Count);
            WriteH((ushort)Field2.Count);
            WriteH((ushort)Field2.Count);
            WriteH((short)0);
        }
    }
}