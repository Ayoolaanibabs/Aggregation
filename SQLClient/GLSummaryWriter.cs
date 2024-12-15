using AggregationCRS.Domain;
using AggregationCRS.Domain.Entities;
using AggregationCRS.Domain.Managers;
using AggregationCRS.Domain.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Security.Cryptography.Xml;

namespace AggregationCRS.SQLClient
{
    public class GLSummaryWriter : IGLSummaryWriter
    {
        private readonly IShortTermMemoryWriter _shortTermMemoryWriter;
        private readonly SqlConnection sqlConnection;
        private SqlCommand sqlCommand;
        private SqlDataAdapter sqlDataAdapter;
        private readonly IConfiguration _config;
        private int daysInHistory = 60;
        private int shortTermMemoryLength = 3;


        public GLSummaryWriter(IConfiguration config, IShortTermMemoryWriter shortTermMemoryWriter)
        {
            _config = config;
            sqlConnection = new SqlConnection(_config.GetConnectionString("Default"));
            sqlCommand = new SqlCommand("", sqlConnection);
            sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            _shortTermMemoryWriter = shortTermMemoryWriter;
        }



        public async Task WriteGLSummaryDataAsync(List<StreamComputation> messageETOs)
        {
            try
            {
                string aggKey = string.Empty;

                string streamId = string.Empty;
                string glAccount = string.Empty;
                string branch = string.Empty;
                string currency = string.Empty;

                decimal expectedFlowAmount = 0;
                decimal actualFlowAmount = 0;
                decimal difference = 0;

                string aggKeysList = string.Empty;
                string aggKeys = string.Empty;

                DateTime lastUpdatedTime = DateTime.Now;
                List<string> uniqueEntities = new List<string>();
                string queryList = string.Empty;
                string refrencesList = string.Empty;
                string refrences = string.Empty;
                List<string> shortTermMemories = new List<string>();

                // build list of aggKeys
                foreach (var item in messageETOs.DistinctBy(x => new { x.GLAccount, x.Branch, x.StreamCurrency, x.StreamId }))
                {
                    aggKeysList += $"'{item.GLAccount}.{item.Branch}.{item.StreamCurrency}.{item.StreamId}',";
                    uniqueEntities.Add($"{item.GLAccount}.{item.Branch}.{item.StreamCurrency}.{item.StreamId}");
                }

                foreach (var item in messageETOs.DistinctBy(x => x.StreamReference))
                {
                    refrencesList += ($"'{item.StreamReference}',");
                }
                aggKeys = aggKeysList[..^1];
                refrences = refrencesList[..^1];

                using (SqlConnection srcConn = new SqlConnection(sqlConnection.ConnectionString))
                {
                    using (SqlCommand cmd2 = new($"SELECT * FROM GLSummaryShortTermMemoryTable WHERE Ref in ({refrences})", srcConn))
                    {
                        if (srcConn.State != ConnectionState.Open) { await srcConn.OpenAsync(); }
                        using (SqlDataReader reader = cmd2.ExecuteReader())
                        {
                            while (await reader.ReadAsync())
                            {

                                string Ref = reader.GetString(reader.GetOrdinal("Ref"));
                                shortTermMemories.Add(Ref);
                            }

                        }

                    }
                    using (SqlCommand cmd = new($"SELECT * FROM GLSummaryTable WHERE EntityKey in ({aggKeys})", srcConn))
                    {
                        if (srcConn.State != ConnectionState.Open) { await srcConn.OpenAsync(); }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                foreach (var uniqueMessageEntity in uniqueEntities)
                                {
                                    queryList = InsertNewItems(messageETOs, shortTermMemories, out streamId, out glAccount, out branch, out currency, out expectedFlowAmount, out actualFlowAmount, out lastUpdatedTime, out difference, queryList, uniqueMessageEntity);
                                }
                            }


                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    string query = string.Empty;
                                    var messagesForDBRecord = messageETOs.Where(x => x.GLAccount == reader[nameof(GLComputationSummary.GLAccount)]?.ToString() && x.StreamId == reader[nameof(GLComputationSummary.StreamId)]?.ToString() && x.Branch == reader[nameof(GLComputationSummary.Branch)]?.ToString() && x.StreamCurrency == reader[nameof(GLComputationSummary.Currency)]?.ToString()).ToList();
                                    aggKey = reader[nameof(GLComputationSummary.EntityKey)]?.ToString();
                                    lastUpdatedTime = messagesForDBRecord.Max(x => x.ActivityDate.Value);
                                    //var shortTermMemory = reader[nameof(GLComputationSummary.ShortTerm)]?.ToString();
                                    expectedFlowAmount = decimal.Parse(reader[nameof(GLComputationSummary.ExpectedFlow)]?.ToString());
                                    actualFlowAmount = decimal.Parse(reader[nameof(GLComputationSummary.ActualFlow)]?.ToString());
                                    difference = decimal.Parse(reader[nameof(GLComputationSummary.Difference)]?.ToString());


                                    foreach (var message in messagesForDBRecord.OrderBy(x => x.StreamDate))
                                    {
                                        if (shortTermMemories.Contains(message.StreamReference))
                                        {
                                            continue;
                                        }
                                        expectedFlowAmount += message.ExpectedFlow.Value;
                                        actualFlowAmount += message.ActualFlow.Value;
                                        difference = (expectedFlowAmount - actualFlowAmount);
                                        queryList += $"INSERT INTO GLSummaryShortTermMemoryTable (Ref, ActDate) VALUES('{message.StreamReference}', '{message.StreamDate.Value.ToString("yyyy-MM-dd HH:mm:ss")}'); ";

                                        //shortTermMemory = _shortTermMemoryWriter.GetGLShortTermMemory(shortTermMemory, message);
                                    }

                                    query = $"UPDATE GLSummaryTable SET LastUpdate = '{lastUpdatedTime.ToString("yyyy-MM-dd HH:mm:ss")}', ActualFlow = {actualFlowAmount},ExpectedFlow = {expectedFlowAmount}, Difference = {difference} WHERE EntityKey = '{aggKey}';";
                                    queryList += query;

                                    uniqueEntities.Remove(aggKey);
                                }

                                foreach (var newItem in uniqueEntities)
                                {
                                    queryList = InsertNewItems(messageETOs, shortTermMemories, out streamId, out glAccount, out branch, out currency, out expectedFlowAmount, out actualFlowAmount, out lastUpdatedTime, out difference, queryList, newItem);
                                }
                            }
                            reader.Close();
                        }

