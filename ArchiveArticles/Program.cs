using System;
using CommandLine;
using NLog;

namespace ArchiveArticles
{
  internal static class Program
  {
    private static readonly ILogger log = LogManager.GetCurrentClassLogger();

    private static void Main(string[] args)
    {
      AppDomain.CurrentDomain.UnhandledException += (s, e) =>
      {
        log.Error(e.ExceptionObject);
        if (e.IsTerminating)
          Environment.Exit(-1);
      };
      Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
    }

    private static void Run(Options options)
    {
      var archiver = new Archiver(options.Path);
      archiver.Archive();
    }
  }
}
