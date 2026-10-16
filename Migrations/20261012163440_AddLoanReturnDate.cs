using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusAssets.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanReturnDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnedAt",
                table: "Loans",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnedAt",
                table: "Loans");
        }
    }
}
