using Entities;
using Microsoft.EntityFrameworkCore;

namespace Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<Bracket> Brackets => Set<Bracket>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<TournamentParticipant> TournamentParticipants => Set<TournamentParticipant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<TournamentParticipant>()
            .HasKey(x => new { x.TournamentId, x.UserId });

        modelBuilder.Entity<TournamentParticipant>()
            .HasOne(x => x.Tournament)
            .WithMany(x => x.Participants)
            .HasForeignKey(x => x.TournamentId);

        modelBuilder.Entity<TournamentParticipant>()
            .HasOne(x => x.User)
            .WithMany(x => x.Tournaments)
            .HasForeignKey(x => x.UserId);

        modelBuilder.Entity<Tournament>()
            .HasOne(x => x.Bracket)
            .WithOne(x => x.Tournament)
            .HasForeignKey<Bracket>(x => x.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Bracket>()
            .HasMany(x => x.Matches)
            .WithOne(x => x.Bracket)
            .HasForeignKey(x => x.BracketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Match>()
            .HasOne(x => x.Player1)
            .WithMany(x => x.MatchesAsPlayer1)
            .HasForeignKey(x => x.Player1Id)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Match>()
            .HasOne(x => x.Player2)
            .WithMany(x => x.MatchesAsPlayer2)
            .HasForeignKey(x => x.Player2Id)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Match>()
            .HasOne(x => x.Winner)
            .WithMany(x => x.MatchesAsWinner)
            .HasForeignKey(x => x.WinnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
