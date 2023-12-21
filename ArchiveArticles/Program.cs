using System;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

namespace ArchiveArticles;

internal static class Program
{
  private static readonly Logger logger = LogManager.GetCurrentClassLogger();

  private static void Main(string[] args)
  {
    LogManager.LoadConfiguration("nlog.config");
    LogManager.ReconfigExistingLoggers();
    SubscribeToUnhandledException();
    logger.Info("Start application");
    Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
  }
    
  private static void SubscribeToUnhandledException()
  {
    AppDomain.CurrentDomain.UnhandledException += (_, e) =>
    {
      logger.Error(e.ExceptionObject);
      LogManager.Shutdown();
      if (e.IsTerminating)
        Environment.Exit(-1);
    };
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

  private static ServiceProvider ConfigureServices(IConfiguration config)
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
