using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthentication(AzureADB2CDefaults.BearerAuthenticationScheme)
        .AddAzureADB2CBearer(
            AzureADB2CDefaults.BearerAuthenticationScheme,
            AzureADB2CDefaults.JwtBearerAuthenticationScheme,
            options => { Configuration.Bind("AzureAdB2C", options); });

    services.PostConfigure<JwtBearerOptions>(AzureADB2CDefaults.JwtBearerAuthenticationScheme,
        options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    // since we're using AADB2C only, the first identity is the only identity
                    var identity = context.Principal.Identities.First();
               
                    // add our test role to the identity's claims collection with the right type
                    var extraClaim = new Claim(identity.RoleClaimType, "Extra");
                    identity.AddClaim(extraClaim);

                    return Task.CompletedTask;
                }
            };
        });

    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
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
