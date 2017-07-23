using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DeusExHackSender.JoinRpg
{
  internal class JoinRpgClient
  {
    private readonly AccessToken _token;
    private readonly string _endpoint;
    private readonly int _projectId;

    public JoinRpgClient(AccessToken token, string endpoint, int projectId)
    {
      _token = token;
      _endpoint = endpoint;
      _projectId = projectId;
    }

    public async Task<IEnumerable<CharacterHeader>> GetModifiedCharacters(DateTime dateTime)
    {
      using (var httpClient = new HttpClient())
      {
        var result = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get,
          $"{_endpoint}/x-game-api/{_projectId}/characters/?modifiedSince={dateTime:O}")
        {
          Headers =
        {
          Authorization = new AuthenticationHeaderValue("Bearer", _token.access_token)
        }
        });

        var value = await result.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<CharacterHeader[]>(value);
      }
    }

    public async Task<CharacterInfo> GetCharacter(int characterId)
    {
      using (var httpClient = new HttpClient())
      {
        var result = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get,
          $"{_endpoint}/x-game-api/{_projectId}/characters/{characterId}")
        {
          Headers =
          {
            Authorization = new AuthenticationHeaderValue("Bearer", _token.access_token)
          }
        });

        var value = await result.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<CharacterInfo>(value);
      }
    }
  }
}