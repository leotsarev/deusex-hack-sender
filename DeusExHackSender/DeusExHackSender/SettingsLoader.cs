using System.IO;
using YamlDotNet.Serialization;

namespace DeusExHackSender
{
  internal static class SettingsLoader
  {
    public static DehsSettings GetSettings()
    {
      var settings = new DehsSettings();
      var deserializer = new Deserializer();
      dynamic s;
      using (TextReader reader = File.OpenText(@"config.yml"))
      {
        s = deserializer.Deserialize(reader);
      }
      var creds = s["credentials"];
      settings.Endpoint = creds["endpoint"];
      settings.Login = creds["login"];
      settings.Password = creds["password"];
      settings.ProjectId = int.Parse(s["game"]["project_id"]);
      settings.AssetFolder = s["assets"]["folder"];

      settings.EmailFieldId = int.Parse(s["fields"]["email"]);

      settings.FromEmail = s["email"]["from_email"];
      settings.FromName = s["email"]["from_name"];
      settings.EmailServer = s["email"]["server"];


      settings.SmtpHost = s["smtp"]["host"];
      settings.SmtpLogin = s["smtp"]["login"];
      settings.SmtpPassword = s["smtp"]["password"];
      settings.SmtpPort = int.Parse(s["smtp"]["port"]);
      return settings;
    }
  }
}