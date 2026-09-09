-- Push quase tempo real: quando accounts.online muda, avisa ouvintes (bot Discord).
-- Canal: frontline_online
-- Payload: JSON com player_id, online, e contagem atual (evita query extra no bot).
-- Idempotente.

CREATE OR REPLACE FUNCTION public.notify_frontline_online()
RETURNS trigger
LANGUAGE plpgsql
AS $$
DECLARE
  online_count integer;
BEGIN
  IF TG_OP = 'UPDATE' AND NEW.online IS NOT DISTINCT FROM OLD.online THEN
    RETURN NEW;
  END IF;

  SELECT COUNT(*)::integer INTO online_count
  FROM public.accounts
  WHERE online = true;

  PERFORM pg_notify(
    'frontline_online',
    json_build_object(
      'player_id', NEW.player_id,
      'online', NEW.online,
      'count', online_count,
      'ts', extract(epoch FROM clock_timestamp())::bigint
    )::text
  );

  RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS trg_notify_frontline_online ON public.accounts;

CREATE TRIGGER trg_notify_frontline_online
AFTER UPDATE OF online ON public.accounts
FOR EACH ROW
EXECUTE FUNCTION public.notify_frontline_online();
