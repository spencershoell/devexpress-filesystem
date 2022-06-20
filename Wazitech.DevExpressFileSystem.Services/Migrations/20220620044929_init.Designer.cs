﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Wazitech.DevExpressFileSystem.Services;

#nullable disable

namespace Wazitech.DevExpressFileSystem.Services.Migrations
{
    [DbContext(typeof(FileManagementDbContext))]
    [Migration("20220620044929_init")]
    partial class init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Wazitech.DevExpressFileSystem.Services.FileItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsDirectory")
                        .HasColumnType("bit");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("datetime2");

                    b.Property<int>("ModifiedById")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ParentId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ModifiedById");

                    b.HasIndex("ParentId");

                    b.ToTable("FileItems");
                });

            modelBuilder.Entity("Wazitech.DevExpressFileSystem.Services.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("User");
                });

            modelBuilder.Entity("Wazitech.DevExpressFileSystem.Services.FileItem", b =>
                {
                    b.HasOne("Wazitech.DevExpressFileSystem.Services.User", "ModifiedBy")
                        .WithMany("Files")
                        .HasForeignKey("ModifiedById")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Wazitech.DevExpressFileSystem.Services.FileItem", "Parent")
                        .WithMany("Files")
                        .HasForeignKey("ParentId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("ModifiedBy");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("Wazitech.DevExpressFileSystem.Services.FileItem", b =>
                {
                    b.Navigation("Files");
                });

            modelBuilder.Entity("Wazitech.DevExpressFileSystem.Services.User", b =>
                {
                    b.Navigation("Files");
                });
#pragma warning restore 612, 618
        }
    }
}
