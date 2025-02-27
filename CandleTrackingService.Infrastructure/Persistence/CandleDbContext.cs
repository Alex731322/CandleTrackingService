using CandleTrackingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CandleTrackingService.Infrastructure.Persistence
{
    public class CandleDbContext : DbContext
    {
        public CandleDbContext(DbContextOptions<CandleDbContext> options)
        : base(options)
        {
        }

        public DbSet<Candle> Candles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Candle>(entity =>
            {
                entity.ToTable("candles");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id");

                entity.Property(e => e.Symbol)
                    .IsRequired()
                    .HasColumnName("symbol")
                    .HasMaxLength(20);

                entity.Property(e => e.TimeStamp)
                    .IsRequired()
                    .HasColumnName("timestamp")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.Open)
                    .HasColumnName("open")
                    .HasColumnType("decimal(18,8)");

                entity.Property(e => e.High)
                    .HasColumnName("high")
                    .HasColumnType("decimal(18,8)");

                entity.Property(e => e.Low)
                    .HasColumnName("low")
                    .HasColumnType("decimal(18,8)");

                entity.Property(e => e.Close)
                    .HasColumnName("close")
                    .HasColumnType("decimal(18,8)");

                entity.Property(e => e.Volume)
                    .HasColumnName("volume")
                    .HasColumnType("decimal(18,8)");

                entity.Property(e => e.TimeFrame)
                    .IsRequired()
                    .HasColumnName("time_frame");

                // Создаем уникальный индекс по символу, таймфрейму и временной метке
                entity.HasIndex(e => new { e.Symbol, e.TimeFrame, e.TimeStamp })
                    .IsUnique();

                // Создаем индекс по временной метке для ускорения запросов
                entity.HasIndex(e => e.TimeStamp);
            });
        }
    }
}
