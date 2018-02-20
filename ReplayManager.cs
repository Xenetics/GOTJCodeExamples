using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using VoxelBusters.NativePlugins;
using System;
using System.Linq;
using System.Web;

/// <summary>
/// Date Created: 12/08/15 ->
/// Will build, save, retrieve, rebuild and manage replays
/// </summary>
public class ReplayManager : MonoBehaviour
{
    private static ReplayManager instance = null;
    public static ReplayManager Instance { get { return instance; } }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }

    /// <summary> Retrieved replays </summary>
    [HideInInspector]
    public List<string> Replays = new List<string>();
    /// <summary> Retrieved replays full versions </summary>
    public List<Replay> ReplaysFull = new List<Replay>();
    /// <summary> Recent replays Saved </summary>
    private List<Replay> m_RecentReplays = new List<Replay>();
    /// <summary> Replay currently being handeled </summary>
    [HideInInspector]
    public Replay CurrentReplay;
    /// <summary> URL of the site we will have the replay sent to </summary>
    [SerializeField]
    private string m_ReplayWebsite;
    /// <summary> Url for the share of a replay </summary>
    [SerializeField]
    private string m_ShareMessage;
    /// <summary> Url for the share of a replay </summary>
    private string m_Link;
    /// <summary> Share options to exclude </summary>
    [SerializeField]
    private eShareOptions[] m_excludedOptions = new eShareOptions[0];
    /// <summary> The place in the list of replays we are at </summary>
    private int m_CurrentPlace = 0;
    /// <summary> Amount of replays to pull down at a time  </summary>
    private int m_ReplaysPerPage = 10;
    /// <summary> Access token for the bitly API </summary>
    [SerializeField]
    private string m_BitlyAccessToken;
    /// <summary> Client ID for bitly oauth access </summary>
    [SerializeField]
    private string m_BitlyClientID;
    /// <summary> Client secret for bitly oauth</summary>
    [SerializeField]
    private string m_BitlySecret;
    /// <summary> Blobhelper hold auth info and we make calls through it to azure </summary>
    private AzureStorageConsole.BlobHelper blobHelper;
    /// <summary> Hash retrieved from the website </summary>
    public string HashFromWeb { get; set; }
    /// <summary> Replay for web </summary>
    public ShamReplayContainer WebReplay;

    /// <summary> Replay comrainers for the day </summary>
    private string[] m_DayContainers = { "dayjungle", "daywaterfall", "daythicket", "daymystery"};
    /// <summary> Replay comrainers for the night </summary>
    private string[] m_NightContainers = { "nightjungle", "nightwaterfall", "nightthicket", "nightmystery" };
    /// <summary> Current Selected container </summary>
    [SerializeField]
    private string m_CurrentContainer;
    /// <summary> Webreplay replayed yes or no</summary>
    private bool m_Replayed = false;

    void Start()
    {
        HashFromWeb = "NoHash";

        blobHelper = new AzureStorageConsole.BlobHelper(AzureStorageConsole.RESTHelper.GetAccount(AzureHelper.AccountName));
    }

    /// <summary> Starts the Web replay process </summary>
    public void LoadWebReplay()
    {
#if !UNITY_EDITOR
        if (GameManager.Instance.WebVersion)
        {
            CallOut();
            
            if (HashFromWeb.Length > 0 && !m_Replayed)
            {
                m_Replayed = true;
                //Debug.Log("The hash is " + HashFromWeb);
                CreateWebReplay();
                GameManager.Instance.SetState(GameManager.GameState.Replay);
            }
        }
#endif
    }

    /// <summary> Starts the test replay process </summary>
    public void LoadTestReplay()
    {
        HashFromWeb = GameManager.Instance.ReplayTestHash;
        if (GameManager.Instance.WebVersion)
        {
            if (HashFromWeb.Length > 0 && !m_Replayed)
            {
                m_Replayed = true;
                CreateWebReplay();
                GameManager.Instance.SetState(GameManager.GameState.Replay);
            }
        }
    }

    /// <summary> Builds a Replay string </summary>
    public void CreateReplay()
    {
        m_CurrentContainer = GetContainerToUse();

        Replay replay = new Replay();
        replay.Replay_ID = LevelManager.Instance.currentArt.ToString()
                            + "L"
                            + GameManager.Instance.timeMode.ToString()
                            + "T"
                            + TerrainGeneration.Instance.seed
                            + "S"
                            + RemoveSymbols(HUDManager.Instance.TimeAtDeath.ToString("F3"))
                            + "G"
                            + RemoveSymbols(HUDManager.Instance.PositionAtDeath.ToString("F"))
                            + "P"
                            + RemoveSymbols(HUDManager.Instance.VelocityAtDeath.ToString("F"))
                            + "V"
                            + ((PowerUpManager.Instance.PermPowerUps.Count > 0) ? (1) : (0))
                            + ((TerrainGeneration.Instance.friendSaved) ? (1) : (0))
                            + ((int)TerrainGeneration.Instance.powerUpChunkDrawn).ToString()
                            + ((int)TerrainGeneration.Instance.obsChunkDrawn).ToString();
        Debug.Log(replay.Replay_ID); //DEBUG
        replay.Level = LevelManager.Instance.currentArt.ToString();
        replay.TimeOfDay = GameManager.Instance.timeMode.ToString();
        replay.Seed = TerrainGeneration.Instance.seed;
        replay.PlatformIndex = TerrainGeneration.Instance.GeorgePlatform;
        replay.FriendSaved = TerrainGeneration.Instance.friendSaved;
        replay.PowerUpChunkDrawn = TerrainGeneration.Instance.powerUpChunkDrawn;
        replay.ObsChunkDrawn = TerrainGeneration.Instance.obsChunkDrawn;
        replay.PlayerInput = GameManager.Instance._playerState.GetComponent<PlayerData>().playerInput;
        replay.Time = GameManager.Instance._playerState.GetComponent<PlayerData>().animator.GetComponent<Player_FSM_Handler>().lastGlobalTime - TerrainGeneration.Instance.lastGameResetTime;
        replay.InputTime = GameManager.Instance._playerState.GetComponent<PlayerData>().timeTillplayerInput;
        replay.PermPowerUps = PowerUpManager.Instance.PermPowerUps;
        replay.XList.Add(GameManager.Instance._playerState.GetComponent<PlayerData>().replayStartPos.x);
        replay.YList.Add(GameManager.Instance._playerState.GetComponent<PlayerData>().replayStartPos.y);
        replay.XList.Add(GameManager.Instance._playerState.GetComponent<PlayerData>().replayStartVel.x);
        replay.YList.Add(GameManager.Instance._playerState.GetComponent<PlayerData>().replayStartVel.y);
        replay.XList.Add(GameManager.Instance._playerState.GetComponent<PlayerData>().playerFlickForce.x);
        replay.YList.Add(GameManager.Instance._playerState.GetComponent<PlayerData>().playerFlickForce.y);
        replay._georgeReplayData = GameManager.Instance._playerState.GetComponent<PlayerData>().replay1;
        replay._inputReplayData = GameManager.Instance._playerState.GetComponent<PlayerData>().inputReplay;
        replay.deathTime = GameManager.Instance._playerState.GetComponent<PlayerData>().timeOfDeath;
        replay.CreationDateTime = DateTime.UtcNow;
        Debug.Log("UTCTTime " + replay.CreationDateTime.ToLongDateString());
        replay.Likes = 0;
        replay.Height = (uint)GameManager.Instance.BestHieghtAttained;

        m_RecentReplays.Add(replay);
        CurrentReplay = replay;
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = File.Create(Application.persistentDataPath + "/TempData.dat");
        bf.Serialize(fs, replay);
        fs.Close();

        FileStream fs2 = File.Open(Application.persistentDataPath + "/TempData.dat", FileMode.Open);
        BinaryReader br = new BinaryReader(fs2);
        byte[] bin = br.ReadBytes(Convert.ToInt32(fs2.Length));
        string rawData = Convert.ToBase64String(bin);
        fs2.Close();
        File.Delete(Application.persistentDataPath + "/TempData.dat");
        blobHelper.PutBlob(m_CurrentContainer, replay.Replay_ID, rawData);

        m_Link = LinkMaker(replay.Replay_ID);
        Debug.Log("Share Link: " + m_Link);

        Share();
    }

    /// <summary> Retrieve a single replay that is chosen to be replayed </summary>
    public Replay RetrieveReplay(string replayID)
    {
        Replay replayReturn;
        string rawData;
        HttpWebResponse response = blobHelper.GetBlob(m_CurrentContainer, replayID);
        Stream inputStream = response.GetResponseStream();
        StreamReader reader = new StreamReader(inputStream);
        rawData = reader.ReadToEnd();
        byte[] bin = Convert.FromBase64String(rawData);
        FileStream fs = File.Create(Application.persistentDataPath + "/TempData.dat");
        BinaryWriter bw = new BinaryWriter(fs);
        bw.Write(bin);
        fs.Close();
        FileStream fs2 = File.Open(Application.persistentDataPath + "/TempData.dat", FileMode.Open);
        BinaryFormatter bf = new BinaryFormatter();
        replayReturn = (Replay)bf.Deserialize(fs2);
        fs2.Close();
        File.Delete(Application.persistentDataPath + "/TempData.dat");
        return replayReturn;
    }

    /// <summary> retrieves a list of replays to display </summary>
    public void RetrieveReplays()
    {
        Reset();
        m_CurrentContainer = GetContainerToUse();
        string rawData = blobHelper.ListBlobs(m_CurrentContainer);
        string temp = "";
        for (int i = 0; i < rawData.Length; i++)
        {
            if (rawData[i] == '@')
            {
                Replays.Add(temp);
                temp = "";
            }
            else
            {
                temp = temp + rawData[i];
            }
        }
    }

    /// <summary> Retrieves a list of full replays from one to another instead of all </summary>
    public void RetrieveRangeOfReplays()
    {
        int count = 0;
        if (RangeCheck())
        {
            count = (m_CurrentPlace + m_ReplaysPerPage);
        }
        else
        {
            count = m_CurrentPlace + (Replays.Count - m_CurrentPlace);
        }
        for (int i = m_CurrentPlace; i < count; i++)
        {
            ReplaysFull.Add(RetrieveReplay(Replays[i]));
            m_CurrentPlace++;
        }
    }

    /// <summary> Used to take the Replay ID and build a site link for the replay. gives you a minified bitly link </summary>
    /// <param name="ReplayID"> ID of the replay you are building the link for </param>
    /// <returns> The bitly crushed link </returns>
    public string LinkMaker(string ReplayID)
    {
        string shortenedURL = "";
        string replayURL = "";
        replayURL += m_ReplayWebsite;
        replayURL += "%23";
        replayURL += ReplayID;
        
        ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => { return true; };

        string uri = "https://api-ssl.bitly.com/"
                    + "v3/"
                    + "shorten?"
                    + "access_token="
                    + m_BitlyAccessToken
                    + "&longURL="
                    + replayURL;
        
        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
        request.ContentType = "application/x-www-form-urlencoded";
        request.Method = "GET";
        request.Headers.Add("Authentication", m_BitlyAccessToken);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        Stream str = response.GetResponseStream();
        StreamReader reader = new StreamReader(str, System.Text.Encoding.UTF8);
        string read = reader.ReadToEnd();
        bool found = false;
        int skipped = 0;
        for (int i = 3; i < read.Length; ++i)
        {
			if(found && skipped < 4)
			{
				skipped++;
			}
			else
			{
				if (found && read[i] == '"')
				{
					break;
				}
				else if (found && read[i] != '\\')
				{
					shortenedURL += read[i];
				}
			}

            if (!found && (read[i - 3]) == '"' && (read[i - 2]) == 'u' && (read[i - 1]) == 'r' && (read[i]) == 'l')
            {
                found = true;
            }
        }
        return shortenedURL;
    }

    /// <summary> Is called from browser to pass down the hash </summary>
    /// <param name="theHash"> the hash from the url </param>
    private void RetrieveUrlHash(string theHash)
    {
        HashFromWeb = theHash;
    }

    /// <summary> tells the browser to call down with a function </summary>
    private void CallOut()
    {
        Application.ExternalCall("GiveHash");
    }

    /// <summary> REmoved symbols and replaces them with Markers </summary>
    /// <param name="dirtyString"> the string with symbols </param>
    /// <returns> a string with no symbols </returns>
	private string RemoveSymbols(string dirtyString)
	{
		string sanatizedString = "";

		for(int i = 0; i < dirtyString.Length; ++i)
		{
			if(dirtyString[i] == '.')
			{
				sanatizedString += "O";
			}
			else if(dirtyString[i] == ',')
			{
				sanatizedString += "C";
			}
            else if (dirtyString[i] == ' ')
            {

            }
			else if(dirtyString[i] == '(' || dirtyString[i] == ')')
			{
				continue;
			}
			else
			{
				sanatizedString += dirtyString[i];
			}
		}

		return sanatizedString;
	}

    /// <summary> Adds the symbols back to the string from url </summary>
    /// <param name="cleanString"> The non symbol string passed in </param>
    /// <returns> a string with symbols again </returns>
    private string AddSymbols(string cleanString)
    {
        string dirtyString = "";

        int symbolsPlaced = 0;

        for(int i = 0; i < cleanString.Length; ++i)
        {
            if (cleanString[i] == 'O')
            {
                dirtyString += ".";
            }
            else if (cleanString[i] == 'C')
            {
                dirtyString += ",";
            }
            else
            {
                dirtyString += cleanString[i];
            }
        }

        return dirtyString;
    }

    /// <summary> Takes the hash from web and converts to web replay </summary>
    public void CreateWebReplay()
    {
        string properString = AddSymbols(HashFromWeb);
        WebReplay = new ShamReplayContainer();
        string temp = "";
        string innerTemp = "";
        for (int i = 0; i < properString.Length; ++i)
        {
            switch (properString[i])
            {
                case 'L':
                    WebReplay.Level = (LevelManager.LevelArt)Enum.Parse(typeof(LevelManager.LevelArt), temp);
                    temp = "";
                    break;
                case 'T':
                    if (i != 1)
                    {
                        WebReplay.Time = (GameManager.TimeMode)Enum.Parse(typeof(GameManager.TimeMode), temp);
                        temp = "";
                    }
                    break;
                case 'S':
                    WebReplay.Seed = uint.Parse(temp);
                    temp = "";
                    break;
                case 'G':
                    WebReplay.GlobalTime = float.Parse(temp);
                    temp = "";
                    break;
                case 'P':
                    temp += properString[i];
                    for (int j = 0; j < temp.Length; ++j)
                    {
                        if (temp[j] == ',')
                        {
                            WebReplay.Position.x = float.Parse(innerTemp);
                            innerTemp = "";
                        }
                        else if (temp[j] == 'P')
                        {
                            WebReplay.Position.y = float.Parse(innerTemp);
                            innerTemp = "";
                        }

                        if (temp[j] != ',' && temp[j] != 'P')
                        {
                            innerTemp += temp[j];
                        }
                    }
                    temp = "";
                    break;
                case 'V':
                    temp += properString[i];
                    for (int j = 0; j < temp.Length; ++j)
                    {
                        if (temp[j] == ',')
                        {
                            WebReplay.Velocity.x = float.Parse(innerTemp);
                            innerTemp = "";
                        }
                        else if (temp[j] == 'V')
                        {
                            WebReplay.Velocity.y = float.Parse(innerTemp);
                            innerTemp = "";
                        }

                        if (temp[j] != ',' && temp[j] != 'V')
                        {
                            innerTemp += temp[j];
                        }
                    }
                    temp = "";
                    break;
            }

            if(i == (properString.Length - 4))
            {
                if (int.Parse(properString[i].ToString()) == 0)
                {
                    WebReplay.IfPowers = false;
                }
                else
                {
                    WebReplay.IfPowers = true;
                }
            }

            if (i == (properString.Length - 3))
            {
                if (int.Parse(properString[i].ToString()) == 0)
                {
                    WebReplay.FriendSaved = false;
                }
                else
                {
                    WebReplay.FriendSaved = true;
                }
            }

            if (i == (properString.Length - 2))
            {
                WebReplay.PowerUpChunkDrawn = (LevelPowerUpTrigger.PowerUps)(int.Parse(properString[i].ToString()));
            }

            if (i == (properString.Length - 1))
            {
                WebReplay.ObsChunkDrawn = (LevelDifficulty.Difficulty)(int.Parse(properString[i].ToString()));
            }

            if (properString[i] != '#' && properString[i] != 'L' && properString[i] != 'T' && properString[i] != 'S' && properString[i] != 'G' && properString[i] != 'P' && properString[i] != 'V')
            {
                temp += properString[i];
            }
            else if (i == 1)
            {
                temp += properString[i];
            }
        }
    }

    /// <summary> Handles the share button </summary>
    private void Share()
    {
#if UNITY_ANDROID || UNITY_IOS
        // Create share sheet
        ShareSheet _shareSheet = new ShareSheet();
        _shareSheet.Text = m_ShareMessage;
        _shareSheet.URL = m_Link;
        _shareSheet.ExcludedShareOptions = m_excludedOptions;

        // Show composer
        NPBinding.UI.SetPopoverPointAtLastTouchPosition();
        NPBinding.Sharing.ShowView(_shareSheet, FinishedSharing);
#endif
    }

    //Callback triggered once sharing is finished.
    private void FinishedSharing(eShareResult _result)
    {
        AchievementMonitor.Instance.UpdateAchievement(AchievementMonitor.Achieve.Share);
        //Debug.Log("Finished sharing");
        //Debug.Log("Share Result = " + _result);
    }

    /// <summary> Class for replay in WebGL </summary>
    public class ShamReplayContainer
    {
        public LevelManager.LevelArt Level;
        public GameManager.TimeMode Time;
        public uint Seed;
        public float GlobalTime;
        public Vector2 Position;
        public Vector2 Velocity;
        public bool IfPowers;
        public bool FriendSaved;
        public LevelPowerUpTrigger.PowerUps PowerUpChunkDrawn;
        public LevelDifficulty.Difficulty ObsChunkDrawn;
    }

    /// <summary> sorts replays by dateTime </summary>
    public void DateSort()
    {
        List<Replay> newList = ReplaysFull.OrderByDescending(o => o.CreationDateTime).ToList();
        ReplaysFull.Clear();
        ReplaysFull = newList;
    }

    /// <summary> sorts replays by Height </summary>
    public void HeightSort()
    {
        List<Replay> newList = ReplaysFull.OrderByDescending(o => o.Height).ToList();
        ReplaysFull.Clear();
        ReplaysFull = newList;
    }

    /// <summary> Sets current container to use on azure </summary>
    /// <param name="container"> the index of container </param>
    public void SetCurrentContainer(int container)
    {
        if (GameManager.Instance.timeMode == GameManager.TimeMode.Day)
        {
            m_CurrentContainer = m_DayContainers[container];
        }
        else if (GameManager.Instance.timeMode == GameManager.TimeMode.Night)
        {
            m_CurrentContainer = m_NightContainers[container];
        }
    }

    /// <summary> Returns what container should be used right now </summary>
    /// <returns> string for container to be used </returns>
    private string GetContainerToUse()
    {
        if (GameManager.Instance.timeMode == GameManager.TimeMode.Day)
        {
            return m_DayContainers[(int)LevelManager.Instance.currentArt];
        }
        else if (GameManager.Instance.timeMode == GameManager.TimeMode.Night)
        {
            return m_NightContainers[(int)LevelManager.Instance.currentArt];
        }
        return "";
    }

    /// <summary> Point on bar to scroll </summary>
    /// <returns> portion to scroll to </returns>
    public float ScrollTo()
    {
        return ((float)m_CurrentPlace / ((float)m_CurrentPlace + (float)m_ReplaysPerPage));
    }

    public bool RangeCheck()
    {
        if((m_CurrentPlace + m_ReplaysPerPage) < Replays.Count)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary> resets the replays </summary>
    public void Reset()
    {
        m_CurrentPlace = 0;
        Replays.Clear();
        ReplaysFull.Clear();
    }
}
