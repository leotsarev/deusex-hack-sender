using JetBrains.Annotations;
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace DeusExHackSender.JoinRpg
{
  [UsedImplicitly]
  internal class AccessToken
  {
    public string access_token { get; set; }
    public string token_type { get; set; }
    public int expires_in { get; set; }
  }
}