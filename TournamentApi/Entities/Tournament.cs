namespace Entities;

public class Tournament
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public string Status { get; set; } = "Created";

    public Bracket? Bracket { get; set; }
    public List<TournamentParticipant> Participants { get; set; } = new();
}
