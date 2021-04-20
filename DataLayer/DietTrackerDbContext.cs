
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
                .HasMany(user => user.UserFuelings)
                .WithOne(userFueling => userFueling.User)
                .HasForeignKey(userFueling => userFueling.UserId);

            modelBuilder.Entity<User>()
                .HasMany(user => user.UserMeals)
                .WithOne(userMeal => userMeal.User)
                .HasForeignKey(userMeal => userMeal.UserId);

            modelBuilder.Entity<User>()
                .HasMany(user => user.UserPlans)
                .WithOne(userPlan => userPlan.User)
                .HasForeignKey(userPlan => userPlan.UserId);

            modelBuilder.Entity<UserDay>()
                .HasKey(userDay => new { userDay.UserId, userDay.Day });

            modelBuilder.Entity<UserPlan>()
                .HasKey(userPlan => new { userPlan.UserId, userPlan.PlanId });

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