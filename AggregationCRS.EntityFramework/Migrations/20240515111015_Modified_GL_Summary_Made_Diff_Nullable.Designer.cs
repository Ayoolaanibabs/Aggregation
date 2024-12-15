﻿// <auto-generated />
using System;
using AggregationCRS.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AggregationCRS.EntityFramework.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240515111015_Modified_GL_Summary_Made_Diff_Nullable")]
    partial class Modified_GL_Summary_Made_Diff_Nullable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("AggregationCRS.Domain.Entities.ActivityAggregation", b =>
                {
                    b.Property<string>("EntityKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AccountNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ActivityCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ActivityId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ActivityName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CustomerNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DailyActivitySummary")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GlobalActivityCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("datetime2");

                    b.Property<string>("ShortTerm")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("EntityKey");

                    b.ToTable("ActivityAggregationTable");
                });

            modelBuilder.Entity("AggregationCRS.Domain.Entities.GLComputationSummary", b =>
                {
                    b.Property<string>("EntityKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<double>("ActualFlow")
                        .HasColumnType("float");

                    b.Property<string>("Branch")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("Difference")
                        .HasColumnType("float");

                    b.Property<double>("ExpectedFlow")
                        .HasColumnType("float");

                    b.Property<string>("GLAccount")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("LastUpdate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ShortTerm")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StreamId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("EntityKey");

                    b.HasIndex("GLAccount");

                    b.ToTable("GLSummaryTable");
                });

            modelBuilder.Entity("AggregationCRS.Domain.Entities.StreamAggregation", b =>
                {
                    b.Property<string>("EntityKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AccountNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ActivityCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ActivityId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CustomerNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DailyStreamSummary")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("datetime2");

                    b.Property<string>("ShortTerm")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StreamCurrency")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StreamId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("EntityKey");

                    b.ToTable("StreamAggregationTable");
                });
#pragma warning restore 612, 618
        }
    }
}
