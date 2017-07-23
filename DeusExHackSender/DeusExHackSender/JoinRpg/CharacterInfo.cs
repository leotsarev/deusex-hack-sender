using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace DeusExHackSender.JoinRpg
{
  /// <summary>
  /// Full character info
  /// </summary>
  [PublicAPI]
  internal class CharacterInfo
  {
    /// <summary>
    /// Id
    /// </summary>
    public int CharacterId { get; set; }
    /// <summary>
    /// Last modified (UTC)
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    /// <summary>
    /// Active /deleted
    /// </summary>
    public bool IsActive { get; set; }
    /// <summary>
    /// True, if character in game (player checked in and not checked out)
    /// </summary>
    public bool InGame { get; set; }
    /// <summary>
    /// Has player or not
    /// </summary>
    public CharacterBusyStatus BusyStatus { get; set; }
    /// <summary>
    /// Groups that character part of 
    /// </summary>
    public IEnumerable<GroupHeader> Groups { get; set; }
    /// <summary>
    /// Field values
    /// </summary>
    public IEnumerable<FieldValue> Fields { get; set; }
    /// <summary>
    /// Player user id
    /// </summary>
    public int? PlayerUserId { get; set; }
  }
}