using System.Diagnostics.Metrics;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;

var builder = WebApplication.CreateBuilder(args);

// Define the service name and version for OpenTelemetry resources
const string serviceName = "ServiceA";
const string serviceVersion = "1.0.0";

// Configure OpenTelemetry Resources
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(serviceName: serviceName, serviceVersion: serviceVersion);

// Add logging with OpenTelemetry
builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(resourceBuilder)
        .AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Endpoint = new Uri("http://40.71.71.152:4317"); // OTLP endpoint for logs
            otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        })
        .AddConsoleExporter(); // Export logs to the console
});

// Add OpenTelemetry services (traces, metrics)
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName, serviceVersion))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation() // Instrument ASP.NET Core requests
        .AddHttpClientInstrumentation() // Instrument HTTP requests
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://40.71.71.152:4317"); // OTLP endpoint for traces
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        })
        .AddConsoleExporter()) // Export traces to the console
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation() // Instrument ASP.NET Core for metrics
        .AddMeter("Microsoft.AspNetCore.Hosting")
        .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
        .AddPrometheusExporter() // Expose metrics for Prometheus
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://40.71.71.152:4317"); // OTLP endpoint for metrics
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        }));

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddHttpClient("backend", client =>
{
    client.BaseAddress = new Uri("http://172.173.135.32:8081/weatherforecastbackend"); // Use the backend URL
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Custom metrics for the application
var appMeter = new Meter(serviceName, serviceVersion);
var requestCounter = appMeter.CreateCounter<int>("app.request.count", description: "Counts the number of requests");

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// Configure the Prometheus scraping endpoint
app.MapPrometheusScrapingEndpoint();

// Add a simple endpoint for testing
app.MapGet("/hello", (ILogger<Program> logger) =>
{
    requestCounter.Add(1); // Increment the request counter
    logger.LogInformation("Hello endpoint called");

    return "Hello, World!";
});

app.Run();

