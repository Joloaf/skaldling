using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skaldling.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarConfigToHero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarConfig",
                table: "Heroes",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarConfig",
                table: "Heroes");
        }
    }
}
