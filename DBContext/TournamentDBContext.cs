using MerkatorS.Models;
using Microsoft.EntityFrameworkCore;

namespace MerkatorS.DBContext
{
	public class TournamentDBContext : DbContext
	{
		public TournamentDBContext(DbContextOptions<TournamentDBContext> options) : base(options)
		{
		}

		public DbSet<Team> Teams { get; set; }
		public DbSet<Player> Players { get; set; }
		public DbSet<Match> Matches { get; set; }
		public DbSet<Administrator> Administrators { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Player>()
				.HasOne(p => p.Team)
				.WithMany(t => t.Players);

			modelBuilder.Entity<Match>()
				.HasOne(m => m.HomeTeam)
				.WithMany()
				.HasForeignKey(m => m.HomeTeamId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Match>()
				.HasOne(m => m.AwayTeam)
				.WithMany()
				.HasForeignKey(m => m.AwayTeamId)
				.OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<Team>()
				  .HasMany(t => t.Players)
				  .WithOne(p => p.Team)
				  .HasForeignKey(p => p.TeamId)
				  .OnDelete(DeleteBehavior.Restrict);

			SeedData.Seed(modelBuilder);

		}
	}
	public static class SeedData
	{
		public static void Seed(ModelBuilder modelBuilder)
		{

			//Administrator

			var administrator = new List<Administrator>
			{
				new Administrator {Id=1,Username="Admin", Password="Admin"},
			};
			modelBuilder.Entity<Administrator>().HasData(administrator);
		}


	}
}
