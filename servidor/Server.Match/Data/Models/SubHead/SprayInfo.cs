using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Match.Data.Models.SubHead
{
    public class SprayInfo
    {
        public byte WeaponFlag;
        public ushort X, Y, Z, Unk1, Unk2, Unk3, Unk4, Unk5, Unk6;
        public byte[] Unknown;
    }
}