-- Insert Dharshini admin user (password = "admin")
INSERT INTO "Users" ("Id","Name","Email","Phone","PasswordHash","Role","CreatedAt","IsActive")
VALUES (
  7,
  'Dharshini K',
  'dharshini.k2022cce@sece.ac.in',
  '9000000001',
  '$2a$11$iu9Bt5Pb5Ikv/r5D4Bmw7ei2fxcWYXlYnIGuT3.KjchUrHlFj9Xfq',
  2,
  '2024-01-01 00:00:00+00',
  true
)
ON CONFLICT ("Id") DO UPDATE
  SET "Email"        = EXCLUDED."Email",
      "PasswordHash" = EXCLUDED."PasswordHash",
      "Name"         = EXCLUDED."Name",
      "Role"         = EXCLUDED."Role";

SELECT "Id","Name","Email","Role" FROM "Users" WHERE "Role" = 2;
