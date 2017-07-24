using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DeusExHackSender.JoinRpg;
using JetBrains.Annotations;
using MailKit.Net.Smtp;
using MimeKit;

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

      foreach (var characterHeader in characters.Where(
        c => characterIdToMonitor.Contains(c.CharacterId)))
      {
        var character = await client.GetCharacter(characterHeader.CharacterId);
        if (character.InGame || true)
        {
          await assetHelper.MarkToSendById(characterHeader.CharacterId);
        }
      }

      using (var mailClient = new MailClient(settings))
      {
        mailClient.Connect();
        await assetHelper.SendAllFiles(
          (file, characterId) => SendFileToCharacter(client, characterId, file, settings, mailClient));
      }
      

      Console.ReadLine();
    }

    private static async Task<bool> SendFileToCharacter(JoinRpgClient joinRpgClient,
      int characterId, FileSystemInfo file, DehsSettings settings, MailClient mailClient)
    {
      var character = await joinRpgClient.GetCharacter(characterId);
      var email = character.Fields
        .SingleOrDefault(field => field.ProjectFieldId == settings.EmailFieldId)?.Value;

      if (string.IsNullOrWhiteSpace(email))
      {
        return false;
      }

      var message = new MimeMessage();
      message.From.Add(new MailboxAddress(settings.FromName,
        settings.FromEmail + "@" + settings.EmailServer));
      message.To.Add(new MailboxAddress(email, email));
      message.Subject = "Новые программы для тебя — КЛИКАЙ БЫСТРЕЙ";

      message.Body = new TextPart("plain")
      {
        Text = File.ReadAllText(file.FullName)
      };

      await mailClient.SendAsync(message);
      return true;
    }
  }

  class MailClient : IDisposable
  {
    private DehsSettings Settings { get; }
    private SmtpClient Client { get; }

    public MailClient(DehsSettings settings)
    {
      Settings = settings;
      Client = new SmtpClient();
      // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
      Client.ServerCertificateValidationCallback = (s, c, h, e) => true;

    }

    public void Connect()
    {
      Client.Connect(Settings.SmtpHost, Settings.SmtpPort);

      // Note: since we don't have an OAuth2 token, disable
      // the XOAUTH2 authentication mechanism.
      Client.AuthenticationMechanisms.Remove("XOAUTH2");

      // Note: only needed if the SMTP server requires authentication
      Client.Authenticate(Settings.SmtpLogin, Settings.SmtpPassword);

    }

    public void Dispose()
    {
      Client.Disconnect(true);
      Client?.Dispose();
    }

    public Task SendAsync(MimeMessage message)
    {
      return Client.SendAsync(message);
    }
  }
}