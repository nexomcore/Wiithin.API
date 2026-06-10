using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Endpoints;
using WithinAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddDataProtection()
    .SetApplicationName("WithinAPI")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".keys")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("WithinClients", policy =>
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrEmpty(origin) || !Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return false;
                }

                if (uri.Host is "localhost" or "127.0.0.1" or "192.168.1.105")
                {
                    return true;
                }

                return uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase)
                    || uri.Host.Equals("discover-within.com", StringComparison.OrdinalIgnoreCase)
                    || uri.Host.EndsWith(".discover-within.com", StringComparison.OrdinalIgnoreCase);
            }));
});

var withinPostgresConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration.GetConnectionString("WithinPostgres")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is required.");

builder.Services.AddDbContext<WithinDbContext>(options =>
    options.UseNpgsql(
        withinPostgresConnectionString,
        postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", WithinDbContext.Schema)));

builder.Services.AddSingleton<WellbeingScoringService>();
builder.Services.AddScoped<AuthTokenService>();
builder.Services.AddScoped<AccountDeletionService>();
builder.Services.AddScoped<PrivacyService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<IMarketFitSubmissionService, MarketFitSubmissionService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Within API",
        Version = "v1",
        Description = "Phase 1 API for auth, events, providers, communities, notifications, and wellbeing."
    });
});

var jwtKey = builder.Configuration["Jwt:SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey is required.");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(nameof(WithinRole.Admin)));
    options.AddPolicy("ProviderOnly", policy => policy.RequireRole(nameof(WithinRole.Provider)));
});

var app = builder.Build();

{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<WithinDbContext>();
        await db.Database.MigrateAsync();
        // Master data (community topics, platform circles, habit templates) is managed
        // from the admin portal, not seeded on startup. See AdminEndpoints / CircleEndpoints /
        // HabitEndpoints / CommunityEndpoints for the admin CRUD surface.
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Within API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "Within API Docs";
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("WithinClients");
app.UseAuthentication();
app.UseAuthorization();

app.MapWithinApiEndpoints();

app.Run();
