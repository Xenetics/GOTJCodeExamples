using UnityEngine;
using System.Collections;

/// <summary>
/// Keeps Important gameplay data
/// </summary>
public class GameManager : MonoBehaviour
{
	private static GameManager instance = null;
	public static GameManager Instance { get { return instance; } }
	
	void Awake()
	{
		DontDestroyOnLoad(gameObject);
		if(instance != null && instance != this)
		{
			Destroy (gameObject);
			return;
		}
		else
		{
			instance = this;
		}
    }

    /// <summary> Previous game save data </summary>
    public static UserData saveData = new UserData();
    /// <summary> Current state of the player (jumping, falling, inAir) </summary>
	public PlayerStateData _playerState;
    /// <summary> GameState enum for Overall gamestate </summary>
	public enum GameState { Menu,  PreGame, Playing, Replay };
    /// <summary> Which state the game is currently in </summary>
    [SerializeField]
	private GameState m_CurrentGameState = GameState.Menu;
    /// <summary> Used to know what state to place the hud in </summary>
    public string HudstateToBe = "PreGame"; 
    /// <summary> Total curreny bugs to spend </summary>
    [HideInInspector]
    public float TotalBugs = 0f;
    /// <summary> Overall Best height ever attained </summary>
    [HideInInspector]
    public float BestHieghtAttained = 0f;
    /// <summary> Bugs collected in a given game </summary>
    [HideInInspector]
    public float bugsCollected = 0f;
    /// <summary> Height Attained in a given game </summary>
    [HideInInspector]
    public float heightAttained = 0f;
    /// <summary> Types of units to measure in </summary>
    public enum HeightUnits { Cm, Murica};
    /// <summary> Which measurment usits we are using </summary>
    public HeightUnits currentUnits;
    /// <summary> Height converted to current units </summary>
    public float heightPerWorldUnit;
    /// <summary> Global time scale for use when going to pause and back </summary>
    [HideInInspector]
    public float TimeScale = 1.0f;
    /// <summary> Enum for times of day </summary>
    public enum TimeMode { Auto, Day, Night }
    /// <summary> Curent time mode for level gen </summary>
    [HideInInspector]
    public TimeMode timeMode = TimeMode.Auto;
    /// <summary> The material use for coloring the UI based on time of day </summary>
    [SerializeField]
    private Material m_UIMaterial;
    /// <summary> The material use for coloring the UI Font based on time of day </summary>
    [SerializeField]
    private Material m_UIFontMaterial;
    /// <summary> The material use for coloring the UI based on opposite time of day </summary>
    [SerializeField]
	private Material m_OppositeUIMaterial;
    /// <summary> The material use for coloring the UI Font based on opposite time of day  </summary>
    [SerializeField]
    private Material m_UIOppositeFontMaterial;
    /// <summary> Color Used for the Day mode UI </summary>
    [SerializeField]
    private Color m_DayColor;
    /// <summary> Color Used for the Night mode UI </summary>
    [SerializeField]
    private Color m_NightColor;
	/// <summary> Main menu state to fall to when leaving game </summary>
	[HideInInspector]
	public MenuManager.MenuState _MenuStateFallBack = MenuManager.MenuState.Splash;
    /// <summary> If we should play the starting anim again </summary>
    [HideInInspector]
    public bool StartAnimPlay = true;
    /// <summary> Wheather this is the web build or not </summary>
    public bool WebVersion = false;
    /// <summary> if set true only use sham replays </summary>
    public bool ShamReplay = false;
    /// <summary> Wheather this is for testing or </summary>
    public bool Testing = false;
    /// <summary> only true if testing replay </summary>
    public bool ReplayTest = false;
    /// <summary> The hash for replay testing </summary>
    public string ReplayTestHash;
    public Transform bugLineIndicator;
    [SerializeField]
    private float globalTime = -1;
    [SerializeField]
    private bool globalTimeInitialized = false;
    /// <summary>A reference to the ground</summary>
    public Transform groundLatch;
    public Platform ground;
    public Landable_Trigger groundTrigger;
    private bool ReplayHud = false;
    public bool replayCoroutineDone = true;
    /// <summary> The Google Analytics Prefab Object </summary>
    public GoogleAnalyticsV3 googleAnalytics;
    private bool m_ShouldPlayAds = true;
	public string versionName = "English";

