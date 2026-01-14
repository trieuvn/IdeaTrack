using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdeaTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddHidePersonalInfoToInitiative : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HidePersonalInfo",
                table: "Initiatives",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HidePersonalInfo",
                table: "Initiatives");
        }
    }
}
