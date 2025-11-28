# ADL Document Summarization Service

A production-ready document summarization API built on .NET 8 using Azure OpenAI. This service provides intelligent document summarization with multiple style options (Executive, Risks, Actions, Technical) and comprehensive error handling.

## Features

- Multiple summarization styles optimized for different use cases
- Clean architecture with testable components
- Global exception handling with structured logging
- Input validation with detailed error responses
- Health check endpoint for monitoring
- Swagger/OpenAPI documentation

## Architecture Overview

The service follows clean architecture principles with clear separation of concerns:

```
Program.cs
    |
    +-- ProblemDetailsExtensions (Exception handling)
    |
    +-- Endpoint (SummarizeEndpoints)
          |
          +-- Service (IDocumentSummaryService -> DocumentSummaryService)
                |
                +-- Client (IAzureOpenAiClient -> AzureOpenAiClient)
                      |
                      +-- Azure OpenAI
```

**Key Components:**
- `Program.cs`: Application entry point with DI registration and middleware configuration
- `SummarizeEndpoints`: Minimal API endpoint mapping
- `DocumentSummaryService`: Business logic for document summarization
- `AzureOpenAiClient`: Integration with Azure OpenAI service
- `ProblemDetailsExtensions`: RFC 7807 ProblemDetails exception handling

## Getting Started

### Prerequisites

- .NET 8 SDK
- Azure OpenAI resource with a deployed model
- Azure OpenAI API key and endpoint

### Local Development Setup

1. **Clone and restore dependencies:**
   ```bash
   git clone <repository-url>
   cd adl-docs-service
   dotnet restore
   ```

2. **Configure Azure OpenAI settings:**

   Create or update `src/Api/appsettings.Development.json`:
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AzureOpenAi": {
       "Endpoint": "https://your-resource.openai.azure.com/",
       "DeploymentName": "your-deployment-name",
       "ApiKey": "your-api-key-here"
     }
   }
   ```

   **Recommended: Use .NET User Secrets for API keys:**
   ```bash
   cd src/Api
   dotnet user-secrets init
   dotnet user-secrets set "AzureOpenAi:Endpoint" "https://your-resource.openai.azure.com/"
   dotnet user-secrets set "AzureOpenAi:ApiKey" "your-api-key-here"
   dotnet user-secrets set "AzureOpenAi:DeploymentName" "your-deployment-name"
   ```

3. **Run tests:**
   ```bash
   dotnet test
   ```

4. **Run the application:**
   ```bash
   cd src/Api
   dotnet run
   ```

   The API will be available at `http://localhost:5135`

## API Usage

### Health Check

```http
GET http://localhost:5135/health
```

**Response:**
```
OK
```

### Summarize Document

```http
POST http://localhost:5135/api/summarize
Content-Type: application/json

{
  "text": "Your document text here...",
  "title": "Document Title",
  "style": "Executive"
}
```

**Request Parameters:**
- `text` (required): The document text to summarize
- `title` (optional): Document title for context
- `style` (optional): Summarization style - `Executive` (default), `Risks`, `Actions`, or `Technical`

**Response (200 OK):**
```json
{
  "summary": "Generated summary text...",
  "originalLength": 245,
  "model": "gpt-4",
  "generatedAt": "2024-11-25T18:30:00Z",
  "style": "Executive"
}
```

**Error Response (400 Bad Request):**
```json
{
  "error": "Validation failed",
  "details": {
    "Text": ["The Text field is required."]
  }
}
```

**Error Response (500 Internal Server Error):**
```json
{
  "error": "An error occurred while processing your request",
  "details": "Detailed error information (in development only)"
}
```

### Example Requests

**Executive Summary (Default):**
```http
POST http://localhost:5135/api/summarize
Content-Type: application/json

{
  "text": "This is a long document that needs to be summarized. It contains multiple sentences and paragraphs of information that should be condensed into a brief summary. The document discusses various topics including business strategy, financial performance, and operational challenges.",
  "title": "Q4 Business Review"
}
```

