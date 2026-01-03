using System.Text.RegularExpressions;

namespace Entities;

public class Bracket
{
    public int Id { get; set; }

    public int TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;

    public List<Match> Matches { get; set; } = new();
}
