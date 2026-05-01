using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ai.WebUI.Database;

public class AppDbContextFactory : IDesignTimeDbContextFactory<MyDbContext>
{
    public MyDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseNpgsql("Host=localhost;Database=webui_ai_log;Username=postgres;Password=postgres")
            .UseSnakeCaseNamingConvention()
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
            .Options;

        return new MyDbContext(options);
    }
}