                        if (srcConn.State != ConnectionState.Open) { await srcConn.OpenAsync(); }

                        cmd.CommandText = queryList;
                        cmd.ExecuteNonQuery();
                        await srcConn.CloseAsync();
                    }
                }
                Console.WriteLine($"Processing Stream Aggregation: {messageETOs.Count}");
                await Task.CompletedTask;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public async Task WriteDailyGLSummaryDataAsync(List<StreamComputation> messageETOs)
        {
            try
            {
                string aggKey = string.Empty;

                string streamId = string.Empty;
                string glAccount = string.Empty;
                string branch = string.Empty;
                string currency = string.Empty;

                decimal expectedFlowAmount = 0;
                decimal actualFlowAmount = 0;
                decimal difference = 0;

                string aggKeysList = string.Empty;
                string aggKeys = string.Empty;
                var today = DateTime.Now.Date;
                DateTime lastUpdatedTime = DateTime.Now;

                List<string> uniqueEntities = new List<string>();
                string queryList = string.Empty;
                string refrencesList = string.Empty;
                string refrences = string.Empty;
                List<string> shortTermMemories = new List<string>();

                // build list of aggKeys
                foreach (var item in messageETOs.DistinctBy(x => new { x.GLAccount, x.Branch, x.StreamCurrency, x.StreamId, AggMonth = new DateOnly(x.StreamDate.Value.Year, x.StreamDate.Value.Month, x.StreamDate.Value.Day) }))
                {
                    var aggDay = new DateOnly(item.StreamDate.Value.Year, item.StreamDate.Value.Month, item.StreamDate.Value.Day).ToString("yyyy-MM-dd");
                    aggKeysList += $"'{item.GLAccount}.{item.Branch}.{item.StreamCurrency}.{item.StreamId}.{aggDay}',";
                    uniqueEntities.Add($"{item.GLAccount}.{item.Branch}.{item.StreamCurrency}.{item.StreamId}.{aggDay}");
                }
                foreach (var item in messageETOs.DistinctBy(x => x.StreamReference))
                {
                    refrencesList += ($"'{item.StreamReference}',");
                }
                aggKeys = aggKeysList[..^1];
                refrences = refrencesList[..^1];

                using (SqlConnection srcConn = new SqlConnection(sqlConnection.ConnectionString))
                {
                    using (SqlCommand cmd2 = new($"SELECT * FROM DailyGLSummaryShortTermMemoryTable WHERE Ref in ({refrences})", srcConn))
                    {
                        if (srcConn.State != ConnectionState.Open) { await srcConn.OpenAsync(); }
                        using (SqlDataReader reader = cmd2.ExecuteReader())
                        {
                            while (await reader.ReadAsync())
                            {

                                string Ref = reader.GetString(reader.GetOrdinal("Ref"));
                                shortTermMemories.Add(Ref);
                            }

                        }

                    }
                    using (SqlCommand cmd = new($"SELECT * FROM DailyGLComputationSummaryTable WHERE EntityKey in ({aggKeys})", srcConn))
                    {
                        if (srcConn.State != ConnectionState.Open) { await srcConn.OpenAsync(); }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                foreach (var uniqueMessageEntity in uniqueEntities)
                                {
                                    queryList = InsertNewDailyItems(messageETOs, shortTermMemories,today, out streamId, out glAccount, out branch, out currency, out expectedFlowAmount, out actualFlowAmount, out lastUpdatedTime, out difference, queryList, uniqueMessageEntity);
                                }
                            }


                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    string query = string.Empty;
                                    var summaryDay = reader[nameof(DailyGLComputationSummary.SummaryDate)]?.ToString();


                                    var messagesForDBRecord = messageETOs.Where(x => x.GLAccount == reader[nameof(DailyGLComputationSummary.GLAccount)]?.ToString() && x.StreamId == reader[nameof(DailyGLComputationSummary.StreamId)]?.ToString() && x.Branch == reader[nameof(DailyGLComputationSummary.Branch)]?.ToString() && x.StreamCurrency == reader[nameof(DailyGLComputationSummary.Currency)]?.ToString() && new DateTime(x.StreamDate.Value.Year, x.StreamDate.Value.Month, x.StreamDate.Value.Day).ToString("dd/MM/yyyy HH:mm:ss") == summaryDay).ToList();
                                    aggKey = reader[nameof(DailyGLComputationSummary.EntityKey)]?.ToString();
                                    lastUpdatedTime = messagesForDBRecord.Max(x => x.StreamDate.Value);
                                    expectedFlowAmount = decimal.Parse(reader[nameof(DailyGLComputationSummary.ExpectedFlow)]?.ToString());
                                    actualFlowAmount = decimal.Parse(reader[nameof(DailyGLComputationSummary.ActualFlow)]?.ToString());
                                    difference = decimal.Parse(reader[nameof(DailyGLComputationSummary.Difference)]?.ToString());


                                    foreach (var message in messagesForDBRecord.OrderBy(x => x.StreamDate))
                                    {
                                        if (shortTermMemories.Contains(message.StreamReference))
                                        {
                                            continue;
                                        }
                                        expectedFlowAmount += message.ExpectedFlow.Value;
                                        actualFlowAmount += message.ActualFlow.Value;
                                        difference = (expectedFlowAmount - actualFlowAmount);
                                        queryList += $"INSERT INTO DailyGLSummaryShortTermMemoryTable (Ref, ActDate) VALUES('{message.StreamReference}', '{message.StreamDate.Value.ToString("yyyy-MM-dd HH:mm:ss")}'); ";

                                    }

                                    query = $"UPDATE DailyGLComputationSummaryTable SET LastUpdate = '{lastUpdatedTime.ToString("yyyy-MM-dd HH:mm:ss")}', ActualFlow = {actualFlowAmount},ExpectedFlow = {expectedFlowAmount}, Difference = {difference} WHERE EntityKey = '{aggKey}';";
                                    queryList += query;

                                    uniqueEntities.Remove(aggKey);
                                }

                                foreach (var newItem in uniqueEntities)
                                {
                                    queryList = InsertNewDailyItems(messageETOs, shortTermMemories,today, out streamId, out glAccount, out branch, out currency, out expectedFlowAmount, out actualFlowAmount, out lastUpdatedTime, out difference, queryList, newItem);
                                }
                            }
                            reader.Close();
                        }

                        if (srcConn.State != ConnectionState.Open) { await srcConn.OpenAsync(); }

                        cmd.CommandText = queryList;
                        cmd.ExecuteNonQuery();
                        await srcConn.CloseAsync();
                    }
                }
                Console.WriteLine($"Processing Daily GLSummary Aggregation: {messageETOs.Count}");
                await Task.CompletedTask;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private string InsertNewItems(List<StreamComputation> messageETOs, List<String> shortTermMemories, out string streamId, out string glAccount, out string branch, out string currency, out decimal expectedFlowAmount, out decimal actualFlowAmount, out DateTime lastUpdatedTime, out decimal difference,  string queryList, string uniqueMessageEntity)
        {
            var messageList = messageETOs.Where(x => string.Concat(x.GLAccount, ".", x.Branch, ".", x.StreamCurrency, ".", x.StreamId) == uniqueMessageEntity).ToList();
            var lastMessage = messageList.OrderByDescending(x => x.StreamDate).First();
            //var shortTermMemory = string.Empty;

            expectedFlowAmount = 0;
            actualFlowAmount = 0;
            difference = 0;

            streamId = lastMessage.StreamId;
            glAccount = lastMessage.GLAccount;
            branch = lastMessage.Branch;
            currency = lastMessage.StreamCurrency;

            lastUpdatedTime = lastMessage.StreamDate.Value;
            streamId = lastMessage.StreamId;

            foreach (var message in messageList.OrderBy(x => x.StreamDate))
            {
                if (shortTermMemories.Contains(message.StreamReference))
                {
                    continue;
                }
                expectedFlowAmount += message.ExpectedFlow.Value;
                actualFlowAmount += message.ActualFlow.Value;
                difference = expectedFlowAmount - actualFlowAmount;
                queryList += $"INSERT INTO GLSummaryShortTermMemoryTable (Ref, ActDate) VALUES('{message.StreamReference}', '{message.StreamDate.Value.ToString("yyyy-MM-dd HH:mm:ss")}'); ";
            }

            var query = $"INSERT INTO GLSummaryTable(EntityKey,GLAccount,Branch,StreamId,Currency,ActualFlow,ExpectedFlow,LastUpdate, Difference) VALUES ('{uniqueMessageEntity}','{glAccount}','{branch}','{streamId}','{currency}',{actualFlowAmount},{expectedFlowAmount},'{lastUpdatedTime.ToString("yyyy-MM-dd HH:mm:ss")}', {difference});";
            queryList += query;
            return queryList;
        }

        private string InsertNewDailyItems(List<StreamComputation> messageETOs, List<String> shortTermMemories, DateTime today, out string streamId, out string glAccount, out string branch, out string currency, out decimal expectedFlowAmount, out decimal actualFlowAmount, out DateTime lastUpdatedTime, out decimal difference, string queryList, string uniqueMessageEntity)
        {
            var messageList = messageETOs.Where(x => string.Concat(x.GLAccount, ".", x.Branch, ".", x.StreamCurrency, ".", x.StreamId, ".", new DateOnly(x.StreamDate.Value.Year, x.StreamDate.Value.Month, x.StreamDate.Value.Day).ToString("yyyy-MM-dd")) == uniqueMessageEntity).ToList();
            var lastMessage = messageList.OrderByDescending(x => x.StreamDate).First();

            expectedFlowAmount = 0;
            actualFlowAmount = 0;
            difference = 0;
            streamId = lastMessage.StreamId;
            glAccount = lastMessage.GLAccount;
            branch = lastMessage.Branch;
            currency = lastMessage.StreamCurrency;

            lastUpdatedTime = lastMessage.StreamDate.Value;
            streamId = lastMessage.StreamId;
            var aggDay = new DateOnly(lastMessage.StreamDate.Value.Year, lastMessage.StreamDate.Value.Month, lastMessage.StreamDate.Value.Day).ToString("yyyy-MM-dd");

            foreach (var message in messageList.OrderBy(x => x.StreamDate))
            {
                if (shortTermMemories.Contains(message.StreamReference))
                {
                    continue;
                }
                expectedFlowAmount += message.ExpectedFlow.Value;
                actualFlowAmount += message.ActualFlow.Value;
                difference = expectedFlowAmount - actualFlowAmount;


                queryList += $"INSERT INTO DailyGLSummaryShortTermMemoryTable (Ref, ActDate) VALUES('{message.StreamReference}', '{message.StreamDate.Value.ToString("yyyy-MM-dd HH:mm:ss")}'); ";
            }

            var query = $"INSERT INTO DailyGLComputationSummaryTable(EntityKey,GLAccount,Branch,StreamId,Currency,ActualFlow,ExpectedFlow,LastUpdate, Difference, SummaryDate) VALUES ('{uniqueMessageEntity}','{glAccount}','{branch}','{streamId}','{currency}',{actualFlowAmount},{expectedFlowAmount},'{lastUpdatedTime.ToString("yyyy-MM-dd HH:mm:ss")}', {difference}, '{aggDay}');";
            queryList += query;
            return queryList;
        }
    }
}
