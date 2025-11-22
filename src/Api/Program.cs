using Ammons.DataLabs.DocsService.Endpoints;
using Ammons.DataLabs.DocsService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register services
builder.Services.AddScoped<IDocumentSummaryService, DocumentSummaryService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Text("OK"))
    .WithName("Health")
    .WithOpenApi();

app.MapSummarizeEndpoints();

app.Run();

// Make Program accessible to tests
public partial class Program { }
