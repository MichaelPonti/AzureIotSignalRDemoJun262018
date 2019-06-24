using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiServer
{
	public class Startup
	{
		private static readonly HashSet<string> _allowedOrigins = new HashSet<string>()
		{
			"http://localhost:8224"
		};


		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.Configure<CookiePolicyOptions>(options =>
			{
				// This lambda determines whether user consent for non-essential cookies is needed for a given request.
				options.CheckConsentNeeded = context => true;
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
			services.AddSignalR();
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
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			// we need to allow cors for any api calls we provide
			app.UseCors(builder =>
			{
				builder.WithOrigins("http://localhost:8224")
					.AllowAnyHeader()
					.WithMethods("GET", "POST")
					.AllowCredentials();
			});

			// add some request handling to determine if the websocket connection
			// is coming from an allowed domain.
			app.Use((context, next) =>
			{
				if (String.Equals(context.Request.Headers["Upgrade"], "websocket"))
				{
					var origin = context.Request.Headers["Origin"];
					if (!String.IsNullOrWhiteSpace(origin) && !_allowedOrigins.Contains(origin))
					{
						context.Response.StatusCode = StatusCodes.Status403Forbidden;
						return Task.CompletedTask;
					}
				}

				return next();
			});

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseCookiePolicy();

			// map routes to signalr hub. The one hub is going to have four
			// separate channels to take readings from.
			app.UseSignalR(routes => routes.MapHub<Hubs.PlantStatusHub>("/plantstatus"));

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
