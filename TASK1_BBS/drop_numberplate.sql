-- Drop the NumberPlate unique index and column (column was already removed by user)
DROP INDEX IF EXISTS "IX_Buses_NumberPlate";
ALTER TABLE "Buses" DROP COLUMN IF EXISTS "NumberPlate";
SELECT 'NumberPlate index and column removed.' AS result;
