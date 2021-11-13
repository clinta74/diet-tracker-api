
using System;
using System.Diagnostics.CodeAnalysis;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace diet_tracker_api.DataLayer
{
    public class DietTrackerDbContext : DbContext
    {
        public DietTrackerDbContext([NotNullAttribute] DbContextOptions options) : base(options)
        {
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder builder)
        {
            builder.Properties<DateOnly>()
                .HaveConversion<DateOnlyConverter>()
                .HaveColumnType("date");

            builder.Properties<DateOnly?>()
                .HaveConversion<NullableDateOnlyConverter>()
                .HaveColumnType("date");
        }

        public DbSet<Fueling> Fuelings { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserDay> UserDays { get; set; }
        public DbSet<UserFueling> UserFuelings { get; set; }
        public DbSet<UserMeal> UserMeals { get; set; }
        public DbSet<UserPlan> UserPlans { get; set; }
        public DbSet<UserDailyTrackingValue> UserDailyTrackingValues { get; set; }
        public DbSet<UserTracking> UserTrackings { get; set; }
        public DbSet<UserTrackingValue> UserTrackingValues { get; set; }
        public DbSet<UserTrackingValueMetadata> UserTrackingValueMetadata { get; set; }
        public DbSet<Victory> Victories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Plan>()
                .HasMany(plan => plan.UserPlans)
                .WithOne(userPlan => userPlan.Plan)
                .HasForeignKey(userPlan => userPlan.PlanId);

            modelBuilder.Entity<User>()
                .HasMany(user => user.UserDays)
                .WithOne(userDay => userDay.User)
                .HasForeignKey(userDay => userDay.UserId);

            modelBuilder.Entity<User>()
                .HasMany(user => user.UserPlans)
                .WithOne(userPlan => userPlan.User)
                .HasForeignKey(userPlan => userPlan.UserId);

            modelBuilder.Entity<User>()
                .HasMany(user => user.UserTrackings)
                .WithOne(userTracking => userTracking.User)
                .HasForeignKey(userTracking => userTracking.UserId);

            modelBuilder.Entity<User>()
                .HasMany(user => user.Victories)
                .WithOne(victory => victory.User)
                .HasForeignKey(victory => victory.UserId);

            modelBuilder.Entity<User>()
                .Property(p => p.WaterSize)
                .HasDefaultValue(8);

            modelBuilder.Entity<User>()
                .Property(p => p.WaterTarget)
                .HasDefaultValue(64);

            modelBuilder.Entity<UserDailyTrackingValue>()
                .HasKey(userDailyTracking => new
                {
                    userDailyTracking.UserId,
                    userDailyTracking.Day,
                    userDailyTracking.UserTrackingValueId,
                    userDailyTracking.Occurrence
                });

            modelBuilder.Entity<UserDay>()
                .HasKey(userDay => new { userDay.UserId, userDay.Day });

            modelBuilder.Entity<UserDay>()
                .HasMany(userDay => userDay.Fuelings)
                .WithOne(userFueling => userFueling.UserDay)
                .HasForeignKey(userFueling => new { userFueling.UserId, userFueling.Day });

            modelBuilder.Entity<UserDay>()
                .HasMany(user => user.Meals)
                .WithOne(userMeal => userMeal.UserDay)
                .HasForeignKey(userMeal => new { userMeal.UserId, userMeal.Day });

            modelBuilder.Entity<UserDay>()
                .HasMany(user => user.TrackingValues)
                .WithOne(userTrackingValue => userTrackingValue.UserDay)
                .HasForeignKey(userTrackingValue => new { userTrackingValue.UserId, userTrackingValue.Day });

            modelBuilder.Entity<UserPlan>()
                .HasKey(userPlan => new { userPlan.UserId, userPlan.PlanId, userPlan.Start });

            modelBuilder.Entity<UserTracking>()
                .HasMany(userTracking => userTracking.Values)
                .WithOne(userTrackingValue => userTrackingValue.Tracking)
                .HasForeignKey(userTrackingValue => userTrackingValue.UserTrackingId);

            modelBuilder.Entity<UserTrackingValue>()
                .Property(v => v.Type)
                .HasConversion<string>();

            modelBuilder.Entity<UserTrackingValue>()
                .HasMany(v => v.DailyTrackingValues)
                .WithOne(v => v.TrackingValue)
                .HasForeignKey(v => v.UserTrackingValueId);

            modelBuilder.Entity<UserTrackingValue>()
                .HasMany(userTrackingValue => userTrackingValue.Metadata)
                .WithOne(userTrackingValueMetadata => userTrackingValueMetadata.UserTrackingValue)
                .HasForeignKey(userTrackingValue => userTrackingValue.UserTrackingValueId);

            modelBuilder.Entity<UserTrackingValueMetadata>()
                .HasKey(userTrackingValueMetadata => new { userTrackingValueMetadata.UserTrackingValueId, userTrackingValueMetadata.Key });

            modelBuilder.Entity<Victory>()
                .Property(v => v.Type)
                .HasConversion<string>();

            // Configure Code First to ignore PluralizingTableName convention
            // If you keep this convention then the generated tables will have pluralized names.
            modelBuilder.RemovePluralizingTableNameConvention();


        }
    }

    public static class ModelBuilderExtensions
    {
        public static void RemovePluralizingTableNameConvention(this ModelBuilder modelBuilder)
        {
            foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.DisplayName());
            }
        }
    }

    /// <summary>
    /// Converts <see cref="DateOnly" /> to <see cref="DateTime"/> and vice versa.
    /// </summary>
    public class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
    {
        /// <summary>
        /// Creates a new instance of this converter.
        /// </summary>
        public DateOnlyConverter() : base(
                d => d.ToDateTime(TimeOnly.MinValue),
                d => DateOnly.FromDateTime(d))
        { }
    }

    /// <summary>
    /// Converts <see cref="DateOnly?" /> to <see cref="DateTime?"/> and vice versa.
    /// </summary>
    public class NullableDateOnlyConverter : ValueConverter<DateOnly?, DateTime?>
    {
        /// <summary>
        /// Creates a new instance of this converter.
        /// </summary>
        public NullableDateOnlyConverter() : base(
            d => d == null
                ? null
                : new DateTime?(d.Value.ToDateTime(TimeOnly.MinValue)),
            d => d == null
                ? null
                : new DateOnly?(DateOnly.FromDateTime(d.Value)))
        { }
    }
}