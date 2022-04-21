using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.Data.Migrations
{
    public partial class AddedProjectsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectID",
                table: "TaskItem",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Project",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskItem_ProjectID",
                table: "TaskItem",
                column: "ProjectID");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItem_Project_ProjectID",
                table: "TaskItem",
                column: "ProjectID",
                principalTable: "Project",
                principalColumn: "ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskItem_Project_ProjectID",
                table: "TaskItem");

            migrationBuilder.DropTable(
                name: "Project");

            migrationBuilder.DropIndex(
                name: "IX_TaskItem_ProjectID",
                table: "TaskItem");

            migrationBuilder.DropColumn(
                name: "ProjectID",
                table: "TaskItem");
        }
    }
}
