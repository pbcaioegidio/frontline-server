-- Ammo10 moved from bit 0x800000 to 0x2000000 so the server enum matches the 122
-- client: FX_GetAbilityEffectSlotFlag @0x902E0C returns 0x2000000 for item 1600185
-- (mov eax, offset dword_2000000 at 0x502e5a is an immediate, not a memory read),
-- while 0x800000 belongs to item 1600207.
--
-- accounts.coupon_effect persists the bitmask and it is written raw to the client
-- (WriteQ in ROOM_JOIN_ACK / GET_SLOTINFO_ACK / GET_SLOTONEINFO_ACK and
-- EquipmentSync.SendUDPPlayerSync), so an account still holding the old bit would
-- have the client render item 1600207's effect instead of Ammo10. The expiry path
-- only subtracts a flag it can match (AllUtils.ProcessExpiredCoupon guards on
-- HasFlag), so the stale bit would never be cleared on its own.
--
-- ONE-SHOT, NOT idempotent. Run exactly once, before the cutover to the new enum,
-- and record it in the migration ledger. It is safe only while no code path writes
-- 0x800000. That bit belongs to item 1600207, which today has no server mapping
-- (0x800000 is a gap in CouponEffects), so nothing can set it and the WHERE clause
-- matches only legacy Ammo10 rows. Once 1600207 is wired server-side, re-running
-- this would convert a legitimate 1600207 effect into Ammo10. Do not re-run after
-- that point.

UPDATE accounts
   SET coupon_effect = (coupon_effect & ~(8388608::bigint)) | 33554432::bigint
 WHERE coupon_effect & 8388608::bigint <> 0;
