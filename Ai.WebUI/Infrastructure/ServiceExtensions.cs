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
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connStr));
        services.AddDbContextFactory<AppDbContext>(options => options.UseNpgsql(connStr));

        services.AddIdentity<AppUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddEntityFrameworkStores<AppDbContext>()
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
}
