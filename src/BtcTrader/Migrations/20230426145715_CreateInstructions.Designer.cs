﻿// <auto-generated />
using System;
using System.Collections.Generic;
using BtcTrader.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BtcTrader.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20230426145715_CreateInstructions")]
    partial class CreateInstructions
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BtcTrader.Domain.Instructions.Instruction", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<double>("Amount")
                        .HasColumnType("double precision");

                    b.Property<DateTime>("InstructionDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("InstructionType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<List<string>>("NotificationChannel")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.ToTable("Instructions");
                });
#pragma warning restore 612, 618
        }
    }
}
