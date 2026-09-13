namespace Plugin.Core.Enums
{
    // Lobby "function" effect ids (class 16). The active-effect id is derived from a used
    // item as 1600000 + (itemId % 1000). These are the effects handled inline in the lobby
    // by PROTOCOL_INVENTORY_USE_ITEM_REQ.Method0. Everything else (buffs, point coupons,
    // battle pass, weapon activation, capsules) is routed to PROTOCOL_AUTH_SHOP_ITEM_AUTH_REQ.
    public static class EffectId
    {
        public const int ClanNameColor = 1600005;
        public const int NameColor = 1600006;
        public const int FakeRank = 1600009;
        public const int FakeNick = 1600010;
        public const int CrosshairColor = 1600014;
        public const int ChangeNick = 1600047;
        public const int ClanName = 1600051;
        public const int ClanLogo = 1600052;
        public const int RoomUserItem = 1600085;
        public const int MuzzleColor = 1600187;
        public const int ClanEffect = 1600193;
        public const int NickBorderColor = 1600205;

        // ---------------------------------------------------------------------------
        // Battle ability "function effect" item ids (class 16), registered by the 121
        // client FX_RegisterAbilityEffectIds @0x924D6E. Each id maps to a slot bit
        // (FX_GetAbilityEffectSlotFlag @0x924B19) that is IDENTICAL to the server-side
        // CouponEffects flag enum -> that enum names every id below. These are NOT lobby
        // cosmetics (kept out of IsLobbyCosmetic); applied in-match as the 45-float
        // ability vector (ShopBaseInfo+52). "// bit" = the CouponEffects flag value.
        // *Alt = an alternate variant id (same effect, different period/variant code).

        // Respawn speed (CouponEffects.Respawn*; HUD STR_TBL_BATTLEGUI_RESPAWN*_ITEM = "Respawn N% mais rapido")
        public const int RespawnSpeed20 = 1600077;   public const int RespawnSpeed20Alt = 1600164;  // bit 0x80000
        public const int RespawnSpeed30 = 1600007;   public const int RespawnSpeed30Alt = 1600163;  // bit 0x100000
        public const int RespawnSpeed50 = 1600064;   public const int RespawnSpeed50Alt = 1600183;  // "Quick Respawn 50%" (bit 0x200000; 1600183 was misnamed Coupon183)
        public const int InstantRespawn = 1600080;   public const int InstantRespawnAlt = 1600182;  // "Instant Respawn" (bit 0x400000; DB name, not "Respawn100")

        // Ammo (system_shop_effects)
        public const int AmmoPlus40 = 1600008;  // "Ammo Up 40%" (bit 0x40000; was mis-RE'd as "ability weight 0.4")
        public const int AmmoPlus10 = 1600185;  // "Ammo Up 10%" (DB suffix 185; 1600207 has no DB name)

        // Defense (CouponEffects.Defense*)
        public const int Defense90 = 1600065;   // bit 0x1
        public const int Defense20 = 1600079;   // bit 0x4
        public const int Defense10 = 1600044;   // bit 0x10
        public const int Defense5  = 1600030;   // bit 0x800

        // HP bonus (CouponEffects.HP*)
        public const int HpPlus5  = 1600040;    // bit 0x20
        public const int HpPlus10 = 1600028;    // bit 0x2000

        // Bullet types (CouponEffects)
        public const int HollowPoint     = 1600032; public const int HollowPointAlt = 1600169;      // bit 0x200
        public const int HollowPointPlus = 1600078;  // bit 0x8
        public const int JackHollowPoint = 1600036;  // bit 0x40
        public const int FullMetalJacket = 1600031; public const int FullMetalJacketAlt = 1600170;  // bit 0x400

        // Quick change (CouponEffects.QuickChange*)
        public const int QuickChangeWeapon = 1600026; public const int QuickChangeWeaponAlt = 1600165; // bit 0x8000
        public const int QuickChangeReload = 1600027; public const int QuickChangeReloadAlt = 1600166; // bit 0x4000

        // Misc battle buffs
        public const int Invincible       = 1600029; public const int InvincibleAlt = 1600167;  // bit 0x1000
        public const int C4SpeedKit       = 1600034;  // bit 0x100
        public const int ExtraGrenade     = 1600035;  // bit 0x80
        public const int GetDroppedWeapon = 1600017; public const int GetDroppedWeaponAlt = 1600168; // bit 0x20000 (pickup enemy/ally dropped weapon)
        public const int FlashProtect     = 1600033;  // CouponEffects.FlashProtect 0x10000; client halves flashbang (ScreenEffect_Flashbang @0xAA25F3)
        public const int Camouflage50     = 1600208;  // bit 0x80000000 (client camo factor 0.8f)
        public const int Camouflage99     = 1600209;  // bit 0x100000000 (client camo factor 0.99f)

        // DB-authoritative name "Increase Smoke Slot" (system_shop_effects suffix 191). The
        // client Ability::GetMaxBulletForAbility @0xD7ABA4 double-ammo path (bit 0x4000000 =
        // CouponEffects.ExtraThrowGrenade) is unrelated/legacy.
        public const int IncreaseSmokeSlot    = 1600191;
        public const int IncreaseSmokeSlotAlt = 1600211;
        public const int IncreaseThrowing2Slot = 1600109;
        public const int IncreaseThrowing2SlotAlt = 1600110;
        // Unmapped: 1600206 (bit 0x1000000, no DB name); 1600207 (bit 0x800000, no DB name).
        //  1600205 (NickBorderColor cosmetic) also carries bit 0x20000000 - vestigial.

        // ---------------------------------------------------------------------------
        // Non-ability function-effect ids (separate client handlers, not the ability registrar).
        public const int ChangeColorVariant = 1600015;       // CHANGE_COLOR popup set with ClanNameColor/NameColor/CrosshairColor
        public const int TimedBuffCountdown = 1600200;       // on-screen HH:MM:SS countdown; expiry sends op144/145
        public const int SeasonExpBoost20 = 1600201;         // "Season Exp Boost 20"
        public const int SeasonExpBoost30 = 1600202;         // "Season Exp Boost 30"
        public const int SeasonExpBoost50 = 1600203;         // "Season Exp Boost 50"
        public const int SeasonExpBoost100 = 1600204;        // "Season Exp Boost 100"
        public const int FreeMoveFreePass = 1600011;         // "Free Move, Free Pass" (variant pair 1600162)
        public const int FreeMoveFreePassAlt = 1600162;
        public const int ExpClan150 = 1600012;               // "EXP Clan 150%"

        // EXP / point boosts (system_shop_effects; not part of the ability bitmask).
        public const int ExpUp110 = 1600001;   // "EXP Up 110%" (also the low bound of the client ability-id loop)
        public const int ExpUp130 = 1600002;   // "EXP Up 130%"
        public const int ExpUp150 = 1600003;   // "EXP Up 150%"
        public const int ExpUp200 = 1600037;   // "EXP Up 200%"
        public const int PointUp130 = 1600004; // "Point Up 130%"
        public const int PointUp150 = 1600119; // "Point Up 150%"
        public const int PointUp200 = 1600038; // "Point Up 200%"
        public const int FakeNickEvent = 1600186; // "Fake Nick (Event)"
        // 1600013: generic use-item, no dedicated client handler (unknown)

        // True when the used item maps to a lobby cosmetic handled by Method0. Non-cosmetic
        // effects fall through to the shared item-auth handler.
        public static bool IsLobbyCosmetic(int effectId)
        {
            switch (effectId)
            {
                case ClanNameColor:
                case NameColor:
                case FakeRank:
                case FakeNick:
                case CrosshairColor:
                case ChangeNick:
                case ClanName:
                case ClanLogo:
                case RoomUserItem:
                case MuzzleColor:
                case ClanEffect:
                case NickBorderColor:
                case FakeNickEvent:
                    return true;
                default:
                    return false;
            }
        }
    }
}
