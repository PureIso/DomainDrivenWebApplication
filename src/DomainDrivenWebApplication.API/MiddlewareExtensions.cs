namespace DomainDrivenWebApplication.API;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseLocalizationMiddleware(this IApplicationBuilder app)
    {
        string[] supportedCultures = { "en-US", "fr-FR", "de-DE" };
        RequestLocalizationOptions localizationOptions = new RequestLocalizationOptions()
            .SetDefaultCulture("en-US")
            .AddSupportedCultures(supportedCultures)
            .AddSupportedUICultures(supportedCultures);

        app.UseRequestLocalization(localizationOptions);
        return app;
    }
}
