using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

#nullable disable

namespace SearchService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "search_schema");

            migrationBuilder.CreateTable(
                name: "processed_attempts",
                schema: "search_schema",
                columns: table => new
                {
                    attempt_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_attempts", x => x.attempt_uid);
                });

            migrationBuilder.CreateTable(
                name: "search_index",
                schema: "search_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    test_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    author_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    author_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    subject = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_public = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    question_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: false)
                        .Annotation("Npgsql:TsVectorConfig", "russian")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "title", "description", "subject", "author_name" }),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_index", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "test_statistics",
                schema: "search_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    test_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    total_attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    passed_attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    pass_rate = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0.00m),
                    avg_score = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0.00m),
                    avg_duration_sec = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_attempt_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_statistics", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_search_index_author_uid",
                schema: "search_schema",
                table: "search_index",
                column: "author_uid");

            migrationBuilder.CreateIndex(
                name: "IX_search_index_is_public",
                schema: "search_schema",
                table: "search_index",
                column: "is_public");

            migrationBuilder.CreateIndex(
                name: "IX_search_index_search_vector",
                schema: "search_schema",
                table: "search_index",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_search_index_subject",
                schema: "search_schema",
                table: "search_index",
                column: "subject");

            migrationBuilder.CreateIndex(
                name: "IX_search_index_test_uid",
                schema: "search_schema",
                table: "search_index",
                column: "test_uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_test_statistics_test_uid",
                schema: "search_schema",
                table: "test_statistics",
                column: "test_uid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "processed_attempts",
                schema: "search_schema");

            migrationBuilder.DropTable(
                name: "search_index",
                schema: "search_schema");

            migrationBuilder.DropTable(
                name: "test_statistics",
                schema: "search_schema");
        }
    }
}