    public string TempMessage;

    // Use this for initialization
    void Start ()
    {
        replayCoroutineDone = true;
        GameSaveSystem.Instance.LoadGame();
        AutoTimeMode();
        BackDropSwap.Instance.Swap(timeMode);
        CalculateHeight();
        InitializeGlobalTime();
        googleAnalytics.StartSession();
        AchievementMonitor.Instance.AParams.noOfSessions++;
        googleAnalytics.LogEvent("Game", "Language", versionName, 0);
    }
	
	// Update is called once per frame
	void Update ()
    {
        HandleState();
        //if(globalTimeInitialized)
        //{
        //    UpdateGlobalTime();
        //}
    }

    /// <summary> Set the state of the Game </summary>
    /// <param name="newState"> Accepts a GameState and changes current state to match </param>
    public void SetState(GameState newState)
    {
        OnStateExit();
        m_CurrentGameState = newState;
        OnStateEnter();
    }

    /// <summary> Is called when GameState is exited </summary>
    private void OnStateExit()
    {
        switch (m_CurrentGameState)
        {
            case GameState.Menu:
                if (MenuManager.Instance != null)
                {
                    MenuManager.Instance.LeavingMenu();
                }
                break;
            case GameState.PreGame:
                break;
            case GameState.Playing:
                saveData.Currency = Instance.TotalBugs;
                GameSaveSystem.Instance.SaveGame();
                break;
            case GameState.Replay:
                ReplayHud = false;
                break;
        }
    }

    /// <summary> Is called every frame and handles per state calls </summary>
    private void HandleState()
    {
        switch (m_CurrentGameState)
        {
            case GameState.Menu:

                break;
            case GameState.PreGame:
                
                break;
            case GameState.Playing:
                if (currentUnits == HeightUnits.Cm)
                {
                    heightAttained = (int)((_playerState.transform.position.y * heightPerWorldUnit) / 30f);
                }
                else
                {
                    heightAttained = (int)((_playerState.transform.position.y * heightPerWorldUnit) / 12f);
                }

                if (heightAttained < 0)
                {
                    heightAttained = 0;
                }
                break;
            case GameState.Replay:
                if(Application.loadedLevelName == "ReplayGame")
                {
                    if (!ReplayHud)
                    {
                        HUDManager.Instance.SetState(HUDManager.HUDState.Replay);
                        ReplayHud = true;
                    }
                }

                if (Application.loadedLevelName == "ReplayGame" && currentUnits == HeightUnits.Cm)
                {
                    heightAttained = (int)((_playerState.transform.position.y * heightPerWorldUnit) / 30f);
                }
                else if (Application.loadedLevelName == "ReplayGame")
                {
                    heightAttained = (int)((_playerState.transform.position.y * heightPerWorldUnit) / 12f);
                }

                if (heightAttained < 0)
                {
                    heightAttained = 0;
                }
                break;
        }
    }

    /// <summary> Is called when state is entered </summary>
    private void OnStateEnter()
    {
        switch (m_CurrentGameState)
        {
            case GameState.Menu:
                replayCoroutineDone = true;
                NotificationManager.Instance.ClearNotifications();
                PowerTimerManager.Instance.ClearTimers();
                PowerTimerManager.Instance.KillInfectTimer();
                Time.timeScale = 1;
                StartAnimPlay = true;
                PowerUpManager.Instance.ClearActiveTemps();
                SoundManager.Instance.DynamicSources.Clear();
                if(NotificationManager.Instance.LoadingOn())
                {
                    NotificationManager.Instance.LoadToggle();
                }
                Application.LoadLevel("Menu");
                break;
            case GameState.PreGame:
                NotificationManager.Instance.ClearNotifications();
                PowerTimerManager.Instance.ClearTimers();
                PowerTimerManager.Instance.KillInfectTimer();
                if (Application.loadedLevelName != "Game")
                {
                    Application.LoadLevel("Game");
                }
                SoundManager.Instance.SetMusicState(SoundManager.MusicState.Game);
                break;
            case GameState.Playing:
                PowerTimerManager.Instance.ClearTimers();
                PowerTimerManager.Instance.KillInfectTimer();
                BestHieghtAttained = 0f;
                heightAttained = 0f;
                bugsCollected = 0f;
                break;
            case GameState.Replay:
                HudstateToBe = "Replay";
                Time.timeScale = 0;
                Application.LoadLevel("ReplayGame");
                break;
        }
    }

