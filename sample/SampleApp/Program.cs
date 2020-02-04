using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;
using System;
using System.IO;

namespace SampleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Logger logger = CreateLogger();

            var host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel()
                .UseStartup<Startup>()
                .ConfigureAppConfiguration(cb =>
                {
                    cb.AddEnvironmentVariables();
                })
                .UseSerilog(logger)
                .Build();

            host.Run();
        }

        private static Logger CreateLogger()
        {
            var logLevel = ParseLoggingLevel(Environment.GetEnvironmentVariable("LOG_LEVEL"));

            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Is(LogEventLevel.Verbose)
                .WriteTo.Console(new JsonFormatter(renderMessage: true), logLevel)
                .CreateLogger();
            return logger;
        }

        private static LogEventLevel ParseLoggingLevel(string logLevelRaw)
        {
            Enum.TryParse(logLevelRaw, out LogEventLevel level);
            return level as LogEventLevel? ?? LogEventLevel.Verbose;
        }
    }
}
