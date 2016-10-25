using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

using WebApplication5.Services;

namespace WebApplication5
{
  public class Startup
  {
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public Startup(IHostingEnvironment env)
    {
      var builder = new ConfigurationBuilder()
                          .SetBasePath(env.ContentRootPath)
                          .AddJsonFile("appsettings.json",                        optional: true, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                          .AddEnvironmentVariables();

      Configuration = builder.Build();
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public IConfigurationRoot Configuration { get; }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
      services.AddSingleton<IConfiguration>(Configuration);

      services.AddMvc();

      services.AddCors();

      services.AddScoped<IDataService,       DataService>();
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {

      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();

        loggerFactory.AddConsole(LogLevel.Information);
        loggerFactory.AddDebug();

        ILogger logger = loggerFactory.CreateLogger<Startup>();
        logger.LogInformation("System starting up in development mode");

      }

      app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().AllowCredentials().Build() );

      app.Use(async (httpContext, next) =>
      {
        httpContext.Response.Headers.Add("X-Frame-Options", "DENY");
        await next();
      });

      app.UseMvc();

    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  }
}
