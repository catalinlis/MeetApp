using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class ChatsBetweenUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    ChatFirstUserId = table.Column<int>(type: "integer", nullable: false),
                    ChatSecondUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => new { x.ChatFirstUserId, x.ChatSecondUserId });
                    table.ForeignKey(
                        name: "FK_Chats_AspNetUsers_ChatFirstUserId",
                        column: x => x.ChatFirstUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chats_AspNetUsers_ChatSecondUserId",
                        column: x => x.ChatSecondUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chats_ChatSecondUserId",
                table: "Chats",
                column: "ChatSecondUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chats");
        }
    }
}
