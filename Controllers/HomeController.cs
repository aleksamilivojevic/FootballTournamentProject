using MerkatorS.DBContext;
using MerkatorS.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Match = MerkatorS.Models.Match;

namespace MerkatorS.Controllers
{

	public class HomeController : Controller
	{

		private readonly IAdministratorService _administratorService;
		private readonly ITournamentService _tournamentService;
		public HomeController(ITournamentService tournamentService,IAdministratorService administratorService)
		{
			_tournamentService = tournamentService;
			_administratorService = administratorService;	
		}
		#region StartMethods
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
		[HttpGet]
		public IActionResult Login()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Login(Administrator model)
		{
			if (ModelState.IsValid)
			{
				var user = await _administratorService.Login(model.Username,model.Password);

				if (user != null)
				{
					var claims = new[]
					{
						new Claim(ClaimTypes.Name, user.Username),
					};

					var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
					var principal = new ClaimsPrincipal(identity);

					await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

					return RedirectToAction("Administrator", "Home");
				}

				ModelState.AddModelError("", "Invalid username or password.");
			}

			return View(model);
		}
		public IActionResult Administrator()
		{
			return View("Administrator");
		}

		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

			return RedirectToAction("Index", "Home");
		}
		#endregion

		#region PLAYER

		[HttpPost]
		public async Task<IActionResult> GetAllPlayers([FromBody] Team team)
		{
			var players = await _tournamentService.GetAllPlayers(team);

			return Json(players);
		}

		[HttpGet]
		public async Task<IActionResult> FetchPlayers()
		{
			var players = await _tournamentService.FetchPlayers();
			return Ok(players);
		}

		[HttpPost]
		public async Task<IActionResult> UpdatePlayer([FromBody] Player player)
		{
			if (ModelState.IsValid)
			{
				await _tournamentService.UpdatePlayer(player);
				return Ok(new { message = "Player updated successfully!" });
			}

			return BadRequest(new { message = "Invalid input! Please provide valid player information." });
		}

		[HttpPost]
		public async Task<IActionResult> AddPlayer([FromBody] Player player)
		{
			if (ModelState.IsValid)
			{
				await _tournamentService.AddPlayer(player);

				return Ok(new { message = "Player added successfully!" });

			}

			return BadRequest(new { message = "Invalid input! Please provide valid player information." });
		}

		[HttpPost]
		public async Task<IActionResult> DeletePlayer([FromBody] Player player)
		{
			if (ModelState.IsValid)
			{
				await _tournamentService.DeletePlayer(player);
				return Ok();
			}


			return BadRequest(ModelState);
		}
		#endregion

		#region TEAM

		public async Task<IActionResult> AddTeams([FromBody] Team team)
		{
			if (ModelState.IsValid)
			{
				await _tournamentService.AddTeam(team);
				return Ok(team);
			}
			return BadRequest(ModelState);
		}
		[HttpGet]
		public async Task<IActionResult> FetchTeams()
		{
			var teams = await _tournamentService.FetchTeams();
			return Ok(teams);
		}

		[HttpPost]
		public async Task<IActionResult> UpdateTeam([FromBody] Team team)
		{
			if (ModelState.IsValid)
			{
				await _tournamentService.UpdateTeam(team);

				return Ok(new { message = "Team updated successfully!" });
			}

			return BadRequest(new { message = "Invalid input! Please provide valid team information." });
		}
		[HttpPost]
		public async Task<IActionResult> DeleteTeams([FromBody] Team team)
		{
			if (ModelState.IsValid)
			{
				await _tournamentService.DeleteTeam(team);
				return Ok();
			}
			return BadRequest(ModelState);
		}




		#endregion

		#region FIXTURE

		[HttpPost]
		public async Task<IActionResult> GenerateFixtures()
		{
			try
			{
				await _tournamentService.GenerateFixtures();
				return Ok();
			}
			catch
			{
				return BadRequest();
			}

		}
		public async Task<IActionResult> GetFixtures()
		{
			var fixtures = await _tournamentService.GetFixtures();

			return Json(fixtures);
		}
		[HttpPost]
		public async Task<IActionResult> UpdateFixtureGoals([FromBody] Match request)
		{
			if (ModelState.IsValid)
			{
				await _tournamentService.UpdateFixtureGoals(request);
				return Ok(new { message = "Fixture goals updated successfully!" });
			}

			return BadRequest(new { message = "Invalid input! Please provide valid fixture information." });
		}
		#endregion
	}
}