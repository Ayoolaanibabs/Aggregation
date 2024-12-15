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
    [Migration("20240316115520_Added_MonthVariables_To_ActivityAggregation")]
    partial class Added_MonthVariables_To_ActivityAggregation
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

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("datetime2");

                    b.Property<double>("MonthAverage")
                        .HasColumnType("float");

                    b.Property<double>("MonthAvg")
                        .HasColumnType("float");

                    b.Property<double>("MonthCount")
                        .HasColumnType("float");

                    b.Property<double>("MonthMax")
                        .HasColumnType("float");

                    b.Property<double>("MonthMin")
                        .HasColumnType("float");

                    b.Property<double>("MonthSum")
                        .HasColumnType("float");

                    b.Property<DateOnly>("MonthYear")
                        .HasColumnType("date");

                    b.HasKey("EntityKey");

                    b.ToTable("ActivityAggregationTable");
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