    /// <summary> Resets the dame when reset is called on pause </summary>
    public void ResetGame()
    {
        PreGameScreenHandler.Instance.SetLoading(true);
        StartAnimPlay = false;
        //_playerState.GetComponent<PlayerData>().ResetGeorge();
        TerrainGeneration.Instance.Reset();
        //CameraSystem.Instance.SetMode(CameraSystem.CameraMode.Restart, new Vector2(0, 0), CameraSystem.SetModeStat, CameraSystem.CameraMode.Track);
        CameraSystem.Instance.transform.position = new Vector3(CameraSystem.Instance.GetOrigin().x, CameraSystem.Instance.GetOrigin().y, CameraSystem.Instance.transform.position.z);
        CameraSystem.Instance.m_CinematicSpeed = 0;
        CameraSystem.Instance.SetMode(CameraSystem.CameraMode.Cinematic);
        _playerState.GetComponent<PlayerData>().ResetGeorge();
        SetState(GameState.PreGame);
        HUDManager.Instance.SetState(HUDManager.HUDState.PreGame);
    }

    private void CalculateHeight()
    {
        float georgeYBound = 0.86f;

        // If George's height = georgeYBound units in world space,
        // then X units in world space = GeorgeHeight / georgeYBound
        if (currentUnits == HeightUnits.Cm)
        {
            // Assuming George is 225 cm tall
            heightPerWorldUnit = 225f / georgeYBound;
        }
        else
        {
            // Therfore equally 88.5827 inch tall
            heightPerWorldUnit = 88.5827f / georgeYBound;
        }
    }

    /// <summary> Sets the initial timeMode to concur with time of day </summary>
    public void AutoTimeMode()
    {
        if (timeMode == TimeMode.Auto)
        {
            int hourOfDay = System.DateTime.Now.Hour;

            if (hourOfDay > 6 && hourOfDay < 18)
            {
                timeMode = TimeMode.Day;

                // hack for chunk testing
                if (MenuManager.Instance != null)
                {
                    TimeModeChange();
                }                
            }
            else
            {
                timeMode = TimeMode.Night;

                // hack for chunk testing
                if (MenuManager.Instance != null)
                {
                    TimeModeChange();
                }
            }
        }
    }

    /// <summary> When time mode changes perform </summary>
    public void TimeModeChange()
    {
        switch (timeMode)
        {
            case TimeMode.Day:
                m_UIMaterial.color = m_DayColor;
                m_UIFontMaterial.color = m_DayColor;
                m_OppositeUIMaterial.color = m_NightColor;
                m_UIOppositeFontMaterial.color = m_NightColor;
                if (m_CurrentGameState == GameState.Menu)
                {
                    MenuManager.Instance.OnTimeChange();
                    BackDropSwap.Instance.Swap(timeMode);
                }
                break;
            case TimeMode.Night:
                m_UIMaterial.color = m_NightColor;
                m_UIFontMaterial.color = m_NightColor;
                m_OppositeUIMaterial.color = m_DayColor;
                m_UIOppositeFontMaterial.color = m_DayColor;
                if (m_CurrentGameState == GameState.Menu)
                {
                    MenuManager.Instance.OnTimeChange();
                    BackDropSwap.Instance.Swap(timeMode);
                }
                break;
        }
    }

