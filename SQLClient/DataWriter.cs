using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System.Data;
using Microsoft.Data.SqlClient;
using AggregationCRS.Domain.Repository;
using AggregationCRS.Domain.Entities;
using AggregationCRS.Domain.Managers;
using Microsoft.AspNetCore.Http;
using System.IO;
using AggregationCRS.Domain.Shared;

namespace AggregationCRS.SQLClient
{
    public class DataWriter : IDataWriter
    {
        private readonly IShortTermMemoryWriter _shortTermMemoryWriter;
        private readonly SqlConnection sqlConnection;
        private SqlCommand sqlCommand;
        private SqlDataAdapter sqlDataAdapter;
        private readonly IConfiguration _config;
        private int daysInHistory = 60;
        private int shortTermMemoryLength = 3;


        public DataWriter(IConfiguration config, IShortTermMemoryWriter shortTermMemoryWriter)
        {
            _config = config;
            sqlConnection = new SqlConnection(_config.GetConnectionString("Default"));
            sqlCommand = new SqlCommand("", sqlConnection);
            sqlDataAdapter = new SqlDataAdapter(sqlCommand);      
            daysInHistory = _config.GetValue<int>("LogHistory");
            shortTermMemoryLength = _config.GetValue<int>("ShortTermMemoryLength");
            _shortTermMemoryWriter = shortTermMemoryWriter;
        }

