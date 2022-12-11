using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoManager.Data.PhotoMigrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Albums",
                columns: table => new
                {
                    AlbumId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.AlbumId);
                });

            migrationBuilder.CreateTable(
                name: "CameraBodys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Model_name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Maker = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Sensor_type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CameraBodys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Model_name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Maker = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Max_focal_length = table.Column<int>(type: "int", nullable: false),
                    Min_focal_length = table.Column<int>(type: "int", nullable: false),
                    Max_aperture = table.Column<double>(type: "float", nullable: false),
                    Min_aperture = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    PhotoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Bit = table.Column<int>(type: "int", nullable: true),
                    Maker = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Model = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Dpi = table.Column<int>(type: "int", nullable: true),
                    Software = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Export_datetime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Exposure = table.Column<int>(type: "int", nullable: true),
                    Aperture = table.Column<double>(type: "float", nullable: true),
                    Exposure_program = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Iso = table.Column<int>(type: "int", nullable: true),
                    Original_datetime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timezone_offset = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Shutter_speed = table.Column<double>(type: "float", nullable: true),
                    Shutter_speed_text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Metering_mode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    White_balance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Focal_length = table.Column<double>(type: "float", nullable: true),
                    Color_space = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Exposure_mode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Focal_length_in_35 = table.Column<double>(type: "float", nullable: true),
                    Lens_model = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gps_latitude_ref = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gps_longitude_ref = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gps_latitude = table.Column<double>(type: "float", nullable: true),
                    Gps_longitude = table.Column<double>(type: "float", nullable: true),
                    Place_city = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Place_sublocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Place_province = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Place_country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    File_format = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    File_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    File_path = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Thumbnail_path = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.PhotoId);
                });

            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    ArticleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Publish_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Edit_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PhotoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.ArticleId);
                    table.ForeignKey(
                        name: "FK_Articles_Photos_PhotoId",
                        column: x => x.PhotoId,
                        principalTable: "Photos",
                        principalColumn: "PhotoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlbumArticle",
                columns: table => new
                {
                    AlbumsAlbumId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArticlesArticleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumArticle", x => new { x.AlbumsAlbumId, x.ArticlesArticleId });
                    table.ForeignKey(
                        name: "FK_AlbumArticle_Albums_AlbumsAlbumId",
                        column: x => x.AlbumsAlbumId,
                        principalTable: "Albums",
                        principalColumn: "AlbumId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumArticle_Articles_ArticlesArticleId",
                        column: x => x.ArticlesArticleId,
                        principalTable: "Articles",
                        principalColumn: "ArticleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArticleTags",
                columns: table => new
                {
                    ArticleTagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArticleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleTags", x => x.ArticleTagId);
                    table.ForeignKey(
                        name: "FK_ArticleTags_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "ArticleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    CommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Publish_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Edit_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArticleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_Comments_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "ArticleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Historys",
                columns: table => new
                {
                    HistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Event = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArticleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Historys", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_Historys_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "ArticleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumArticle_ArticlesArticleId",
                table: "AlbumArticle",
                column: "ArticlesArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_Name",
                table: "Albums",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Articles_PhotoId",
                table: "Articles",
                column: "PhotoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleTags_ArticleId",
                table: "ArticleTags",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_CameraBodys_Model_name_Maker",
                table: "CameraBodys",
                columns: new[] { "Model_name", "Maker" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ArticleId",
                table: "Comments",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_Historys_ArticleId",
                table: "Historys",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_Lens_Model_name_Maker",
                table: "Lens",
                columns: new[] { "Model_name", "Maker" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Photos_Export_datetime",
                table: "Photos",
                column: "Export_datetime");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_File_path",
                table: "Photos",
                column: "File_path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Photos_Model_Maker",
                table: "Photos",
                columns: new[] { "Model", "Maker" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlbumArticle");

            migrationBuilder.DropTable(
                name: "ArticleTags");

            migrationBuilder.DropTable(
                name: "CameraBodys");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Historys");

            migrationBuilder.DropTable(
                name: "Lens");

            migrationBuilder.DropTable(
                name: "Albums");

            migrationBuilder.DropTable(
                name: "Articles");

            migrationBuilder.DropTable(
                name: "Photos");
        }
    }
}
