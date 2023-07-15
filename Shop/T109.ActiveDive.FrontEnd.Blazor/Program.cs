using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using T109.ActiveDive.Core;
using T109.ActiveDive.DataAccess.DataAccess;
using T109.ActiveDive.FrontEnd.Blazor.Data;

namespace T109.ActiveDive.FrontEnd.Blazor
{
    public class Program
    {
        private static Serilog.ILogger _logger;

        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<App>("#app");

            var rootFolder = Directory.GetCurrentDirectory();

            var logsFolder = System.IO.Path.Combine(rootFolder, "logs");

            string logFilePath = System.IO.Path.Combine(logsFolder, GetNextFreeFileName(logsFolder, "T109.EventWebApi.Startup.Logs", "txt"));

            _logger = new LoggerConfiguration()
                                   .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
                                   .Enrich.FromLogContext()
                                   .WriteTo.BrowserConsole()
                                   .WriteTo.File(logFilePath)
                                   .CreateLogger();

            _logger.Information("p1--App entry point to configuration");

            builder.Services.AddSingleton(typeof(Serilog.ILogger), (x) => _logger);

            builder.Services.AddScoped(typeof(WebApiAsyncRepository<ActiveDiveEvent>), (x) => new WebApiAsyncRepository<ActiveDiveEvent>(_logger)
                                                                                                            .SetGetAllHostPath("getall/")
                                                                                                            .SetSearchHostPath("search/")
                                                                                                            .SetGetByIdOrNullHostPath("getbyIdornull/")
                                                                                                            .SetBaseAddress("https://storeapi01.t109.tech"));

            builder.Services.AddScoped(typeof(StoreManager), typeof(StoreManager));

            builder.Services.AddScoped(typeof(ComponentHub), (x) => new ComponentHub());

            builder.Services.AddSingleton(typeof(FrontEndSettings), (x) => new FrontEndSettings(false, false, false));

            _logger.Information("p2--configuration passed");

            await builder.Build().RunAsync();
        }

        private static string GetNextFreeFileName(string path, string nameBase, string extention)
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