    /// <summary> Spends bugs when needed </summary>
    /// <param name="amount"> amount of bugs to spend </param>
    public void SpendBugs(int amount)
    {
        TotalBugs -= amount;
        saveData.Currency -= amount;
        GameSaveSystem.Instance.SaveGame();

        if (m_CurrentGameState == GameState.Menu)
        {
            StatusBarHandler.Instance.TopIndicatorUpdates();
        }
        else if (m_CurrentGameState == GameState.Playing || m_CurrentGameState == GameState.PreGame)
        {
            StatusBarManager.Instance.UpdateCounters();
        }
    }

    /// <summary> Adds bugs to total when earned </summary>
    /// <param name="amount"> amount to add </param>
    public void EarnBugs(int amount)
    {
        TotalBugs += amount;
        saveData.Currency += amount;
        GameSaveSystem.Instance.SaveGame();

        if (m_CurrentGameState == GameState.Menu)
        {
            StatusBarHandler.Instance.TopIndicatorUpdates();
        }
        else if (m_CurrentGameState == GameState.Playing || m_CurrentGameState == GameState.PreGame)
        {
            StatusBarManager.Instance.UpdateCounters();
        }
    }

    /// <summary>
    /// Get the globaltime at this instance
    /// </summary>
    public float GetGlobalTime()
    {
        return globalTime;
    }

    /// <summary>
    /// Initializes the globalTime
    /// </summary>
    public void InitializeGlobalTime()
    {
        globalTime = -1;
        globalTimeInitialized = true;
    }

    /// <summary>
    /// Stops the globall time tick
    /// </summary>
    public void StopGlobalTime()
    {
        globalTimeInitialized = false;
    }

    /// <summary> Ticks the globalTime </summary>
    void UpdateGlobalTime()
    {
        float tempTime = globalTime + Time.deltaTime;
        if(tempTime > 1)
        {
            tempTime -= 2;
        }
        globalTime = tempTime;
    }

    /// <summary> Getter for gamestate </summary>
    /// <returns> What state the game is in </returns>
    public GameState GetState()
    {
        return m_CurrentGameState;
    } 

    /// <summary> Returns the state of the metronome </summary>
    /// <returns> Metronome time </returns>
    public float GetMetronomeState()
    {
        float stateTime = Time.timeSinceLevelLoad;
        return (stateTime - Mathf.Floor(stateTime));
    }

    public float GetMetronomeState(float levelTime)
    {
        return (levelTime - Mathf.Floor(levelTime));
    }

    public Color GetCurrentColor()
    {
        if (timeMode == TimeMode.Day)
        {
            return m_DayColor;
        }
        else
        {
            return m_NightColor;
        }
    }

    /// <summary> Wow Abhi WTF </summary>
    /// <returns> a lazy mans return </returns>
    public bool isReplay()
    {
        return m_CurrentGameState == GameState.Replay;
    }

    /// <summary> Initializes george in the replay scene </summary>
    public void InitGeorgeReplay()
    {
        //GameObject george_root = _playerState.gameObject.transform.parent.gameObject;
        //george_root.transform.position = TerrainGeneration.Instance.instantiatedPlatforms[ReplayManager.Instance.CurrentReplay.PlatformIndex].GetComponent<Platform>().latchPoint.transform.position;
        //_playerState.GetComponent<PullAndRelease>().enabled = false;
        ////Vector2 vel = new Vector2(ReplayManager.Instance.CurrentReplay.XList[1], ReplayManager.Instance.CurrentReplay.YList[1]);
        CameraSystem.Instance.m_CinematicSpeed = 0;
        //CameraSystem.Instance.transform.position = new Vector3(0, TerrainGeneration.Instance.instantiatedPlatforms[ReplayManager.Instance.CurrentReplay.PlatformIndex].GetComponent<Platform>().latchPoint.transform.position.y, CameraSystem.Instance.transform.position.z);
        Time.timeScale = 1;
        //_playerState.GetComponent<Rigidbody2D>().AddForce(_playerState.GetComponent<PlayerData>().totalMass * vel, ForceMode2D.Impulse);     
        //StartCoroutine("InitGeorge");
        InitGeorge();
    }

