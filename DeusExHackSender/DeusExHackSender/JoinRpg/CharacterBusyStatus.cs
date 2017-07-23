namespace DeusExHackSender.JoinRpg
{
  /// <summary>
  /// Has player or not
  /// </summary>
  internal enum CharacterBusyStatus
  {
    /// <summary>
    /// Has player
    /// </summary>
    HasPlayer,
    /// <summary>
    /// Has some claims, but nothing approved
    /// </summary>
    Discussed,
    /// <summary>
    /// No actve claims
    /// </summary>
    NoClaims,
    /// <summary>
    /// NPC should not have any claims
    /// </summary>
    Npc,
  }
}