using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroAir.Migrations
{
    /// <inheritdoc />
    public partial class AddAqiThresholdSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AqiThresholdSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    GoodThreshold = table.Column<int>(type: "int", nullable: false),
                    ModerateThreshold = table.Column<int>(type: "int", nullable: false),
                    UnhealthySensitiveThreshold = table.Column<int>(type: "int", nullable: false),
                    UnhealthyThreshold = table.Column<int>(type: "int", nullable: false),
                    VeryUnhealthyThreshold = table.Column<int>(type: "int", nullable: false),
                    HazardousThreshold = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AqiThresholdSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sensors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AQI = table.Column<int>(type: "int", nullable: false),
                    AirQualityStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sensors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SimulationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UpdateFrequencyMinutes = table.Column<int>(type: "int", nullable: false),
                    IsRunning = table.Column<bool>(type: "bit", nullable: false),
                    BasePM2_5 = table.Column<double>(type: "float", nullable: false),
                    BasePM10 = table.Column<double>(type: "float", nullable: false),
                    BaseNO2 = table.Column<double>(type: "float", nullable: false),
                    BaseSO2 = table.Column<double>(type: "float", nullable: false),
                    BaseCO = table.Column<double>(type: "float", nullable: false),
                    BaseO3 = table.Column<double>(type: "float", nullable: false),
                    DailyVariationFactor = table.Column<double>(type: "float", nullable: false),
                    RandomVariationFactor = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulationSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AirQualityHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SensorId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AQI = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PM2_5 = table.Column<double>(type: "float", nullable: false),
                    PM10 = table.Column<double>(type: "float", nullable: false),
                    NO2 = table.Column<double>(type: "float", nullable: false),
                    SO2 = table.Column<double>(type: "float", nullable: false),
                    CO = table.Column<double>(type: "float", nullable: false),
                    O3 = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirQualityHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AirQualityHistory_Sensors_SensorId",
                        column: x => x.SensorId,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AqiThresholdSettings",
                columns: new[] { "Id", "GoodThreshold", "HazardousThreshold", "ModerateThreshold", "UnhealthySensitiveThreshold", "UnhealthyThreshold", "VeryUnhealthyThreshold" },
                values: new object[] { 1, 50, 500, 100, 150, 200, 300 });

            migrationBuilder.InsertData(
                table: "SimulationSettings",
                columns: new[] { "Id", "BaseCO", "BaseNO2", "BaseO3", "BasePM10", "BasePM2_5", "BaseSO2", "DailyVariationFactor", "IsRunning", "RandomVariationFactor", "UpdateFrequencyMinutes" },
                values: new object[] { 1, 0.5, 15.0, 20.0, 45.0, 30.0, 5.0, 0.29999999999999999, true, 0.20000000000000001, 5 });

            migrationBuilder.CreateIndex(
                name: "IX_AirQualityHistory_SensorId",
                table: "AirQualityHistory",
                column: "SensorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AirQualityHistory");

            migrationBuilder.DropTable(
                name: "AqiThresholdSettings");

            migrationBuilder.DropTable(
                name: "SimulationSettings");

            migrationBuilder.DropTable(
                name: "Sensors");
        }
    }
}
