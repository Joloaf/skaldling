using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Skaldling.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSpriteCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sprites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AssetPath = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Layer = table.Column<int>(type: "integer", nullable: false),
                    ArchetypeFamily = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    Tags = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sprites", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Sprites",
                columns: new[] { "Id", "ArchetypeFamily", "AssetPath", "Description", "IsDefault", "Layer", "Name", "Tags", "Type" },
                values: new object[,]
                {
                    { new Guid("11111111-0001-0001-0000-000000000001"), "Human", "/sprites/body/human-base.svg", "A slender human figure standing at ease, neutral posture.", true, 10, "human-base", null, "BodyArchetype" },
                    { new Guid("11111111-0001-0002-0000-000000000001"), "Beast", "/sprites/body/beast-base.svg", "A sturdy creaturely figure with broad shoulders and a slight crouch, standing on its hind legs.", true, 10, "beast-base", null, "BodyArchetype" },
                    { new Guid("11111111-0001-0003-0000-000000000001"), "Robot", "/sprites/body/robot-base.svg", "An angular humanoid robot frame in muted brushed steel, standing upright.", true, 10, "robot-base", null, "BodyArchetype" },
                    { new Guid("11111111-0002-0001-0000-000000000001"), "Human", "/sprites/face/human-face.svg", "A calm, attentive human face with a small mouth and a steady gaze.", true, 40, "human-face", null, "Face" },
                    { new Guid("11111111-0002-0002-0000-000000000001"), "Beast", "/sprites/face/beast-face.svg", "An expressive creature face with a warm-toned snout and watchful eyes.", true, 40, "beast-face", null, "Face" },
                    { new Guid("11111111-0002-0003-0000-000000000001"), "Robot", "/sprites/face/robot-face.svg", "A smooth metallic faceplate with sensor eyes and a thin speaker grille.", true, 40, "robot-face", null, "Face" },
                    { new Guid("11111111-0003-0000-0000-000000000001"), "Human", "/sprites/eyes/green-eyes.svg", "Sharp green eyes that catch the light.", true, 50, "green-eyes", null, "Eyes" },
                    { new Guid("11111111-0003-0000-0000-000000000002"), "Human", "/sprites/eyes/brown-eyes.svg", "Warm brown eyes, gentle and curious.", true, 50, "brown-eyes", null, "Eyes" },
                    { new Guid("11111111-0004-0000-0000-000000000001"), "Human", "/sprites/hair/curly-red-hair.svg", "Long curly rust-red hair tied with a thin leather thong, falling past the shoulders.", true, 60, "curly-red-hair", null, "Hair" },
                    { new Guid("11111111-0004-0000-0000-000000000002"), "Human", "/sprites/hair/short-black-hair.svg", "Short jet-black hair, slightly tousled.", true, 60, "short-black-hair", null, "Hair" },
                    { new Guid("11111111-0004-0000-0000-000000000003"), "Human", "/sprites/hair/long-blonde-hair.svg", "Long flowing honey-blonde hair that reaches the upper back.", true, 60, "long-blonde-hair", null, "Hair" },
                    { new Guid("11111111-0005-0000-0000-000000000001"), "Human", "/sprites/outfit-top/leather-vest.svg", "A worn leather vest with brass buttons over a coarse linen shirt.", true, 30, "leather-vest", null, "OutfitTop" },
                    { new Guid("11111111-0005-0000-0000-000000000002"), "Human", "/sprites/outfit-top/woolen-tunic.svg", "A heavy moss-green woolen tunic, cinched at the waist.", true, 30, "woolen-tunic", null, "OutfitTop" },
                    { new Guid("11111111-0006-0000-0000-000000000001"), "Human", "/sprites/outfit-bottom/brown-trousers.svg", "Practical brown trousers tucked into short boots.", true, 20, "brown-trousers", null, "OutfitBottom" },
                    { new Guid("11111111-0007-0000-0000-000000000001"), "Human", "/sprites/accessory/feathered-cap.svg", "A dusty-blue cap with a single eagle feather angling skyward.", true, 70, "feathered-cap", null, "Accessory" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sprites_Name",
                table: "Sprites",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sprites");
        }
    }
}