    public void InitGeorge()
    {
        Vector2 currGrav = Physics2D.gravity;
        Physics2D.gravity = Vector2.zero;
        Vector2 f = _playerState.GetComponent<PlayerData>().totalMass * new Vector2(ReplayManager.Instance.CurrentReplay._georgeReplayData.velx, ReplayManager.Instance.CurrentReplay._georgeReplayData.vely);
        _playerState.GetComponent<PlayerData>().animator.GetComponent<Player_FSM_Handler>()._playerData.flickForce = f;
        Debug.Log("My Position = " + _playerState.GetComponent<PlayerData>().transform.position);
        //GameObject george_root = _playerState.gameObject.transform.parent.gameObject;
        //george_root.transform.position = TerrainGeneration.Instance.instantiatedPlatforms[ReplayManager.Instance.CurrentReplay.PlatformIndex].GetComponent<Platform>().latchPoint.transform.position;
        //_playerState.GetComponent<PlayerData>().animator.GetComponent<Player_FSM_Handler>().latchPoint = TerrainGeneration.Instance.instantiatedPlatforms[ReplayManager.Instance.CurrentReplay.PlatformIndex].GetComponent<Platform>().latchPoint.transform;
        ////TerrainGeneration.Instance.instantiatedPlatforms[ReplayManager.Instance.CurrentReplay.PlatformIndex].transform.GetChild(0).GetComponent<Landable_Trigger>().ForceEnter(_playerState.GetComponent<PlayerData>().animator.GetComponent<Collider2D>());
        //_playerState.GetComponent<PullAndRelease>().enabled = false;
        //CameraSystem.Instance.SetMode(CameraSystem.CameraMode.Attached);
        ////yield return new WaitForSeconds(0.2f);
        //_playerState.GetComponent<PlayerData>().animator.GetComponent<Player_FSM_Handler>().snapToLatch = true;
        //_playerState.GetComponent<PlayerData>().animator.SetBool("jump", false);
        _playerState.GetComponent<PullAndRelease>().enabled = false;
        //CameraSystem.Instance.SetMode(CameraSystem.CameraMode.Attached);
        //Vector2 currGrav = Physics2D.gravity;
        //Physics2D.gravity = Vector2.zero;
        GameObject george_root = _playerState.gameObject.transform.parent.gameObject;
        _playerState.GetComponent<PlayerData>().animator.GetComponent<Player_FSM_Handler>().latchPoint = _playerState.transform;
        george_root.transform.position = new Vector3(ReplayManager.Instance.CurrentReplay._georgeReplayData.posx, ReplayManager.Instance.CurrentReplay._georgeReplayData.posy, george_root.transform.position.z);
        //Vector2 f = _playerState.GetComponent<PlayerData>().totalMass * new Vector2(ReplayManager.Instance.CurrentReplay._georgeReplayData.velx, ReplayManager.Instance.CurrentReplay._georgeReplayData.vely);
        _playerState.GetComponent<Ointment>().timer = ReplayManager.Instance.CurrentReplay._georgeReplayData.ointTimer;
        _playerState.GetComponent<Ointment>().isExecuting = ReplayManager.Instance.CurrentReplay._georgeReplayData.ointBool;
        _playerState.GetComponent<SteelBoots>().count = ReplayManager.Instance.CurrentReplay._georgeReplayData.steelCount;
        _playerState.GetComponent<SteelBoots>().isExecuting = ReplayManager.Instance.CurrentReplay._georgeReplayData.steelBool;
        _playerState.GetComponent<StickyBoots>().count = ReplayManager.Instance.CurrentReplay._georgeReplayData.stickyCount;
        _playerState.GetComponent<StickyBoots>().isExecuting = ReplayManager.Instance.CurrentReplay._georgeReplayData.stickyBool;
        _playerState.GetComponent<TreeHugger>().count = ReplayManager.Instance.CurrentReplay._georgeReplayData.treeCount;
        _playerState.GetComponent<TreeHugger>().isExecuting = ReplayManager.Instance.CurrentReplay._georgeReplayData.treeBool;
        _playerState.GetComponent<ThicketShield>().count = ReplayManager.Instance.CurrentReplay._georgeReplayData.thicketCount;
        _playerState.GetComponent<ThicketShield>().isExecuting = ReplayManager.Instance.CurrentReplay._georgeReplayData.thicketBool;
        _playerState.GetComponent<SuperBounce>().isExecuting = ReplayManager.Instance.CurrentReplay._georgeReplayData.isSuperBounce;
        if(ReplayManager.Instance.CurrentReplay._georgeReplayData.isSuperBounce)
        {
            _playerState.GetComponent<SuperBounce>().landed = true;
            _playerState.GetComponent<PlayerData>().animator.GetComponent<Collider2D>().enabled = false;
        }
        _playerState.GetComponent<PlayerData>().animator.SetBool("jumpReady", true);
        _playerState.GetComponent<PlayerData>().animator.SetBool("jump", true);
        _playerState.GetComponent<PlayerData>().hip.SetActive(true);
        CameraSystem.Instance.transform.position = new Vector3(CameraSystem.Instance.transform.position.x, _playerState.transform.position.y, CameraSystem.Instance.transform.position.z);
        CameraSystem.Instance.SetMode(CameraSystem.CameraMode.Attached);
        Camera.main.transform.FindChild("CameraUp").GetComponent<CameraUpCheck>().enabled = true;
        Camera.main.transform.FindChild("CameraDown").GetComponent<CameraDownCheck>().enabled = true;
        Physics2D.gravity = currGrav;
        //yield break;
    }

