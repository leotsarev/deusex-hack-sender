using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DeusExHackSender.JoinRpg;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using MailKit.Net.Smtp;
using MimeKit;

namespace DeusExHackSender
{
  static class Program
  {
    static void Main() => MainAsync().GetAwaiter().GetResult();

    private static readonly ILog Log = LogManager.GetLogger("test", typeof(Hierarchy));

    private static async Task MainAsync()
    {



      var settings = SettingsLoader.GetSettings();
      ConfigureLogging();
      Log.Info("Start");

      var assetHelper = new AssetHelper(settings);
      var client = await JoinRpgFacade.CreateClient(settings);

      var sender = Task.Run(async () => await SendAllFles(settings, assetHelper, client, settings));

    var characterIdToMonitor = assetHelper.GetCharactersToMonitor();

      Log.InfoFormat("Characters to monitor: {0}", characterIdToMonitor.Count);

     
      var characters = await client.GetModifiedCharacters(new DateTime(2017, 07, 21));

      foreach (var characterHeader in characters.Where(
        c => characterIdToMonitor.Contains(c.CharacterId)))
      {
        Log.Info($"Loading data about character {characterHeader.CharacterId}");
        var character = await client.GetCharacter(characterHeader.CharacterId);
        if (character.InGame)
        {
          Log.Info($"Character in game {characterHeader.CharacterId}");
          await assetHelper.MarkToSendById(characterHeader.CharacterId);
        }
      }

      await sender;

      //Run again
      await SendAllFles(settings, assetHelper, client, settings);
    }

    private static async Task SendAllFles(DehsSettings settings1, AssetHelper assetHelper,
      JoinRpgClient joinRpgClient, DehsSettings settings)
    {
      using (var mailClient = new MailClient(settings1))
      {
        mailClient.Connect();
        await assetHelper.SendAllFiles(
          (file, characterId) => SendFileToCharacter(joinRpgClient, characterId, file, settings,
            mailClient));
      }
    }

    private static void ConfigureLogging()
    {
      var repo = LogManager.CreateRepository(
        "test", typeof(Hierarchy));

      var hierarchy = (Hierarchy) repo;
      var layout = new PatternLayout("%date [%thread] %level - %message%newline");
      hierarchy.Root.AddAppender(new ConsoleAppender
      {
        Layout = layout
      });

      hierarchy.Configured = true;
    }

    private static async Task<bool> SendFileToCharacter(JoinRpgClient joinRpgClient,
      int characterId, FileSystemInfo file, DehsSettings settings, MailClient mailClient)
    {
      Log.Info($"[{characterId}] File {file.Name}");
      var character = await joinRpgClient.GetCharacter(characterId);
      
      var email = character.Fields
        .SingleOrDefault(field => field.ProjectFieldId == settings.EmailFieldId)?.Value;

      Log.Info($"[{characterId}] Email {email} ready, applying delay");

      await Task.Delay(20* 1000);

      if (string.IsNullOrWhiteSpace(email))
      {
        return false;
      }

      try
      {

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(settings.FromName, settings.FromEmail));
        message.To.Add(new MailboxAddress(email, email + "@" + settings.EmailServer));
        message.Subject = "Хакерский аккаунт и стартовый набор кодов";

        message.Body = new TextPart("plain")
        {
          Text = File.ReadAllText(file.FullName)
        };

        await mailClient.SendAsync(message);
      }
      catch (Exception exception)
      {
        Log.Warn($"Failed to send to address {email}. Reason {exception}");
        return false;
      }
      Log.Info("Send success");
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