using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModelaFlow.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class DesignAndExport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "export_jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatternModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatternVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Format = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResultBytes = table.Column<byte[]>(type: "bytea", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_export_jobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pattern_models",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ReferenceCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BaseKind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pattern_models", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pattern_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatternModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    ParametersJson = table.Column<string>(type: "text", nullable: false),
                    GeometryJson = table.Column<string>(type: "text", nullable: true),
                    QualityIssuesJson = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pattern_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pattern_versions_pattern_models_PatternModelId",
                        column: x => x.PatternModelId,
                        principalTable: "pattern_models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "technical_sheets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatternModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialsNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ConstructionNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_technical_sheets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_technical_sheets_pattern_models_PatternModelId",
                        column: x => x.PatternModelId,
                        principalTable: "pattern_models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_export_jobs_TenantId",
                table: "export_jobs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_export_jobs_TenantId_PatternModelId",
                table: "export_jobs",
                columns: new[] { "TenantId", "PatternModelId" });

            migrationBuilder.CreateIndex(
                name: "IX_pattern_models_TenantId",
                table: "pattern_models",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_pattern_models_TenantId_UpdatedAt",
                table: "pattern_models",
                columns: new[] { "TenantId", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_pattern_versions_PatternModelId",
                table: "pattern_versions",
                column: "PatternModelId");

            migrationBuilder.CreateIndex(
                name: "IX_pattern_versions_TenantId",
                table: "pattern_versions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_pattern_versions_TenantId_PatternModelId_Version",
                table: "pattern_versions",
                columns: new[] { "TenantId", "PatternModelId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_technical_sheets_PatternModelId",
                table: "technical_sheets",
                column: "PatternModelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_technical_sheets_TenantId",
                table: "technical_sheets",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_technical_sheets_TenantId_PatternModelId",
                table: "technical_sheets",
                columns: new[] { "TenantId", "PatternModelId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "export_jobs");

            migrationBuilder.DropTable(
                name: "pattern_versions");

            migrationBuilder.DropTable(
                name: "technical_sheets");

            migrationBuilder.DropTable(
                name: "pattern_models");
        }
    }
}
