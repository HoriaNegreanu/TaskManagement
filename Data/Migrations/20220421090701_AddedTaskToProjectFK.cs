using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.Data.Migrations
{
    public partial class AddedTaskToProjectFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskItem_Project_ProjectID",
                table: "TaskItem");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectID",
                table: "TaskItem",
                type: "int",
                nullable: true,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItem_Project_ProjectID",
                table: "TaskItem",
                column: "ProjectID",
                principalTable: "Project",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskItem_Project_ProjectID",
                table: "TaskItem");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectID",
                table: "TaskItem",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItem_Project_ProjectID",
                table: "TaskItem",
                column: "ProjectID",
                principalTable: "Project",
                principalColumn: "ID");
        }
    }
}
