using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace ArchiveArticles;

internal sealed class Archiver(ILogger<Archiver> logger)
{
  public void Archive(string folderPath)
  {
    ArgumentNullException.ThrowIfNull(folderPath);

    var rootDirectory = new DirectoryInfo(folderPath);
    if (!rootDirectory.Exists)
      throw new DirectoryNotFoundException($"Folder not found: \"{folderPath}\"");

    logger.LogTrace("Search articles");
    var infos = rootDirectory.GetFileSystemInfos("*", SearchOption.TopDirectoryOnly);
    foreach (var filesFolder in infos.OfType<DirectoryInfo>().Where(di => di.Name.EndsWith("_files")))
    {
      var htmlFileName = filesFolder.Name[..^"_files".Length];
      logger.LogTrace("Process \"{htmlFileName}\"", htmlFileName);
      var htmlFile = infos.OfType<FileInfo>()
        .FirstOrDefault(fi => fi.Name == $"{htmlFileName}.html" || fi.Name == $"{htmlFileName}.htm");
      if (htmlFile == null)
        continue;
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
      logger.LogInformation("Archive \"{htmlFileName}\"", htmlFile.FullName);
      using var process = Process.Start(si);
      if (process == null)
      {
        logger.LogWarning("Failed to start archiver");
        continue;
      }
      process.WaitForExit();
      if (process.ExitCode != 0)
        logger.LogError(process.StandardError.ReadToEnd());
      else
        logger.LogInformation("Compress complete: \"{htmlFileName}\"", htmlFileName);
    }
  }
}
