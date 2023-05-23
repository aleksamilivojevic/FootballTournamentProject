using MerkatorS.Controllers;
using MerkatorS.DBContext;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace MerkatorS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                // Retrieve the database context
                var dbContext = services.GetRequiredService<TournamentDBContext>();

                // Apply any pending migrations
                dbContext.Database.Migrate();
            }
            ApplyMigrations(host);
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices((hostContext, services) =>
                    {
						services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
						.AddCookie(options =>
						{
							options.LoginPath = "/Home/Login";
							options.LogoutPath = "/Home/Logout";
						});

						services.AddControllersWithViews();

                        var connectionString = hostContext.Configuration.GetConnectionString("TournamentDatabase");
                        services.AddDbContext<TournamentDBContext>(options => options.UseSqlServer(connectionString));
						services.AddScoped<ITournamentService, TournamentService>();
                        services.AddScoped<IAdministratorService, AdministratorService>();
					})
                    .Configure((hostContext, app) =>
                    {
                        var env = hostContext.HostingEnvironment;

                        if (env.IsDevelopment())
                        {
                            app.UseDeveloperExceptionPage();
                        }
                        else
                        {
                            app.UseExceptionHandler("/Home/Error");
                            app.UseHsts();
                        }

                        app.UseHttpsRedirection();
                        app.UseStaticFiles();

                        app.UseRouting();

                        app.UseAuthentication();
                        app.UseAuthorization();

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllerRoute(
                                name: "default",
                                pattern: "{controller=Home}/{action=Index}/{id?}");
                        });
                    });
                });
		public static void ApplyMigrations(IHost host)
		{
			using (var scope = host.Services.CreateScope())
			{
				var services = scope.ServiceProvider;
				var dbContext = services.GetRequiredService<TournamentDBContext>();
				dbContext.Database.Migrate();
			}
		}
	}

}