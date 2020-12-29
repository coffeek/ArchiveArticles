using System;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

namespace ArchiveArticles
{
  internal static class Program
  {
    private static readonly NLog.ILogger logger = LogManager.GetCurrentClassLogger();

    private static void Main(string[] args)
    {
      LogManager.LoadConfiguration("nlog.config");
      LogManager.ReconfigExistingLoggers();
      
      AppDomain.CurrentDomain.UnhandledException += (s, e) =>
      {
        logger.Error(e.ExceptionObject);
        LogManager.Shutdown();
        if (e.IsTerminating)
          Environment.Exit(-1);
      };
      logger.Info("Start application");
      Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
    }

    private static void Run(Options options)
    {
      logger.Trace("Configure application");
      var config = new ConfigurationBuilder().Build();
      var servicesProvider = ConfigureServices(config);

      logger.Trace("Start archiver");
      var archiver = servicesProvider.GetRequiredService<Archiver>();
      archiver.Archive(options.Path);

      logger.Info("Shutdown application");
    }

    private static IServiceProvider ConfigureServices(IConfiguration config)
    {
      return new ServiceCollection()
        .AddTransient<Archiver>()
        .AddLogging(loggingBuilder =>
        {
          loggingBuilder.ClearProviders();
          loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
          loggingBuilder.AddNLog(config);
        })
        .BuildServiceProvider();
    }
  }
}