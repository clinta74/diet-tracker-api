
using System.Diagnostics.CodeAnalysis;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace diet_tracker_api.DataLayer
{
    public class DietTrackerDbContext : DbContext
    {
        public DietTrackerDbContext([NotNullAttribute] DbContextOptions options) : base(options)
        {
        }

        public DbSet<Fueling> Fuelings { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserDay> UserDays { get; set; }
        public DbSet<UserFueling> UserFuelings { get; set; }
        public DbSet<UserMeal> UserMeals { get; set; }
        public DbSet<UserPlan> UserPlans { get; set; }
        public DbSet<UserDailyTracking> UserDailyTrackings { get; set; }
        public DbSet<UserTracking> UserTrackings { get; set; }
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

            modelBuilder.Entity<UserPlan>()
                .HasKey(userPlan => new { userPlan.UserId, userPlan.PlanId, userPlan.Start });

            modelBuilder.Entity<UserDay>()
                .HasMany(user => user.Trackings)
                .WithOne(userTracking => userTracking.UserDay)
                .HasForeignKey(userTracking => new { userTracking.UserId, userTracking.Day });

            modelBuilder.Entity<UserDailyTracking>()
                .Property(v => v.Type)
                .HasConversion<string>();
            
            modelBuilder.Entity<UserTracking>()
                .Property(v => v.Type)
                .HasConversion<string>();

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
}