**Risk Analysis:**
```http
POST http://localhost:5135/api/summarize
Content-Type: application/json

{
  "text": "The project is currently behind schedule due to resource constraints. Budget overruns have been identified in three key areas. Technical debt is accumulating in the legacy systems. Vendor dependencies pose potential delivery risks.",
  "title": "Project Status Report",
  "style": "Risks"
}
```

**Action Items:**
```http
POST http://localhost:5135/api/summarize
Content-Type: application/json

{
  "text": "Meeting notes: Team agreed to refactor the authentication module by end of Q1. John will coordinate with the security team. Sarah needs to update the API documentation. We must schedule a review meeting next week.",
  "style": "Actions"
}
```

**Technical Summary:**
```http
POST http://localhost:5135/api/summarize
Content-Type: application/json

{
  "text": "The system uses a microservices architecture with event-driven communication via message queues. Authentication is handled through OAuth2 with JWT tokens. Data persistence layer implements repository pattern with Entity Framework Core.",
  "style": "Technical"
}
```

## Configuration

### Azure OpenAI Configuration

The service requires three configuration values:

| Setting | Description | Example |
|---------|-------------|---------|
| `AzureOpenAi:Endpoint` | Azure OpenAI resource endpoint URL | `https://your-resource.openai.azure.com/` |
| `AzureOpenAi:ApiKey` | Azure OpenAI API key | `abc123...` |
| `AzureOpenAi:DeploymentName` | Deployed model name | `gpt-4` |

Configuration sources (in priority order):
1. .NET User Secrets (recommended for local development)
2. Environment variables
3. `appsettings.Development.json` / `appsettings.json`

### Summary Styles

| Style | Use Case | Output Focus |
|-------|----------|--------------|
| `Executive` | High-level business summaries | Key points and strategic insights |
| `Risks` | Risk assessment and analysis | Potential issues, threats, and concerns |
| `Actions` | Action item extraction | Tasks, decisions, and next steps |
| `Technical` | Technical documentation | Technical details and implementation notes |

## Project Structure

```
adl-docs-service/
├── src/Api/
│   ├── Configuration/        # Configuration and extension methods
│   ├── Endpoints/            # Minimal API endpoints (SummarizeEndpoints)
│   ├── Models/               # Request/response DTOs
│   ├── Services/             # Business logic and Azure OpenAI integration
│   ├── Program.cs            # Application entry point
│   └── appsettings.json      # Application configuration
└── tests/Api.Tests/          # Integration and unit tests
```

## Testing

Run all tests:
```bash
dotnet test
```

Run tests with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

The test suite includes:
- Integration tests using WebApplicationFactory
- Service layer unit tests with mocked dependencies
- Input validation tests
- Error handling tests

## Development

### Swagger UI

When running in Development mode, Swagger UI is available at:
```
http://localhost:5135/swagger
```

### Development Tools

The project includes:
- `Api.http` file for testing endpoints in Rider/Visual Studio
- Swagger/OpenAPI for API exploration
- Structured logging with configurable log levels

## Deployment

This service is designed to be deployed as a container or to Azure App Service.

For a full example of production-ready Bicep infrastructure (App Service, APIM, Service Bus, monitoring, etc.), see:

- `adl-m365-automation-starter` - Azure + M365 integration, Logic Apps, Service Bus, APIM, Bicep.
- `HoldThatThread` - .NET 8 reasoning chat service with Bicep infra and APIM configuration.

A simple Docker-based deployment example is provided below.

## Run with Docker

You can run the API in a local Docker container:

```bash
docker build -t adl-docs-service .

docker run -p 8080:8080 \
  -e AzureOpenAi__Endpoint="https://your-endpoint.openai.azure.com" \
  -e AzureOpenAi__ApiKey="your-api-key" \
  -e AzureOpenAi__DeploymentName="your-deployment-name" \
  adl-docs-service
```

The API will be available at `http://localhost:8080`.
See the **Configuration** section above for details on required settings.

## License

MIT License - see LICENSE file for details