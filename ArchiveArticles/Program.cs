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
  private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

  private static void Main(string[] args)
  {
    LogManager.LoadConfiguration("nlog.config");
    LogManager.ReconfigExistingLoggers();
    SubscribeToUnhandledException();
    Logger.Info("Start application");
    Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
  }
    
  private static void SubscribeToUnhandledException()
  {
    AppDomain.CurrentDomain.UnhandledException += (_, e) =>
    {
      Logger.Error(e.ExceptionObject);
      LogManager.Shutdown();
      if (e.IsTerminating)
        Environment.Exit(-1);
    };
  }

  private static void Run(Options options)
  {
    Logger.Trace("Configure application");
    var config = new ConfigurationBuilder().Build();
    var servicesProvider = ConfigureServices(config);

    Logger.Trace("Start archiver");
    var archiver = servicesProvider.GetRequiredService<Archiver>();
    archiver.Archive(options.Path);

    Logger.Info("Shutdown application");
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
