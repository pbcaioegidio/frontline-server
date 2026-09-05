-- FrontLine - camadas de seguranca (FL Guard)
-- Token OTP, identidade de maquina, ban multi-identificador, evasao, auditoria e termo de uso.
-- Idempotente: pode rodar mais de uma vez.

BEGIN;

-- ---------------------------------------------------------------------------
-- accounts: colunas novas
-- ---------------------------------------------------------------------------
ALTER TABLE accounts ADD COLUMN IF NOT EXISTS hwid_exe        varchar(128) NOT NULL DEFAULT '';
ALTER TABLE accounts ADD COLUMN IF NOT EXISTS tos_version     integer      NOT NULL DEFAULT 0;
ALTER TABLE accounts ADD COLUMN IF NOT EXISTS tos_accepted_at timestamp(6) without time zone;
ALTER TABLE accounts ADD COLUMN IF NOT EXISTS probation_until timestamp(6) without time zone;

-- hwid (fingerprint do launcher) pode ser SHA-256 hex (64) - ja cabe em varchar(64).

-- ---------------------------------------------------------------------------
-- account_devices: cada maquina que ja logou em cada conta
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS account_devices (
    id              bigserial PRIMARY KEY,
    player_id       bigint       NOT NULL,
    fingerprint     varchar(64)  NOT NULL,
    components_json jsonb        NOT NULL DEFAULT '{}'::jsonb,
    first_seen      timestamp(6) without time zone NOT NULL DEFAULT now(),
    last_seen       timestamp(6) without time zone NOT NULL DEFAULT now(),
    last_ip         varchar(64)  NOT NULL DEFAULT '',
    login_count     integer      NOT NULL DEFAULT 1,
    CONSTRAINT ux_account_devices UNIQUE (player_id, fingerprint)
);
CREATE INDEX IF NOT EXISTS ix_account_devices_fp ON account_devices (fingerprint);
CREATE INDEX IF NOT EXISTS ix_account_devices_player ON account_devices (player_id);
-- busca por componente individual (motherboard, bios_uuid, tpm, ...)
CREATE INDEX IF NOT EXISTS ix_account_devices_components ON account_devices USING gin (components_json);

-- ---------------------------------------------------------------------------
-- ban_identifiers: qualquer identificador banido (conta, hardware, rede)
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS ban_identifiers (
    id               bigserial PRIMARY KEY,
    kind             varchar(24)  NOT NULL,
    value            varchar(128) NOT NULL,
    player_id_origem bigint       NOT NULL DEFAULT 0,
    reason           varchar(255) NOT NULL DEFAULT '',
    created_at       timestamp(6) without time zone NOT NULL DEFAULT now(),
    expire_at        timestamp(6) without time zone,
    created_by       varchar(64)  NOT NULL DEFAULT 'system',
    CONSTRAINT ck_ban_identifiers_kind CHECK (kind IN (
        'account', 'fingerprint', 'motherboard', 'bios_uuid', 'disk', 'ram',
        'tpm', 'cpu', 'gpu', 'machine_guid', 'mac', 'ip', 'ip_subnet24', 'hwid_exe'
    )),
    CONSTRAINT ux_ban_identifiers UNIQUE (kind, value)
);
CREATE INDEX IF NOT EXISTS ix_ban_identifiers_value ON ban_identifiers (value);
CREATE INDEX IF NOT EXISTS ix_ban_identifiers_origem ON ban_identifiers (player_id_origem);

-- ---------------------------------------------------------------------------
-- account_links: contas vinculadas por hardware/rede (evasao de ban)
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS account_links (
    id         bigserial PRIMARY KEY,
    player_a   bigint       NOT NULL,
    player_b   bigint       NOT NULL,
    reason     varchar(255) NOT NULL DEFAULT '',
    score      integer      NOT NULL DEFAULT 0,
    created_at timestamp(6) without time zone NOT NULL DEFAULT now(),
    CONSTRAINT ck_account_links_order CHECK (player_a < player_b),
    CONSTRAINT ux_account_links UNIQUE (player_a, player_b)
);
CREATE INDEX IF NOT EXISTS ix_account_links_a ON account_links (player_a);
CREATE INDEX IF NOT EXISTS ix_account_links_b ON account_links (player_b);

