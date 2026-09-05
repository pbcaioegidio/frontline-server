-- FrontLine - fila de captura de evidência (screenshot / clip)
-- Idempotente.

BEGIN;

CREATE TABLE IF NOT EXISTS capture_requests (
    id            bigserial PRIMARY KEY,
    player_id     bigint       NOT NULL,
    kind          varchar(16)  NOT NULL,   -- screenshot | clip
    reason        varchar(255) NOT NULL DEFAULT '',
    requested_by  varchar(64)  NOT NULL DEFAULT 'system',
    gm_id         bigint       NOT NULL DEFAULT 0,
    created_at    timestamp(6) without time zone NOT NULL DEFAULT now(),
    delivered_at  timestamp(6) without time zone,
    completed_at  timestamp(6) without time zone,
    status        varchar(16)  NOT NULL DEFAULT 'pending',
    evidence_path varchar(512) NOT NULL DEFAULT '',
    event_id      bigint       NOT NULL DEFAULT 0,
    error         varchar(255) NOT NULL DEFAULT '',
    CONSTRAINT ck_capture_requests_kind CHECK (kind IN ('screenshot', 'clip')),
    CONSTRAINT ck_capture_requests_status CHECK (status IN (
        'pending', 'delivered', 'completed', 'failed', 'blocked', 'expired'
    ))
);

CREATE INDEX IF NOT EXISTS ix_capture_requests_pending
    ON capture_requests (player_id, status, created_at)
    WHERE status = 'pending';

CREATE INDEX IF NOT EXISTS ix_capture_requests_player
    ON capture_requests (player_id, created_at DESC);

-- evidencia em security_events já cobre o log; path fica no evidence_json + aqui

COMMIT;
