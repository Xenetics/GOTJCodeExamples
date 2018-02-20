using System;
using System.Collections.Generic;

/// <summary>
/// Date Created: 12/08/15 ->
/// Class for the replay to be presented through replay manager
/// </summary>
[Serializable]
public class Replay
{
    /// <summary> ID for the replay used by azure </summary>
    public string Replay_ID;
    /// <summary> Date of replay Creation </summary>
    public DateTime CreationDateTime;
    /// <summary> How many Times the has been liked </summary>
    public uint Likes;
    /// <summary> Height of this replay </summary>
    public uint Height;
    /// <summary> THe level the replay was recorded on </summary>
    public string Level;
    /// <summary> The time of day </summary>
    public string TimeOfDay;
    /// <summary> Seed and random count </summary>
    public uint Seed;
    /// <summary> Platform george was last on when he dies </summary>
    public int PlatformIndex;
    /// <summary> If friend was saved </summary>
    public bool FriendSaved;
    /// <summary> Keeps track of powerup chunk drawn per run </summary>
    public LevelPowerUpTrigger.PowerUps PowerUpChunkDrawn;
    /// <summary> Keeps track of obstacle chunk drawn per run </summary>
    public LevelDifficulty.Difficulty ObsChunkDrawn;
    /// <summary> Whether or not the player gave input on death </summary>
    public bool PlayerInput;
    /// <summary> Global time scale for setting moving pieces of the generation </summary>
    public float Time;
    /// <summary> Time till player input </summary>
    public float InputTime;
    /// <summary> Perm powerups active for this run </summary>
    public List<string> PermPowerUps = new List<string>();
    /// <summary> X Data to set george in motion </summary>
    public List<float> XList = new List<float>();
    /// <summary> X Data to set george in motion </summary>
    public List<float> YList = new List<float>();
    /// <summary> replay Data for George </summary>
    public GeorgeReplayData _georgeReplayData;
    /// <summary> replay Data for player Input </summary>
    public PlayerInputReplayData _inputReplayData;
    /// <summary> Time till player death </summary>
    public float deathTime;
}
