using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace samvaad_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSocieties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SocietyId",
                table: "Posts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Societies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AvatarUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MembersCount = table.Column<int>(type: "integer", nullable: false),
                    PostsCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Societies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Societies_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SocietyMembers",
                columns: table => new
                {
                    SocietyId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocietyMembers", x => new { x.SocietyId, x.UserId });
                    table.ForeignKey(
                        name: "FK_SocietyMembers_Societies_SocietyId",
                        column: x => x.SocietyId,
                        principalTable: "Societies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SocietyMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_SocietyId",
                table: "Posts",
                column: "SocietyId");

            migrationBuilder.CreateIndex(
                name: "IX_Societies_CreatedAt",
                table: "Societies",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Societies_CreatedByUserId",
                table: "Societies",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Societies_Name",
                table: "Societies",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SocietyMembers_UserId",
                table: "SocietyMembers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Societies_SocietyId",
                table: "Posts",
                column: "SocietyId",
                principalTable: "Societies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Societies_SocietyId",
                table: "Posts");

            migrationBuilder.DropTable(
                name: "SocietyMembers");

            migrationBuilder.DropTable(
                name: "Societies");

            migrationBuilder.DropIndex(
                name: "IX_Posts_SocietyId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "SocietyId",
                table: "Posts");
        }
    }
}
