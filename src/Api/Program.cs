using Ammons.DataLabs.DocsService.Endpoints;
using Ammons.DataLabs.DocsService.Services;
using Ammons.DataLabs.DocsService.Configuration;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add ProblemDetails support (RFC 7807)
builder.Services.AddProblemDetailsWithTracing(builder.Environment);

// Configure JSON to accept string enum values
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Configure Azure OpenAI options
builder.Services
    .AddOptions<AzureOpenAiOptions>()
    .Bind(builder.Configuration.GetSection(AzureOpenAiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Register services
builder.Services.AddScoped<IAzureOpenAiClient, AzureOpenAiClient>();
builder.Services.AddScoped<IDocumentSummaryService, DocumentSummaryService>();
builder.Services.AddSingleton<IChatClientFactory, ChatClientFactory>();

var app = builder.Build();

app.UseProblemDetailsExceptionHandler(app.Environment);

app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Text("OK"))
    .WithName("Health")
    .WithOpenApi();

app.MapSummarizeEndpoints();

app.Run();

// Make Program accessible to tests
public partial class Program { }
