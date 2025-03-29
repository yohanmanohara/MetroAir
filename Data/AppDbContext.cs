using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserRoles.Models;
using MetroAir.Models;

namespace UserRoles.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<AirQualityHistory> AirQualityHistory { get; set; }
        public DbSet<SimulationSettings> SimulationSettings { get; set; }
        public DbSet<AqiThresholdSettings> AqiThresholdSettings { get; set; }
        public DbSet<AqiAlert> AqiAlerts { get; set; }
        public DbSet<AlertNotification> AlertNotifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Simulation Settings
            builder.Entity<SimulationSettings>()
                .HasData(new SimulationSettings { Id = 1 });

            // Air Quality History relationship
            builder.Entity<AirQualityHistory>()
                .HasOne(h => h.Sensor)
                .WithMany(s => s.HistoricalData)
                .HasForeignKey(h => h.SensorId);

            // AQI Threshold Settings
            builder.Entity<AqiThresholdSettings>().HasData(
                new AqiThresholdSettings
                {
                    Id = 1,
                    GoodThreshold = 50,
                    ModerateThreshold = 100,
                    UnhealthySensitiveThreshold = 150,
                    UnhealthyThreshold = 200,
                    VeryUnhealthyThreshold = 300,
                    HazardousThreshold = 500
                }
            );

            // Configure AQI Threshold as single row table
            builder.Entity<AqiThresholdSettings>()
                .HasKey(a => a.Id);

            builder.Entity<AqiThresholdSettings>()
                .Property(a => a.Id)
                .ValueGeneratedNever(); // Prevent auto-generation of ID

            // Configure AqiAlerts relationship
            builder.Entity<AqiAlert>()
                .HasOne(a => a.Sensor)
                .WithMany()
                .HasForeignKey(a => a.SensorId);

            // Configure AlertNotifications
            builder.Entity<AlertNotification>()
                .Property(a => a.CreatedAt)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}