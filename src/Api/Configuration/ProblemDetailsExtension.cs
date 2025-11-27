using Ammons.DataLabs.DocsService.Services;
using Microsoft.AspNetCore.Diagnostics;

namespace Ammons.DataLabs.DocsService.Configuration;

public static class ProblemDetailsExtensions
{
    public static IServiceCollection AddProblemDetailsWithTracing(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance = context.HttpContext.Request.Path;
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                if (environment.IsDevelopment() && context.Exception != null)
                {
                    context.ProblemDetails.Extensions["exceptionDetails"] = context.Exception.ToString();
                }
            };
        });

        return services;
    }

    public static IApplicationBuilder UseProblemDetailsExceptionHandler(
        this IApplicationBuilder app,
        IHostEnvironment environment)
    {
        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = exceptionFeature?.Error;

                var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = context.TraceIdentifier }
                };

                if (exception is DocumentSummaryException dse)
                {
                    problemDetails.Status = dse.StatusCode;
                    problemDetails.Title = dse.StatusCode == 503 ? "Service unavailable": "An error occurred";
                    problemDetails.Detail = dse.Message;
                }
                else
                {
                    problemDetails.Status = StatusCodes.Status500InternalServerError;
                    problemDetails.Title = "An unexpected error occurred";
                    problemDetails.Detail = environment.IsDevelopment()
                        ? exception?.Message
                        : "Please try again later or contect support";
                }

                context.Response.StatusCode = problemDetails.Status.Value;

                await context.Response.WriteAsJsonAsync(problemDetails, options: null,
                    contentType: "application/problem+json");
            });
        });

        return app;
    }
}