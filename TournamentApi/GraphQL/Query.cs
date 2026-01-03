using System.Security.Claims;
using Data;
using Entities;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GraphQL;

public class Query
{
    public IQueryable<Tournament> tournaments([Service] AppDbContext db)
        => db.Tournaments.AsNoTracking();

    public IQueryable<Bracket> brackets([Service] AppDbContext db)
        => db.Brackets.AsNoTracking();

    public IQueryable<Match> matches([Service] AppDbContext db)
        => db.Matches.AsNoTracking();

    [Authorize]
    public async Task<List<Match>> myMatches(
        [Service] AppDbContext db,
        ClaimsPrincipal user,
        bool? played)
    {
        var idStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(idStr)) throw new InvalidOperationException("Brak użytkownika.");
        var userId = int.Parse(idStr);

        var q = db.Matches
            .AsNoTracking()
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .Include(m => m.Winner)
            .Where(m => m.Player1Id == userId || m.Player2Id == userId);

        if (played.HasValue)
        {
            if (played.Value) q = q.Where(m => m.WinnerId != null);
            else q = q.Where(m => m.WinnerId == null);
        }

        return await q
            .OrderBy(m => m.Round)
            .ThenBy(m => m.Id)
            .ToListAsync();
    }
}
