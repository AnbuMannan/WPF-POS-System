-- Update EF Core Migration History
-- Run this AFTER manually creating the Batches table

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('99999999999999_AddBatchesTableManually', '9.0.0');

SELECT 'Migration history updated successfully!' AS Status;
