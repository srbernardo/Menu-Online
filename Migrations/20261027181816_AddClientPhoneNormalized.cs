using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuOnline.Migrations
{
    /// <inheritdoc />
    public partial class AddClientPhoneNormalized : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNormalized",
                table: "Clients",
                type: "text",
                nullable: false,
                computedColumnSql: "regexp_replace(\"WhatsappNumber\", '[^0-9]', '', 'g')",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_PhoneNormalized",
                table: "Clients",
                column: "PhoneNormalized",
                unique: true,
                filter: "\"PhoneNormalized\" <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clients_PhoneNormalized",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PhoneNormalized",
                table: "Clients");
        }
    }
}
