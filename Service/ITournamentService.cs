using MerkatorS.DBContext;
using MerkatorS.Models;

public interface ITournamentService
{
	List<Player> GetAllPlayers(Team team);
	List<Player> FetchPlayers();
	void UpdatePlayer(Player player);
	void AddPlayer(Player player);
	void DeletePlayer(Player player);

	List<Team> FetchTeams();
	void UpdateTeam(Team team);
	void AddTeam(Team team);
	void DeleteTeam(Team team);

	List<Match> GetFixtures();
	void GenerateFixtures();
	void UpdateFixtureGoals(Match match);

}
