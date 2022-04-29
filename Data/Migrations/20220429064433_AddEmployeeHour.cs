using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.Data.Migrations
{
    public partial class AddEmployeeHour : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeHour",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkedHours = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TaskItemID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeHour", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EmployeeHour_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeHour_TaskItem_TaskItemID",
                        column: x => x.TaskItemID,
                        principalTable: "TaskItem",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeHour_TaskItemID",
                table: "EmployeeHour",
                column: "TaskItemID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeHour_UserID",
                table: "EmployeeHour",
                column: "UserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeHour");
        }
    }
}
