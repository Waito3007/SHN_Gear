using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SHNGearBE.Migrations
{
    /// <inheritdoc />
    public partial class AddWebhookAndRefundTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RefundRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    RefundTransactionId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CaptureTransactionId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    RefundAmountUsd = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCapturedAmountUsd = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RefundReason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    RefundedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefundRecords_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebhookEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    EventId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    EventType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CaptureId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebhookEvents_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefundRecords_OrderId",
                table: "RefundRecords",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundRecords_RefundTransactionId",
                table: "RefundRecords",
                column: "RefundTransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebhookEvents_OrderId",
                table: "WebhookEvents",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookEvents_Provider_EventId",
                table: "WebhookEvents",
                columns: new[] { "Provider", "EventId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefundRecords");

            migrationBuilder.DropTable(
                name: "WebhookEvents");
        }
    }
}
