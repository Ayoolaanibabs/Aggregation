using AggregationCRS.Domain.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NHibernate.SqlCommand;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace AggregationCRS.SQLClient
{
    public class BackgroundJobWriter : IBackgroundJobWriter
    {
        private readonly SqlConnection sqlConnection;
        private readonly IConfiguration _config;
        private SqlCommand sqlCommand;
        private SqlDataAdapter sqlDataAdapter;

        public BackgroundJobWriter(IConfiguration config)
        {
            _config = config;
            sqlConnection = new SqlConnection(_config.GetConnectionString("Default"));
            sqlCommand = new SqlCommand("", sqlConnection);
            sqlDataAdapter = new SqlDataAdapter(sqlCommand);
        }
        public async Task ClearActivityShortTermMemoryTable()
        {
            var cutoffDate = DateTime.Now.AddDays(-6).ToString("yyyy-MM-dd HH:mm:ss");
            using (SqlConnection srcConn = new SqlConnection(sqlConnection.ConnectionString))
            {
                await srcConn.OpenAsync();
                using (SqlCommand cmd = new($"DELETE FROM ActivityShortTermMemoryTable WHERE ActDate < '{cutoffDate}'", srcConn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                await srcConn.CloseAsync();
            }
        }

        public async Task ClearConcessionUtilizationSummaryShortTermMemoryTable()
        {
            var cutoffDate = DateTime.Now.AddDays(-6).ToString("yyyy-MM-dd HH:mm:ss");

            using (SqlConnection srcConn = new SqlConnection(sqlConnection.ConnectionString))
            {
                await srcConn.OpenAsync();
                using (SqlCommand cmd = new($"DELETE FROM ConcessionUtilizationSummaryShortTermTable WHERE ActDate < '{cutoffDate}'", srcConn))
                {

                    await cmd.ExecuteNonQueryAsync();
                }
                await srcConn.CloseAsync();

            }
        }

        public async Task ClearCustomerActivityAggregationTable()
        {
            var cutoffDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-12)).ToString("yyyy-MM-dd HH:mm:ss");
            using (SqlConnection srcConn = new SqlConnection(sqlConnection.ConnectionString))
            {
                await srcConn.OpenAsync();

                using (SqlCommand cmd = new($"DELETE FROM ActivityShortTermMemoryTable WHERE AggregationMonth < '{cutoffDate}'", srcConn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                await srcConn.CloseAsync();
            }
        }

        public async Task ClearCustomerActivityShortTermMemoryTable()
        {
            var cutoffDate = DateTime.Now.AddDays(-6).ToString("yyyy-MM-dd HH:mm:ss");
            using (SqlConnection srcConn = new SqlConnection(sqlConnection.ConnectionString))
            {
                await srcConn.OpenAsync();

                using (SqlCommand cmd = new($"DELETE FROM CustomerActivityShortTermMemoryTable WHERE ActDate < '{cutoffDate}'", srcConn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                await srcConn.CloseAsync();
            }
        }

        public async Task ClearCustomerStreamAggregationTable()
        {
            var cutoffDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-12)).ToString("yyyy-MM-dd HH:mm:ss");
            using (SqlConnection srcConn = new SqlConnection(sqlConnection.ConnectionString))
            {
                await srcConn.OpenAsync();

                using (SqlCommand cmd = new($"DELETE FROM ActivityShortTermMemoryTable WHERE AggregationMonth < '{cutoffDate}'", srcConn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                await srcConn.CloseAsync();
            }
        }

        public async Task ClearCustomerStreamShortTermMemoryTable()
        {
            var cutoffDate = DateTime.Now.AddDays(-6).ToString("yyyy-MM-dd HH:mm:ss");
            using (SqlConnection srcConn = new SqlConnection(sqlConnection.ConnectionString))
            {
                await srcConn.OpenAsync();

                using (SqlCommand cmd = new($"DELETE FROM CustomerStreamShortTermMemoryTable WHERE ActDate < '{cutoffDate}'", srcConn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                await srcConn.CloseAsync();
            }
        }

        public async Task ClearDailyGLSummaryShortTermMemoryTable()
        {
            var cutoffDate = DateTime.Now.AddDays(-6).ToString("yyyy-MM-dd HH:mm:ss");
            using (SqlConnection srcConn = new SqlConnection(sqlConnection.ConnectionString))
            {
                await srcConn.OpenAsync();

                using (SqlCommand cmd = new($"DELETE FROM DailyGLSummaryShortTermMemoryTable WHERE ActDate < '{cutoffDate}'", srcConn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                await srcConn.CloseAsync();
            }
        }

        public async Task ClearGLSummaryShortTermMemoryTable()
        {
            var cutoffDate = DateTime.Now.AddDays(-6).ToString("yyyy-MM-dd HH:mm:ss");
            using (SqlConnection srcConn = new SqlConnection(sqlConnection.ConnectionString))
            {
                await srcConn.OpenAsync();

                using (SqlCommand cmd = new($"DELETE FROM GLSummaryShortTermMemoryTable WHERE ActDate < '{cutoffDate}'", srcConn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                await srcConn.CloseAsync();
            }
        }

        public async Task ClearStreamShortTermMemoryTable()
        {
            var cutoffDate = DateTime.Now.AddDays(-6).ToString("yyyy-MM-dd HH:mm:ss");
            using (SqlConnection srcConn = new SqlConnection(sqlConnection.ConnectionString))
            {
                await srcConn.OpenAsync();
                using (SqlCommand cmd = new($"DELETE FROM StreamShortTermMemoryTable WHERE ActDate < '{cutoffDate}'", srcConn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                await srcConn.CloseAsync();
            }
        }
    }
}
