using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PlaylistManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedSongs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Songs",
                columns: new[] { "Id", "Album", "Artist", "ContentType", "Duration", "FileExtension", "FileName", "FilePath", "FileSize", "Genre", "Title", "UploadedAt" },
                values: new object[,]
                {
                    { 1, "After Hours", "The Weeknd", "audio/mpeg", new TimeSpan(0, 0, 3, 20, 0), ".mp3", "1.mp3", "uploads/songs/1.mp3", 5400000L, "Synth-pop", "Blinding Lights", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "÷ (Divide)", "Ed Sheeran", "audio/mpeg", new TimeSpan(0, 0, 3, 53, 0), ".mp3", "2.mp3", "uploads/songs/2.mp3", 6100000L, "Pop", "Shape of You", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "A Night at the Opera", "Queen", "audio/mpeg", new TimeSpan(0, 0, 5, 55, 0), ".mp3", "3.mp3", "uploads/songs/3.mp3", 8800000L, "Rock", "Bohemian Rhapsody", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "Future Nostalgia", "Dua Lipa", "audio/mpeg", new TimeSpan(0, 0, 3, 23, 0), ".mp3", "4.mp3", "uploads/songs/4.mp3", 5600000L, "Disco-pop", "Levitating", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, "Led Zeppelin IV", "Led Zeppelin", "audio/mpeg", new TimeSpan(0, 0, 8, 2, 0), ".mp3", "5.mp3", "uploads/songs/5.mp3", 12500000L, "Rock", "Stairway to Heaven", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
