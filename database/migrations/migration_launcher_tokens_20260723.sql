BEGIN;

DO $$
BEGIN
  IF (SELECT count(*) FROM accounts WHERE username = 'testbr50') <> 1
     OR (SELECT count(*) FROM accounts WHERE username = 'testbr51') <> 1
     OR (SELECT count(*) FROM accounts WHERE username = 'testbr16') <> 1 THEN
    RAISE EXCEPTION 'launcher token migration requires exactly one testbr50, testbr51, and testbr16 account';
  END IF;

  IF EXISTS (
    SELECT 1
    FROM accounts
    WHERE (username = 'testbr50' AND token NOT IN ('', 'AAECAwQFBgcICQoLDA0OMg=='))
       OR (username = 'testbr51' AND token NOT IN ('', 'AAECAwQFBgcICQoLDA0OMw=='))
       OR (username = 'testbr16' AND token NOT IN ('', 'AQIDBAUGBwgJCgsMDQ4PEA=='))
  ) THEN
    RAISE EXCEPTION 'launcher token migration found a conflicting existing token';
  END IF;
END $$;

UPDATE accounts
SET token = CASE username
  WHEN 'testbr50' THEN 'AAECAwQFBgcICQoLDA0OMg=='
  WHEN 'testbr51' THEN 'AAECAwQFBgcICQoLDA0OMw=='
  WHEN 'testbr16' THEN 'AQIDBAUGBwgJCgsMDQ4PEA=='
END
WHERE username IN ('testbr50', 'testbr51', 'testbr16');

CREATE UNIQUE INDEX IF NOT EXISTS ux_accounts_token_nonempty
ON accounts (token)
WHERE token <> '';

COMMIT;
