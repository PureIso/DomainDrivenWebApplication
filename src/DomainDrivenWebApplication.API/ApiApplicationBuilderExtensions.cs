using DomainDrivenWebApplication.API.Middleware;

namespace DomainDrivenWebApplication.API;

/// <summary>
/// Provides extension methods for configuring middleware and localization in the application's request pipeline.
/// </summary>
public static class ApiApplicationBuilderExtensions
{
    /// <summary>
    /// Configures the application middleware pipeline with localization, exception handling, correlation ID tracking, routing, and authorization.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> instance to configure.</param>
    /// <returns>The configured <see cref="IApplicationBuilder"/> instance.</returns>
    public static IApplicationBuilder UseApplicationExtensions(this IApplicationBuilder app)
    {
        string[] supportedCultures = ["en-US", "fr-FR", "de-DE"];
        RequestLocalizationOptions localizationOptions = new RequestLocalizationOptions()
            .SetDefaultCulture("en-US")
            .AddSupportedCultures(supportedCultures)
            .AddSupportedUICultures(supportedCultures);

        app.UseRequestLocalization(localizationOptions);
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseRouting();
        app.UseAuthorization();

        return app;
    }
}