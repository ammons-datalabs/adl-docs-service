using System.ComponentModel.DataAnnotations;

namespace Ammons.DataLabs.DocsService.Configuration;

public class AzureOpenAiOptions
{
    public const string SectionName = "AzureOpenAi";

    [Required, Url]
    public string Endpoint { get; set; } = string.Empty;
    
    [Required]
    public string ApiKey { get; set; } = string.Empty;
    
    [Required]
    public string DeploymentName { get; set; } = string.Empty;
}