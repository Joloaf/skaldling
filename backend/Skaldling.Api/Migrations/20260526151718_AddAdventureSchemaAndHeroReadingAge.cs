using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skaldling.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAdventureSchemaAndHeroReadingAge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReadingAge",
                table: "Heroes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Themes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Themes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Adventures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HeroId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThemeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Tone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NarrativeStyle = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Moral = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FinaleReward = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adventures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Adventures_Heroes_HeroId",
                        column: x => x.HeroId,
                        principalTable: "Heroes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Adventures_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdventureTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdventureId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Difficulty = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PointValue = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdventureTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdventureTasks_Adventures_AdventureId",
                        column: x => x.AdventureId,
                        principalTable: "Adventures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Days",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdventureId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayNumber = table.Column<int>(type: "integer", nullable: false),
                    NarrativeIntro = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    NarrativeConvergence = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Days", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Days_Adventures_AdventureId",
                        column: x => x.AdventureId,
                        principalTable: "Adventures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DayId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchLabel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branches_Days_DayId",
                        column: x => x.DayId,
                        principalTable: "Days",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Nodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    NarrativeText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SceneType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AdventureTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Nodes_AdventureTasks_AdventureTaskId",
                        column: x => x.AdventureTaskId,
                        principalTable: "AdventureTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Nodes_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Themes",
                columns: new[] { "Id", "Description", "IsDefault", "Name" },
                values: new object[] { new Guid("22222222-0001-0000-0000-000000000001"), "Mist-cloaked fjords, ancient longships, and creatures from the old sagas. Heroes brave the cold north, sailing wild seas, walking whispering forests, and meeting trolls and skalds alike.", true, "Norse Fantasy" });

            migrationBuilder.CreateIndex(
                name: "IX_Adventures_HeroId",
                table: "Adventures",
                column: "HeroId");

            migrationBuilder.CreateIndex(
                name: "IX_Adventures_ThemeId",
                table: "Adventures",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_AdventureTasks_AdventureId",
                table: "AdventureTasks",
                column: "AdventureId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_DayId",
                table: "Branches",
                column: "DayId");

            migrationBuilder.CreateIndex(
                name: "IX_Days_AdventureId_DayNumber",
                table: "Days",
                columns: new[] { "AdventureId", "DayNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_AdventureTaskId",
                table: "Nodes",
                column: "AdventureTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_BranchId",
                table: "Nodes",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Themes_Name",
                table: "Themes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Nodes");

            migrationBuilder.DropTable(
                name: "AdventureTasks");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropTable(
                name: "Days");

            migrationBuilder.DropTable(
                name: "Adventures");

            migrationBuilder.DropTable(
                name: "Themes");

            migrationBuilder.DropColumn(
                name: "ReadingAge",
                table: "Heroes");
        }
    }
}
