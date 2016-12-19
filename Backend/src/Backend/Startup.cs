using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using SimpleTokenProvider;
using Backend.Database;
using Backend.Database.Model;
using System.Security.Claims;

namespace Backend
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            
            
        }

        private static readonly string secretKey = "mysupersecret_secretkey!123";

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();




            //Database
            services.AddDbContext<UserContext>(options => options
                .UseInMemoryDatabase(databaseName: "UsersAuth")
                );
            services.AddScoped<UserContext>(provider => provider.GetService<UserContext>());
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            app.UseStaticFiles();

            // Add JWT generation endpoint:
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
            var options = new TokenProviderOptions
            {
                Audience = "ExampleAudience",
                Issuer = "ExampleIssuer",
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
            };

            var optionsBuilder = new DbContextOptionsBuilder<UserContext>();
            optionsBuilder.UseInMemoryDatabase(databaseName: "UsersAuth");

            using (var dbcontext = new UserContext(optionsBuilder.Options))
            {
                var testuser = new User() { Name = "klaasteake", EmailAddress = "klaasteake@hotmail.com", Password = "1234" };
                if (!dbcontext.Users.Contains(testuser))
                {
                    dbcontext.Users.Add(testuser);
                    dbcontext.SaveChanges();
                }

                app.UseMiddleware<TokenProviderMiddleware>(Options.Create(options));

                app.UseSimpleTokenProvider(new TokenProviderOptions
                {
                    Path = "/api/token",
                    Audience = "ExampleAudience",
                    Issuer = "ExampleIssuer",
                    SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
                    IdentityResolver = GetIdentity
                });
            }

                

            app.UseMvc();

            
        }

        private Task<ClaimsIdentity> GetIdentity(string username, string password, DbContext<UserContext> context)
        {
            if (context.Users.Contains())
            if (username == "TEST" && password == "TEST123")
            {
                return Task.FromResult(new ClaimsIdentity(new System.Security.Principal.GenericIdentity(username, "Token"), new Claim[] { }));
            }

            return Task.FromResult<ClaimsIdentity>(null);
        }
    }
    
}
