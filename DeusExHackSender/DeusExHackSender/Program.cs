using System;
using System.Linq;
using System.Threading.Tasks;
using DeusExHackSender.JoinRpg;

namespace DeusExHackSender
{
  static class Program
  {
    static void Main()
    {
      MainAsync().GetAwaiter().GetResult();
    }

    private static async Task MainAsync()
    {
      var settings = SettingsLoader.GetSettings();
      var assetHelper = new AssetHelper(settings);

      var characterIdToMonitor = assetHelper.GetCharactersToMonitor();

      var client = await JoinRpgFacade.CreateClient(settings);

      var characters = await client.GetModifiedCharacters(new DateTime(2017, 07, 21));

      foreach (var characterHeader in characters.Where(c => characterIdToMonitor.Contains(c.CharacterId)))
      {
        var character = await client.GetCharacter(characterHeader.CharacterId);
        if (character.InGame || true)
        {
          await assetHelper.MarkToSendById(characterHeader.CharacterId);
        }
      }

      await assetHelper.SendAllFiles((file, characterId) => Task.FromResult(true));

      Console.ReadLine();
    }
  }
}