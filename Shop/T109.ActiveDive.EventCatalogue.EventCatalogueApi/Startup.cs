using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace T109.ActiveDive.EventCatalogue.EventCatalogueApi
{
    public class Startup
    {

        private Serilog.ILogger _logger;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region Logger
            
            var rootFolder = Directory.GetCurrentDirectory();

            var logsFolder = System.IO.Path.Combine(rootFolder, "logs");

            string logFilePath = System.IO.Path.Combine(logsFolder, GetNextFreeFileName(logsFolder, "T109.EventWebApi.Startup.Logs", "txt"));

             _logger = new LoggerConfiguration()
                                    .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
                                    .Enrich.FromLogContext()
                                    .WriteTo.File(logFilePath)
                                    .WriteTo.Debug()
                                    .CreateLogger();

            _logger.Information("Startup entry point");

            services.AddSingleton(typeof(Serilog.ILogger), (x) => _logger);

            #endregion


            services.AddCors(confg =>
                confg.AddPolicy("AllowAll",
                    p => p.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()));

            services.AddSingleton(typeof(DiveEventInMemoryManager), (x) => new DiveEventInMemoryManager("https://storeapi01.t109.tech"));

            services.AddControllersWithViews();
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ActiveDiveEventWebApi", Version = "v1" });
            });

            _logger.Information("Startup passed ok");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ActiveDiveEventWebApi"));

            app.UseStaticFiles();
            app.UseCors("AllowAll");
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private string GetNextFreeFileName(string path, string nameBase, string extention)
        {
            String s;
            int i = 0;
            do
            {
                s = Path.Combine(path, $"{nameBase}_{i}.{extention}");
                if (!File.Exists(s)) { return s; }
                i++;
                if (i > 10000)
                {
                    throw new Exception($"Too much log files in directory {path}, please consider clearing");
                }
            }
            while (true);
        }
    }
}

