using System.Text.RegularExpressions;

namespace Entities;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;

    public List<TournamentParticipant> Tournaments { get; set; } = new();

    public List<Match> MatchesAsPlayer1 { get; set; } = new();
    public List<Match> MatchesAsPlayer2 { get; set; } = new();
    public List<Match> MatchesAsWinner { get; set; } = new();
}
