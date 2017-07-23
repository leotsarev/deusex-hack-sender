using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DeusExHackSender.JoinRpg
{
  internal static class JoinRpgFacade
  {
    public static async Task<JoinRpgClient> CreateClient(DehsSettings settings)
    {
      var token = await Authorize(settings);
      return  new JoinRpgClient(token, settings.Endpoint, settings.ProjectId);
    }

    private static async Task<AccessToken> Authorize(DehsSettings settings)
    {
      using (var httpClient = new HttpClient())
      {
        var result = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post,
          settings.Endpoint + "/x-api/token")
        {
          Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
          {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", settings.Login),
            new KeyValuePair<string, string>("password", settings.Password)
          }),
          Headers =
          {
            Accept = {new MediaTypeWithQualityHeaderValue("application/json")},
          }
        });

        var value = await result.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<AccessToken>(value);
      }
    }
  }
}