-- =====================================================================
-- Within API - 01_reset_database.sql
-- ---------------------------------------------------------------------
-- Wipes ALL data from every table in the "within" schema while keeping
-- the table structures, Postgres enum types, and the EF migration
-- history intact. After running this the API boots WITHOUT re-running
-- migrations; run 02_seed_master_data.sql to repopulate master data.
--
-- This is destructive: every user, event, circle, post, check-in, etc.
-- is permanently deleted. Take a backup first if unsure.
--
-- Usage (psql):  psql "<connection string>" -f 01_reset_database.sql
-- =====================================================================

-- TRUNCATE every table in the "within" schema in one statement, except
-- the EF migrations history table. A single TRUNCATE ... CASCADE clears
-- all rows and resets identity sequences regardless of FK ordering.
DO $$
DECLARE
    truncate_sql text;
BEGIN
    SELECT 'TRUNCATE TABLE '
           || string_agg(format('%I.%I', schemaname, tablename), ', ')
           || ' RESTART IDENTITY CASCADE'
    INTO truncate_sql
    FROM pg_tables
    WHERE schemaname = 'within'
      AND tablename <> '__EFMigrationsHistory';

    IF truncate_sql IS NULL THEN
        RAISE NOTICE 'No tables found in schema "within" - nothing to truncate.';
    ELSE
        RAISE NOTICE 'Executing: %', truncate_sql;
        EXECUTE truncate_sql;
        RAISE NOTICE 'All "within" tables truncated (migration history preserved).';
    END IF;
END $$;
