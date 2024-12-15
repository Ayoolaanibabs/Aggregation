using AggregationCRS.Domain;
using AggregationCRS.Domain.Entities;
using AggregationCRS.Domain.Managers;
using AggregationCRS.Domain.Repository;
using FluentNHibernate.Testing.Values;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NHibernate.Util;
using System.Data;
using System.Security.Cryptography.Xml;

namespace AggregationCRS.SQLClient
{
    public class ConcessionUtilizationWriter : IConcessionUtilizationWriter
    {
        private readonly IShortTermMemoryWriter _shortTermMemoryWriter;
        private readonly SqlConnection sqlConnection;
        private SqlCommand sqlCommand;
        private SqlDataAdapter sqlDataAdapter;
        private readonly IConfiguration _config;



        public ConcessionUtilizationWriter(IConfiguration config, IShortTermMemoryWriter shortTermMemoryWriter)
        {
            _config = config;
            sqlConnection = new SqlConnection(_config.GetConnectionString("Default"));
            sqlCommand = new SqlCommand("", sqlConnection);
            sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            _shortTermMemoryWriter = shortTermMemoryWriter;
        }



        public async Task WriteConcessionUtilizationDataAsync(List<StreamComputation> messageETOs)
        {
            try
            {
                string aggKey = string.Empty;
                string query = string.Empty;
                string streamId = string.Empty;
                string glAccount = string.Empty;
                string branch = string.Empty;
                string currency = string.Empty;
                string customerNumber = string.Empty;
                string accountNumber = string.Empty;
                string streamReference = string.Empty;
                string activityReference = string.Empty;
                string concessionId;
                DateTime utilizationDate = DateTime.Now;
                decimal UtilizedAmount = 0;


                string queryList = string.Empty;

                foreach (var message in messageETOs)
                {
                    aggKey = $"{message.CustomerNumber}.{message.AccountNumber}.{message.GLAccount}.{message.StreamId}.{message.StreamReference}";
                    streamId = message.StreamId;
                    glAccount = message.GLAccount;
                    branch = message.Branch;
                    currency = message.StreamCurrency;
                    customerNumber = message.CustomerNumber;
                    accountNumber = message.AccountNumber;
                    streamReference = message.StreamReference;
                    activityReference = message.ActivityReference;
                    concessionId = message.ConcessionId;
                    utilizationDate = message.ActivityDate.Value;
                    UtilizedAmount = message.ConceededFlowAmountLCY.GetValueOrDefault();

                    query = $"UPDATE CONCESSION_UTILIZATION_HISTORY SET UtilizedAmount = {UtilizedAmount} WHERE EntityKey = '{aggKey}'  " +
                        $"  IF @@ROWCOUNT=0 " +
                        $"  INSERT INTO CONCESSION_UTILIZATION_HISTORY(EntityKey,GLAccount,Branch,StreamId,Currency,CustomerNumber,AccountNumber,StreamReference,ActivityReference,ConcessionId,UtilizationDate,UtilizedAmount) VALUES ('{aggKey}','{glAccount}','{branch}','{streamId}','{currency}','{customerNumber}','{accountNumber}','{streamReference}','{activityReference}','{concessionId}','{utilizationDate.ToString("yyyy-MM-dd HH:mm:ss")}', {UtilizedAmount}); ";

                    queryList += query;
                }


                using (SqlConnection srcConn = new SqlConnection(sqlConnection.ConnectionString))
                {
                    using (SqlCommand cmd = new(queryList, srcConn))
                    {
                        if (srcConn.State != ConnectionState.Open) { await srcConn.OpenAsync(); }
                        cmd.CommandText = queryList;
                        cmd.ExecuteNonQuery();
                        await srcConn.CloseAsync();
                    }
                }
                Console.WriteLine($"Processed Concession Utilisation: {messageETOs.Count}");
                await Task.CompletedTask;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task WriteConcessionUtilizationSummaryDataAsync(List<StreamComputation> messageETOs)
        {
            try
            {
                string aggKey = string.Empty;
                string query = string.Empty;
                string streamId = string.Empty;
                string glAccount = string.Empty;
                string branch = string.Empty;
                string currency = string.Empty;
                string customerNumber = string.Empty;
                string accountNumber = string.Empty;
                string streamReference = string.Empty;
                string activityReference = string.Empty;
                string concessionId;
                DateTime utilizationDate = DateTime.Now;
                decimal UtilizedAmount = 0;
                DateOnly monthYear = new DateOnly();

                string aggKeysList = string.Empty;
                string aggKeys = string.Empty;
                string refrencesList = string.Empty;
                string refrences = string.Empty;
                DateTime lastUpdatedTime = DateTime.Now;
                List<string> uniqueEntities = new List<string>();
                List<string> shortTermMemories = new List<string>();




                string queryList = string.Empty;


                // build list of aggKeys
                foreach (var item in messageETOs.DistinctBy(x => new { x.CustomerNumber, x.AccountNumber, x.GLAccount, x.StreamId, MonthYear = new DateOnly(x.ActivityDate.Value.Year, x.ActivityDate.Value.Month, 1) }))
                {
                    aggKeysList += $"'{item.CustomerNumber}.{item.AccountNumber}.{item.StreamId}.{item.GLAccount}.{new DateOnly(item.ActivityDate.Value.Year, item.ActivityDate.Value.Month, 1)}',";
                    uniqueEntities.Add($"{item.CustomerNumber}.{item.AccountNumber}.{item.StreamId}.{item.GLAccount}.{new DateOnly(item.ActivityDate.Value.Year, item.ActivityDate.Value.Month, 1)}");
                }

                foreach (var item in messageETOs.DistinctBy(x => x.ActivityReference))
                {
                    refrencesList += ($"'{item.ActivityReference}',");
                }

                aggKeys = aggKeysList[..^1];
                refrences = refrencesList[..^1];


                string fetchQuery = $"SELECT * FROM ConcessionUtilizationSummary WHERE EntityKey IN ({aggKeys});";

                using (SqlConnection srcConn = new SqlConnection(sqlConnection.ConnectionString))
                {
                    using (SqlCommand cmd = new(fetchQuery, srcConn))
                    {
                        if (srcConn.State != ConnectionState.Open) { await srcConn.OpenAsync(); }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                foreach (var uniqueMessageEntity in uniqueEntities)
                                {
                                    InsertNewUtilisationSummary(messageETOs, out aggKey, out query, out streamId, out glAccount, out branch, out currency, out customerNumber, out accountNumber, out streamReference, out activityReference, out concessionId, ref UtilizedAmount, out monthYear, ref lastUpdatedTime, ref queryList, uniqueMessageEntity);
                                }
                            }


                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    query = string.Empty;
                                    var month = reader[nameof(ConcessionUtilizationSummary.MonthYear)]?.ToString();
                                    var messagesForDBRecord = messageETOs.Where(x => x.GLAccount == reader[nameof(ConcessionUtilizationSummary.GLAccount)]?.ToString() && x.StreamId == reader[nameof(ConcessionUtilizationSummary.StreamId)]?.ToString() && x.CustomerNumber == reader[nameof(ConcessionUtilizationSummary.CustomerNumber)]?.ToString() && x.AccountNumber == reader[nameof(ConcessionUtilizationSummary.AccountNumber)]?.ToString() && new DateTime(x.ActivityDate.Value.Year, x.ActivityDate.Value.Month, 1).ToString("MM/dd/yyyy HH:mm:ss") == month).ToList();
                                    aggKey = reader[nameof(ConcessionUtilizationSummary.EntityKey)]?.ToString();
                                    lastUpdatedTime = messagesForDBRecord.Max(x => x.ActivityDate.Value);
                                    //var shortTermMemory = reader[nameof(ConcessionUtilizationSummary.ShortMemory)]?.ToString();
                                    UtilizedAmount = decimal.Parse(reader[nameof(ConcessionUtilizationSummary.UtilizedAmount)]?.ToString());


                                    foreach (var message in messagesForDBRecord.OrderBy(x => x.ActivityDate))
                                    {
                                        if (shortTermMemories.Contains(message.ActivityReference))
                                        {
                                            continue;
                                        }
                                        UtilizedAmount += message.ConceededFlowAmountLCY.GetValueOrDefault();
                                        queryList += $"INSERT INTO ConcessionUtilizationSummaryShortTermTable (Ref, ActDate) VALUES('{message.ActivityReference}', '{message.ActivityDate.Value.ToString("yyyy-MM-dd HH:mm:ss")}'); ";

                                        //shortTermMemory = _shortTermMemoryWriter.GetGLShortTermMemory(shortTermMemory, message);
                                    }

                                    query = $"UPDATE ConcessionUtilizationSummary SET LastUpdate = '{lastUpdatedTime.ToString("yyyy-MM-dd HH:mm:ss")}', UtilizedAmount = {UtilizedAmount} WHERE EntityKey = '{aggKey}';";
                                    queryList += query;

                                    uniqueEntities.Remove(aggKey);
                                }

                                foreach (var newItem in uniqueEntities)
                                {
                                    InsertNewUtilisationSummary(messageETOs, out aggKey, out query, out streamId, out glAccount, out branch, out currency, out customerNumber, out accountNumber, out streamReference, out activityReference, out concessionId, ref UtilizedAmount, out monthYear, ref lastUpdatedTime, ref queryList, newItem);
                                }
                            }
                            reader.Close();
                        }

                        cmd.CommandText = queryList;
                        cmd.ExecuteNonQuery();
                        await srcConn.CloseAsync();
                    }
                }
                Console.WriteLine($"Processed Concession Utilisation: {messageETOs.Count}");
                await Task.CompletedTask;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void InsertNewUtilisationSummary(List<StreamComputation> messageETOs, out string aggKey, out string query, out string streamId, out string glAccount, out string branch, out string currency, out string customerNumber, out string accountNumber, out string streamReference, out string activityReference, out string concessionId, ref decimal UtilizedAmount, out DateOnly monthYear, ref DateTime lastUpdatedTime, ref string queryList, string uniqueMessageEntity)
        {
            var messageList = messageETOs.Where(x => string.Concat(x.CustomerNumber, ".", x.AccountNumber, ".", x.StreamId  , ".", x.GLAccount , ".", new DateOnly(x.ActivityDate.Value.Year, x.ActivityDate.Value.Month, 1)) == uniqueMessageEntity).ToList();
            var message = messageList.OrderByDescending(x => x.ActivityDate).First();
            var shortTermMemory = string.Empty;

            aggKey = $"{message.CustomerNumber}.{message.AccountNumber}.{message.StreamId}.{message.GLAccount}.{new DateOnly(message.ActivityDate.Value.Year, message.ActivityDate.Value.Month, 1)}";
            streamId = message.StreamId;
            glAccount = message.GLAccount;
            branch = message.Branch;
            currency = message.StreamCurrency;
            customerNumber = message.CustomerNumber;
            accountNumber = message.AccountNumber;
            streamReference = message.StreamReference;
            activityReference = message.ActivityReference;
            concessionId = message.ConcessionId;
            monthYear = new DateOnly(message.ActivityDate.Value.Year, message.ActivityDate.Value.Month, 1);

            foreach (var messageEntity in messageList.OrderBy(x => x.ActivityDate))
            {
                UtilizedAmount += messageEntity.ConceededFlowAmountLCY.GetValueOrDefault();
                queryList += $"INSERT INTO ConcessionUtilizationSummaryShortTermTable (Ref, ActDate) VALUES('{message.ActivityReference}', '{message.ActivityDate.Value.ToString("yyyy-MM-dd HH:mm:ss")}'); ";
                //shortTermMemory = _shortTermMemoryWriter.GetGLShortTermMemory(shortTermMemory, messageEntity);
                lastUpdatedTime = messageEntity.ActivityDate.Value;
            }

            query = $"INSERT INTO ConcessionUtilizationSummary(EntityKey,GLAccount,Branch,StreamId,Currency,CustomerNumber,AccountNumber,UtilizedAmount,MonthYear,LastUpdate) VALUES ('{aggKey}','{glAccount}','{branch}','{streamId}','{currency}','{customerNumber}','{accountNumber}', {UtilizedAmount},'{monthYear}','{lastUpdatedTime.ToString("yyyy-MM-dd HH:mm:ss")}'); ";
            queryList += query;
        }
    }
}
