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

namespace MerkatorS.Controllers
{
	public class AdministratorService : IAdministratorService
	{
		private readonly TournamentDBContext _dbContext;
		private readonly ILogger<AdministratorService> _logger;

		public AdministratorService(TournamentDBContext dbContext, ILogger<AdministratorService> logger)
		{
			_dbContext = dbContext;
			_logger = logger;
		}

		public async Task<Administrator> Login(string username, string password)
		{
			var user = await _dbContext.Administrators.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

			if (user != null)
			{
				return user;
			}

			throw new Exception($"Not found in DB!");
		}
	}
}
