using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TestTakingService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "test_taking_schema");

            migrationBuilder.CreateTable(
                name: "attempts",
                schema: "test_taking_schema",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    test_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "IN_PROGRESS"),
                    snapshot = table.Column<string>(type: "jsonb", nullable: false),
                    snapshot_version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    finished_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attempts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "answers",
                schema: "test_taking_schema",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    attempt_id = table.Column<long>(type: "bigint", nullable: false),
                    question_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    selected_option_uids = table.Column<string>(type: "jsonb", nullable: false),
                    text_answer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_correct = table.Column<bool>(type: "boolean", nullable: true),
                    points_earned = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    answered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_answers", x => x.id);
                    table.ForeignKey(
                        name: "FK_answers_attempts_attempt_id",
                        column: x => x.attempt_id,
                        principalSchema: "test_taking_schema",
                        principalTable: "attempts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "results",
                schema: "test_taking_schema",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_id = table.Column<long>(type: "bigint", nullable: false),
                    test_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    total_score = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    max_score = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    passed = table.Column<bool>(type: "boolean", nullable: false),
                    calculated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_results", x => x.id);
                    table.ForeignKey(
                        name: "FK_results_attempts_attempt_id",
                        column: x => x.attempt_id,
                        principalSchema: "test_taking_schema",
                        principalTable: "attempts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_answers_attempt_id_question_uid",
                schema: "test_taking_schema",
                table: "answers",
                columns: new[] { "attempt_id", "question_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_attempts_status",
                schema: "test_taking_schema",
                table: "attempts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_attempts_uid",
                schema: "test_taking_schema",
                table: "attempts",
                column: "uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_attempts_user_uid_test_uid",
                schema: "test_taking_schema",
                table: "attempts",
                columns: new[] { "user_uid", "test_uid" });

            migrationBuilder.CreateIndex(
                name: "IX_results_attempt_id",
                schema: "test_taking_schema",
                table: "results",
                column: "attempt_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_results_uid",
                schema: "test_taking_schema",
                table: "results",
                column: "uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_results_user_uid_test_uid",
                schema: "test_taking_schema",
                table: "results",
                columns: new[] { "user_uid", "test_uid" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "answers",
                schema: "test_taking_schema");

            migrationBuilder.DropTable(
                name: "results",
                schema: "test_taking_schema");

            migrationBuilder.DropTable(
                name: "attempts",
                schema: "test_taking_schema");
        }
    }
}
