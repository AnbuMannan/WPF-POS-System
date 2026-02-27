using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.API.Migrations
{
    /// <inheritdoc />
    public partial class AlterStoresAddContactNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @cnt := (
  SELECT COUNT(*)
  FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'Stores'
    AND COLUMN_NAME = 'ContactNumber'
);
SET @sql := IF(@cnt = 0,
  'ALTER TABLE `Stores` ADD COLUMN `ContactNumber` varchar(20) NULL;',
  'SELECT 1;'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
