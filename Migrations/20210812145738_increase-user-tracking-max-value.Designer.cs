﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using diet_tracker_api.DataLayer;

namespace diet_tracker_api.Migrations
{
    [DbContext(typeof(DietTrackerDbContext))]
    [Migration("20210812145738_increase-user-tracking-max-value")]
    partial class increaseusertrackingmaxvalue
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.8")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.Fueling", b =>
                {
                    b.Property<int>("FuelingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("FuelingId");

                    b.ToTable("Fueling");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.Plan", b =>
                {
                    b.Property<int>("PlanId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("FuelingCount")
                        .HasColumnType("int");

                    b.Property<int>("MealCount")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("PlanId");

                    b.ToTable("Plan");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.User", b =>
                {
                    b.Property<string>("UserId")
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<bool>("Autosave")
                        .HasColumnType("bit");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("EmailAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("WaterSize")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(8);

                    b.Property<int>("WaterTarget")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(64);

                    b.HasKey("UserId");

                    b.ToTable("User");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.UserDailyTrackingValue", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(250)");

                    b.Property<DateTime>("Day")
                        .HasColumnType("date");

                    b.Property<int>("UserTrackingValueId")
                        .HasColumnType("int");

                    b.Property<int>("Occurrence")
                        .HasColumnType("int");

                    b.Property<decimal>("Value")
                        .HasColumnType("decimal(32,2)");

                    b.Property<DateTime?>("When")
                        .HasColumnType("datetime2");

                    b.HasKey("UserId", "Day", "UserTrackingValueId", "Occurrence");

                    b.HasIndex("UserTrackingValueId");

                    b.ToTable("UserDailyTrackingValue");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.UserDay", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(250)");

                    b.Property<DateTime>("Day")
                        .HasColumnType("date");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Water")
                        .HasColumnType("int");

                    b.Property<decimal>("Weight")
                        .HasColumnType("decimal(5,2)");

                    b.HasKey("UserId", "Day");

                    b.ToTable("UserDay");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.UserFueling", b =>
                {
                    b.Property<int>("UserFuelingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Day")
                        .HasColumnType("date");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(250)");

                    b.Property<DateTime?>("When")
                        .HasColumnType("datetime2");

                    b.HasKey("UserFuelingId");

                    b.HasIndex("UserId", "Day");

                    b.ToTable("UserFueling");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.UserMeal", b =>
                {
                    b.Property<int>("UserMealId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Day")
                        .HasColumnType("date");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(250)");

                    b.Property<DateTime?>("When")
                        .HasColumnType("datetime2");

                    b.HasKey("UserMealId");

                    b.HasIndex("UserId", "Day");

                    b.ToTable("UserMeal");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.UserPlan", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(250)");

                    b.Property<int>("PlanId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Start")
                        .HasColumnType("datetime2");

                    b.HasKey("UserId", "PlanId", "Start");

                    b.HasIndex("PlanId");

                    b.ToTable("UserPlan");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.UserTracking", b =>
                {
                    b.Property<int>("UserTrackingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Disabled")
                        .HasColumnType("bit");

                    b.Property<int>("Occurrences")
                        .HasColumnType("int");

                    b.Property<int>("Order")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("UseTime")
                        .HasColumnType("bit");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(250)");

                    b.HasKey("UserTrackingId");

                    b.HasIndex("UserId");

                    b.ToTable("UserTracking");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.UserTrackingValue", b =>
                {
                    b.Property<int>("UserTrackingValueId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Disabled")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Order")
                        .HasColumnType("int");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserTrackingId")
                        .HasColumnType("int");

                    b.HasKey("UserTrackingValueId");

                    b.HasIndex("UserTrackingId");

                    b.ToTable("UserTrackingValue");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.UserTrackingValueMetadata", b =>
                {
                    b.Property<int>("UserTrackingValueId")
                        .HasColumnType("int");

                    b.Property<string>("Key")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserTrackingValueId", "Key");

                    b.ToTable("UserTrackingValueMetadata");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.Victory", b =>
                {
                    b.Property<int>("VictoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(250)");

                    b.Property<DateTime?>("When")
                        .HasColumnType("datetime2");

                    b.HasKey("VictoryId");

                    b.HasIndex("UserId");

                    b.ToTable("Victory");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.UserDailyTrackingValue", b =>
                {
                    b.HasOne("diet_tracker_api.DataLayer.Models.UserTrackingValue", "TrackingValue")
                        .WithMany("DailyTrackingValues")
                        .HasForeignKey("UserTrackingValueId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("diet_tracker_api.DataLayer.Models.UserDay", "UserDay")
                        .WithMany("TrackingValues")
                        .HasForeignKey("UserId", "Day")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TrackingValue");

                    b.Navigation("UserDay");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.UserDay", b =>
                {
                    b.HasOne("diet_tracker_api.DataLayer.Models.User", "User")
                        .WithMany("UserDays")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.UserFueling", b =>
                {
                    b.HasOne("diet_tracker_api.DataLayer.Models.User", null)
                        .WithMany("UserFuelings")
                        .HasForeignKey("UserId");

                    b.HasOne("diet_tracker_api.DataLayer.Models.UserDay", "UserDay")
                        .WithMany("Fuelings")
                        .HasForeignKey("UserId", "Day");

                    b.Navigation("UserDay");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.UserMeal", b =>
                {
                    b.HasOne("diet_tracker_api.DataLayer.Models.User", null)
                        .WithMany("UserMeals")
                        .HasForeignKey("UserId");

                    b.HasOne("diet_tracker_api.DataLayer.Models.UserDay", "UserDay")
                        .WithMany("Meals")
                        .HasForeignKey("UserId", "Day");

                    b.Navigation("UserDay");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.UserPlan", b =>
                {
                    b.HasOne("diet_tracker_api.DataLayer.Models.Plan", "Plan")
                        .WithMany("UserPlans")
                        .HasForeignKey("PlanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("diet_tracker_api.DataLayer.Models.User", "User")
                        .WithMany("UserPlans")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Plan");

                    b.Navigation("User");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.UserTracking", b =>
                {
                    b.HasOne("diet_tracker_api.DataLayer.Models.User", "User")
                        .WithMany("UserTrackings")
                        .HasForeignKey("UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.UserTrackingValue", b =>
                {
                    b.HasOne("diet_tracker_api.DataLayer.Models.UserTracking", "Tracking")
                        .WithMany("Values")
                        .HasForeignKey("UserTrackingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Tracking");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.UserTrackingValueMetadata", b =>
                {
                    b.HasOne("diet_tracker_api.DataLayer.Models.UserTrackingValue", "UserTrackingValue")
                        .WithMany("Metadata")
                        .HasForeignKey("UserTrackingValueId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserTrackingValue");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.Victory", b =>
                {
                    b.HasOne("diet_tracker_api.DataLayer.Models.User", "User")
                        .WithMany("Victories")
                        .HasForeignKey("UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.Plan", b =>
                {
                    b.Navigation("UserPlans");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.User", b =>
                {
                    b.Navigation("UserDays");

                    b.Navigation("UserFuelings");

                    b.Navigation("UserMeals");

                    b.Navigation("UserPlans");

                    b.Navigation("UserTrackings");

                    b.Navigation("Victories");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.UserDay", b =>
                {
                    b.Navigation("Fuelings");

                    b.Navigation("Meals");

                    b.Navigation("TrackingValues");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.UserTracking", b =>
                {
                    b.Navigation("Values");
                });

            modelBuilder.Entity("diet_tracker_api.DataLayer.Models.UserTrackingValue", b =>
                {
                    b.Navigation("DailyTrackingValues");

                    b.Navigation("Metadata");
                });
#pragma warning restore 612, 618
        }
    }
}