-- ---------------------------------------------------------------------------
-- login_audit: toda tentativa de login (launcher e auth)
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS login_audit (
    id          bigserial PRIMARY KEY,
    ts          timestamp(6) without time zone NOT NULL DEFAULT now(),
    source      varchar(16)  NOT NULL,              -- socket | auth
    username    varchar(32)  NOT NULL DEFAULT '',
    player_id   bigint       NOT NULL DEFAULT 0,
    result      varchar(32)  NOT NULL,              -- ok | bad_password | device_banned | ip_banned | token_expired | ...
    ip          varchar(64)  NOT NULL DEFAULT '',
    fingerprint varchar(64)  NOT NULL DEFAULT '',
    reason      varchar(255) NOT NULL DEFAULT ''
);
CREATE INDEX IF NOT EXISTS ix_login_audit_ts ON login_audit (ts DESC);
CREATE INDEX IF NOT EXISTS ix_login_audit_user ON login_audit (username);
CREATE INDEX IF NOT EXISTS ix_login_audit_ip ON login_audit (ip, ts DESC);

-- ---------------------------------------------------------------------------
-- security_events: todo flag / kick / ban / captura com motivo e evidencia
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS security_events (
    id            bigserial PRIMARY KEY,
    ts            timestamp(6) without time zone NOT NULL DEFAULT now(),
    player_id     bigint       NOT NULL DEFAULT 0,
    username      varchar(32)  NOT NULL DEFAULT '',
    nickname      varchar(32)  NOT NULL DEFAULT '',
    action        varchar(24)  NOT NULL,
    source        varchar(16)  NOT NULL,
    category      varchar(32)  NOT NULL,
    reason        varchar(255) NOT NULL,
    evidence_json jsonb        NOT NULL DEFAULT '{}'::jsonb,
    severity      smallint     NOT NULL DEFAULT 1,
    auto          boolean      NOT NULL DEFAULT true,
    gm_id         bigint       NOT NULL DEFAULT 0,
    ban_id        bigint       NOT NULL DEFAULT 0,   -- base_ban_history.object_id quando gerou ban
    client_code   varchar(12)  NOT NULL DEFAULT '',  -- codigo curto mostrado ao jogador (ex.: FG-102)
    CONSTRAINT ck_security_events_action CHECK (action IN (
        'flag', 'kick', 'autoban', 'hardban', 'login_denied', 'heartbeat_lost',
        'module_injection', 'evasion_link', 'screenshot', 'clip', 'capture_blocked', 'info'
    )),
    CONSTRAINT ck_security_events_source CHECK (source IN (
        'socket', 'auth', 'game', 'match', 'gm', 'rcon', 'launcher'
    )),
    CONSTRAINT ck_security_events_severity CHECK (severity BETWEEN 1 AND 5)
);
CREATE INDEX IF NOT EXISTS ix_security_events_ts ON security_events (ts DESC);
CREATE INDEX IF NOT EXISTS ix_security_events_player ON security_events (player_id, ts DESC);
CREATE INDEX IF NOT EXISTS ix_security_events_action ON security_events (action, ts DESC);
CREATE INDEX IF NOT EXISTS ix_security_events_category ON security_events (category, ts DESC);

-- ---------------------------------------------------------------------------
-- security_codes: mapa codigo curto -> categoria (suporte identifica sem expor a regra)
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS security_codes (
    code        varchar(12)  PRIMARY KEY,
    category    varchar(32)  NOT NULL,
    description varchar(255) NOT NULL
);

