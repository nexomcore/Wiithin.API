-- =====================================================================
-- Within API - 02_seed_admin_user.sql
-- ---------------------------------------------------------------------
-- Seeds the SINGLE bootstrap admin user for a fresh database. That is
-- the only data this script inserts.
--
-- All master data (community topics, platform circles, habit templates)
-- is created and managed from the admin portal at runtime -- there are
-- no master-data seed scripts and no startup auto-seeders. After running
-- this, sign into the portal as the admin below and add master data there.
--
-- Idempotent: safe to re-run (ON CONFLICT on the unique Email).
--
-- Admin login:  admin@within.local  /  Within!Admin1
-- (PBKDF2-SHA256, 100k iterations, matching WithinAPI Passwords.Hash)
--
-- Usage (psql):  psql "<connection string>" -f 02_seed_admin_user.sql
-- =====================================================================

INSERT INTO within."Users"
    ("Id", "DisplayName", "Email", "PasswordHash", "PreferredLens", "Role", "CreatedUtc")
VALUES
    ('06f31bfe-09b2-445c-91db-9a707b64660b',
     'Within Admin',
     'admin@within.local',
     'pbkdf2:YjwAj6/Xv2+lEw0+Gm9HaQ==:wYvQFmu24mhAWboaaAp+XVRI1kfZd3C8MV3uhZJuESo=',
     0,   -- PreferredLens = Move
     2,   -- Role = Admin
     now())
ON CONFLICT ("Email") DO NOTHING;
