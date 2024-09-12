using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMS_TRAINING.Data.Migrations
{
    public partial class OcrInjection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "FileMetadatas",
                newName: "RecognitionText");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RecognitionText",
                table: "FileMetadatas",
                newName: "ContentType");
        }
    }
}
