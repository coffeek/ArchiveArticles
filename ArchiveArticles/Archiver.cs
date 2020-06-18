using System.Diagnostics;
using System.IO;
using System.Linq;
using NLog;

namespace ArchiveArticles
{
  internal sealed class Archiver
  {
    private readonly string folderPath;

    private readonly ILogger log = LogManager.GetCurrentClassLogger();

    public void Archive()
    {
      var rootDirectory = new DirectoryInfo(this.folderPath);
      if (!rootDirectory.Exists)
        throw new DirectoryNotFoundException($"Folder not found: \"{this.folderPath}\"");

      var infos = rootDirectory.GetFileSystemInfos("*", SearchOption.TopDirectoryOnly);
      foreach (var filesFolder in infos.OfType<DirectoryInfo>().Where(di => di.Name.EndsWith("_files")))
      {
        var htmlFileName = filesFolder.Name.Substring(0, filesFolder.Name.Length - "_files".Length);
        var htmlFile = infos.OfType<FileInfo>()
          .FirstOrDefault(fi => fi.Name == $"{htmlFileName}.html" || fi.Name == $"{htmlFileName}.htm");
        if (htmlFile != null)
        {
          var si = new ProcessStartInfo
          {
            Arguments = $"a -r -sdel \"{htmlFileName}.7z\" -- \"{filesFolder.FullName}\" \"{htmlFile.FullName}\"",
            CreateNoWindow = true,
            FileName = "7z",
            ErrorDialog = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            WorkingDirectory = rootDirectory.FullName
          };
          this.log.Info($"Archive \"{htmlFile.FullName}\"");
          using var process = Process.Start(si);
          if (process != null)
          {
              process.WaitForExit();
              if (process.ExitCode != 0)
                  this.log.Error(process.StandardError.ReadToEnd());
              else
                  this.log.Info($"Compress complete: \"{htmlFileName}\"");
          }
        }
      }
    }

    public Archiver(string folderPath)
    {
      this.folderPath = folderPath;
    }
  }
}
