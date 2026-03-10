using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.API.Migrations
{
    /// <inheritdoc />
    public partial class FixSupplierTransactionStoreCodeNulls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix NULL StoreCode values in SupplierTransactions table
            migrationBuilder.Sql("UPDATE SupplierTransactions SET StoreCode = 1 WHERE StoreCode IS NULL");
            
            // Ensure StoreCode column is not nullable
            migrationBuilder.AlterColumn<int>(
                name: "StoreCode",
                table: "SupplierTransactions",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert the StoreCode column to allow NULL values
            migrationBuilder.AlterColumn<int>(
                name: "StoreCode",
                table: "SupplierTransactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);
        }
    }
}
