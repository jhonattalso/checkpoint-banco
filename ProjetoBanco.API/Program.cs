using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;
using ProjetoBanco.API.Data;
using ProjetoBanco.API.Messaging;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/projetobanco-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"),
    preserveStaticLogger: true
);

var isTest = builder.Environment.IsEnvironment("Testing");

if (isTest)
    builder.Services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("TestDb"));
else
    builder.Services.AddDbContext<AppDbContext>(o =>
        o.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));

var health = builder.Services.AddHealthChecks();
if (!isTest)
    health.AddDbContextCheck<AppDbContext>("oracle-db");

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("ProjetoBanco.API"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpExporter(o => 
        {
            o.Endpoint = new Uri(
                builder.Configuration["OpenTelemetry:JaegerEndpoint"] ?? "http://localhost:4317");
            o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        })
        .AddConsoleExporter()
    );

builder.Services.AddSingleton<IContratacaoProducer, ContratacaoProducer>();

if (!isTest)
    builder.Services.AddHostedService<ContratacaoConsumer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.SwaggerDoc("v1", new() { Title = "Projeto Banco API", Version = "v1" }));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapHealthChecks("/health");
app.UseSerilogRequestLogging();
app.MapControllers();

app.Run();

public partial class Program { }