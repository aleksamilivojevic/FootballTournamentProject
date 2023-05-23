using MerkatorS.Controllers;
using MerkatorS.DBContext;
using MerkatorS.Models;
using Microsoft.EntityFrameworkCore;

public class TournamentService : ITournamentService
{
	private readonly TournamentDBContext _dbContext;
	private readonly ILogger<HomeController> _logger;


	public TournamentService(ILogger<HomeController> logger, TournamentDBContext dbContext)
	{
		_dbContext = dbContext;
		_logger = logger;
	}

	public List<Player> GetAllPlayers(Team team)
	{
		var players = _dbContext.Players.Where(p => p.TeamName == team.Name).ToList();
		return players;
	}

	public List<Player> FetchPlayers()
	{

		var players = _dbContext.Players.ToList();
		return players;
	}

	public void DeletePlayer(Player player)
	{
		var element = _dbContext.Players.FirstOrDefault(i => i.Name == player.Name);

		if (element != null)
		{
			_dbContext.Players.Remove(element);
			_dbContext.SaveChanges();
		}
	}

	public void UpdatePlayer(Player player)
	{
		var existingPlayer = _dbContext.Players.FirstOrDefault(p => p.Name == player.Name);
		if (existingPlayer == null)
		{
			throw new Exception("Player not found!");
		}

		var team = _dbContext.Teams.FirstOrDefault(i => i.Name == player.TeamName);
		if (team == null)
		{
			throw new Exception("Team not found!");
		}

		existingPlayer.Name = player.Name;
		existingPlayer.TeamId = team.TeamId;
		existingPlayer.Team = team;
		existingPlayer.TeamName = team.Name;

		_dbContext.SaveChanges();
	}

	public void AddPlayer(Player player)
	{
		bool isItemNameTaken = _dbContext.Players.Any(i => i.Name == player.Name);
		if (isItemNameTaken)
		{
			throw new Exception($"Player with name '{player.Name}' already exists!");
		}

		Team team = _dbContext.Teams.FirstOrDefault(i => i.Name == player.TeamName);
		if (team == null)
		{
			throw new Exception("Team not found!");
		}

		if (player.Team == null)
			player.Team = team;

		if (player.TeamId == null)
			player.TeamId = team.TeamId;

		_dbContext.Players.Add(player);
		_dbContext.SaveChanges();
	}
	public List<Team> FetchTeams()
	{
		var teams = _dbContext.Teams.OrderByDescending(t => t.Points).ToList();
		return teams;
	}

	public void DeleteTeam(Team team)
	{
		var element = _dbContext.Teams.FirstOrDefault(i => i.Name == team.Name);

		if (element != null)
		{
			var matches = _dbContext.Matches.Where(m => m.HomeTeamId == element.TeamId || m.AwayTeamId == element.TeamId);
			var teamm = _dbContext.Teams.First(i => i.Name != team.Name);
			if (team.Players != null)
			{
				// Remove the players associated with the team
				foreach (var player in team.Players)
				{
					player.TeamName = teamm.Name;
					player.TeamId = teamm.TeamId;
					player.Team = teamm;
					_dbContext.Update(player);
					_dbContext.SaveChanges();
				}
			}

			_dbContext.Matches.RemoveRange(matches);
			element.Players = null;
			_dbContext.SaveChanges(true);

			foreach (var fteam in _dbContext.Teams)
			{
				fteam.Points = 0;
			}
			_dbContext.SaveChanges(true);

			foreach (var matche in _dbContext.Matches)
			{
				GetPoints(matche.HomeTeamName, matche.AwayTeamName, matche.WhoWon);
			}

			_dbContext.Teams.Remove(element);

			// Perform other necessary operations

			_dbContext.SaveChanges(true);
		}
	}
	public void UpdateTeam(Team team)
	{
		var existingTeam = _dbContext.Teams.FirstOrDefault(t => t.TeamId == team.TeamId);
		if (existingTeam == null)
		{
			throw new Exception("Team not found!");
		}

		_dbContext.Entry(existingTeam).State = EntityState.Detached;

		foreach (var player in _dbContext.Players)
		{
			if (player.TeamId == team.TeamId)
			{
				player.TeamName = team.Name;
				player.Team = team;
				_dbContext.Update(player);
			}
		}

		foreach (var match in _dbContext.Matches)
		{
			if (match.HomeTeamId == team.TeamId)
			{
				match.HomeTeamName = team.Name;
			}
			else if (match.AwayTeamId == team.TeamId)
			{
				match.AwayTeamName = team.Name;
			}
			_dbContext.Update(match);
		}

		existingTeam.Name = team.Name;

		_dbContext.Attach(existingTeam);
		_dbContext.Update(existingTeam);

		_dbContext.SaveChanges();
	}

