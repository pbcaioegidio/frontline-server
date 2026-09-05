// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.PlayerEquipment
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

namespace Plugin.Core.Models
{
    public class PlayerEquipment
    {
        public long OwnerId { get; set; }

        public int WeaponPrimary { get; set; }

        public int WeaponSecondary { get; set; }

        public int WeaponMelee { get; set; }

        public int WeaponExplosive { get; set; }

        public int WeaponSpecial { get; set; }

        public int CharaRedId { get; set; }

        public int CharaBlueId { get; set; }

        public int PartHead { get; set; }

        public int PartFace { get; set; }

        public int PartJacket { get; set; }

        public int PartPocket { get; set; }

        public int PartGlove { get; set; }

        public int PartBelt { get; set; }

        public int PartHolster { get; set; }

        public int PartSkin { get; set; }

        public int BeretItem { get; set; }

        public int DinoItem { get; set; }

        public int AccessoryId { get; set; }

        public int SprayId { get; set; }

        public int NameCardId { get; set; }

        // 6 equipped emoticon ids (the emote-wheel loadout). These map to the client
        // loadout block at +164 (12 dwords = 6 pairs), read by the emoticon panel and the
        // battle emote wheel (CHAR_EQUIPMENT_EMOTION_COUNT=6, dump121 EmotionUtil::
        // BringUsedEmotionItemID @0xD0ADB2 / CharaEmotion::GetItemInfo). Index 0..5 <->
        // DB columns emoticon_0..emoticon_5. 0 = empty slot.
        public int[] Emoticons { get; set; }

        public PlayerEquipment()
        {
            this.Emoticons = new int[6];
            this.WeaponPrimary = 103004;
            this.WeaponSecondary = 202022;
            this.WeaponMelee = 301012;
            this.WeaponExplosive = 407056;
            this.WeaponSpecial = 508002;
            this.CharaRedId = 601666;
            this.CharaBlueId = 602002;
            this.PartHead = 0;
            this.PartFace = 0;
            this.PartJacket = 0;
            this.PartPocket = 0;
            this.PartGlove = 0;
            this.PartBelt = 0;
            this.PartHolster = 0;
            this.PartSkin = 0;
            this.DinoItem = 1500511;
        }
    }
}