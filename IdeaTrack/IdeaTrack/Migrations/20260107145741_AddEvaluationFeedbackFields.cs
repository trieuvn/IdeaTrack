using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdeaTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddEvaluationFeedbackFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Limitations",
                table: "InitiativeAssignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Recommendations",
                table: "InitiativeAssignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Strengths",
                table: "InitiativeAssignments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Limitations",
                table: "InitiativeAssignments");

            migrationBuilder.DropColumn(
                name: "Recommendations",
                table: "InitiativeAssignments");

            migrationBuilder.DropColumn(
                name: "Strengths",
                table: "InitiativeAssignments");
        }
    }
}
