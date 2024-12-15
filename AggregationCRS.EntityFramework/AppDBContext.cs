using AggregationCRS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AggregationCRS.EntityFramework
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ActivityAggregation> ActivityAggregationTable { get; set; }

        public DbSet<StreamAggregation> StreamAggregationTable { get; set; }

        public DbSet<GLComputationSummary> GLSummaryTable { get; set; }
        public DbSet<ConcessionUtilizationSummary> ConcessionUtilizationSummary { get; set; }
        public DbSet<ConcessionUtilization> CONCESSION_UTILIZATION_HISTORY { get; set; }
        public DbSet<ActivityShortTermMemory> ActivityShortTermMemoryTable { get; set; }
        public DbSet<StreamShortTermMemory> StreamShortTermMemoryTable { get; set; }
        public DbSet<GLSummaryShortTermMemory> GLSummaryShortTermMemoryTable { get; set; }
        public DbSet<DailyGLSummaryShortTermMemory> DailyGLSummaryShortTermMemoryTable { get; set; }
        public DbSet<DailyGLComputationSummary> DailyGLComputationSummaryTable { get; set; }
        public DbSet<CustomerStreamAggregation> CustomerStreamAggregationTable { get; set; }
        public DbSet<CustomerStreamShortTermMemory> CustomerStreamShortTermMemoryTable { get; set; }
        public DbSet<ConcessionUtilizationSummaryShortTerm> ConcessionUtilizationSummaryShortTermTable { get; set; }
        public DbSet<CustomerActivityAggregation> CustomerActivityAggregationTable { get; set; }
        public DbSet<CustomerActivityShortTermMemory> CustomerActivityShortTermMemoryTable { get; set; }
    }
}