    public void InitWebGeorgeReplay()
    {
        GameObject george_root = _playerState.gameObject.transform.parent.gameObject;
        george_root.transform.position = ReplayManager.Instance.WebReplay.Position;
        _playerState.GetComponent<PullAndRelease>().enabled = false;
        _playerState.GetComponent<PlayerData>().animator.SetBool("dead", true);
        CameraSystem.Instance.transform.position = new Vector3(CameraSystem.Instance.transform.position.x, _playerState.transform.position.y, CameraSystem.Instance.transform.position.z);
        CameraSystem.Instance.SetMode(CameraSystem.CameraMode.Attached);
        Camera.main.transform.FindChild("CameraUp").GetComponent<CameraUpCheck>().enabled = true;
        Camera.main.transform.FindChild("CameraDown").GetComponent<CameraDownCheck>().enabled = true;
        Time.timeScale = 1;
        //_playerState.GetComponent<Rigidbody2D>().AddForce(_playerState.GetComponent<PlayerData>().totalMass * ReplayManager.Instance.WebReplay.Velocity, ForceMode2D.Impulse);
    }

    /// <summary> Sets a friend to saved in save data </summary>
    /// <param name="friend"> The friend we are saving </param>
    public void FriendSaved(Friend_Platform_Property.Buddy friend)
    {
        saveData.FriendSaves[(int)friend] = true;
        switch (friend)
        {
            case Friend_Platform_Property.Buddy.Ape:
                PreGameScreenHandler.Instance.ApeButton.Unlock();
                break;
            case Friend_Platform_Property.Buddy.Shep:
                PreGameScreenHandler.Instance.ShepButton.Unlock();
                break;
            case Friend_Platform_Property.Buddy.Tookie:
                PreGameScreenHandler.Instance.TookieButton.Unlock();
                break;
            case Friend_Platform_Property.Buddy.Magnolia:
                PreGameScreenHandler.Instance.MagnoliaButton.Unlock();
                break;
        }
        AchievementMonitor.Instance.UpdateAchievement(AchievementMonitor.Achieve.Friends);
        GameSaveSystem.Instance.SaveGame();
    }

    /// <summary> Loads the shop from save data </summary>
    public void InitShopItems()
    {
        for(int i = 0; i < PowerUpManager.Instance.ShopListings.Count; ++i)
        {
            IfOwnedInuseAdd(i);
        }
    }

