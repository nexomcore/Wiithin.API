using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Endpoints;

public static class ConnectionEndpoints
{
    public static IEndpointRouteBuilder MapConnectionEndpoints(this IEndpointRouteBuilder app)
    {
        var connections = app.MapGroup("/api/connections").RequireAuthorization();

        connections.MapGet("", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var items = await db.Connections
                .Where(item => item.Status == ConnectionStatus.Accepted && (item.RequesterUserId == userId || item.ReceiverUserId == userId))
                .OrderByDescending(item => item.UpdatedAt)
                .ToArrayAsync();
            return Results.Ok(await ToConnectionDtos(db, items, userId));
        });

        connections.MapGet("/pending", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var items = await db.Connections
                .Where(item => item.Status == ConnectionStatus.Pending && (item.RequesterUserId == userId || item.ReceiverUserId == userId))
                .OrderByDescending(item => item.CreatedAt)
                .ToArrayAsync();
            return Results.Ok(await ToConnectionDtos(db, items, userId));
        });

        connections.MapGet("/requests", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var items = await db.Connections
                .Where(item => item.Status == ConnectionStatus.Pending && item.ReceiverUserId == userId)
                .OrderByDescending(item => item.CreatedAt)
                .ToArrayAsync();
            return Results.Ok(await ToConnectionDtos(db, items, userId));
        });

        connections.MapPost("/request", async (ConnectionRequestDto request, WithinDbContext db, PrivacyService privacy, ClaimsPrincipal principal) =>
        {
            var requesterUserId = principal.UserId();
            if (!await db.Users.AnyAsync(item => item.Id == request.ReceiverUserId)) return Results.NotFound();
            if (!await privacy.CanSendConnectionRequest(requesterUserId, request.ReceiverUserId))
            {
                return Results.BadRequest(new { message = "Connection request is not allowed by privacy settings." });
            }

            var existing = await FindConnection(db, requesterUserId, request.ReceiverUserId);
            if (existing is not null && existing.Status is ConnectionStatus.Pending or ConnectionStatus.Accepted)
            {
                return Results.Conflict(new { message = "A connection already exists or is pending." });
            }

            var now = DateTimeOffset.UtcNow;
            var connection = existing ?? new Connection
            {
                Id = Guid.NewGuid(),
                RequesterUserId = requesterUserId,
                ReceiverUserId = request.ReceiverUserId,
                CreatedAt = now
            };
            connection.Status = ConnectionStatus.Pending;
            connection.UpdatedAt = now;
            connection.RespondedAt = null;
            connection.BlockedAt = null;
            connection.BlockedByUserId = null;
            if (existing is null) db.Connections.Add(connection);
            await db.SaveChangesAsync();
            return Results.Created($"/api/connections/{connection.Id}", await ToConnectionDto(db, connection, requesterUserId));
        });

        connections.MapPost("/{connectionId:guid}/accept", async (Guid connectionId, WithinDbContext db, ClaimsPrincipal principal) =>
            await UpdateIncoming(db, principal.UserId(), connectionId, ConnectionStatus.Accepted));

        connections.MapPost("/{connectionId:guid}/reject", async (Guid connectionId, WithinDbContext db, ClaimsPrincipal principal) =>
            await UpdateIncoming(db, principal.UserId(), connectionId, ConnectionStatus.Rejected));

        connections.MapPost("/{connectionId:guid}/cancel", async (Guid connectionId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var connection = await db.Connections.FindAsync(connectionId);
            if (connection is null) return Results.NotFound();
            if (connection.RequesterUserId != userId || connection.Status != ConnectionStatus.Pending) return Results.Forbid();
            connection.Status = ConnectionStatus.Cancelled;
            connection.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        connections.MapPost("/{connectionId:guid}/remove", async (Guid connectionId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var connection = await db.Connections.FindAsync(connectionId);
            if (connection is null) return Results.NotFound();
            if (connection.RequesterUserId != userId && connection.ReceiverUserId != userId) return Results.Forbid();
            connection.Status = ConnectionStatus.Removed;
            connection.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        connections.MapPost("/block", async (BlockUserDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var blockerUserId = principal.UserId();
            if (blockerUserId == request.UserId) return Results.BadRequest(new { message = "You cannot block yourself." });
            if (!await db.Users.AnyAsync(item => item.Id == request.UserId)) return Results.NotFound();

            var now = DateTimeOffset.UtcNow;
            var connection = await FindConnection(db, blockerUserId, request.UserId);
            if (connection is null)
            {
                connection = new Connection
                {
                    Id = Guid.NewGuid(),
                    RequesterUserId = blockerUserId,
                    ReceiverUserId = request.UserId,
                    CreatedAt = now
                };
                db.Connections.Add(connection);
            }

            connection.Status = ConnectionStatus.Blocked;
            connection.BlockedByUserId = blockerUserId;
            connection.BlockedAt = now;
            connection.UpdatedAt = now;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        connections.MapPost("/reports", async (UserReportRequestDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var reporterUserId = principal.UserId();
            if (reporterUserId == request.ReportedUserId) return Results.BadRequest(new { message = "You cannot report yourself." });
            if (!await db.Users.AnyAsync(item => item.Id == request.ReportedUserId)) return Results.NotFound();
            var now = DateTimeOffset.UtcNow;
            db.UserReports.Add(new UserReport
            {
                Id = Guid.NewGuid(),
                ReportedByUserId = reporterUserId,
                ReportedUserId = request.ReportedUserId,
                SourceType = request.SourceType,
                SourceId = request.SourceId,
                Reason = request.Reason,
                Details = request.Details?.Trim(),
                Status = UserReportStatus.Open,
                CreatedAt = now,
                UpdatedAt = now
            });
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return app;
    }

    private static async Task<IResult> UpdateIncoming(WithinDbContext db, Guid userId, Guid connectionId, ConnectionStatus status)
    {
        var connection = await db.Connections.FindAsync(connectionId);
        if (connection is null) return Results.NotFound();
        if (connection.ReceiverUserId != userId || connection.Status != ConnectionStatus.Pending) return Results.Forbid();
        connection.Status = status;
        connection.RespondedAt = DateTimeOffset.UtcNow;
        connection.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<Connection?> FindConnection(WithinDbContext db, Guid firstUserId, Guid secondUserId) =>
        await db.Connections.FirstOrDefaultAsync(item =>
            (item.RequesterUserId == firstUserId && item.ReceiverUserId == secondUserId) ||
            (item.RequesterUserId == secondUserId && item.ReceiverUserId == firstUserId));

    private static async Task<ConnectionDto[]> ToConnectionDtos(WithinDbContext db, Connection[] connections, Guid currentUserId)
    {
        var response = new List<ConnectionDto>(connections.Length);
        foreach (var connection in connections)
        {
            response.Add(await ToConnectionDto(db, connection, currentUserId));
        }
        return response.ToArray();
    }

    private static async Task<ConnectionDto> ToConnectionDto(WithinDbContext db, Connection connection, Guid currentUserId)
    {
        var otherUserId = connection.RequesterUserId == currentUserId ? connection.ReceiverUserId : connection.RequesterUserId;
        var other = await db.Users.FindAsync(otherUserId);
        return new ConnectionDto(
            connection.Id,
            otherUserId,
            other?.DisplayName ?? "Within user",
            connection.Status,
            connection.RequesterUserId == currentUserId,
            connection.CreatedAt,
            connection.UpdatedAt);
    }
}
