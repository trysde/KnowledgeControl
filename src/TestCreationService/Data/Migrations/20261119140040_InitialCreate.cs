using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TestCreationService.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "test_creation_schema");

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "test_creation_schema",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "moderation_reports",
                schema: "test_creation_schema",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    reporter_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    target_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    target_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Pending"),
                    moderator_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    moderator_comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_moderation_reports", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                schema: "test_creation_schema",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "questions",
                schema: "test_creation_schema",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    author_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    question_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    correct_text_answer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_public = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_blocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questions", x => x.id);
                    table.ForeignKey(
                        name: "FK_questions_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "test_creation_schema",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tests",
                schema: "test_creation_schema",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    author_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    time_limit_seconds = table.Column<int>(type: "integer", nullable: true),
                    max_attempts = table.Column<int>(type: "integer", nullable: true),
                    passing_score = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    starts_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ends_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_public = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_hidden = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Draft"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tests", x => x.id);
                    table.ForeignKey(
                        name: "FK_tests_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "test_creation_schema",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "answer_options",
                schema: "test_creation_schema",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    question_id = table.Column<long>(type: "bigint", nullable: false),
                    text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_correct = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_answer_options", x => x.id);
                    table.ForeignKey(
                        name: "FK_answer_options_questions_question_id",
                        column: x => x.question_id,
                        principalSchema: "test_creation_schema",
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "question_tags",
                schema: "test_creation_schema",
                columns: table => new
                {
                    question_id = table.Column<long>(type: "bigint", nullable: false),
                    tag_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_question_tags", x => new { x.question_id, x.tag_id });
                    table.ForeignKey(
                        name: "FK_question_tags_questions_question_id",
                        column: x => x.question_id,
                        principalSchema: "test_creation_schema",
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_question_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalSchema: "test_creation_schema",
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "test_questions",
                schema: "test_creation_schema",
                columns: table => new
                {
                    test_id = table.Column<long>(type: "bigint", nullable: false),
                    question_id = table.Column<long>(type: "bigint", nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    points = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    snapshot = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_questions", x => new { x.test_id, x.question_id });
                    table.ForeignKey(
                        name: "FK_test_questions_questions_question_id",
                        column: x => x.question_id,
                        principalSchema: "test_creation_schema",
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_test_questions_tests_test_id",
                        column: x => x.test_id,
                        principalSchema: "test_creation_schema",
                        principalTable: "tests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_answer_options_question_id_position",
                schema: "test_creation_schema",
                table: "answer_options",
                columns: new[] { "question_id", "position" });

            migrationBuilder.CreateIndex(
                name: "IX_answer_options_uid",
                schema: "test_creation_schema",
                table: "answer_options",
                column: "uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_name",
                schema: "test_creation_schema",
                table: "categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_uid",
                schema: "test_creation_schema",
                table: "categories",
                column: "uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_moderation_reports_status_created_at",
                schema: "test_creation_schema",
                table: "moderation_reports",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_moderation_reports_target_type_target_uid",
                schema: "test_creation_schema",
                table: "moderation_reports",
                columns: new[] { "target_type", "target_uid" });

            migrationBuilder.CreateIndex(
                name: "IX_moderation_reports_uid",
                schema: "test_creation_schema",
                table: "moderation_reports",
                column: "uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_question_tags_tag_id",
                schema: "test_creation_schema",
                table: "question_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_questions_author_uid",
                schema: "test_creation_schema",
                table: "questions",
                column: "author_uid");

            migrationBuilder.CreateIndex(
                name: "IX_questions_category_id_is_public_is_deleted_is_blocked",
                schema: "test_creation_schema",
                table: "questions",
                columns: new[] { "category_id", "is_public", "is_deleted", "is_blocked" });

            migrationBuilder.CreateIndex(
                name: "IX_questions_uid",
                schema: "test_creation_schema",
                table: "questions",
                column: "uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tags_name",
                schema: "test_creation_schema",
                table: "tags",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tags_uid",
                schema: "test_creation_schema",
                table: "tags",
                column: "uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_test_questions_question_id",
                schema: "test_creation_schema",
                table: "test_questions",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_test_questions_test_id_order_index",
                schema: "test_creation_schema",
                table: "test_questions",
                columns: new[] { "test_id", "order_index" });

            migrationBuilder.CreateIndex(
                name: "IX_tests_author_uid",
                schema: "test_creation_schema",
                table: "tests",
                column: "author_uid");

            migrationBuilder.CreateIndex(
                name: "IX_tests_category_id_status_is_public_is_hidden",
                schema: "test_creation_schema",
                table: "tests",
                columns: new[] { "category_id", "status", "is_public", "is_hidden" });

            migrationBuilder.CreateIndex(
                name: "IX_tests_uid",
                schema: "test_creation_schema",
                table: "tests",
                column: "uid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "answer_options",
                schema: "test_creation_schema");

            migrationBuilder.DropTable(
                name: "moderation_reports",
                schema: "test_creation_schema");

            migrationBuilder.DropTable(
                name: "question_tags",
                schema: "test_creation_schema");

            migrationBuilder.DropTable(
                name: "test_questions",
                schema: "test_creation_schema");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "test_creation_schema");

            migrationBuilder.DropTable(
                name: "questions",
                schema: "test_creation_schema");

            migrationBuilder.DropTable(
                name: "tests",
                schema: "test_creation_schema");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "test_creation_schema");
        }
    }
}
