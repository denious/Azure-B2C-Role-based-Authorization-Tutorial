using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApplication1.Services;

namespace WebApplication1
{
    public class Startup
    {
        private static ServiceProvider _serviceProvider;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // inject msgraph client as transient to make sure bearer token is always renewed
            services.AddTransient(provider =>
            {
                var options = new MSGraphOptions();
                Configuration.Bind("MicrosoftGraph", options);

                var client = new MSGraphClient(options);
                return client;
            });

            // add AADB2C authentication
            services.AddAuthentication(AzureADB2CDefaults.BearerAuthenticationScheme)
                .AddAzureADB2CBearer(
                    AzureADB2CDefaults.BearerAuthenticationScheme,
                    AzureADB2CDefaults.JwtBearerAuthenticationScheme,
                    options => { Configuration.Bind("AzureAdB2C", options); });

            // configure identity post token validation to retrieve user roles and add them to identity claims
            services.PostConfigure<JwtBearerOptions>(AzureADB2CDefaults.JwtBearerAuthenticationScheme,
                options =>
                {
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            // get AADB2C identity by client ID
                            var applicationId = Configuration["AzureAdB2C:ClientId"];
                            var identity = context.Principal.Identities.First(o => o.HasClaim("aud", applicationId));

                            // get authenticated user ID
                            var subjectId = identity.FindFirst(ClaimTypes.NameIdentifier).Value;

                            // query user roles
                            var client = _serviceProvider.GetRequiredService<MSGraphClient>();
                            var roles = await client.GetUserRolesAsync(subjectId);

                            // add roles to identity's claims collection with the right type
                            foreach (var role in roles)
                            {
                                var roleClaim = new Claim(identity.RoleClaimType, role);
                                identity.AddClaim(roleClaim);
                            }
                        }
                    };
                });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // save DI container reference
            _serviceProvider = services.BuildServiceProvider();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
