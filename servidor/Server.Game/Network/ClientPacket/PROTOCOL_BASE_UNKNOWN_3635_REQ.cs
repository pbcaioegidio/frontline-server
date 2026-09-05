using Plugin.Core;
using Plugin.Core.Enums;
using System;
using System.Runtime.CompilerServices;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_UNKNOWN_3635_REQ : GameClientPacket
    {
        private byte Field0;
        private byte Field1;
        private byte Field2;
        private string Field3;
        private short Field4;

        public override void Read()
        {
            this.Field0 = this.ReadC();
            this.Field3 = this.ReadU(66);
            this.ReadD();
            int num1 = (int)this.ReadH();
            this.Field1 = this.ReadC();
            int num2 = (int)this.ReadH();
            this.ReadB(16 /*0x10*/);
            this.ReadB(12);
            this.Field4 = this.ReadH();
            this.Field2 = this.ReadC();
        }

        public override void Run()
        {
            try
            {
                CLogger.Print($"{this.GetType().Name}; Unk1: {this.Field0}; Nickname: {this.Field3}; Unk2: {this.Field1}; Unk3: {this.Field4}; Unk4: {this.Field2}", LoggerType.Warning);
            }
            catch (Exception ex)
            {
                CLogger.Print($"{this.GetType().Name} Error: {ex.Message}", LoggerType.Error);
            }
        }
    }
}