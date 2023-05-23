using MerkatorS.Models;


	public interface IAdministratorService
	{
		Task<Administrator> Login(string username, string password);
	}


