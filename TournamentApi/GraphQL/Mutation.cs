using System.Security.Claims;
using Auth;
using Data;
using Entities;
using Microsoft.EntityFrameworkCore;

namespace GraphQL;

public class Mutation
{
    public async Task<AuthPayload> register(
        [Service] AuthService auth,
        [Service] TokenService tokens,
        string firstName,
        string lastName,
        string email,
        string password)
    {
        var user = await auth.Register(firstName, lastName, email, password);
        var token = tokens.CreateToken(user);
        return new AuthPayload(user, token);
    }

    public async Task<AuthPayload> login(
        [Service] AuthService auth,
        string email,
        string password)
    {
        var (user, token) = await auth.Login(email, password);
        return new AuthPayload(user, token);
    }

    public async Task<Tournament> createTournament([Service] AppDbContext db, string name)
    {
        var t = new Tournament
        {
            Name = name.Trim(),
            StartDate = DateTime.UtcNow,
            Status = "Created"
        };

        db.Tournaments.Add(t);
        await db.SaveChangesAsync();
        return t;
    }

    public async Task<Tournament> addParticipant(
        [Service] AppDbContext db,
        int tournamentId,
        int userId)
    {
        var tournament = await db.Tournaments
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.Id == tournamentId);

        if (tournament == null) throw new InvalidOperationException("Nie ma takiego turnieju.");
        if (tournament.Status != "Created") throw new InvalidOperationException("Do turnieju nie da się już dopisać.");

        var existsUser = await db.Users.AnyAsync(x => x.Id == userId);
        if (!existsUser) throw new InvalidOperationException("Nie ma takiego użytkownika.");

        var exists = tournament.Participants.Any(p => p.UserId == userId);
        if (!exists)
        {
            db.TournamentParticipants.Add(new TournamentParticipant
            {
                TournamentId = tournamentId,
                UserId = userId
            });

            await db.SaveChangesAsync();
        }

        return tournament;
    }

    public async Task<Tournament> start([Service] AppDbContext db, int tournamentId)
    {
        var tournament = await db.Tournaments
            .Include(x => x.Participants)
            .Include(x => x.Bracket)
            .FirstOrDefaultAsync(x => x.Id == tournamentId);

        if (tournament == null) throw new InvalidOperationException("Nie ma takiego turnieju.");
        if (tournament.Status != "Created") return tournament;

        var count = tournament.Participants.Count;
        if (count < 2) throw new InvalidOperationException("Za mało uczestników.");
        if (count % 2 != 0) throw new InvalidOperationException("Liczba uczestników musi być parzysta.");

        tournament.Status = "Started";
        tournament.StartDate = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return tournament;
    }

    public async Task<Tournament> finish([Service] AppDbContext db, int tournamentId)
    {
        var tournament = await db.Tournaments.FirstOrDefaultAsync(x => x.Id == tournamentId);
        if (tournament == null) throw new InvalidOperationException("Nie ma takiego turnieju.");

        tournament.Status = "Finished";
        await db.SaveChangesAsync();
        return tournament;
    }

    public async Task<Bracket> generateBracket([Service] AppDbContext db, int tournamentId)
    {
        var tournament = await db.Tournaments
            .Include(x => x.Participants)
            .Include(x => x.Bracket)
            .FirstOrDefaultAsync(x => x.Id == tournamentId);

        if (tournament == null) throw new InvalidOperationException("Nie ma takiego turnieju.");
        if (tournament.Status != "Started") throw new InvalidOperationException("Turniej musi być Started.");
        if (tournament.Bracket != null) throw new InvalidOperationException("Drabinka już istnieje.");

        var participantIds = tournament.Participants
            .Select(p => p.UserId)
            .OrderBy(x => x)
            .ToList();

        if (participantIds.Count < 2) throw new InvalidOperationException("Za mało uczestników.");
        if (participantIds.Count % 2 != 0) throw new InvalidOperationException("Liczba uczestników musi być parzysta.");

        var bracket = new Bracket { TournamentId = tournamentId };
        db.Brackets.Add(bracket);
        await db.SaveChangesAsync();

        for (int i = 0; i < participantIds.Count; i += 2)
        {
            db.Matches.Add(new Match
            {
                BracketId = bracket.Id,
                Round = 1,
                Player1Id = participantIds[i],
                Player2Id = participantIds[i + 1]
            });
        }

        await db.SaveChangesAsync();
        return bracket;
    }

    public async Task<List<Match>> getMatchesForRound([Service] AppDbContext db, int tournamentId, int round)
    {
        var bracket = await db.Brackets.FirstOrDefaultAsync(x => x.TournamentId == tournamentId);
        if (bracket == null) throw new InvalidOperationException("Ten turniej nie ma drabinki.");

        return await db.Matches
            .AsNoTracking()
            .Where(m => m.BracketId == bracket.Id && m.Round == round)
            .OrderBy(m => m.Id)
            .ToListAsync();
    }

    public async Task<Match> play([Service] AppDbContext db, int matchId, int winnerUserId)
    {
        var match = await db.Matches.FirstOrDefaultAsync(x => x.Id == matchId);
        if (match == null) throw new InvalidOperationException("Nie ma takiego meczu.");
        if (match.WinnerId != null) throw new InvalidOperationException("Ten mecz jest już rozegrany.");

        if (winnerUserId != match.Player1Id && winnerUserId != match.Player2Id)
            throw new InvalidOperationException("Zwycięzca musi być jednym z graczy tego meczu.");

        match.WinnerId = winnerUserId;
        await db.SaveChangesAsync();
        return match;
    }
}

public record AuthPayload(User user, string token);
