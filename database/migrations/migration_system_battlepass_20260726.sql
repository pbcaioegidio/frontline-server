-- Migration: battle pass seasons and cards from JSON to DB
-- Source replaced: Data/BattlepassInfo.json (BattlePassLoader)
-- Idempotent: safe to re-run. Seed reflects the values running on 2026-07-26.
--
-- Dates keep the client's decimal yyMMddHHmm shape (e.g. 2502010559), which does not fit
-- in an integer, so they are bigint. Price is bigint for the same reason.

CREATE TABLE IF NOT EXISTS system_battlepass_seasons (
    season_id           integer PRIMARY KEY,
    name                varchar(64)  NOT NULL DEFAULT '',
    description         varchar(255) NOT NULL DEFAULT '',
    enabled             integer      NOT NULL DEFAULT 0,
    enabled_for_free    boolean      NOT NULL DEFAULT true,
    enabled_for_premium boolean      NOT NULL DEFAULT true,
    price               bigint       NOT NULL DEFAULT 0,
    start_date          bigint       NOT NULL DEFAULT 0,
    end_date            bigint       NOT NULL DEFAULT 0,
    max_daily_points    integer      NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS system_battlepass_cards (
    season_id      integer NOT NULL,
    card_number    integer NOT NULL,
    required_exp   integer NOT NULL DEFAULT 0,
    normal_card    integer NOT NULL DEFAULT 0,
    premium_card_a integer NOT NULL DEFAULT 0,
    premium_card_b integer NOT NULL DEFAULT 0,
    PRIMARY KEY (season_id, card_number)
);

INSERT INTO system_battlepass_seasons (season_id, name, description, enabled,
                                       enabled_for_free, enabled_for_premium, price,
                                       start_date, end_date, max_daily_points) VALUES
    (1, 'Britannia Season 1', 'Suba de nivel jogando e desbloqueie skins e personagens exclusivos.', 1, true, true, 10000, 2502010559, 2612120559, 0)
ON CONFLICT (season_id) DO NOTHING;

INSERT INTO system_battlepass_cards (season_id, card_number, required_exp,
                                     normal_card, premium_card_a, premium_card_b) VALUES
    (1, 1, 500, 31500912, 10355712, 10387012),
    (1, 2, 1500, 31501612, 10357012, 10387412),
    (1, 3, 3000, 31502112, 10359912, 10388412),
    (1, 4, 5000, 31502712, 10361112, 10388512),
    (1, 5, 7500, 31503212, 60132712, 10389212),
    (1, 6, 10500, 31503312, 10364312, 10389312),
    (1, 7, 14000, 31500312, 10365112, 10389412),
    (1, 8, 18000, 31501012, 10367712, 10390212),
    (1, 9, 22500, 31504312, 10370012, 10390712),
    (1, 10, 27500, 32300412, 60134712, 10390812),
    (1, 11, 33000, 32300212, 10384912, 10391012),
    (1, 12, 39000, 31502412, 10386112, 10391712),
    (1, 13, 45500, 10401412, 10560412, 10391812),
    (1, 14, 52500, 10402112, 10560912, 10392212),
    (1, 15, 60000, 31500212, 60206212, 10392312),
    (1, 16, 68000, 10524012, 10561012, 10392412),
    (1, 17, 76500, 10501512, 10561912, 10392512),
    (1, 18, 85500, 10503612, 10563012, 10392612),
    (1, 19, 95000, 10509212, 10563112, 10392712),
    (1, 20, 105000, 10512712, 60206312, 10393312),
    (1, 21, 115500, 10516912, 10564812, 10393812),
    (1, 22, 126500, 10522512, 10565112, 10393912),
    (1, 23, 138000, 10528412, 10565512, 10394012),
    (1, 24, 150000, 10531912, 10566312, 10394612),
    (1, 25, 162500, 10536712, 60206512, 10394712),
    (1, 26, 175500, 10544412, 10570512, 10394812),
    (1, 27, 189000, 10551012, 10507312, 10394912),
    (1, 28, 203000, 10309212, 10507912, 10395312),
    (1, 29, 217500, 10322112, 10508312, 10396312),
    (1, 30, 232500, 10326412, 60232712, 10396412),
    (1, 31, 248000, 10345912, 10508712, 31501112),
    (1, 32, 264000, 10348512, 10509912, 31502512),
    (1, 33, 280500, 10350312, 10512112, 31502912),
    (1, 34, 297500, 10353112, 10513212, 10381412),
    (1, 35, 315000, 10355412, 60235112, 10395412),
    (1, 36, 333000, 11600512, 10513812, 10451312),
    (1, 37, 351500, 11600612, 10513912, 10631912),
    (1, 38, 370500, 10515812, 10514012, 13608612),
    (1, 39, 390000, 10521712, 10514712, 13638512),
    (1, 40, 410000, 10530912, 60102812, 13638712),
    (1, 41, 430500, 10531012, 10514812, 13642812),
    (1, 42, 451500, 10556312, 10515912, 13643912),
    (1, 43, 473000, 10557912, 10516112, 10568012),
    (1, 44, 495000, 10558212, 10516812, 10632312),
    (1, 45, 517500, 10558312, 60202912, 13650312),
    (1, 46, 540500, 10558412, 10517012, 13650512),
    (1, 47, 564000, 10559012, 10517112, 13651012),
    (1, 48, 588000, 10559212, 10517312, 13904312),
    (1, 49, 612500, 10559612, 10517712, 13904912),
    (1, 50, 637500, 10560112, 60102612, 10627612)
ON CONFLICT (season_id, card_number) DO NOTHING;
