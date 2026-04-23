-- Restore NumberPlate column to sync DB with EF Core model
ALTER TABLE "Buses" ADD COLUMN IF NOT EXISTS "NumberPlate" TEXT NOT NULL DEFAULT '';

-- Create the unique index that EF expects
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Buses_NumberPlate" ON "Buses" ("NumberPlate");

-- Populate placeholder values so existing rows don't violate NOT NULL
UPDATE "Buses" SET "NumberPlate" = 'TN' || LPAD("Id"::text, 8, '0') WHERE "NumberPlate" = '';

SELECT "Id", "BusName", "NumberPlate" FROM "Buses";
