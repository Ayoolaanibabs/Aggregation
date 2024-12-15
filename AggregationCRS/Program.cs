using AggregationCRS.Domain.Managers;
using AggregationCRS.Domain.Repository;
using AggregationCRS.EntityFramework;
using AggregationCRS.SQLClient;
using Confluent.Kafka;
using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.EntityFrameworkCore;
using Serilog.Events;
using Serilog;
using KafkaFlow.Admin.Dashboard;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Meta;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

const string internalTopic = "internalTopic";
const string activityTopicPrefix = "activity-topic-";
const string streamTopicPrefix = "stream-topic-";
const string producerName = "transactionProducer";
const string adminTopicName = "adminTopic";
const string monthlyStatusTopic = "monthlyStatusTopic";
const string computationCompletedTopic = "computationCompletedTopic";
const string concessionUtilizedTopic = "concessionUtilizedTopic";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Error)
    .Enrich.FromLogContext()
    .WriteTo.Async(c => c.File("Logs/logs.txt", LogEventLevel.Verbose,
    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
    null, 10485760, null, false, false, null, RollingInterval.Infinite, true, 10))
    .WriteTo.Async(c => c.Console())
    .CreateLogger();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IDataWriter, DataWriter>();
builder.Services.AddTransient<IShortTermMemoryWriter, ShortTermMemoryWriter>();
builder.Services.AddTransient < IGLSummaryWriter,GLSummaryWriter> ();
builder.Services.AddTransient < IBackgroundJobWriter,BackgroundJobWriter> ();
builder.Services.AddTransient <IConcessionUtilizationWriter, ConcessionUtilizationWriter> ();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
//builder.Services.AddHangfire(x => x.UseSqlServerStorage(builder.Configuration.GetConnectionString("HangFire")));
//builder.Services.AddHangfireServer();



//  Used for testing purposes
//builder.Services.AddHostedService<RandomTransactionGenerator>();

var settings = builder.Configuration.GetSection("Kafka:Connections:Default");
var producerConfig = new ProducerConfig
{
    BootstrapServers = settings["BootstrapServers"],
    SaslUsername = settings["SaslUsername"] ?? null,
    SaslPassword = settings["SaslPassword"] ?? null,
    SaslMechanism = SaslMechanism.Plain,
    SslCaLocation = settings["SslCaLocation"] ?? null,
    ApiVersionRequest = bool.Parse(settings["ApiVersionRequest"])
};



builder.Services.AddKafka(kafka => kafka
    .UseConsoleLog()
    .AddCluster(cluster => cluster
        .WithBrokers(new[] { producerConfig.BootstrapServers })
        .CreateTopicIfNotExists(internalTopic, 6, 1)
        .CreateTopicIfNotExists(monthlyStatusTopic, 12, 1)
        .AddConsumer(consumer => consumer
           .Topic($"^{activityTopicPrefix}*") // Any topic starting with `activity-topic-*` 
           //.Topic($"^{streamTopicPrefix}*")
           .WithGroupId("CRSAggregationConsumerActivity")
            .WithBufferSize(10)
            .WithWorkersCount(int.Parse(settings["WorkerCount"] ?? "10"))
            .WithAutoOffsetReset(KafkaFlow.AutoOffsetReset.Earliest)
            .WithConsumerConfig(new ConsumerConfig()
            {
                TopicMetadataRefreshIntervalMs = 60000 // discover new topics every 1min to milliseconds
            })
            .AddMiddlewares(middlewares => middlewares
                .AddDeserializer<JsonCoreDeserializer>()
                .AddBatching(int.Parse(settings["BatchCount"] ?? "100"), TimeSpan.FromSeconds(int.Parse(settings["BatchTimeInSeconds"] ?? "10")))
                .Add<HandleAggregation>()))
                .AddConsumer(consumer => consumer
           .Topic($"^{streamTopicPrefix}*")
           .WithGroupId("CRSAggregationConsumerStream")
            .WithBufferSize(10)
            .WithWorkersCount(int.Parse(settings["WorkerCount"] ?? "10"))
            .WithAutoOffsetReset(KafkaFlow.AutoOffsetReset.Earliest)
            .WithConsumerConfig(new ConsumerConfig()
            {
                TopicMetadataRefreshIntervalMs = 60000 // discover new topics every 1min to milliseconds
            })
            .AddMiddlewares(middlewares => middlewares
                .AddDeserializer<JsonCoreDeserializer>()
                .AddBatching(int.Parse(settings["BatchCount"] ?? "100"), TimeSpan.FromSeconds(int.Parse(settings["BatchTimeInSeconds"] ?? "10")))
                .Add<HandleStreamAggregation>()))
                .AddConsumer(consumer => consumer
           .Topic($"{computationCompletedTopic}")
           .WithGroupId("CRSGLSummaryConsumer")
            .WithBufferSize(10)
            .WithWorkersCount(1)
            .WithAutoOffsetReset(KafkaFlow.AutoOffsetReset.Earliest)
            .WithConsumerConfig(new ConsumerConfig()
            {
                TopicMetadataRefreshIntervalMs = 60000 // discover new topics every 1min to milliseconds
            })
            .AddMiddlewares(middlewares => middlewares
                .AddDeserializer<JsonCoreDeserializer>()
                .AddBatching(int.Parse(settings["BatchCount"] ?? "1000"), TimeSpan.FromSeconds(int.Parse(settings["BatchTimeInSeconds"] ?? "10")))
                .Add<GLSummaryHandleAggregation>()))
                                .AddConsumer(consumer => consumer
           .Topic($"{concessionUtilizedTopic}")
           .WithGroupId("CRSConcessionConsumer")
            .WithBufferSize(10)
            .WithWorkersCount(1)
            .WithAutoOffsetReset(KafkaFlow.AutoOffsetReset.Earliest)
            .WithConsumerConfig(new ConsumerConfig()
            {
                TopicMetadataRefreshIntervalMs = 60000 // discover new topics every 1min to milliseconds
            })
            .AddMiddlewares(middlewares => middlewares
                .AddDeserializer<JsonCoreDeserializer>()
                .AddBatching(int.Parse(settings["BatchCount"] ?? "1000"), TimeSpan.FromSeconds(int.Parse(settings["BatchTimeInSeconds"] ?? "10")))
                .Add<ConcessionUtilizationAggregation>()))
        .EnableTelemetry(adminTopicName)
        .EnableAdminMessages(adminTopicName)
        .AddProducer(producerName, producer => producer
                .DefaultTopic(internalTopic)
               .AddMiddlewares(middlewares => middlewares.AddSerializer<JsonCoreSerializer>()))));

builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("Default"), healthQuery: "Select 1")
    .AddKafka(producerConfig);


builder.Services.AddHealthChecksUI(options =>
{
    options.SetEvaluationTimeInSeconds(60);
    options.MaximumHistoryEntriesPerEndpoint(60);
    options.SetApiMaxActiveRequests(1);
    options.AddHealthCheckEndpoint("HealthCheck API", "/health");
}).AddInMemoryStorage();


builder.Host.UseSerilog();

var app = builder.Build();

//app.UseHangfireDashboard();
//RecurringJob.AddOrUpdate<IBackgroundJobWriter>("ClearActivityShortTermMemoryTable", x => x.ClearActivityShortTermMemoryTable(), "0 1 * * *");
//RecurringJob.AddOrUpdate<IBackgroundJobWriter>("ClearConcessionUtilizationSummaryShortTermMemoryTable", x => x.ClearConcessionUtilizationSummaryShortTermMemoryTable(), "0 1 * * *");
//RecurringJob.AddOrUpdate<IBackgroundJobWriter>("ClearCustomerActivityAggregationTable", x => x.ClearCustomerActivityAggregationTable(), "0 1 * * *");
//RecurringJob.AddOrUpdate<IBackgroundJobWriter>("ClearCustomerActivityShortTermMemoryTable", x => x.ClearCustomerActivityShortTermMemoryTable(), "0 1 * * *");
//RecurringJob.AddOrUpdate<IBackgroundJobWriter>("ClearCustomerStreamAggregationTable", x => x.ClearCustomerStreamAggregationTable(), "0 1 * * *");
//RecurringJob.AddOrUpdate<IBackgroundJobWriter>("ClearCustomerStreamShortTermMemoryTable", x => x.ClearCustomerStreamShortTermMemoryTable(), "0 1 * * *");
//RecurringJob.AddOrUpdate<IBackgroundJobWriter>("ClearDailyGLSummaryShortTermMemoryTable", x => x.ClearDailyGLSummaryShortTermMemoryTable(), "0 1 * * *");
//RecurringJob.AddOrUpdate<IBackgroundJobWriter>("ClearGLSummaryShortTermMemoryTable", x => x.ClearGLSummaryShortTermMemoryTable(), "0 1 * * *");
//RecurringJob.AddOrUpdate<IBackgroundJobWriter>("ClearStreamShortTermMemoryTable", x => x.ClearStreamShortTermMemoryTable(), "0 1 * * *");


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
    app.UseSwaggerUI();
//}

app.MapControllers();
app.UseKafkaFlowDashboard();

app.UseHttpsRedirection();

app.UseAuthorization();

var kafkaBus = app.Services.CreateKafkaBus();
app.Lifetime.ApplicationStarted.Register(() => kafkaBus.StartAsync());

app.Run();