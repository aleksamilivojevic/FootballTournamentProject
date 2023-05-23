using MerkatorS.DBContext;
using MerkatorS.Models;

public interface ITournamentService
{
	Task<List<Player>> GetAllPlayers(Team team);
	Task<List<Player>> FetchPlayers();
	Task UpdatePlayer(Player player);
	Task AddPlayer(Player player);
	Task DeletePlayer(Player player);

	Task<List<Team>> FetchTeams();
	Task UpdateTeam(Team team);
	Task AddTeam(Team team);
	Task DeleteTeam(Team team);

	Task<List<Match>> GetFixtures();
	Task GenerateFixtures();
	Task UpdateFixtureGoals(Match match);

}
