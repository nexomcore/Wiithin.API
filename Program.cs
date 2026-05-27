using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;
using WithinAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("WithinClients", policy =>
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .WithOrigins(
                "http://localhost:19006",
                "http://localhost:8081",
                "http://localhost:8082",
                "http://localhost:4200",
                "http://127.0.0.1:19006",
                "http://127.0.0.1:8081",
                "http://127.0.0.1:8082",
                "http://127.0.0.1:4200",
                "http://192.168.1.105:19006",
                "http://192.168.1.105:8081",
                "http://192.168.1.105:8082",
                "http://192.168.1.105:4200"));
});

builder.Services.AddDbContext<WithinDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("WithinPostgres"),
        postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", WithinDbContext.Schema)));
builder.Services.AddSingleton<WellbeingScoringService>();
builder.Services.AddScoped<AuthTokenService>();
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
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<WithinDbContext>();
    await db.Database.MigrateAsync();
    await WithinSeedData.EnsureAsync(db);
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

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    service = "WithinAPI",
    utc = DateTimeOffset.UtcNow
}));

var auth = app.MapGroup("/api/auth");
auth.MapPost("/register", async (RegisterDto request, WithinDbContext db, AuthTokenService tokens) =>
{
    var email = request.Email.Trim().ToLowerInvariant();
    if (await db.Users.AnyAsync(user => user.Email == email))
    {
        return Results.Conflict(new { message = "Email is already registered." });
    }

    var user = new User
    {
        Id = Guid.NewGuid(),
        DisplayName = request.DisplayName.Trim(),
        Email = email,
        PasswordHash = Passwords.Hash(request.Password),
        Role = request.Role,
        CreatedUtc = DateTimeOffset.UtcNow
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Ok(await tokens.CreateResponse(user));
});

auth.MapPost("/login", async (LoginDto request, WithinDbContext db, AuthTokenService tokens) =>
{
    var email = request.Email.Trim().ToLowerInvariant();
    var user = await db.Users.FirstOrDefaultAsync(item => item.Email == email);
    if (user is null || !Passwords.Verify(request.Password, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    return Results.Ok(await tokens.CreateResponse(user));
});

var home = app.MapGroup("/api/home").RequireAuthorization();
home.MapGet("", async (WithinDbContext db, ClaimsPrincipal principal) =>
{
    var userId = principal.UserId();
    var user = await db.Users.FindAsync(userId);
    if (user is null) return Results.Unauthorized();

    var recommended = await ProjectEvents(
            db.Events
                .Where(item => item.Status == EventStatus.Published)
                .OrderBy(item => item.StartUtc),
            db,
            userId)
        .Take(5)
        .ToArrayAsync();
    var communities = await ProjectCommunities(db.Communities, db, userId).Take(3).ToArrayAsync();
    var upcoming = await ProjectEvents(
            from evt in db.Events
            join reg in db.EventRegistrations on evt.Id equals reg.EventId
            where reg.UserId == userId && reg.State == EventJoinState.Going
            orderby evt.StartUtc
            select evt,
            db,
            userId)
        .Take(3)
        .ToArrayAsync();

    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var todayCheckIn = await db.DailyCheckIns
        .Where(item => item.UserId == userId && item.CheckInDate == today)
        .Select(item => new DailyCheckInDto
        {
            Id = item.Id.ToString(),
            CheckInDate = item.CheckInDate.ToString("yyyy-MM-dd"),
            MoodScore = item.MoodScore,
            EnergyScore = item.EnergyScore,
            StressScore = item.StressScore,
            ConnectionScore = item.ConnectionScore,
            MeaningScore = item.MeaningScore,
            Note = item.Note,
            DailyBalanceScore = item.DailyBalanceScore
        })
        .FirstOrDefaultAsync();

    return Results.Ok(new HomeDashboardDto(
        user.ToDto(),
        todayCheckIn,
        recommended,
        communities,
        $"Choose one {user.PreferredLens} action that supports your wellbeing today.",
        upcoming));
});

var providers = app.MapGroup("/api/providers");
providers.MapGet("", async (WithinDbContext db, WithinLens? lens) =>
{
    var query = db.Providers.AsQueryable();
    if (lens is not null) query = query.Where(item => item.Lens == lens);
    return Results.Ok(await query.OrderBy(item => item.Name).Select(item => item.ToDto()).ToArrayAsync());
});

providers.MapGet("/{id:guid}", async (Guid id, WithinDbContext db) =>
{
    var provider = await db.Providers.FindAsync(id);
    return provider is null ? Results.NotFound() : Results.Ok(provider.ToDto());
});

providers.MapPost("", async (UpsertProviderDto request, WithinDbContext db, ClaimsPrincipal principal) =>
{
    var userId = principal.UserId();
    var provider = new Provider
    {
        Id = Guid.NewGuid(),
        OwnerUserId = userId,
        Name = request.Name.Trim(),
        Slug = Slugs.From(request.Name),
        Bio = request.Bio.Trim(),
        Lens = request.Lens,
        Location = request.Location.Trim(),
        WebsiteUrl = request.WebsiteUrl,
        InstagramUrl = request.InstagramUrl,
        IsVerified = false,
        CreatedUtc = DateTimeOffset.UtcNow
    };
    db.Providers.Add(provider);
    await db.SaveChangesAsync();
    return Results.Created($"/api/providers/{provider.Id}", provider.ToDto());
}).RequireAuthorization();

var events = app.MapGroup("/api/events");
events.MapGet("", async (
    WithinDbContext db,
    ClaimsPrincipal principal,
    WithinLens? lens,
    bool? free,
    bool? online,
    bool? weekend,
    string? search,
    string? tag,
    Guid? providerId) =>
{
    var userId = principal.TryUserId();
    var query = db.Events.Where(item => item.Status == EventStatus.Published);
    if (lens is not null) query = query.Where(item => item.Lens == lens);
    if (free is true) query = query.Where(item => item.PriceAmount == 0);
    if (online is not null) query = query.Where(item => item.IsOnline == online);
    if (weekend is true) query = query.Where(item => item.StartUtc.DayOfWeek == DayOfWeek.Saturday || item.StartUtc.DayOfWeek == DayOfWeek.Sunday);
    if (!string.IsNullOrWhiteSpace(search)) query = query.Where(item => item.Title.ToLower().Contains(search.Trim().ToLower()));
    if (!string.IsNullOrWhiteSpace(tag)) query = query.Where(item => item.Tags.Contains(tag.Trim().ToLower()));
    if (providerId is not null) query = query.Where(item => item.ProviderId == providerId);
    return Results.Ok(await ProjectEvents(query.OrderBy(item => item.StartUtc), db, userId).ToArrayAsync());
});

events.MapGet("/{id:guid}", async (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
{
    var userId = principal.TryUserId();
    var item = await ProjectEvents(db.Events.Where(evt => evt.Id == id), db, userId).FirstOrDefaultAsync();
    return item is null ? Results.NotFound() : Results.Ok(item);
});

events.MapPost("", async (UpsertEventDto request, WithinDbContext db, ClaimsPrincipal principal) =>
{
    var provider = await db.Providers.FirstOrDefaultAsync(item => item.OwnerUserId == principal.UserId());
    if (provider is null) return Results.Forbid();

    var evt = request.ToEntity(provider.Id);
    db.Events.Add(evt);
    await db.SaveChangesAsync();
    return Results.Created($"/api/events/{evt.Id}", await ProjectEvents(db.Events.Where(item => item.Id == evt.Id), db, principal.UserId()).FirstAsync());
}).RequireAuthorization();

events.MapPut("/{id:guid}", async (Guid id, UpsertEventDto request, WithinDbContext db, ClaimsPrincipal principal) =>
{
    var evt = await db.Events.FindAsync(id);
    var provider = evt is null ? null : await db.Providers.FindAsync(evt.ProviderId);
    if (evt is null || provider is null) return Results.NotFound();
    if (provider.OwnerUserId != principal.UserId()) return Results.Forbid();

    request.ApplyTo(evt);
    await db.SaveChangesAsync();
    return Results.Ok(await ProjectEvents(db.Events.Where(item => item.Id == evt.Id), db, principal.UserId()).FirstAsync());
}).RequireAuthorization();

events.MapPost("/{id:guid}/join", async (Guid id, JoinEventDto request, WithinDbContext db, ClaimsPrincipal principal) =>
{
    var userId = principal.UserId();
    var evt = await db.Events.FindAsync(id);
    if (evt is null) return Results.NotFound();

    var registration = await db.EventRegistrations.FirstOrDefaultAsync(item => item.EventId == id && item.UserId == userId);
    if (registration is null)
    {
        registration = new EventRegistration { Id = Guid.NewGuid(), EventId = id, UserId = userId, CreatedUtc = DateTimeOffset.UtcNow };
        db.EventRegistrations.Add(registration);
    }

    registration.State = request.State;
    registration.UpdatedUtc = DateTimeOffset.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok(await ProjectEvents(db.Events.Where(item => item.Id == id), db, userId).FirstAsync());
}).RequireAuthorization();

events.MapPost("/{id:guid}/save", async (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
{
    var userId = principal.UserId();
    if (!await db.Events.AnyAsync(item => item.Id == id)) return Results.NotFound();
    if (!await db.SavedEvents.AnyAsync(item => item.EventId == id && item.UserId == userId))
    {
        db.SavedEvents.Add(new SavedEvent { Id = Guid.NewGuid(), EventId = id, UserId = userId, CreatedUtc = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();
    }
    return Results.NoContent();
}).RequireAuthorization();

events.MapDelete("/{id:guid}/save", async (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
{
    var userId = principal.UserId();
    await db.SavedEvents.Where(item => item.EventId == id && item.UserId == userId).ExecuteDeleteAsync();
    return Results.NoContent();
}).RequireAuthorization();

events.MapGet("/{id:guid}/comments", async (Guid id, WithinDbContext db) =>
    Results.Ok(await ProjectComments(db.Comments.Where(item => item.EventId == id && !item.IsHidden), db).ToArrayAsync()));

events.MapPost("/{id:guid}/comments", async (Guid id, UpsertCommentDto request, WithinDbContext db, ClaimsPrincipal principal) =>
{
    if (!await db.Events.AnyAsync(item => item.Id == id)) return Results.NotFound();
    var comment = new Comment { Id = Guid.NewGuid(), EventId = id, AuthorUserId = principal.UserId(), Body = request.Body.Trim(), CreatedUtc = DateTimeOffset.UtcNow };
    db.Comments.Add(comment);
    await db.SaveChangesAsync();
    return Results.Created($"/api/events/{id}/comments/{comment.Id}", comment);
}).RequireAuthorization();

events.MapPost("/{id:guid}/reviews", async (Guid id, UpsertReviewDto request, WithinDbContext db, ClaimsPrincipal principal) =>
{
    if (request.Rating is < 1 or > 5) return Results.BadRequest(new { message = "Rating must be 1-5." });
    var userId = principal.UserId();
    var review = await db.Reviews.FirstOrDefaultAsync(item => item.EventId == id && item.UserId == userId);
    if (review is null)
    {
        review = new Review { Id = Guid.NewGuid(), EventId = id, UserId = userId, CreatedUtc = DateTimeOffset.UtcNow };
        db.Reviews.Add(review);
    }
    review.Rating = request.Rating;
    review.Body = request.Body.Trim();
    await db.SaveChangesAsync();
    return Results.Ok(review);
}).RequireAuthorization();

var communities = app.MapGroup("/api/communities");
communities.MapGet("", async (WithinDbContext db, ClaimsPrincipal principal, WithinLens? lens) =>
{
    var userId = principal.TryUserId();
    var query = db.Communities.AsQueryable();
    if (lens is not null) query = query.Where(item => item.Lens == lens);
    return Results.Ok(await ProjectCommunities(query, db, userId).ToArrayAsync());
});

communities.MapPost("/{id:guid}/join", async (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
{
    var userId = principal.UserId();
    if (!await db.Communities.AnyAsync(item => item.Id == id)) return Results.NotFound();
    if (!await db.CommunityMembers.AnyAsync(item => item.CommunityId == id && item.UserId == userId))
    {
        db.CommunityMembers.Add(new CommunityMember { Id = Guid.NewGuid(), CommunityId = id, UserId = userId, JoinedUtc = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();
    }
    return Results.NoContent();
}).RequireAuthorization();

communities.MapGet("/{id:guid}/posts", async (Guid id, WithinDbContext db) =>
    Results.Ok(await ProjectPosts(db.Posts.Where(item => item.CommunityId == id && !item.IsHidden), db).ToArrayAsync()));

communities.MapPost("/{id:guid}/posts", async (Guid id, UpsertCommunityPostDto request, WithinDbContext db, ClaimsPrincipal principal) =>
{
    if (!await db.Communities.AnyAsync(item => item.Id == id)) return Results.NotFound();
    var post = new Post { Id = Guid.NewGuid(), CommunityId = id, EventId = request.EventId, AuthorUserId = principal.UserId(), Body = request.Body.Trim(), CreatedUtc = DateTimeOffset.UtcNow };
    db.Posts.Add(post);
    await db.SaveChangesAsync();
    return Results.Created($"/api/communities/{id}/posts/{post.Id}", post);
}).RequireAuthorization();

var posts = app.MapGroup("/api/posts").RequireAuthorization();
posts.MapPost("/{id:guid}/comments", async (Guid id, UpsertCommentDto request, WithinDbContext db, ClaimsPrincipal principal) =>
{
    if (!await db.Posts.AnyAsync(item => item.Id == id)) return Results.NotFound();
    var comment = new Comment { Id = Guid.NewGuid(), PostId = id, AuthorUserId = principal.UserId(), Body = request.Body.Trim(), CreatedUtc = DateTimeOffset.UtcNow };
    db.Comments.Add(comment);
    await db.SaveChangesAsync();
    return Results.Created($"/api/posts/{id}/comments/{comment.Id}", comment);
});

posts.MapPost("/{id:guid}/reactions/{kind}", async (Guid id, string kind, WithinDbContext db, ClaimsPrincipal principal) =>
{
    if (!await db.Posts.AnyAsync(item => item.Id == id)) return Results.NotFound();
    var userId = principal.UserId();
    if (!await db.Reactions.AnyAsync(item => item.PostId == id && item.UserId == userId && item.Kind == kind))
    {
        db.Reactions.Add(new Reaction { Id = Guid.NewGuid(), PostId = id, UserId = userId, Kind = kind, CreatedUtc = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();
    }
    return Results.NoContent();
});

posts.MapPost("/{id:guid}/report", async (Guid id, WithinDbContext db) =>
{
    var post = await db.Posts.FindAsync(id);
    if (post is null) return Results.NotFound();
    post.IsHidden = true;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

var notifications = app.MapGroup("/api/notifications").RequireAuthorization();
notifications.MapPost("/device-token", async (DeviceTokenDto request, WithinDbContext db, ClaimsPrincipal principal) =>
{
    var userId = principal.UserId();
    var existing = await db.DeviceTokens.FirstOrDefaultAsync(item => item.Token == request.Token);
    if (existing is null)
    {
        db.DeviceTokens.Add(new DeviceToken { Id = Guid.NewGuid(), UserId = userId, Token = request.Token, Platform = request.Platform, CreatedUtc = DateTimeOffset.UtcNow });
    }
    else
    {
        existing.UserId = userId;
        existing.Platform = request.Platform;
    }
    await db.SaveChangesAsync();
    return Results.NoContent();
});

notifications.MapGet("/preferences", async (WithinDbContext db, ClaimsPrincipal principal) =>
{
    var userId = principal.UserId();
    var prefs = await db.NotificationPreferences.FirstOrDefaultAsync(item => item.UserId == userId);
    return Results.Ok(prefs is null
        ? new NotificationPreferencesDto(true, true, true, true, WithinLens.Feel)
        : new NotificationPreferencesDto(prefs.DailyMotivationEnabled, prefs.EventRemindersEnabled, prefs.CommunitySummariesEnabled, prefs.ProviderNewEventsEnabled, prefs.PreferredLens));
});

notifications.MapPut("/preferences", async (NotificationPreferencesDto request, WithinDbContext db, ClaimsPrincipal principal) =>
{
    var userId = principal.UserId();
    var prefs = await db.NotificationPreferences.FirstOrDefaultAsync(item => item.UserId == userId);
    if (prefs is null)
    {
        prefs = new NotificationPreference { Id = Guid.NewGuid(), UserId = userId };
        db.NotificationPreferences.Add(prefs);
    }
    prefs.DailyMotivationEnabled = request.DailyMotivationEnabled;
    prefs.EventRemindersEnabled = request.EventRemindersEnabled;
    prefs.CommunitySummariesEnabled = request.CommunitySummariesEnabled;
    prefs.ProviderNewEventsEnabled = request.ProviderNewEventsEnabled;
    prefs.PreferredLens = request.PreferredLens;
    await db.SaveChangesAsync();
    return Results.Ok(request);
});

var wellbeing = app.MapGroup("/api/wellbeing").RequireAuthorization();
wellbeing.MapPost("/daily-checkin", async (DailyCheckInDto request, WithinDbContext db, WellbeingScoringService scoring, ClaimsPrincipal principal) =>
{
    var userId = principal.UserId();
    var date = DateOnly.Parse(request.CheckInDate);
    var saved = await db.DailyCheckIns.FirstOrDefaultAsync(item => item.UserId == userId && item.CheckInDate == date);
    if (saved is null)
    {
        saved = new DailyCheckIn { Id = Guid.NewGuid(), UserId = userId, CheckInDate = date };
        db.DailyCheckIns.Add(saved);
    }
    saved.MoodScore = request.MoodScore;
    saved.EnergyScore = request.EnergyScore;
    saved.StressScore = request.StressScore;
    saved.ConnectionScore = request.ConnectionScore;
    saved.MeaningScore = request.MeaningScore;
    saved.Note = request.Note;
    saved.DailyBalanceScore = scoring.CalculateDailyBalance(request);
    await db.SaveChangesAsync();
    return Results.Ok(new { saved.Id, saved.DailyBalanceScore, message = "Daily check-in saved." });
});

app.Run();

static IQueryable<EventDto> ProjectEvents(IQueryable<Event> query, WithinDbContext db, Guid? userId) =>
    from evt in query
    join provider in db.Providers on evt.ProviderId equals provider.Id
    select new EventDto(
        evt.Id,
        evt.ProviderId,
        provider.Name,
        evt.Title,
        evt.Description,
        evt.Lens,
        evt.LocationName,
        evt.IsOnline,
        evt.StartUtc,
        evt.EndUtc,
        evt.PriceAmount,
        evt.Currency,
        evt.Capacity,
        db.EventRegistrations.Count(reg => reg.EventId == evt.Id && reg.State == EventJoinState.Going),
        userId != null && db.SavedEvents.Any(saved => saved.EventId == evt.Id && saved.UserId == userId),
        userId == null ? null : db.EventRegistrations.Where(reg => reg.EventId == evt.Id && reg.UserId == userId).Select(reg => (EventJoinState?)reg.State).FirstOrDefault(),
        evt.SignupType,
        evt.ExternalBookingUrl,
        evt.ImageUrl,
        evt.Status,
        evt.Tags);

static IQueryable<CommunityDto> ProjectCommunities(IQueryable<Community> query, WithinDbContext db, Guid? userId) =>
    query.Select(item => new CommunityDto(
        item.Id,
        item.ProviderId,
        item.Name,
        item.Description,
        item.Lens,
        item.Location,
        db.CommunityMembers.Count(member => member.CommunityId == item.Id),
        userId != null && db.CommunityMembers.Any(member => member.CommunityId == item.Id && member.UserId == userId)));

static IQueryable<PostDto> ProjectPosts(IQueryable<Post> query, WithinDbContext db) =>
    from post in query
    join user in db.Users on post.AuthorUserId equals user.Id
    orderby post.CreatedUtc descending
    select new PostDto(
        post.Id,
        post.CommunityId,
        post.EventId,
        user.DisplayName,
        post.Body,
        db.Reactions.Count(reaction => reaction.PostId == post.Id),
        db.Comments.Count(comment => comment.PostId == post.Id && !comment.IsHidden),
        post.CreatedUtc);

static IQueryable<CommentDto> ProjectComments(IQueryable<Comment> query, WithinDbContext db) =>
    from comment in query
    join user in db.Users on comment.AuthorUserId equals user.Id
    orderby comment.CreatedUtc
    select new CommentDto(comment.Id, user.DisplayName, comment.Body, comment.CreatedUtc);

public static class ApiMapping
{
    public static Guid UserId(this ClaimsPrincipal principal) =>
        Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("User id claim missing."));

    public static Guid? TryUserId(this ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;

    public static UserSummaryDto ToDto(this User user) => new(user.Id, user.DisplayName, user.Email, user.Role, user.PreferredLens);

    public static ProviderDto ToDto(this Provider provider) => new(
        provider.Id,
        provider.Name,
        provider.Slug,
        provider.Bio,
        provider.Lens,
        provider.Location,
        provider.WebsiteUrl,
        provider.InstagramUrl,
        provider.IsVerified);

    public static Event ToEntity(this UpsertEventDto request, Guid providerId)
    {
        var evt = new Event
        {
            Id = Guid.NewGuid(),
            ProviderId = providerId,
            CreatedUtc = DateTimeOffset.UtcNow,
            Status = EventStatus.Published
        };
        return request.ApplyTo(evt);
    }

    public static Event ApplyTo(this UpsertEventDto request, Event evt)
    {
        evt.Title = request.Title.Trim();
        evt.Description = request.Description.Trim();
        evt.Lens = request.Lens;
        evt.LocationName = request.LocationName.Trim();
        evt.IsOnline = request.IsOnline;
        evt.StartUtc = request.StartUtc;
        evt.EndUtc = request.EndUtc;
        evt.PriceAmount = request.PriceAmount;
        evt.Currency = string.IsNullOrWhiteSpace(request.Currency) ? "AUD" : request.Currency.Trim().ToUpperInvariant();
        evt.Capacity = request.Capacity;
        evt.SignupType = request.SignupType;
        evt.ExternalBookingUrl = request.ExternalBookingUrl;
        evt.ImageUrl = request.ImageUrl;
        evt.Tags = request.Tags.Select(tag => tag.Trim().ToLowerInvariant()).Where(tag => tag.Length > 0).Distinct().ToArray();
        return evt;
    }
}

public sealed class AuthTokenService(IConfiguration configuration, WithinDbContext db)
{
    public async Task<TokenResponseDto> CreateResponse(User user)
    {
        var accessToken = CreateAccessToken(user);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = Passwords.Hash(refreshToken),
            CreatedUtc = DateTimeOffset.UtcNow,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
        });
        await db.SaveChangesAsync();
        return new TokenResponseDto(accessToken, refreshToken, user.ToDto());
    }

    private string CreateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SigningKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        var token = new JwtSecurityToken(
            configuration["Jwt:Issuer"],
            configuration["Jwt:Audience"],
            claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public static class Passwords
{
    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"pbkdf2:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string stored)
    {
        var parts = stored.Split(':');
        if (parts.Length != 3 || parts[0] != "pbkdf2") return false;
        var salt = Convert.FromBase64String(parts[1]);
        var expected = Convert.FromBase64String(parts[2]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}

public static class Slugs
{
    public static string From(string value)
    {
        var builder = new StringBuilder();
        foreach (var ch in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch)) builder.Append(ch);
            else if (builder.Length > 0 && builder[^1] != '-') builder.Append('-');
        }
        return builder.ToString().Trim('-');
    }
}
