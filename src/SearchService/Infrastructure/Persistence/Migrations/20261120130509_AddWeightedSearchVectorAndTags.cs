using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace SearchService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWeightedSearchVectorAndTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_search_index_search_vector",
                schema: "search_schema",
                table: "search_index");

            migrationBuilder.DropColumn(
                name: "search_vector",
                schema: "search_schema",
                table: "search_index");

            migrationBuilder.AddColumn<string>(
                name: "tags",
                schema: "search_schema",
                table: "search_index",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "search_vector",
                schema: "search_schema",
                table: "search_index",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "setweight(to_tsvector('russian', coalesce(title, '')), 'A') || setweight(to_tsvector('russian', coalesce(description, '')), 'B') || setweight(to_tsvector('russian', coalesce(tags, '')), 'B') || setweight(to_tsvector('russian', coalesce(subject, '')), 'C') || setweight(to_tsvector('russian', coalesce(author_name, '')), 'C')",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_search_index_search_vector",
                schema: "search_schema",
                table: "search_index",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_search_index_search_vector",
                schema: "search_schema",
                table: "search_index");

            migrationBuilder.DropColumn(
                name: "search_vector",
                schema: "search_schema",
                table: "search_index");

            migrationBuilder.DropColumn(
                name: "tags",
                schema: "search_schema",
                table: "search_index");

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "search_vector",
                schema: "search_schema",
                table: "search_index",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('russian', coalesce(title, '') || ' ' || coalesce(description, '') || ' ' || coalesce(subject, '') || ' ' || coalesce(author_name, ''))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_search_index_search_vector",
                schema: "search_schema",
                table: "search_index",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }
    }
}