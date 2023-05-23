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
	#region PLAYER
	public async Task<List<Player>> GetAllPlayers(Team team)
	{
		var players = await _dbContext.Players.Where(p => p.TeamName == team.Name).ToListAsync();
		return players;
	}

	public async Task<List<Player>> FetchPlayers()
	{

		var players = await _dbContext.Players.ToListAsync();
		return players;
	}

	public async Task DeletePlayer(Player player)
	{
		var element = await _dbContext.Players.FirstOrDefaultAsync(i => i.Name == player.Name);

		if (element != null)
		{ 
			_dbContext.Players.Remove(element);
			await _dbContext.SaveChangesAsync();
		}
	}

	public async Task UpdatePlayer(Player player)
	{
		var existingPlayer = await _dbContext.Players.FirstOrDefaultAsync(p => p.Name == player.Name);
		if (existingPlayer == null)
		{
			throw new Exception("Player not found!");
		}

		var team = await _dbContext.Teams.FirstOrDefaultAsync(i => i.Name == player.TeamName);
		if (team == null)
		{
			throw new Exception("Team not found!");
		}

		existingPlayer.Name = player.Name;
		existingPlayer.TeamId = team.TeamId;
		existingPlayer.Team = team;
		existingPlayer.TeamName = team.Name;

		await _dbContext.SaveChangesAsync();
	}

	public async Task AddPlayer(Player player)
	{
		bool isItemNameTaken = await _dbContext.Players.AnyAsync(i => i.Name == player.Name);
		if (isItemNameTaken)
		{
			throw new Exception($"Player with name '{player.Name}' already exists!");
		}

		Team team = await _dbContext.Teams.FirstOrDefaultAsync(i => i.Name == player.TeamName);
		if (team == null)
		{
			throw new Exception("Team not found!");
		}

		if (player.Team == null)
			player.Team = team;

		if (player.TeamId == null)
			player.TeamId = team.TeamId;

		await _dbContext.Players.AddAsync(player);
		await _dbContext.SaveChangesAsync();
	}
#endregion
	#region TEAMS
	public async Task<List<Team>> FetchTeams()
	{
		var teams =await _dbContext.Teams.OrderByDescending(t => t.Points).ToListAsync();
		return teams;
	}

	public async Task DeleteTeam(Team team)
	{
		var element = await _dbContext.Teams.FirstOrDefaultAsync(i => i.Name == team.Name);

		if (element != null)
		{
			var matches = _dbContext.Matches.Where(m => m.HomeTeamId == element.TeamId || m.AwayTeamId == element.TeamId);
			var teamm = await _dbContext.Teams.FirstAsync(i => i.Name != team.Name);
			if (team.Players != null)
			{
				// Remove the players associated with the team
				foreach (var player in team.Players)
				{
					player.TeamName = teamm.Name;
					player.TeamId = teamm.TeamId;
					player.Team = teamm;
					_dbContext.Update(player);
					await _dbContext.SaveChangesAsync();
				}
			}

			_dbContext.Matches.RemoveRange(matches);
			element.Players = null;
			await _dbContext.SaveChangesAsync(true);

			foreach (var fteam in _dbContext.Teams)
			{
				fteam.Points = 0;
			}
			await _dbContext.SaveChangesAsync(true);

			foreach (var matche in _dbContext.Matches)
			{
				await GetPoints(matche.HomeTeamName, matche.AwayTeamName, matche.WhoWon);
			}

			_dbContext.Teams.Remove(element);

			// Perform other necessary operations

			await _dbContext.SaveChangesAsync(true);
		}
	}
	public async Task UpdateTeam(Team team)
	{
		var existingTeam =await _dbContext.Teams.FirstOrDefaultAsync(t => t.TeamId == team.TeamId);
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

		await _dbContext.SaveChangesAsync();
	}

	public async Task AddTeam(Team team)
	{
		bool isItemNameTaken = await _dbContext.Teams.AnyAsync(i => i.Name == team.Name);
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
			await _dbContext.Teams.AddAsync(team);
			await _dbContext.SaveChangesAsync();
		}
	}
#endregion
	#region Fixtures
	public async Task<List<Match>> GetFixtures()
	{
		var fixtures = await _dbContext.Matches.ToListAsync();
		return fixtures;
	}

	public async Task GenerateFixtures()
	{
		await _dbContext.Matches.ExecuteDeleteAsync();
		await _dbContext.SaveChangesAsync();

		var teams =await _dbContext.Teams.ToListAsync();
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
				await GetPoints(homeTeam.Name, awayTeam.Name, match.WhoWon);
			}
		}

		await _dbContext.Matches.AddRangeAsync(fixtures);
		await _dbContext.SaveChangesAsync();
	}

	public async Task UpdateFixtureGoals(Match request)
	{
		var existingFixture =await _dbContext.Matches.FirstOrDefaultAsync(f => f.Id == request.Id);
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
		await  GetPointsEdit(existingFixture.HomeTeamName, existingFixture.AwayTeamName, whoWin, previouswinner);

		// Save changes to the database
		await _dbContext.SaveChangesAsync();
	}
	#endregion
	#region HelpMETHODSFixture
	private int GenerateRandomGoals()
	{
		Random random = new Random();
		return random.Next(0, 6);
	}
	public async Task GetPoints(string Team1, string Team2, int winner)
	{
		var team1 = await _dbContext.Teams.SingleOrDefaultAsync(b => b.Name == Team1);
		var team2 = await _dbContext.Teams.SingleOrDefaultAsync(b => b.Name == Team2);
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

		await _dbContext.SaveChangesAsync();
	}
	public async Task GetPointsEdit(string Team1, string Team2, int winner, int previouswinner)
	{
		var team1 = await _dbContext.Teams.SingleOrDefaultAsync(b => b.Name == Team1);
		var team2 = await _dbContext.Teams.SingleOrDefaultAsync(b => b.Name == Team2);
		if (previouswinner == 1)
		{
			team1.Points -= 3;
		}
		else if (previouswinner == 2)
		{
			team2.Points -= 3;
		}
		else
		{
			team1.Points -= 1;
			team2.Points -= 1;
		}
		await _dbContext.SaveChangesAsync();
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
		await _dbContext.SaveChangesAsync();

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

