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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<SimulationSettings>()
                .HasData(new SimulationSettings { Id = 1 });

            builder.Entity<AirQualityHistory>()
                .HasOne(h => h.Sensor)
                .WithMany(s => s.HistoricalData)
                .HasForeignKey(h => h.SensorId);
        }

    }
}
