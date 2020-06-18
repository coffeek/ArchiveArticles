using CommandLine;

namespace ArchiveArticles
{
  public class Options
  {
    [Option('p', "path", Required = true, HelpText = "Path to file storage. Specified path should exists")]
    public string Path { get; set; }
  }
}
