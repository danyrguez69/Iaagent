using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IaAgent.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Listings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "TEXT", nullable: true),
                    MercadoLibreListingId = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<string>(type: "TEXT", nullable: false),
                    PriceClp = table.Column<decimal>(type: "TEXT", nullable: false),
                    Stock = table.Column<int>(type: "INTEGER", nullable: false),
                    ConditionType = table.Column<string>(type: "TEXT", nullable: false),
                    ListingType = table.Column<string>(type: "TEXT", nullable: false),
                    ImageUrls = table.Column<string>(type: "TEXT", nullable: false),
                    Attributes = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalSales = table.Column<int>(type: "INTEGER", nullable: false),
                    Revenue = table.Column<decimal>(type: "TEXT", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Listings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Opportunities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MercadoLibreProductId = table.Column<string>(type: "TEXT", nullable: false),
                    ProductTitle = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<string>(type: "TEXT", nullable: false),
                    CurrentChileanPriceClp = table.Column<decimal>(type: "TEXT", nullable: false),
                    DemandScore = table.Column<decimal>(type: "TEXT", nullable: false),
                    SoldLast30Days = table.Column<int>(type: "INTEGER", nullable: false),
                    ActiveListingsCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Keywords = table.Column<string>(type: "TEXT", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opportunities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChineseProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SupplierName = table.Column<string>(type: "TEXT", nullable: false),
                    SupplierContact = table.Column<string>(type: "TEXT", nullable: false),
                    ProductUrl = table.Column<string>(type: "TEXT", nullable: false),
                    QuantityOrdered = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitCostUsd = table.Column<decimal>(type: "TEXT", nullable: false),
                    TotalCostUsd = table.Column<decimal>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ExternalOrderId = table.Column<string>(type: "TEXT", nullable: false),
                    TrackingNumber = table.Column<string>(type: "TEXT", nullable: false),
                    OrderPlacedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstimatedArrival = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActualArrival = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArbitrageCalculations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChineseProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductCostUsd = table.Column<decimal>(type: "TEXT", nullable: false),
                    ShippingToChileCostUsd = table.Column<decimal>(type: "TEXT", nullable: false),
                    CifValueUsd = table.Column<decimal>(type: "TEXT", nullable: false),
                    CustomsDutyUsd = table.Column<decimal>(type: "TEXT", nullable: false),
                    IvaUsd = table.Column<decimal>(type: "TEXT", nullable: false),
                    MercadoLibreFeeUsd = table.Column<decimal>(type: "TEXT", nullable: false),
                    TotalCostUsd = table.Column<decimal>(type: "TEXT", nullable: false),
                    UsdToClpRate = table.Column<decimal>(type: "TEXT", nullable: false),
                    TargetSellingPriceClp = table.Column<decimal>(type: "TEXT", nullable: false),
                    TargetSellingPriceUsd = table.Column<decimal>(type: "TEXT", nullable: false),
                    GrossProfitUsd = table.Column<decimal>(type: "TEXT", nullable: false),
                    GrossMarginPct = table.Column<decimal>(type: "TEXT", nullable: false),
                    NetProfitUsd = table.Column<decimal>(type: "TEXT", nullable: false),
                    NetMarginPct = table.Column<decimal>(type: "TEXT", nullable: false),
                    Roi = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsProfitable = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rationale = table.Column<string>(type: "TEXT", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArbitrageCalculations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArbitrageCalculations_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChineseProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourcePlatform = table.Column<string>(type: "TEXT", nullable: false),
                    ExternalProductId = table.Column<string>(type: "TEXT", nullable: false),
                    ProductUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    PriceUsd = table.Column<decimal>(type: "TEXT", nullable: false),
                    MinOrderQuantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    ShippingCostUsd = table.Column<decimal>(type: "TEXT", nullable: false),
                    EstimatedShippingDays = table.Column<int>(type: "INTEGER", nullable: false),
                    SupplierRating = table.Column<decimal>(type: "TEXT", nullable: false),
                    OrderCount = table.Column<int>(type: "INTEGER", nullable: false),
                    SupplierName = table.Column<string>(type: "TEXT", nullable: false),
                    ImageUrls = table.Column<string>(type: "TEXT", nullable: false),
                    Specifications = table.Column<string>(type: "TEXT", nullable: false),
                    FoundAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChineseProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChineseProducts_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArbitrageCalculations_IsProfitable",
                table: "ArbitrageCalculations",
                column: "IsProfitable");

            migrationBuilder.CreateIndex(
                name: "IX_ArbitrageCalculations_OpportunityId",
                table: "ArbitrageCalculations",
                column: "OpportunityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChineseProducts_OpportunityId_ExternalProductId_SourcePlatform",
                table: "ChineseProducts",
                columns: new[] { "OpportunityId", "ExternalProductId", "SourcePlatform" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Listings_MercadoLibreListingId",
                table: "Listings",
                column: "MercadoLibreListingId");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_Status",
                table: "Listings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_DemandScore",
                table: "Opportunities",
                column: "DemandScore");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_MercadoLibreProductId",
                table: "Opportunities",
                column: "MercadoLibreProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_Status",
                table: "Opportunities",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_OpportunityId",
                table: "PurchaseOrders",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_Status",
                table: "PurchaseOrders",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArbitrageCalculations");

            migrationBuilder.DropTable(
                name: "ChineseProducts");

            migrationBuilder.DropTable(
                name: "Listings");

            migrationBuilder.DropTable(
                name: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "Opportunities");
        }
    }
}