INSERT INTO security_codes (code, category, description) VALUES
    ('FG-100', 'TOKEN_EXPIRED',    'Token de sessao expirado ou ausente'),
    ('FG-101', 'DEVICE_BANNED',    'Maquina bloqueada'),
    ('FG-102', 'IP_BANNED',        'Endereco bloqueado'),
    ('FG-103', 'ACCOUNT_BANNED',   'Conta bloqueada'),
    ('FG-104', 'EVASION',          'Vinculo com conta banida'),
    ('FG-105', 'TOS_REQUIRED',     'Termo de uso nao aceito'),
    ('FG-110', 'HEARTBEAT_LOST',   'FL Guard desconectado'),
    ('FG-111', 'MODULE',           'Modulo nao autorizado no jogo'),
    ('FG-112', 'PROCESS',          'Programa nao autorizado em execucao'),
    ('FG-113', 'FILE_TAMPER',      'Arquivo do jogo alterado'),
    ('FG-114', 'CAPTURE_BLOCKED',  'Captura de tela bloqueada'),
    ('FG-120', 'SPEED',            'Movimento invalido'),
    ('FG-121', 'HIT_RATE',         'Cadencia de tiro invalida'),
    ('FG-122', 'AMMO',             'Municao invalida'),
    ('FG-123', 'DISTANCE',         'Distancia de acerto invalida'),
    ('FG-124', 'WEAPON',           'Arma nao equipada'),
    ('FG-125', 'KPS',              'Abates rapidos demais'),
    ('FG-130', 'HS_RATIO',         'Taxa de headshot suspeita'),
    ('FG-131', 'PREFIRE',          'Disparo antes de visao'),
    ('FG-132', 'SNAP',             'Mira instantanea'),
    ('FG-133', 'TRACK_WALL',       'Mira acompanha alvo atras de parede'),
    ('FG-140', 'FLOOD',            'Excesso de pacotes'),
    ('FG-141', 'MAX_CONN_IP',      'Conexoes demais do mesmo endereco'),
    ('FG-142', 'BRUTE_FORCE',      'Tentativas de login em excesso'),
    ('FG-150', 'SHOP',             'Operacao de loja invalida')
ON CONFLICT (code) DO NOTHING;

-- ---------------------------------------------------------------------------
-- live_sessions: heartbeat do launcher (Game consulta pra kick)
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS live_sessions (
    player_id      bigint       PRIMARY KEY,
    session_id     varchar(64)  NOT NULL,
    fingerprint    varchar(64)  NOT NULL DEFAULT '',
    ip             varchar(64)  NOT NULL DEFAULT '',
    started_at     timestamp(6) without time zone NOT NULL DEFAULT now(),
    last_heartbeat timestamp(6) without time zone NOT NULL DEFAULT now(),
    status         varchar(16)  NOT NULL DEFAULT 'ok',   -- ok | suspect | tamper
    status_reason  varchar(255) NOT NULL DEFAULT '',
    modules_hash   varchar(64)  NOT NULL DEFAULT ''
);
CREATE INDEX IF NOT EXISTS ix_live_sessions_hb ON live_sessions (last_heartbeat);

-- ---------------------------------------------------------------------------
-- module_whitelist: DLLs permitidas dentro do FrontLine.exe
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS module_whitelist (
    name       varchar(128) PRIMARY KEY,   -- nome do arquivo em minusculo (ex.: crypto.dll)
    md5        varchar(32)  NOT NULL DEFAULT '',   -- vazio = qualquer versao (DLLs do Windows)
    note       varchar(255) NOT NULL DEFAULT '',
    created_at timestamp(6) without time zone NOT NULL DEFAULT now()
);

-- ---------------------------------------------------------------------------
-- shop_audit: toda compra/presente/extensao
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS shop_audit (
    id           bigserial PRIMARY KEY,
    ts           timestamp(6) without time zone NOT NULL DEFAULT now(),
    player_id    bigint  NOT NULL,
    op           varchar(16) NOT NULL,      -- buy | gift | extend
    good_id      integer NOT NULL DEFAULT 0,
    item_id      integer NOT NULL DEFAULT 0,
    currency     varchar(8) NOT NULL DEFAULT '',   -- gold | cash | tags
    price        integer NOT NULL DEFAULT 0,
    balance_before integer NOT NULL DEFAULT 0,
    balance_after  integer NOT NULL DEFAULT 0,
    target_player  bigint NOT NULL DEFAULT 0,
    ip           varchar(64) NOT NULL DEFAULT ''
);
CREATE INDEX IF NOT EXISTS ix_shop_audit_player ON shop_audit (player_id, ts DESC);

COMMIT;
