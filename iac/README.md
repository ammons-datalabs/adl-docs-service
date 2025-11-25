# Infrastructure as Code (Planned)

This folder will contain Bicep templates for deploying the ADL Document Summarization Service to Azure.

## Planned Resources

### Core Infrastructure
- **App Service Plan** - Linux-based hosting for .NET 8
- **App Service (Web App)** - API runtime environment
- **Azure OpenAI** - Cognitive Services resource with GPT-4 deployment

### Security & Configuration
- **Key Vault** - Secure storage for API keys and connection strings
- **Managed Identity** - Passwordless authentication between services

### Monitoring & Observability
- **Application Insights** - APM, distributed tracing, and telemetry
- **Log Analytics Workspace** - Centralized logging and queries

### Optional Components
- **API Management** - Rate limiting, caching, and API gateway policies
- **Front Door** - CDN and global load balancing

## Deployment Structure

```
iac/
├── bicep/
│   ├── main.bicep              # Root deployment orchestrator
│   ├── modules/
│   │   ├── app-service.bicep   # Web app + plan
│   │   ├── openai.bicep        # Azure OpenAI resource
│   │   ├── keyvault.bicep      # Key Vault + secrets
│   │   └── monitoring.bicep    # App Insights + Log Analytics
│   └── parameters/
│       ├── dev.bicepparam      # Development environment
│       └── prod.bicepparam     # Production environment
├── scripts/
│   ├── deploy.sh               # Deployment automation
│   └── teardown.sh             # Cleanup script
└── README.md                   # This file
```

## Deployment Commands (Once Implemented)

```bash
# Login and set subscription
az login
az account set --subscription "your-subscription-id"

# Deploy to development
az deployment sub create \
  --location eastus \
  --template-file bicep/main.bicep \
  --parameters bicep/parameters/dev.bicepparam

# Deploy to production
az deployment sub create \
  --location eastus \
  --template-file bicep/main.bicep \
  --parameters bicep/parameters/prod.bicepparam
```

## Status

Coming soon. This infrastructure setup will demonstrate:
- Production-ready Azure deployment patterns
- Separation of environments (dev/staging/prod)
- Security best practices (managed identity, Key Vault)
- Observability from day one (Application Insights)