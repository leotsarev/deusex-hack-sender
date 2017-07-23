using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DeusExHackSender
{
  internal class AssetHelper
  {
    public AssetHelper(DehsSettings settings)
    {
      Settings = settings;
    }

    private DehsSettings Settings { get; }

    public List<int> GetCharactersToMonitor()
    {
      var characterIdToMonitor = new List<int>();
      foreach (var file in new DirectoryInfo(NotSentFolder).GetFileSystemInfos())
      {
        var characterId = GetCharacterId(file);
        if (characterId != null) {
          characterIdToMonitor.Add((int) characterId);
        }
      }
      return characterIdToMonitor;
    }

    private static int? GetCharacterId(FileSystemInfo file)
    {
      var characterIdSString = file.Name.Split('.').FirstOrDefault();
      if (int.TryParse(characterIdSString, out var characterId))
      {
        return characterId;
        
      }
      return null;
    }

    private string NotSentFolder => Settings.AssetFolder+ "\\NotSent";
    private string PendingFolder => Settings.AssetFolder + "\\Pending";
    private string SentFolder => Settings.AssetFolder + "\\Sent";

    public async Task MarkToSendById(int characterId)
    {
      var files = await GetCharacterFiles(characterId, NotSentFolder);
      Directory.CreateDirectory(PendingFolder);
      await ProcessAllFiles(files, f => MoveToFolder(f, PendingFolder));
    }

    private void MoveToFolder(FileSystemInfo f, string folderName) => File.Move(f.FullName, folderName + "\\" + f.Name);

    private static async Task ProcessAllFiles(IEnumerable<FileSystemInfo> files, Action<FileSystemInfo> function) 
      => await Task.WhenAll(files.Select(file => { return Task.Run(() => function(file)); }));

    private static async Task ProcessAllFiles(IEnumerable<FileSystemInfo> files, Func<FileSystemInfo,Task> function)
      => await Task.WhenAll(files.Select(function));


    private static async Task<IReadOnlyCollection<FileSystemInfo>>
      GetCharacterFiles(int characterId, string folder) => 
      await Task.Run(() => new DirectoryInfo(folder).GetFileSystemInfos($"{characterId}.*.txt").ToList());

    private static async Task<IReadOnlyCollection<FileSystemInfo>>
      GetAllFiles(string folder) =>
      await Task.Run(() => new DirectoryInfo(folder).GetFileSystemInfos("*.txt").ToList());

    public async Task SendAllFiles(Func<FileSystemInfo, int, Task<bool>> sender)
    {
      var files = await GetAllFiles(PendingFolder);

      async Task SendFile(FileSystemInfo file)
      {
        var characterId = GetCharacterId(file);
        if (characterId != null)
        {
          var result = await sender(file, (int) characterId);
          if (result)
          {
            MoveToFolder(file, SentFolder);
          }
        }
      }

      await ProcessAllFiles(files, SendFile);
    }
  }
}