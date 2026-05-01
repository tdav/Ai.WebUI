using Ai.WebUI.Database;
using Ai.WebUI.Services;
using Ai.WebUI.Services.AI;
using Ai.WebUI.Database.Entities;
using Ai.WebUI.DataFormats;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Ai.WebUI.Infrastructure;

public static class ServiceExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connStr = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContextFactory<MyDbContext>(options => options.UseNpgsql(connStr), ServiceLifetime.Scoped);

        services.AddIdentity<AppUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddEntityFrameworkStores<MyDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }

    public static IServiceCollection AddAiServices(this IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:8000/";
        var defaultModel = configuration["Ollama:DefaultModel"] ?? "llama3.2";

#pragma warning disable SKEXP0070
        services.AddKernel()
            .AddOllamaChatCompletion(modelId: defaultModel, endpoint: new Uri(baseUrl));
#pragma warning restore SKEXP0070

        services.AddHttpClient("ollama", client =>
            client.BaseAddress = new Uri(baseUrl));

        services.AddScoped<OllamaChatService>();
        services.AddScoped<IOllamaChatService>(sp => sp.GetRequiredService<OllamaChatService>());
        services.AddScoped<IHistoryReducer, ChatHistoryReducer>();

        return services;
    }

    public static IServiceCollection AddDocumentServices(this IServiceCollection services)
    {
        services.AddDefaultContentDecoders();
        services.AddScoped<DocumentService>();
        return services;
    }

    public static async Task UpdateMigrateDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        await db.Database.MigrateAsync();
        await SeedAdminUserAsync(scope.ServiceProvider, app.Configuration);
    }

    private static async Task SeedAdminUserAsync(IServiceProvider services, IConfiguration configuration)
    {
        var email = configuration["AdminUser:Email"] ?? "admin@admin.com";
        var password = configuration["AdminUser:Password"] ?? "Admin123!";
        var displayName = configuration["AdminUser:DisplayName"] ?? "Administrator";

        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var admin = new AppUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to seed admin user: {errors}");
        }
    }
}
