﻿// <auto-generated />
using Eliza.Database.DatabaseContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Eliza.Server.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    [Migration("20200505211758_RequestableRoleAdded")]
    partial class RequestableRoleAdded
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.3");

            modelBuilder.Entity("Eliza.Database.Models.RequestableRole", b =>
                {
                    b.Property<ulong>("RoleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("RoleName")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(32);

                    b.HasKey("RoleId");

                    b.ToTable("RequestableRoles");
                });

            modelBuilder.Entity("Eliza.Database.Models.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TagName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("TagName")
                        .IsUnique();

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("Eliza.Database.Models.User", b =>
                {
                    b.Property<ulong>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Eliza.Database.Models.UserBlacklistedTag", b =>
                {
                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TagId")
                        .HasColumnType("INTEGER");

                    b.HasKey("UserId", "TagId");

                    b.HasIndex("TagId");

                    b.ToTable("UserBlacklistedTags");
                });

            modelBuilder.Entity("Eliza.Database.Models.UserSubcribedTag", b =>
                {
                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TagId")
                        .HasColumnType("INTEGER");

                    b.HasKey("UserId", "TagId");

                    b.HasIndex("TagId");

                    b.ToTable("UserSubscribedTags");
                });

            modelBuilder.Entity("Eliza.Database.Models.UserBlacklistedTag", b =>
                {
                    b.HasOne("Eliza.Database.Models.Tag", "Tag")
                        .WithMany("Blacklisters")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Eliza.Database.Models.User", "User")
                        .WithMany("BlacklistedTags")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Eliza.Database.Models.UserSubcribedTag", b =>
                {
                    b.HasOne("Eliza.Database.Models.Tag", "Tag")
                        .WithMany("Subscribers")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Eliza.Database.Models.User", "User")
                        .WithMany("SubscribedTags")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