    /// <summary> Adds the power to list if owned </summary>
    private void IfOwnedInuseAdd(int i)
    {
        if (PowerUpManager.Instance.ShopListings[i].Owned && PowerUpManager.Instance.ShopListings[i].On)
        {
            switch (PowerUpManager.Instance.ShopListings[i].PowerUpType)
            {
                case ShopItem.PowerType.Perma:
                    if (!PowerUpManager.Instance.PermPowerUps.Contains(PowerUpManager.Instance.ShopListings[i].Literal))
                    {
                        PowerUpManager.Instance.AddPermPower(PowerUpManager.Instance.ShopListings[i].Literal);
                    }
                    break;
                case ShopItem.PowerType.Temp:
                    if (!PowerUpManager.Instance.TempPowerUps.Contains(PowerUpManager.Instance.ShopListings[i].Literal))
                    {
                        PowerUpManager.Instance.AddTempPower(PowerUpManager.Instance.ShopListings[i].Literal);
                    }
                    break;
                case ShopItem.PowerType.Vanity:
                    if (!PowerUpManager.Instance.VanityItems.Contains(PowerUpManager.Instance.ShopListings[i].Literal))
                    {
                        PowerUpManager.Instance.AddVanity(PowerUpManager.Instance.ShopListings[i].Literal);
                    }
                    break;
            }
        }
    }

    /// <summary> Returns true if key has been purchased </summary>
    public bool DoesOwnKey()
    {
        return saveData.KeyBought;
    }

    /// <summary> Returns true if ads can be played </summary>
    public bool GetShouldPlayAds()
    {
        return m_ShouldPlayAds;
    }

    /// <summary> Returns true if friendsaves contains a true </summary>
    public bool AFriendSaved()
    {
        for(int i = 0; i < saveData.FriendSaves.Length; ++i)
        {
            if(saveData.FriendSaves[i])
            {
                return true;
            }
        }
        return false;
    }

    /// <summary> Sets weather ads should play </summary>
    public void LoadAdStatus(bool t_ShouldAdsPlay)
    {
        saveData.ShowAds = t_ShouldAdsPlay;
        m_ShouldPlayAds = t_ShouldAdsPlay;
    }

    /// <summary> Turns ads off and saves data </summary>
    public void TurnAdsOff()
    {
        m_ShouldPlayAds = false;
        saveData.ShowAds = false;
        GameSaveSystem.Instance.SaveGame();
    }

    /// <summary> unlocks levels and powerups </summary>
    public void BuyKey()
    {
        saveData.KeyBought = true;
        LevelManager.Instance.LevelsUnlocked = 4;
        saveData.LevelsUnlocked = LevelManager.Instance.LevelsUnlocked;
        AchievementMonitor.Instance.UpdateAchievement(AchievementMonitor.Achieve.Level2);
        AchievementMonitor.Instance.UpdateAchievement(AchievementMonitor.Achieve.Level3);
        AchievementMonitor.Instance.UpdateAchievement(AchievementMonitor.Achieve.Level4);
        for(int i = 0; i < saveData.ItemsOwned.Length; i++)
        {
            saveData.ItemsOwned[i] = true;
        }
        for(int i = 0; i < PowerUpManager.Instance.ShopListings.Count; ++i)
        {
            if(PowerUpManager.Instance.ShopListings[i].PowerUpType == ShopItem.PowerType.Perma
            || PowerUpManager.Instance.ShopListings[i].PowerUpType == ShopItem.PowerType.Vanity)
            {
                PowerUpManager.Instance.ShopListings[i].Owned = true;
            }
        }
        AchievementMonitor.Instance.UpdateAchievement(AchievementMonitor.Achieve.Cosmetics);
        AchievementMonitor.Instance.UpdateAchievement(AchievementMonitor.Achieve.Powerups);
        GameSaveSystem.Instance.SaveGame();
        if (GetState() == GameState.Menu)
        {
            MainMenuHandler.Instance.UnlockLevelCheck();
        }
    }

    /// <summary> Returns true if network is available </summary>
    public bool TestConnection(bool t_overlay = true)
    {
        bool netOn = false;

        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            netOn = true;
        }
        else
        {
            if (t_overlay)
            {
                NotificationManager.Instance.AddOverlay(OverlayObject.OverlayType.General, NotificationManager.Instance.DefaultImage, 0, "Offline!", "Please check your connection settings and try again.");
            }
        }

        return netOn;
    }
}