	public void AddTeam(Team team)
	{
		bool isItemNameTaken = _dbContext.Teams.Any(i => i.Name == team.Name);
		if (isItemNameTaken)
		{
			throw new Exception($"Team with name '{team.Name}' already exists!");
		}
		else
		{
			if (team.Matches == null)
			{
				team.Matches = new Team().Matches;
			}

			if (team.Players == null)
			{
				team.Players = new Team().Players;
			}

			if (team.Points == null)
			{
				team.Points = 0;
			}
			_dbContext.Teams.Add(team);
			_dbContext.SaveChanges();
		}
	}
	public List<Match> GetFixtures()
	{
		var fixtures = _dbContext.Matches.ToList();
		return fixtures;
	}

	public void GenerateFixtures()
	{
		_dbContext.Matches.ExecuteDelete();
		_dbContext.SaveChanges();

		var teams = _dbContext.Teams.ToList();
		var totalTeams = teams.Count;

		foreach (var team in teams)
		{
			team.Points = 0;
		}

		if (totalTeams < 2)
		{
			throw new Exception($"Not enought teams!");
		}

		var matchesPerTeam = totalTeams - 1;
		var fixtures = new List<Match>();

		for (int i = 0; i < totalTeams - 1; i++)
		{
			for (int j = i + 1; j < totalTeams; j++)
			{
				var homeTeam = teams[i];
				var awayTeam = teams[j];
				var match = new Match
				{
					HomeTeam = homeTeam,
					AwayTeam = awayTeam,
					HomeTeamName = homeTeam.Name,
					AwayTeamName = awayTeam.Name,
					HomeTeamGoals = GenerateRandomGoals(),
					AwayTeamGoals = GenerateRandomGoals()
				};
				match.WhoWon = Winner(match);
				fixtures.Add(match);
				GetPoints(homeTeam.Name, awayTeam.Name, match.WhoWon);
			}
		}

		_dbContext.Matches.AddRange(fixtures);
		_dbContext.SaveChanges();
	}

	public void UpdateFixtureGoals(Match request)
	{
		var existingFixture = _dbContext.Matches.FirstOrDefault(f => f.Id == request.Id);
		if (existingFixture == null)
		{

			throw new Exception($"Fixture not found!");
		}
		if (request.HomeTeamGoals > 5 || request.AwayTeamGoals > 5)
		{

			throw new Exception($"Invalid input! Please provide valid fixture information.");
		}
		// Update the fixture goals
		existingFixture.HomeTeamGoals = request.HomeTeamGoals;
		existingFixture.AwayTeamGoals = request.AwayTeamGoals;
		int previouswinner = existingFixture.WhoWon;
		int whoWin = Winner(request);
		GetPointsEdit(existingFixture.HomeTeamName, existingFixture.AwayTeamName, whoWin, previouswinner);

		// Save changes to the database
		_dbContext.SaveChanges();
	}
	#region HelpMETHODSFixture
	private int GenerateRandomGoals()
	{
		Random random = new Random();
		return random.Next(0, 6);
	}
	public void GetPoints(string Team1, string Team2, int winner)
	{
		var team1 = _dbContext.Teams.SingleOrDefault(b => b.Name == Team1);
		var team2 = _dbContext.Teams.SingleOrDefault(b => b.Name == Team2);
		if (winner == 1)
		{
			team1.Points += 3;
		}
		else if (winner == 2)
		{
			team2.Points += 3;
		}
		else
		{
			team1.Points += 1;
			team2.Points += 1;
		}

		_dbContext.SaveChanges();
	}
	public void GetPointsEdit(string Team1, string Team2, int winner, int previouswinner)
	{
		var team1 = _dbContext.Teams.SingleOrDefault(b => b.Name == Team1);
		var team2 = _dbContext.Teams.SingleOrDefault(b => b.Name == Team2);
		if (previouswinner == 1)
		{
			team1.Points -= 3;
			_dbContext.SaveChanges();
		}
		else if (previouswinner == 2)
		{
			team2.Points -= 3;
			_dbContext.SaveChanges();
		}
		else
		{
			team1.Points -= 1;
			team2.Points -= 1;
			_dbContext.SaveChanges();
		}

		if (winner == 1)
		{
			team1.Points += 3;
			_dbContext.SaveChanges();
		}
		else if (winner == 2)
		{
			team2.Points += 3;
			_dbContext.SaveChanges();
		}
		else
		{
			team1.Points += 1;
			team2.Points += 1;
			_dbContext.SaveChanges();
		}

	}
	public int Winner(Match match)
	{
		if (match.HomeTeamGoals > match.AwayTeamGoals)
		{
			match.WhoWon = 1;

		}
		else if (match.AwayTeamGoals > match.HomeTeamGoals)
		{
			match.WhoWon = 2;
		}
		else
		{
			match.WhoWon = 0;
		}
		return match.WhoWon;
	}
	#endregion
}

