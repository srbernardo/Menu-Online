using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuOnline.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE \"Users\" SET \"Slug\" = 'temp-' || \"Id\"::text " +
                "WHERE \"Slug\" IS NULL OR \"Slug\" = '';");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Slug",
                table: "Users",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Slug",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: false);

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Users");
        }
    }
}
