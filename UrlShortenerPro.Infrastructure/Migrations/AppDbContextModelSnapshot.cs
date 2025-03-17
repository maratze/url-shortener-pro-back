﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using UrlShortenerPro.Infrastructure.Data;

#nullable disable

namespace UrlShortenerPro.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("UrlShortenerPro.Infrastructure.Models.ClickData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Browser")
                        .HasColumnType("text");

                    b.Property<string>("City")
                        .HasColumnType("text");

                    b.Property<DateTime>("ClickedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Country")
                        .HasColumnType("text");

                    b.Property<string>("DeviceType")
                        .HasColumnType("text");

                    b.Property<string>("IpAddress")
                        .HasColumnType("text");

                    b.Property<string>("ReferrerUrl")
                        .HasColumnType("text");

                    b.Property<int>("UrlId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("UrlId");

                    b.ToTable("ClickData");
                });

            modelBuilder.Entity("UrlShortenerPro.Infrastructure.Models.ClientUsage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClientId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("LastRequestAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("UsedRequests")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ClientId")
                        .IsUnique();

                    b.ToTable("ClientUsages");
                });

            modelBuilder.Entity("UrlShortenerPro.Infrastructure.Models.Url", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CustomCode")
                        .HasColumnType("text");

                    b.Property<DateTime?>("ExpiresAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<string>("OriginalUrl")
                        .HasColumnType("text");

                    b.Property<string>("ShortCode")
                        .HasColumnType("text");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ShortCode")
                        .IsUnique();

                    b.HasIndex("UserId");

                    b.ToTable("Urls");
                });

            modelBuilder.Entity("UrlShortenerPro.Infrastructure.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<string>("FirstName")
                        .HasColumnType("text");

                    b.Property<bool>("IsPremium")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("LastLoginAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LastName")
                        .HasColumnType("text");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("UrlShortenerPro.Infrastructure.Models.ClickData", b =>
                {
                    b.HasOne("UrlShortenerPro.Infrastructure.Models.Url", "Url")
                        .WithMany("Clicks")
                        .HasForeignKey("UrlId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Url");
                });

            modelBuilder.Entity("UrlShortenerPro.Infrastructure.Models.Url", b =>
                {
                    b.HasOne("UrlShortenerPro.Infrastructure.Models.User", "User")
                        .WithMany("Urls")
                        .HasForeignKey("UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("UrlShortenerPro.Infrastructure.Models.Url", b =>
                {
                    b.Navigation("Clicks");
                });

            modelBuilder.Entity("UrlShortenerPro.Infrastructure.Models.User", b =>
                {
                    b.Navigation("Urls");
                });
#pragma warning restore 612, 618
        }
    }
}
