using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BookLib.Migrations
{
    public partial class SeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Authors",
                columns: new[] { "Id", "BirthDate", "BirthPlace", "Email", "Name" },
                values: new object[,]
                {
                    { new byte[] { 245, 181, 213, 114, 8, 48, 183, 73, 176, 214, 204, 51, 127, 26, 51, 48 }, new DateTimeOffset(new DateTime(1960, 11, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 8, 0, 0, 0)), "Beijing", "author1@xxx.com", "Author 1" },
                    { new byte[] { 142, 164, 4, 125, 78, 190, 142, 70, 140, 226, 58, 192, 160, 199, 149, 73 }, new DateTimeOffset(new DateTime(1976, 8, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 8, 0, 0, 0)), "Hubei", "author2@xxx.com", "Author 2" },
                    { new byte[] { 62, 177, 6, 132, 147, 167, 18, 75, 132, 203, 127, 226, 166, 148, 185, 170 }, new DateTimeOffset(new DateTime(1973, 2, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 8, 0, 0, 0)), "Hubei", "author3@xxx.com", "Author 3" },
                    { new byte[] { 189, 106, 85, 116, 108, 26, 32, 77, 168, 167, 39, 29, 212, 57, 59, 46 }, new DateTimeOffset(new DateTime(1978, 7, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 8, 0, 0, 0)), "Shandong", "author4@xxx.com", "Author 4" },
                    { new byte[] { 87, 219, 41, 16, 92, 193, 12, 76, 128, 160, 200, 17, 183, 153, 92, 180 }, new DateTimeOffset(new DateTime(1973, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 8, 0, 0, 0)), "Beijing", "author5@xxx.com", "Author 5" },
                    { new byte[] { 246, 140, 151, 15, 109, 223, 169, 71, 142, 242, 210, 114, 60, 194, 156, 200 }, new DateTimeOffset(new DateTime(1981, 5, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 8, 0, 0, 0)), "Beijing", "author6@xxx.com", "Author 6" },
                    { new byte[] { 118, 57, 238, 16, 114, 214, 17, 68, 174, 28, 50, 103, 186, 169, 64, 235 }, new DateTimeOffset(new DateTime(1954, 9, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 8, 0, 0, 0)), "Shandong", "author7@xxx.com", "Author 7" },
                    { new byte[] { 156, 167, 51, 38, 74, 159, 213, 72, 174, 90, 112, 148, 95, 184, 88, 60 }, new DateTimeOffset(new DateTime(1981, 9, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 8, 0, 0, 0)), "Shandong", "author8@xxx.com", "Author 8" }
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "AuthorId", "Description", "Pages", "Title" },
                values: new object[,]
                {
                    { new byte[] { 169, 189, 142, 125, 52, 38, 15, 76, 148, 105, 6, 149, 214, 19, 33, 83 }, new byte[] { 245, 181, 213, 114, 8, 48, 183, 73, 176, 214, 204, 51, 127, 26, 51, 48 }, "Description of Book 1", 281, "Book 1" },
                    { new byte[] { 151, 118, 212, 30, 125, 170, 194, 72, 170, 57, 48, 93, 14, 19, 179, 170 }, new byte[] { 245, 181, 213, 114, 8, 48, 183, 73, 176, 214, 204, 51, 127, 26, 51, 48 }, "Description of Book 2", 370, "Book 2" },
                    { new byte[] { 82, 200, 130, 95, 93, 55, 38, 73, 163, 183, 132, 182, 63, 193, 191, 174 }, new byte[] { 142, 164, 4, 125, 78, 190, 142, 70, 140, 226, 58, 192, 160, 199, 149, 73 }, "Description of Book 3", 229, "Book 3" },
                    { new byte[] { 32, 91, 138, 65, 11, 70, 4, 70, 190, 23, 43, 8, 9, 225, 154, 205 }, new byte[] { 142, 164, 4, 125, 78, 190, 142, 70, 140, 226, 58, 192, 160, 199, 149, 73 }, "Description of Book 4", 440, "Book 4" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new byte[] { 246, 140, 151, 15, 109, 223, 169, 71, 142, 242, 210, 114, 60, 194, 156, 200 });

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new byte[] { 87, 219, 41, 16, 92, 193, 12, 76, 128, 160, 200, 17, 183, 153, 92, 180 });

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new byte[] { 118, 57, 238, 16, 114, 214, 17, 68, 174, 28, 50, 103, 186, 169, 64, 235 });

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new byte[] { 156, 167, 51, 38, 74, 159, 213, 72, 174, 90, 112, 148, 95, 184, 88, 60 });

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new byte[] { 189, 106, 85, 116, 108, 26, 32, 77, 168, 167, 39, 29, 212, 57, 59, 46 });

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new byte[] { 62, 177, 6, 132, 147, 167, 18, 75, 132, 203, 127, 226, 166, 148, 185, 170 });

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: new byte[] { 151, 118, 212, 30, 125, 170, 194, 72, 170, 57, 48, 93, 14, 19, 179, 170 });

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: new byte[] { 32, 91, 138, 65, 11, 70, 4, 70, 190, 23, 43, 8, 9, 225, 154, 205 });

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: new byte[] { 82, 200, 130, 95, 93, 55, 38, 73, 163, 183, 132, 182, 63, 193, 191, 174 });

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: new byte[] { 169, 189, 142, 125, 52, 38, 15, 76, 148, 105, 6, 149, 214, 19, 33, 83 });

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new byte[] { 245, 181, 213, 114, 8, 48, 183, 73, 176, 214, 204, 51, 127, 26, 51, 48 });

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new byte[] { 142, 164, 4, 125, 78, 190, 142, 70, 140, 226, 58, 192, 160, 199, 149, 73 });
        }
    }
}
