SELECT item_id, name, item_count, kind, seq FROM system_template_items ORDER BY kind, seq;
SELECT column_default FROM information_schema.columns WHERE table_name = 'accounts' AND column_name IN ('gold','cash','rank','experience','pc_cafe');
