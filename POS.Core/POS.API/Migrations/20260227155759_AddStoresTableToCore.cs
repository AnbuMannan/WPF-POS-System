using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddStoresTableToCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `Stores` (
  `StoreCode` int NOT NULL,
  `StoreName` varchar(200) NOT NULL,
  `Address` varchar(500) NULL,
  `ContactNumber` varchar(20) NULL,
  `TaxId` varchar(20) NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT TRUE,
  PRIMARY KEY (`StoreCode`)
) CHARACTER SET=utf8mb4;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
