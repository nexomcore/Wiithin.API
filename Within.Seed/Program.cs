using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WithinAPI.Data;

var shouldApply = args.Any(arg => string.Equals(arg, "--apply", StringComparison.OrdinalIgnoreCase));
var shouldMigrate = args.Any(arg => string.Equals(arg, "--migrate", StringComparison.OrdinalIgnoreCase));

if (!shouldApply)
{
    Console.WriteLine("This command inserts or updates demo seed data.");
    Console.WriteLine("Run with --apply to seed data, optionally with --migrate to apply migrations first.");
    return;
}

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    ?? "Development";

var apiProjectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
var configuration = new ConfigurationBuilder()
    .SetBasePath(apiProjectPath)
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

var connectionString = configuration.GetConnectionString("WithinPostgres")
    ?? throw new InvalidOperationException("Connection string 'WithinPostgres' is required.");

var dbOptions = new DbContextOptionsBuilder<WithinDbContext>()
    .UseNpgsql(
        connectionString,
        postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", WithinDbContext.Schema))
    .Options;

await using var db = new WithinDbContext(dbOptions);

if (shouldMigrate)
{
    Console.WriteLine("Applying database migrations...");
    await db.Database.MigrateAsync();
}

Console.WriteLine("Applying demo seed data...");
await WithinSeedData.EnsureAsync(db);
Console.WriteLine("Seed data applied.");
