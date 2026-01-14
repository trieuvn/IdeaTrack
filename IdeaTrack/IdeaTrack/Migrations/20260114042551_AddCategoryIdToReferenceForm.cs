using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdeaTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryIdToReferenceForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "ReferenceForms",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceForms_CategoryId",
                table: "ReferenceForms",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReferenceForms_InitiativeCategories_CategoryId",
                table: "ReferenceForms",
                column: "CategoryId",
                principalTable: "InitiativeCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReferenceForms_InitiativeCategories_CategoryId",
                table: "ReferenceForms");

            migrationBuilder.DropIndex(
                name: "IX_ReferenceForms_CategoryId",
                table: "ReferenceForms");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "ReferenceForms");
        }
    }
}
