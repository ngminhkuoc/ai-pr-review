using System.Net.Http.Headers;
using System.Text;
using AiPrReviewer.WebHooks.Services.AiReviewers;
using AiPrReviewer.WebHooks.Services.Externals.GitHub;
using AiPrReviewer.WebHooks.Services.Externals.GitHub.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Http.Resilience;
using NLog;
using NLog.Web;
using Polly;
using Refit;
using static System.Net.Mime.MediaTypeNames;

internal static class Program
{
    private static void Main(string[] args)
    {
        var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();

        // Add services to the container.
        var services = builder.Services;
        var configuration = builder.Configuration;

        services.AddControllers();

        services
            .AddGitHubApi(configuration)
            .AddScoped<ICodeReviewService, AiCodeReviewService>();

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        app.UseHttpsRedirection();

        app.UseRouting();

        app.MapControllers();

        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(UnhandledExceptionHandler(logger));
        });

        app.Run();
    }

    private static IServiceCollection AddGitHubApi(this IServiceCollection services, ConfigurationManager configuration)
    {
        var configurationSection = configuration.GetSection(GitHubApiOptions.Section);
        var gitHubApiOptions = configurationSection.Get<GitHubApiOptions>()!;
        services.Configure<GitHubApiOptions>(configurationSection);

        services
            .AddRefitClient<IGitHubApiService>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(gitHubApiOptions.BaseUrl);
                c.DefaultRequestHeaders.UserAgent.ParseAdd("AI-PR-Reviewer");
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", gitHubApiOptions.Token);
            })
            .AddResilienceHandler("retry-pipeline", builder => builder.AddRetry(
                new HttpRetryStrategyOptions
                {
                    ShouldHandle = args => args.Outcome switch
                    {
                        { Exception: ApiException } => PredicateResult.True(),
                        { Exception: HttpRequestException } => PredicateResult.True(),
                        { Result: HttpResponseMessage response } when !response.IsSuccessStatusCode => PredicateResult.True(),
                        _ => PredicateResult.False()
                    },
                    UseJitter = true,
                    BackoffType = DelayBackoffType.Exponential,
                    MaxRetryAttempts = 5,
                })
            );


        return services;
    }

    private static RequestDelegate UnhandledExceptionHandler(Logger logger)
    {
        return async context =>
        {
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var error = exceptionHandlerPathFeature?.Error;

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = Application.Json;

            var strBuilder = new StringBuilder("An exception was thrown: ");
            var loggedExceptions = new HashSet<Exception>();
            while (error is not null && !loggedExceptions.Contains(error))
            {
                logger.Error(error, error.Message);
                strBuilder.AppendLine(error.Message);

                loggedExceptions.Add(error);
                error = error.InnerException;
            }

            await context.Response.WriteAsJsonAsync(new ProblemDetails()
            {
                Title = "Unexpected Error",
                Detail = strBuilder.ToString(),
                Status = StatusCodes.Status500InternalServerError,
            });
        };
    }
}