-- =====================================================================
-- Within API - 03_seed_habit_templates.sql
-- ---------------------------------------------------------------------
-- Optional master-data seed: a starter set of habit templates spanning
-- the five HabitCategory values (Mind, Body, Lifestyle, Social, Nature).
-- Providers/users build on these in the app; admins can edit or add more
-- from the admin portal.
--
-- Category is stored as TEXT (EF HasConversion<string>): use the enum
-- NAMES exactly ('Mind','Body','Lifestyle','Social','Nature').
-- IconKey is a free-form hint; we use Material Symbol names.
--
-- Idempotent: "Name" is UNIQUE, so re-running upserts each row in place
-- (keeps category/description/icon/order/active in sync) without dupes.
--
-- Usage (psql):  psql "<connection string>" -f 03_seed_habit_templates.sql
-- =====================================================================

INSERT INTO within."HabitTemplates"
    ("Id", "Name", "Category", "Description", "IconKey", "SortOrder", "IsActive")
VALUES
    -- ---- Mind ----
    (gen_random_uuid(), 'Meditate',            'Mind',      'Sit quietly and follow your breath for a few minutes.',        'self_improvement', 10, true),
    (gen_random_uuid(), 'Journal',             'Mind',      'Write down a few thoughts, feelings, or reflections.',         'edit_note',        11, true),
    (gen_random_uuid(), 'Read',                'Mind',      'Read something for pleasure or growth.',                       'menu_book',        12, true),
    (gen_random_uuid(), 'Note one gratitude',  'Mind',      'Name one thing you are grateful for today.',                   'favorite',         13, true),
    (gen_random_uuid(), 'Screen-free wind down','Mind',     'Switch off screens before bed to settle your mind.',           'bedtime_off',      14, true),

    -- ---- Body ----
    (gen_random_uuid(), 'Move your body',      'Body',      'Walk, run, stretch, or work out — anything that moves you.',    'directions_run',   20, true),
    (gen_random_uuid(), 'Stretch',             'Body',      'Loosen up with a few minutes of stretching or mobility.',       'sports_gymnastics',21, true),
    (gen_random_uuid(), 'Drink water',         'Body',      'Stay hydrated across the day.',                                'water_drop',       22, true),
    (gen_random_uuid(), 'Sleep 7+ hours',      'Body',      'Give your body a full night of rest.',                         'bedtime',          23, true),
    (gen_random_uuid(), 'Eat a whole-food meal','Body',     'Choose a fresh, minimally processed meal.',                    'restaurant',       24, true),

    -- ---- Lifestyle ----
    (gen_random_uuid(), 'Make your bed',       'Lifestyle', 'Start the day with one small, finished task.',                 'bed',              30, true),
    (gen_random_uuid(), 'Plan tomorrow',       'Lifestyle', 'Jot down your top priorities for the next day.',               'event_note',       31, true),
    (gen_random_uuid(), 'Tidy your space',     'Lifestyle', 'Spend a few minutes clearing your environment.',               'cleaning_services',32, true),
    (gen_random_uuid(), 'Mindful spending',    'Lifestyle', 'Pause before a purchase and check in with yourself.',          'savings',          33, true),

    -- ---- Social ----
    (gen_random_uuid(), 'Reach out to someone','Social',    'Message or call a friend or family member.',                   'call',             40, true),
    (gen_random_uuid(), 'Share a meal',        'Social',    'Eat with someone, in person or online.',                       'group',            41, true),
    (gen_random_uuid(), 'Give a compliment',   'Social',    'Brighten someone''s day with genuine appreciation.',           'volunteer_activism',42, true),
    (gen_random_uuid(), 'Join a circle chat',  'Social',    'Show up in a Within circle conversation.',                     'forum',            43, true),

    -- ---- Nature ----
    (gen_random_uuid(), 'Step outside',        'Nature',    'Spend a few minutes outdoors and breathe.',                    'park',             50, true),
    (gen_random_uuid(), 'Get morning sunlight','Nature',    'Catch some daylight early to steady your rhythm.',             'wb_sunny',         51, true),
    (gen_random_uuid(), 'Tend a plant',        'Nature',    'Water or care for a plant.',                                   'potted_plant',     52, true),
    (gen_random_uuid(), 'Walk in nature',      'Nature',    'Take a walk somewhere green.',                                 'hiking',           53, true)
ON CONFLICT ("Name") DO UPDATE SET
    "Category"    = EXCLUDED."Category",
    "Description" = EXCLUDED."Description",
    "IconKey"     = EXCLUDED."IconKey",
    "SortOrder"   = EXCLUDED."SortOrder",
    "IsActive"    = EXCLUDED."IsActive";