        public async Task ExecuteNonQueryAsync(string query)
        {
            try
            {
                OpenConnection();
                sqlCommand.CommandType = System.Data.CommandType.Text;
                sqlCommand.CommandText = query;
                await sqlCommand.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Error($"Failed writing ExeucuteNonQueryAsync {query} = = = = = {e.Message} Retrying.");
                try
                {
                    sqlCommand.CommandText = query.Replace("GO", "");
                    await sqlCommand.ExecuteNonQueryAsync();
                }
                catch (Exception x)
                {
                    Log.Error($"Failed writing ExeucuteNonQueryAsync {x.Message} Request terminated.");
                }
                finally
                {
                    CloseConnection();
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        private void OpenConnection()
        {
            try
            {
                if (sqlConnection.State != ConnectionState.Open)
                {
                    sqlCommand = new SqlCommand("", sqlConnection);
                    sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                    //sqlCommandBuilder = new SqlCommandBuilder(sqlDataAdapter);
                    sqlConnection.Open();
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed openning sql connection. {e.Message}");
            }
        }

        private void CloseConnection()
        {
            try
            {
                if (sqlConnection.State != System.Data.ConnectionState.Closed) sqlConnection.Close();
            }
            catch (Exception e)
            {
                Log.Error($"Failed openning sql connection. {e.Message}");
            }
        }



        public async Task WriteActivityDataAsync(List<ActivityMessageETO> messageETOs)
        {
            try
            {
                string customerNumber = string.Empty;
                string activityId = string.Empty;
                string accountNumber = string.Empty;
                string activityCode = string.Empty;
                string currency = string.Empty;
                string activityName = string.Empty;
                string globalActivityCode = string.Empty;
                string dailyactivitySUmmary = string.Empty;
                DateTime lastUpdatedTime = DateTime.Now;
                DateTime monthYear = DateTime.Now;
                string aggKey = string.Empty;
                string aggKeys = string.Empty;
                List<string> treatedAggKeys = new List<string>();
                string aggKeysList = string.Empty;
                string queryStrings = string.Empty;
                string finalQuery = string.Empty;
                List<string> uniqueEntities = new List<string>();
                string refrencesList = string.Empty;
                string refrences = string.Empty;
                List<string> shortTermMemories = new List<string>();



                // build list of aggKeys
                foreach (var item in messageETOs.DistinctBy(x => x.EntityKey))
                {
                    aggKeysList += ($"'{item.EntityKey}',");
                    uniqueEntities.Add($"{item.EntityKey}");
                }

                foreach (var item in messageETOs.DistinctBy(x => x.ActivityReference))
                {
                    refrencesList += ($"'{item.ActivityReference}',");
                }

                aggKeys = aggKeysList[..^1];
                refrences = refrencesList[..^1];

                using (SqlConnection srcConn = new SqlConnection(sqlConnection.ConnectionString))
                {
                    using (SqlCommand cmd2 = new ($"SELECT * FROM ActivityShortTermMemoryTable WHERE Ref in ({refrences})", srcConn))
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


                    using (SqlCommand cmd = new($"SELECT * FROM ActivityAggregationTable WHERE EntityKey in ({aggKeys})", srcConn))
                    {
                        if (srcConn.State != ConnectionState.Open) { await srcConn.OpenAsync(); }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // Do this if there are no records at all from the DB
                            // Get all the messages for the unique entities
                            // Then create a query string for those entites

                            if (!reader.HasRows)
                            {
                                foreach (var uniqueMessageEntity in uniqueEntities)
                                {
                                    queryStrings = GetInsertActivityQuery(messageETOs, shortTermMemories, out customerNumber, out activityId, out accountNumber, out activityCode, out currency, out activityName, out dailyactivitySUmmary, out lastUpdatedTime, out monthYear, out aggKey, queryStrings, uniqueMessageEntity,out globalActivityCode);
                                }
                            }

                            // Do this if there are some records from the DB.
                            // Build an update query for the records that were fetched for the records that exist
                            // Check for the rest of the records that were not fetched and create an insert script for them.

                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    string query = string.Empty;
                                    var messagesForDBRecord = messageETOs.Where(x => x.EntityKey == $"{reader[nameof(ActivityAggregation.CustomerNumber)]?.ToString()}.{reader[nameof(ActivityAggregation.AccountNumber)]?.ToString()}.{reader[nameof(ActivityAggregation.ActivityCode)]?.ToString()}.{reader[nameof(ActivityAggregation.Currency)]?.ToString()}").ToList();
                                    aggKey = reader[nameof(ActivityAggregation.EntityKey)]?.ToString();
                                    var currentMaxUpdateTime = DateTime.Parse(reader[nameof(ActivityAggregation.LastUpdated)]?.ToString());
                                    var maxUpdateTime = currentMaxUpdateTime;
                                   
                                    //var maxMonthUpdate = new DateTime(maxUpdateTime.Year, maxUpdateTime.Month, 1);
                                    var historicalString = reader[nameof(ActivityAggregation.DailyActivitySummary)]?.ToString();
                                    //string shortTermMemory = reader[nameof(ActivityAggregation.ShortTerm)]?.ToString();

                                    double maxVolumeThisMonth, minVolumeThisMonth, countThisMonth, totalThisMonth, avgThisMonth;

                                    foreach (var message in messagesForDBRecord.OrderBy(x => x.ActivityDate))
                                    {
                                        if (shortTermMemories.Contains(message.ActivityReference))
                                        {
                                            continue;
                                        }

                                        if (message.ActivityDate > maxUpdateTime) maxUpdateTime = message.ActivityDate;
                                        queryStrings += $"INSERT INTO ActivityShortTermMemoryTable (Ref, ActDate) VALUES('{message.ActivityReference}', '{message.ActivityDate.ToString("yyyy-MM-dd HH:mm:ss")}'); ";
                                        //shortTermMemory = _shortTermMemoryWriter.GetShortTermMemory(shortTermMemory,message, RefrenceDiscriminatorEnum.Activity);
                                        historicalString = GetDailyActivity(historicalString, message);
                                        dailyactivitySUmmary = historicalString;
                                    }

                                    //GetMonthlyComputations(messagesForDBRecord.OrderBy(x => x.ActivityDate).Last(), dailyactivitySUmmary, out maxVolumeThisMonth, out minVolumeThisMonth, out countThisMonth, out totalThisMonth, out avgThisMonth);

                                    query = $"UPDATE ActivityAggregationTable SET LastUpdated = '{maxUpdateTime.ToString("yyyy-MM-dd HH:mm:ss")}', DailyActivitySummary = '{dailyactivitySUmmary}'  where EntityKey='{aggKey}';";
                                    //query += $"UPDATE ActivityShortTermMemoryTable SET ShortTerm = '{shortTermMemory}' where EntityKey='{aggKey}';";

                                    queryStrings += query;
                                    treatedAggKeys.Add(reader[nameof(ActivityAggregation.EntityKey)]?.ToString());
                                }


                                // This code block is to deal with the queries that need to be inserted
                                // for new records
                                foreach (var uniqueMessageEntity in uniqueEntities)
                                {
                                    if (!treatedAggKeys.Any(x => x == uniqueMessageEntity))
                                    {
                                        queryStrings = GetInsertActivityQuery(messageETOs, shortTermMemories, out customerNumber, out activityId, out accountNumber, out activityCode, out currency, out activityName, out dailyactivitySUmmary, out lastUpdatedTime, out monthYear, out aggKey, queryStrings, uniqueMessageEntity, out globalActivityCode);
                                    }
                                }

                            }
                            reader.Close();
                        }

                        if (srcConn.State != ConnectionState.Open) { await srcConn.OpenAsync(); }

                        cmd.CommandText = queryStrings;
                        cmd.ExecuteNonQuery();
                        await srcConn.CloseAsync();
                    }

                    Console.WriteLine($"Processing Activity aggregations: {messageETOs.Count}");
                    await Task.CompletedTask;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        private string GetInsertActivityQuery(List<ActivityMessageETO> messageETOs, List<String> shortTermMemories, out string customerNumber, out string activityId, out string accountNumber, out string activityCode, out string currency, out string activityName, out string dailyactivitySUmmary, out DateTime lastUpdatedTime, out DateTime monthYear, out string aggKey, string queryStrings, string uniqueMessageEntity,out string globalActivityCode)
        {
            var messageList = messageETOs.Where(x => string.Concat(x.CustomerNumber, ".", x.AccountNumber, ".", x.ActivityCode, ".", x.ActivityCurrency) == uniqueMessageEntity).ToList();
            var lastMessage = messageList.OrderByDescending(x => x.ActivityDate).First();
            string shortTermMemory = string.Empty;

            aggKey = uniqueMessageEntity;
            customerNumber = lastMessage.CustomerNumber;
            accountNumber = lastMessage.AccountNumber;
            activityId = lastMessage.ActivityId;
            activityCode = lastMessage.ActivityCode;
            currency = lastMessage.ActivityCurrency;
            activityName = lastMessage.ActivityName;
            globalActivityCode = lastMessage.GlobalActivityCode;


            lastUpdatedTime = lastMessage.ActivityDate;

            monthYear = new DateTime(lastMessage.ActivityDate.Year, lastMessage.ActivityDate.Month, 1);

            dailyactivitySUmmary = "[]";
            var historicalInformation = "[]";

            foreach (var message in messageList.OrderBy(x => x.ActivityDate))
            {
                if (shortTermMemories.Contains(message.ActivityReference))
                {
                    continue;
                }
                //shortTermMemory = _shortTermMemoryWriter.GetShortTermMemory(shortTermMemory, message, RefrenceDiscriminatorEnum.Activity);
                queryStrings += $"INSERT INTO ActivityShortTermMemoryTable (Ref, ActDate) VALUES('{message.ActivityReference}', '{message.ActivityDate.ToString("yyyy-MM-dd HH:mm:ss")}'); ";
                historicalInformation = GetDailyActivity(historicalInformation, message);
            }

            dailyactivitySUmmary = historicalInformation;

            //double maxVolumeThisMonth, minVolumeThisMonth, countThisMonth, totalThisMonth, avgThisMonth;
           // GetMonthlyComputations(lastMessage, historicalInformation, out maxVolumeThisMonth, out minVolumeThisMonth, out countThisMonth, out totalThisMonth, out avgThisMonth);

            var query = $"INSERT INTO ActivityAggregationTable (EntityKey,CustomerNumber,AccountNumber,Currency,ActivityId,ActivityCode,ActivityName,DailyActivitySummary,LastUpdated,GlobalActivityCode) VALUES('{aggKey}','{customerNumber}','{accountNumber}','{currency}','{activityId}','{activityCode}','{activityName}','{dailyactivitySUmmary}','{lastUpdatedTime.ToString("yyyy-MM-dd HH:mm:ss")}','{globalActivityCode}');   ";
            //query += $"INSERT INTO ActivityShortTermMemoryTable (EntityKey, ShortTerm) VALUES('{aggKey}', '{shortTermMemory}'); ";

            queryStrings += query;
            return queryStrings;
        }

        public async Task WriteCustomerActivityDataAsync(List<ActivityMessageETO> messageETOs)
        {
            try
            {
                string customerNumber = string.Empty;
                string activityId = string.Empty;
                string accountNumber = string.Empty;
                string activityCode = string.Empty;
                string currency = string.Empty;
                string activityName = string.Empty;
                string globalActivityCode = string.Empty;
                string dailyactivitySUmmary = string.Empty;
                DateTime lastUpdatedTime = DateTime.Now;
                DateTime monthYear = DateTime.Now;
                string aggKey = string.Empty;
                string aggKeys = string.Empty;
                List<string> treatedAggKeys = new List<string>();
                string aggKeysList = string.Empty;
                string queryStrings = string.Empty;
                string finalQuery = string.Empty;
                List<string> uniqueEntities = new List<string>();
                string refrencesList = string.Empty;
                string refrences = string.Empty;
                List<string> shortTermMemories = new List<string>();
                double accumulatedFlowAmount = 0;
                int accumulatedCount = 0;




                // build list of aggKeys
                foreach (var item in messageETOs.DistinctBy(x => new { x.EntityKey, AggMonth = new DateOnly(x.ActivityDate.Year, x.ActivityDate.Month, 1).ToString("yyyy-MM-dd")} ))
                {
                    var aggMonth = new DateOnly(item.ActivityDate.Year, item.ActivityDate.Month, 1).ToString("yyyy-MM-dd");
                    aggKeysList += ($"'{item.EntityKey}.{aggMonth}',");
                    uniqueEntities.Add($"{item.EntityKey}.{aggMonth}");
                }

                foreach (var item in messageETOs.DistinctBy(x => x.ActivityReference))
                {
                    refrencesList += ($"'{item.ActivityReference}',");
                }

                aggKeys = aggKeysList[..^1];
                refrences = refrencesList[..^1];

                using (SqlConnection srcConn = new SqlConnection(sqlConnection.ConnectionString))
                {
                    using (SqlCommand cmd2 = new($"SELECT * FROM CustomerActivityShortTermMemoryTable WHERE Ref in ({refrences})", srcConn))
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


                    using (SqlCommand cmd = new($"SELECT * FROM CustomerActivityAggregationTable WHERE EntityKey in ({aggKeys})", srcConn))
                    {
                        if (srcConn.State != ConnectionState.Open) { await srcConn.OpenAsync(); }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // Do this if there are no records at all from the DB
                            // Get all the messages for the unique entities
                            // Then create a query string for those entites

                            if (!reader.HasRows)
                            {
                                foreach (var uniqueMessageEntity in uniqueEntities)
                                {
                                    queryStrings = GetInsertCustomerActivityQuery(messageETOs, shortTermMemories, out customerNumber, out activityId, out accountNumber, out activityCode, out currency, out activityName, out lastUpdatedTime, out monthYear, out aggKey, queryStrings, uniqueMessageEntity, out globalActivityCode);
                                }
                            }

                            // Do this if there are some records from the DB.
                            // Build an update query for the records that were fetched for the records that exist
                            // Check for the rest of the records that were not fetched and create an insert script for them.

                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    string query = string.Empty;
                                    var aggregationMonth = reader[nameof(CustomerActivityAggregation.AggregationMonth)]?.ToString();
                                    var messagesForDBRecord = messageETOs.Where(x => $"{x.EntityKey}.{new DateTime(x.ActivityDate.Year, x.ActivityDate.Month, 1).ToString("dd/MM/yyyy HH:mm:ss")}" == $"{reader[nameof(CustomerActivityAggregation.CustomerNumber)]?.ToString()}.{reader[nameof(CustomerActivityAggregation.AccountNumber)]?.ToString()}.{reader[nameof(CustomerActivityAggregation.ActivityCode)]?.ToString()}.{reader[nameof(CustomerActivityAggregation.Currency)]?.ToString()}.{aggregationMonth}").ToList();
                                    aggKey = reader[nameof(CustomerActivityAggregation.EntityKey)]?.ToString();
                                    var currentMaxUpdateTime = DateTime.Parse(reader[nameof(CustomerActivityAggregation.LastUpdated)]?.ToString());
                                    var maxUpdateTime = currentMaxUpdateTime;
                                    accumulatedFlowAmount = double.Parse(reader[nameof(CustomerActivityAggregation.TotalAmount)].ToString());
                                    accumulatedCount = int.Parse(reader[nameof(CustomerActivityAggregation.TotalCount)].ToString());



                                    //var maxMonthUpdate = new DateTime(maxUpdateTime.Year, maxUpdateTime.Month, 1);
                                    //var historicalString = reader[nameof(ActivityAggregation.DailyActivitySummary)]?.ToString();
                                    //string shortTermMemory = reader[nameof(ActivityAggregation.ShortTerm)]?.ToString();

                                    double maxVolumeThisMonth, minVolumeThisMonth, countThisMonth, totalThisMonth, avgThisMonth;

                                    foreach (var message in messagesForDBRecord.OrderBy(x => x.ActivityDate))
                                    {
                                        if (shortTermMemories.Contains(message.ActivityReference))
                                        {
                                            continue;
                                        }

                                        if (message.ActivityDate > maxUpdateTime) maxUpdateTime = message.ActivityDate;
                                        accumulatedFlowAmount += message.ActivityVolume;
                                        accumulatedCount += 1;
                                        queryStrings += $"INSERT INTO CustomerActivityShortTermMemoryTable (Ref, ActDate) VALUES('{message.ActivityReference}', '{message.ActivityDate.ToString("yyyy-MM-dd HH:mm:ss")}'); ";
                                        //shortTermMemory = _shortTermMemoryWriter.GetShortTermMemory(shortTermMemory,message, RefrenceDiscriminatorEnum.Activity);
                                        //historicalString = GetDailyActivity(historicalString, message);
                                        //dailyactivitySUmmary = historicalString;
                                    }

                                    //GetMonthlyComputations(messagesForDBRecord.OrderBy(x => x.ActivityDate).Last(), dailyactivitySUmmary, out maxVolumeThisMonth, out minVolumeThisMonth, out countThisMonth, out totalThisMonth, out avgThisMonth);

                                    query = $"UPDATE CustomerActivityAggregationTable SET LastUpdated = '{maxUpdateTime.ToString("yyyy-MM-dd HH:mm:ss")}', TotalAmount = {accumulatedFlowAmount}, TotalCount = {accumulatedCount}  where EntityKey='{aggKey}';";
                                    //query += $"UPDATE ActivityShortTermMemoryTable SET ShortTerm = '{shortTermMemory}' where EntityKey='{aggKey}';";

                                    queryStrings += query;
                                    treatedAggKeys.Add(reader[nameof(CustomerActivityAggregation.EntityKey)]?.ToString());
                                }


                                // This code block is to deal with the queries that need to be inserted
                                // for new records
                                foreach (var uniqueMessageEntity in uniqueEntities)
                                {
                                    if (!treatedAggKeys.Any(x => x == uniqueMessageEntity))
                                    {
                                        queryStrings = GetInsertCustomerActivityQuery(messageETOs, shortTermMemories, out customerNumber, out activityId, out accountNumber, out activityCode, out currency, out activityName, out lastUpdatedTime, out monthYear, out aggKey, queryStrings, uniqueMessageEntity, out globalActivityCode);
                                    }
                                }

                            }
                            reader.Close();
                        }

                        if (srcConn.State != ConnectionState.Open) { await srcConn.OpenAsync(); }

                        cmd.CommandText = queryStrings;
                        cmd.ExecuteNonQuery();
                        await srcConn.CloseAsync();
                    }

                    Console.WriteLine($"Processing Customer Activity aggregations: {messageETOs.Count}");
                    await Task.CompletedTask;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        private string GetInsertCustomerActivityQuery(List<ActivityMessageETO> messageETOs, List<String> shortTermMemories, out string customerNumber, out string activityId, out string accountNumber, out string activityCode, out string currency, out string activityName, out DateTime lastUpdatedTime, out DateTime monthYear, out string aggKey, string queryStrings, string uniqueMessageEntity, out string globalActivityCode)
        {
            var messageList = messageETOs.Where(x => string.Concat(x.CustomerNumber, ".", x.AccountNumber, ".", x.ActivityCode, ".", x.ActivityCurrency,".", new DateOnly(x.ActivityDate.Year, x.ActivityDate.Month, 1).ToString("yyyy-MM-dd")) == uniqueMessageEntity).ToList();
            var lastMessage = messageList.OrderByDescending(x => x.ActivityDate).First();
            string shortTermMemory = string.Empty;

            aggKey = uniqueMessageEntity;
            customerNumber = lastMessage.CustomerNumber;
            accountNumber = lastMessage.AccountNumber;
            activityId = lastMessage.ActivityId;
            activityCode = lastMessage.ActivityCode;
            currency = lastMessage.ActivityCurrency;
            activityName = lastMessage.ActivityName;
            globalActivityCode = lastMessage.GlobalActivityCode;
            double accumulatedFlowAmount = 0;
            int accumulatedCount = 0;



            lastUpdatedTime = lastMessage.ActivityDate;

            monthYear = new DateTime(lastMessage.ActivityDate.Year, lastMessage.ActivityDate.Month, 1);
            var aggMonth = new DateOnly(lastMessage.ActivityDate.Year, lastMessage.ActivityDate.Month, 1).ToString("yyyy-MM-dd");




            foreach (var message in messageList.OrderBy(x => x.ActivityDate))
            {
                if (shortTermMemories.Contains(message.ActivityReference))
                {
                    continue;
                }
                //shortTermMemory = _shortTermMemoryWriter.GetShortTermMemory(shortTermMemory, message, RefrenceDiscriminatorEnum.Activity);
                queryStrings += $"INSERT INTO CustomerActivityShortTermMemoryTable (Ref, ActDate) VALUES('{message.ActivityReference}', '{message.ActivityDate.ToString("yyyy-MM-dd HH:mm:ss")}'); ";
                accumulatedFlowAmount += message.ActivityVolume;
                accumulatedCount += 1;
                //historicalInformation = GetDailyActivity(historicalInformation, message);
            }

            //dailyactivitySUmmary = historicalInformation;

            //double maxVolumeThisMonth, minVolumeThisMonth, countThisMonth, totalThisMonth, avgThisMonth;
            // GetMonthlyComputations(lastMessage, historicalInformation, out maxVolumeThisMonth, out minVolumeThisMonth, out countThisMonth, out totalThisMonth, out avgThisMonth);

            var query = $"INSERT INTO CustomerActivityAggregationTable (EntityKey,CustomerNumber,AccountNumber,Currency,ActivityId,ActivityCode,ActivityName,LastUpdated,GlobalActivityCode, AggregationMonth, TotalAmount, TotalCount) VALUES('{aggKey}','{customerNumber}','{accountNumber}','{currency}','{activityId}','{activityCode}','{activityName}','{lastUpdatedTime.ToString("yyyy-MM-dd HH:mm:ss")}','{globalActivityCode}', '{aggMonth}', {accumulatedFlowAmount}, {accumulatedCount});   ";
            //query += $"INSERT INTO ActivityShortTermMemoryTable (EntityKey, ShortTerm) VALUES('{aggKey}', '{shortTermMemory}'); ";

            queryStrings += query;
            return queryStrings;
        }

        private static void GetMonthlyComputations(ActivityMessageETO lastMessage, string historicalInformation, out double maxVolumeThisMonth, out double minVolumeThisMonth, out double countThisMonth, out double totalThisMonth, out double avgThisMonth)
        {
            var listOfActivitySummary = JsonConvert.DeserializeObject<List<ActivitySummary>>(historicalInformation);

            var relevantSummary = listOfActivitySummary.Where(x => x.ActDate.Month == lastMessage.ActivityDate.Month && x.ActDate.Year == lastMessage.ActivityDate.Year).ToList();

            maxVolumeThisMonth = relevantSummary.Max(x => x.VolMax);
            minVolumeThisMonth = relevantSummary.Min(x => x.VolMin);
            countThisMonth = relevantSummary.Sum(x => x.Cnt);
            totalThisMonth = relevantSummary.Sum(x => x.Vol);
            avgThisMonth = Math.Round(relevantSummary.Sum(x => x.Vol) / relevantSummary.Sum(x => x.Cnt), 2);
        }

        private string GetDailyActivity(string historicalActivitiesString, ActivityMessageETO message)
        {
            int countToday = 1;
            double minToday = message.ActivityVolume;
            double maxToday = message.ActivityVolume;
            double avgToday = message.ActivityVolume;
            double totalToday = message.ActivityVolume;

            if (string.IsNullOrEmpty(historicalActivitiesString)) historicalActivitiesString = "[]";

            List<ActivitySummary> recentList = JsonConvert.DeserializeObject<List<ActivitySummary>>(historicalActivitiesString)!
                .Where(f => f.ActDate >= DateOnly.FromDateTime(message.ActivityDate).AddDays(-daysInHistory))
                .OrderByDescending(x => x.ActDate)
                .Take(daysInHistory)
                .ToList();

            recentList ??= new List<ActivitySummary>();

            var todayActivitySummary = recentList.FirstOrDefault(x => x.ActDate == DateOnly.FromDateTime(message.ActivityDate));

            if (todayActivitySummary != null)
            {
                countToday = todayActivitySummary.Cnt + 1;
                maxToday = Math.Max(todayActivitySummary.VolMax, maxToday);
                minToday = Math.Min(todayActivitySummary.VolMin, minToday);
                totalToday = totalToday + todayActivitySummary.Vol;
                avgToday = Math.Round(totalToday / countToday, 2);
                recentList.Remove(todayActivitySummary);
            }

            recentList.Add(new ActivitySummary()
            {
                ActDate = DateOnly.FromDateTime(new DateTime(message.ActivityDate.Year, message.ActivityDate.Month, message.ActivityDate.Day)),
                Cnt = countToday,
                Vol = totalToday,
                VolAvg = avgToday,
                VolMax = maxToday,
                VolMin = minToday
            });
            
            return JsonConvert.SerializeObject(recentList.OrderBy(x => x.ActDate).ToList());
        }










        public async Task WriteStreamDataAsync(List<ActivityMessageETO> messageETOs)
        {
            try
            {
                string aggKey = string.Empty;
                string customerNumber = string.Empty;
                string accountNumber = string.Empty;
                string activityId = string.Empty;
                string streamId = string.Empty;
                string activityCode = string.Empty;
                string streamName = string.Empty;
                string currency = string.Empty;
                string dailyStreamSummary = "[]";
                string aggKeysList = string.Empty;
                string aggKeys = string.Empty;
                DateTime lastUpdatedTime = DateTime.Now;
                List<string> uniqueEntities = new List<string>();
                List<string> treatedAggKeys = new List<string>();
                string queryList = string.Empty;
                string refrencesList = string.Empty;
                string refrences = string.Empty;
                List<string> shortTermMemories = new List<string>();

                // build list of aggKeys
                foreach (var item in messageETOs.DistinctBy(x => new { x.AccountNumber, x.StreamId, x.StreamCurrency }))
                {
                    aggKeysList += $"'{item.CustomerNumber}.{item.AccountNumber}.{item.StreamId}.{item.StreamCurrency}',";
                    uniqueEntities.Add($"{item.CustomerNumber}.{item.AccountNumber}.{item.StreamId}.{item.StreamCurrency}");
                }

                foreach (var item in messageETOs.DistinctBy(x => x.StreamReference))
                {
                    refrencesList += ($"'{item.StreamReference}',");
                }

                aggKeys = aggKeysList[..^1];
                refrences = refrencesList[..^1];

                using (SqlConnection srcConn = new SqlConnection(sqlConnection.ConnectionString))
                {
                    using (SqlCommand cmd2 = new($"SELECT * FROM StreamShortTermMemoryTable WHERE Ref in ({refrences})", srcConn))
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

                    using (SqlCommand cmd = new($"SELECT * FROM StreamAggregationTable WHERE EntityKey in ({aggKeys})", srcConn))
                    {
                        if (srcConn.State != ConnectionState.Open) { await srcConn.OpenAsync(); }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                foreach (var uniqueMessageEntity in uniqueEntities)
                                {
                                    queryList = GetInsertStreamQuery(messageETOs, shortTermMemories, out customerNumber, out activityId, out streamId, out accountNumber, out activityCode, out currency, out streamName, out dailyStreamSummary, out lastUpdatedTime, out aggKey, queryList, uniqueMessageEntity);

                                    /*var messageList = messageETOs.Where(x => string.Concat(x.CustomerNumber, ".", x.AccountNumber, ".", x.StreamId, ".", x.StreamCurrency) == uniqueMessageEntity).ToList();
                                    var lastMessage = messageList.OrderByDescending(x => x.StreamDate).First();
                                    //var shortTermMemory = string.Empty;

                                    aggKey = uniqueMessageEntity;
                                    customerNumber = lastMessage.CustomerNumber;
                                    accountNumber = lastMessage.AccountNumber;
                                    activityId = lastMessage.ActivityId;
                                    activityCode = lastMessage.ActivityCode;
                                    currency = lastMessage.StreamCurrency;
                                    lastUpdatedTime = lastMessage.StreamDate;
                                    streamName = lastMessage.StreamName;
                                    streamId = lastMessage.StreamId;
                                    var historicalString = "[]";

                                    foreach (var message in messageList.OrderBy(x => x.StreamDate))
                                    {
                                        if (shortTermMemories.Contains(message.StreamReference))
                                        {
                                            continue;
                                        }
                                        queryList += $"INSERT INTO StreamShortTermMemoryTable (Ref, ActDate) VALUES('{message.StreamReference}', '{message.StreamDate.ToString("yyyy-MM-dd HH:mm:ss")}'); ";

                                        //shortTermMemory = _shortTermMemoryWriter.GetShortTermMemory(shortTermMemory, message, RefrenceDiscriminatorEnum.Stream);   
                                        historicalString = GetDailyStream(historicalString,message);
                                    }

                                    dailyStreamSummary = historicalString;
                                    var query = $"INSERT INTO StreamAggregationTable(EntityKey,CustomerNumber,AccountNumber,StreamCurrency,StreamId,ActivityId , ActivityCode , DailyStreamSummary , LastUpdated, StreamName) VALUES ('{uniqueMessageEntity}','{customerNumber}','{accountNumber}','{currency}','{streamId}','{activityId}','{activityCode}','{dailyStreamSummary}','{lastUpdatedTime.ToString("yyyy-MM-dd HH:mm:ss")}', '{streamName}');";
                                    //query += $"INSERT INTO StreamShortTermMemoryTable (EntityKey, ShortTerm) VALUES('{aggKey}', '{shortTermMemory}'); ";

                                    queryList += query;*/
                                }
                            }


                            if (reader.HasRows) { 

                                while(reader.Read())
                                {
                                    string query = string.Empty;
                                    var messagesForDBRecord = messageETOs.Where(x => x.CustomerNumber == reader[nameof(StreamAggregation.CustomerNumber)]?.ToString() && x.AccountNumber == reader[nameof(StreamAggregation.AccountNumber)]?.ToString() && x.ActivityCode == reader[nameof(StreamAggregation.ActivityCode)]?.ToString() && x.StreamCurrency == reader[nameof(StreamAggregation.StreamCurrency)]?.ToString()).ToList();
                                    aggKey = reader[nameof(StreamAggregation.EntityKey)]?.ToString();
                                    lastUpdatedTime = messagesForDBRecord.Max(x => x.StreamDate);
                                    var historyStream = reader[nameof(StreamAggregation.DailyStreamSummary)]?.ToString();
                                   //var shortTermMemory = reader[nameof(StreamAggregation.ShortTerm)]?.ToString();

                                    foreach (var message in messagesForDBRecord)
                                    {
                                        if (shortTermMemories.Contains(message.StreamReference))
                                        {
                                            continue;
                                        }

                                        queryList += $"INSERT INTO StreamShortTermMemoryTable (Ref, ActDate) VALUES('{message.StreamReference}', '{message.StreamDate.ToString("yyyy-MM-dd HH:mm:ss")}'); ";
                                        //shortTermMemory = _shortTermMemoryWriter.GetShortTermMemory(shortTermMemory, message, RefrenceDiscriminatorEnum.Stream);
                                        historyStream = GetDailyStream(historyStream, message);
                                    }
                                    dailyStreamSummary = historyStream;

                                    query = $"UPDATE StreamAggregationTable SET LastUpdated = '{lastUpdatedTime.ToString("yyyy-MM-dd HH:mm:ss")}', DailyStreamSummary = '{dailyStreamSummary}' WHERE EntityKey = '{aggKey}';";
                                    //query += $"UPDATE StreamShortTermMemoryTable SET ShortTerm = '{shortTermMemory}' where EntityKey='{aggKey}';";

                                    queryList += query;
                                    treatedAggKeys.Add(reader[nameof(StreamAggregation.EntityKey)]?.ToString());
                                }

                                foreach (var uniqueMessageEntity in uniqueEntities)
                                {
                                    if (!treatedAggKeys.Any(x => x == uniqueMessageEntity))
                                    {
                                        queryList = GetInsertStreamQuery(messageETOs, shortTermMemories, out customerNumber, out activityId, out streamId, out accountNumber, out activityCode, out currency, out streamName, out dailyStreamSummary, out lastUpdatedTime, out aggKey, queryList, uniqueMessageEntity);

                                    }
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

        public async Task WriteCustomerStreamDataAsync(List<ActivityMessageETO> messageETOs)
        {
            try
            {
                string aggKey = string.Empty;
                string customerNumber = string.Empty;
                string accountNumber = string.Empty;
                string activityId = string.Empty;
                string streamId = string.Empty;
                string activityCode = string.Empty;
                string streamName = string.Empty;
                string currency = string.Empty;
                string dailyStreamSummary = "[]";
                string aggKeysList = string.Empty;
                string aggKeys = string.Empty;
                DateTime lastUpdatedTime = DateTime.Now;
                List<string> uniqueEntities = new List<string>();
                List<string> treatedAggKeys = new List<string>();
                string queryList = string.Empty;
                string refrencesList = string.Empty;
                string refrences = string.Empty;
                double accumulatedFlowAmount = 0;
                int accumulatedCount = 0;
                int streamType;
                List<string> shortTermMemories = new List<string>();

                // build list of aggKeys
                foreach (var item in messageETOs.DistinctBy(x => new { x.AccountNumber, x.StreamId, x.StreamCurrency, AggMonth = new DateOnly(x.StreamDate.Year, x.StreamDate.Month, 1).ToString("yyyy-MM-dd") }))
                {
                    var aggMonth = new DateOnly(item.StreamDate.Year, item.StreamDate.Month, 1).ToString("yyyy-MM-dd");
                    aggKeysList += $"'{item.CustomerNumber}.{item.AccountNumber}.{item.StreamId}.{item.StreamCurrency}.{aggMonth}',";
                    uniqueEntities.Add($"{item.CustomerNumber}.{item.AccountNumber}.{item.StreamId}.{item.StreamCurrency}.{aggMonth}");
                }

                foreach (var item in messageETOs.DistinctBy(x => x.StreamReference))
                {
                    refrencesList += ($"'{item.StreamReference}',");
                }

                aggKeys = aggKeysList[..^1];
                refrences = refrencesList[..^1];

                using (SqlConnection srcConn = new SqlConnection(sqlConnection.ConnectionString))
                {
                    using (SqlCommand cmd2 = new($"SELECT * FROM CustomerStreamShortTermMemoryTable WHERE Ref in ({refrences})", srcConn))
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

                    using (SqlCommand cmd = new($"SELECT * FROM CustomerStreamAggregationTable WHERE EntityKey in ({aggKeys})", srcConn))
                    {
                        if (srcConn.State != ConnectionState.Open) { await srcConn.OpenAsync(); }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                foreach (var uniqueMessageEntity in uniqueEntities)
                                {
                                    queryList = GetInsertCustomerStreamQuery(messageETOs, shortTermMemories, out customerNumber, out activityId, out streamId, out accountNumber, out activityCode, out currency, out streamName, out streamType, out lastUpdatedTime, out aggKey, queryList, uniqueMessageEntity);
                                }
                            }


                            if (reader.HasRows)
                            {

                                while (reader.Read())
                                {
                                    string query = string.Empty;
                                    var aggregationMonth = reader[nameof(CustomerStreamAggregation.AggregationMonth)]?.ToString();
                                    var messagesForDBRecord = messageETOs.Where(x => x.CustomerNumber == reader[nameof(CustomerStreamAggregation.CustomerNumber)]?.ToString() && x.AccountNumber == reader[nameof(CustomerStreamAggregation.AccountNumber)]?.ToString() && x.StreamCurrency == reader[nameof(CustomerStreamAggregation.StreamCurrency)]?.ToString() && new DateTime(x.StreamDate.Year, x.StreamDate.Month, 1).ToString("dd/MM/yyyy HH:mm:ss") == aggregationMonth).ToList();
                                    aggKey = reader[nameof(CustomerStreamAggregation.EntityKey)]?.ToString();
                                    accumulatedFlowAmount = double.Parse(reader[nameof(CustomerStreamAggregation.TotalAmount)].ToString());
                                    accumulatedCount = int.Parse(reader[nameof(CustomerStreamAggregation.TotalCount)].ToString());

                                    lastUpdatedTime = messagesForDBRecord.Max(x => x.StreamDate);

                                    foreach (var message in messagesForDBRecord)
                                    {
                                        if (shortTermMemories.Contains(message.StreamReference))
                                        {
                                            continue;
                                        }

                                        queryList += $"INSERT INTO CustomerStreamShortTermMemoryTable (Ref, ActDate) VALUES('{message.StreamReference}', '{message.StreamDate.ToString("yyyy-MM-dd HH:mm:ss")}'); ";

                                        accumulatedFlowAmount += message.ActualFlow;
                                        accumulatedCount += 1;
                                    }

                                    query = $"UPDATE CustomerStreamAggregationTable SET LastUpdated = '{lastUpdatedTime.ToString("yyyy-MM-dd HH:mm:ss")}', TotalAmount = {accumulatedFlowAmount}, TotalCount = {accumulatedCount} WHERE EntityKey = '{aggKey}';";

                                    queryList += query;
                                    treatedAggKeys.Add(reader[nameof(CustomerStreamAggregation.EntityKey)]?.ToString());

                                }

                                foreach (var uniqueMessageEntity in uniqueEntities)
                                {
                                    if (!treatedAggKeys.Any(x => x == uniqueMessageEntity))
                                    {
                                        queryList = GetInsertCustomerStreamQuery(messageETOs, shortTermMemories, out customerNumber, out activityId, out streamId, out accountNumber, out activityCode, out currency, out streamName, out streamType, out lastUpdatedTime, out aggKey, queryList, uniqueMessageEntity);
                                    }
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

        private string GetInsertStreamQuery(List<ActivityMessageETO> messageETOs, List<String> shortTermMemories, out string customerNumber, out string activityId, out string streamId,  out string accountNumber, out string activityCode, out string currency, out string streamName, out string dailyStreamSummary, out DateTime lastUpdatedTime, out string aggKey, string queryList, string uniqueMessageEntity)
        {
                var messageList = messageETOs.Where(x => string.Concat(x.CustomerNumber, ".", x.AccountNumber, ".", x.StreamId, ".", x.StreamCurrency) == uniqueMessageEntity).ToList();
                var lastMessage = messageList.OrderByDescending(x => x.StreamDate).First();
                //var shortTermMemory = string.Empty;

                aggKey = uniqueMessageEntity;
                customerNumber = lastMessage.CustomerNumber;
                accountNumber = lastMessage.AccountNumber;
                activityId = lastMessage.ActivityId;
                activityCode = lastMessage.ActivityCode;
                currency = lastMessage.StreamCurrency;
                lastUpdatedTime = lastMessage.StreamDate;
                streamName = lastMessage.StreamName;
                streamId = lastMessage.StreamId;
                var historicalString = "[]";
                dailyStreamSummary = "[]";



            foreach (var message in messageList.OrderBy(x => x.StreamDate))
                {
                    if (shortTermMemories.Contains(message.StreamReference))
                    {
                        continue;
                    }
                    queryList += $"INSERT INTO StreamShortTermMemoryTable (Ref, ActDate) VALUES('{message.StreamReference}', '{message.StreamDate.ToString("yyyy-MM-dd HH:mm:ss")}'); ";

                    //shortTermMemory = _shortTermMemoryWriter.GetShortTermMemory(shortTermMemory, message, RefrenceDiscriminatorEnum.Stream);   
                    historicalString = GetDailyStream(historicalString, message);
                }

                dailyStreamSummary = historicalString;
                var query = $"INSERT INTO StreamAggregationTable(EntityKey,CustomerNumber,AccountNumber,StreamCurrency,StreamId,ActivityId , ActivityCode , DailyStreamSummary , LastUpdated, StreamName) VALUES ('{uniqueMessageEntity}','{customerNumber}','{accountNumber}','{currency}','{streamId}','{activityId}','{activityCode}','{dailyStreamSummary}','{lastUpdatedTime.ToString("yyyy-MM-dd HH:mm:ss")}', '{streamName}');";
                //query += $"INSERT INTO StreamShortTermMemoryTable (EntityKey, ShortTerm) VALUES('{aggKey}', '{shortTermMemory}'); ";

                queryList += query;

                return queryList;
        }

        private string GetInsertCustomerStreamQuery(List<ActivityMessageETO> messageETOs, List<String> shortTermMemories, out string customerNumber, out string activityId, out string streamId, out string accountNumber, out string activityCode, out string currency, out string streamName, out int streamType, out DateTime lastUpdatedTime, out string aggKey, string queryList, string uniqueMessageEntity)
        {
            var messageList = messageETOs.Where(x => string.Concat(x.CustomerNumber, ".", x.AccountNumber, ".", x.StreamId, ".", x.StreamCurrency, ".", new DateOnly(x.StreamDate.Year, x.StreamDate.Month, 1).ToString("yyyy-MM-dd")) == uniqueMessageEntity).ToList();
            var lastMessage = messageList.OrderByDescending(x => x.StreamDate).First();

            aggKey = uniqueMessageEntity;
            customerNumber = lastMessage.CustomerNumber;
            accountNumber = lastMessage.AccountNumber;
            activityId = lastMessage.ActivityId;
            activityCode = lastMessage.ActivityCode;
            currency = lastMessage.StreamCurrency;
            lastUpdatedTime = lastMessage.StreamDate;
            streamId = lastMessage.StreamId;
            streamName = lastMessage.StreamName;
            double accumulatedFlowAmount = 0;
            int accumulatedCount = 0;
            streamType = (int)lastMessage.StreamType;

            var aggMonth = new DateOnly(lastMessage.StreamDate.Year, lastMessage.StreamDate.Month, 1).ToString("yyyy-MM-dd");



            foreach (var message in messageList.OrderBy(x => x.StreamDate))
            {
                if (shortTermMemories.Contains(message.StreamReference))
                {
                    continue;
                }
                queryList += $"INSERT INTO CustomerStreamShortTermMemoryTable (Ref, ActDate) VALUES('{message.StreamReference}', '{message.StreamDate.ToString("yyyy-MM-dd HH:mm:ss")}'); ";

                accumulatedFlowAmount += message.ActualFlow;
                accumulatedCount += 1;
            }

            var query = $"INSERT INTO CustomerStreamAggregationTable(EntityKey,CustomerNumber,AccountNumber,StreamCurrency,StreamId,ActivityId , TotalAmount, LastUpdated, StreamType, AggregationMonth, StreamName, TotalCount) VALUES ('{uniqueMessageEntity}','{customerNumber}','{accountNumber}','{currency}','{streamId}','{activityId}', {accumulatedFlowAmount} ,'{lastUpdatedTime.ToString("yyyy-MM-dd HH:mm:ss")}', {streamType}, '{aggMonth}', '{streamName}', {accumulatedCount});";

            queryList += query;
            return queryList;
        }
        private string GetDailyStream(string historicalstreamString, ActivityMessageETO message)
        {
            int countToday = 1;
            double actualFlowAmount = message.ActualFlow;
            double expectedFlowAmount = message.ExpectedFlowAmount;
            double conceededFlowAmount = message.ConceededFlowAmount;

            if (string.IsNullOrEmpty(historicalstreamString)) historicalstreamString = "[]";

            DateTime cutOffDate = message.StreamDate.AddDays(-daysInHistory);
            DateOnly cutOffDateOnly = DateOnly.FromDateTime(cutOffDate);

            List<StreamSummary> recentList = JsonConvert.DeserializeObject<List<StreamSummary>>(historicalstreamString)!
                .Where(f => f.FlowDt >= cutOffDateOnly)
                .OrderByDescending(x => x.FlowDt)
                .Take(daysInHistory)
                .ToList();

            recentList ??= new List<StreamSummary>();

            var todayStreamSummary = recentList.FirstOrDefault(x => x.FlowDt == DateOnly.FromDateTime(message.StreamDate));

            if (todayStreamSummary != null)
            {
                countToday = todayStreamSummary.Cnt + 1;
                actualFlowAmount += todayStreamSummary.ActAmt;
                expectedFlowAmount += todayStreamSummary.ExpAmt;
                conceededFlowAmount += todayStreamSummary.ConcAmt;
                recentList.Remove(todayStreamSummary);
            }

            recentList.Add(new StreamSummary()
            {
                FlowDt = DateOnly.FromDateTime(message.StreamDate),
                Cnt = countToday,
                ExpAmt = expectedFlowAmount,
                ActAmt = actualFlowAmount,
                ConcAmt = conceededFlowAmount
            });
            return JsonConvert.SerializeObject(recentList.OrderBy(x => x.FlowDt).ToList());
        }


        public async Task ShrinkDbLogFile()
        {
            var sixMonthsAgo = DateTime.Now.AddDays(-180).ToString();

            string query = $@"DECLARE @dbname VARCHAR(MAX) = '{sqlConnection.Database}';
                            DECLARE @ScriptToExecute VARCHAR(MAX);
                            SET @ScriptToExecute = 'ALTER DATABASE ' + @dbname + ' SET RECOVERY SIMPLE;';
                            EXEC (@ScriptToExecute)
                            SET @ScriptToExecute= '';
                            SELECT @ScriptToExecute = @ScriptToExecute + 'USE '+ QUOTENAME(d.name) + '; CHECKPOINT; DBCC SHRINKFILE ('+ QUOTENAME(f.name) +');'
                            FROM sys.master_files f INNER JOIN sys.databases d ON d.database_id = f.database_id
                            WHERE f.type = 1 AND d.database_id > 4 and d.state = 0 AND d.name = @dbname 
                            SELECT @ScriptToExecute ScriptToExecute
                            EXEC (@ScriptToExecute)
                            SET @ScriptToExecute = 'ALTER DATABASE ' + @dbname + ' SET RECOVERY FULL;';
                            EXEC (@ScriptToExecute)
							SET @ScriptToExecute = 'DELETE FROM ' + @dbname + '.ActivityAggregationTable WHERE LastUpdated < ' + '{sixMonthsAgo};'
                            EXEC (@ScriptToExecute)";

            await ExecuteNonQueryAsync(query);
        }

        public async Task<DataTable> ExecuteQueryAsync(string query)
        {
            OpenConnection();
            sqlCommand.Parameters.Clear();
            sqlCommand.CommandText = query;
            DataTable dt = new();
            sqlDataAdapter.Fill(dt);
            CloseConnection();
            return await Task.FromResult(dt);
        }
    }
}
