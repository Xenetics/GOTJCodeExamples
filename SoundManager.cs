using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Date Created: 11/09/15 ->
/// Handles sound play requests
/// </summary>
public class SoundManager : MonoBehaviour
{
    private static SoundManager instance = null;
    public static SoundManager Instance { get { return instance; } }

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
    /// <summary> Types of sound that can be called </summary>
    public enum SoundType { Music, Ambient, SFX, Voice, System }
	/// <summary> States the music can be in </summary>
	public enum MusicState { Menu, Shop, Game, Infection, Owie }
	/// <summary> Current state of the music </summary>
	private MusicState m_CurMusicState = MusicState.Menu;
	/// <summary> Next state of the music </summary>
	private MusicState m_NextMusicState = MusicState.Menu;
    [Header("Sound & Music Lists")]
	/// <summary> These will contain all level specific tracks </summary>
	public List<Theme> LevelThemes = new List<Theme>();
	/// <summary> The current musical theme </summary>
	[HideInInspector]
	public Theme theme;
    /// <summary> signals a theme change </summary>
    private bool m_ThemeChange = false;
    /// <summary> List of musics in the game </summary>
    [SerializeField]
    private List<AudioClip> m_Music_List = new List<AudioClip>();
	/// <summary> list of ambient sounds in the game </summary>
	[SerializeField]
	private List<AudioClip> m_Ambient_List = new List<AudioClip>();
    /// <summary> list of Sound FX in the game </summary>
    [SerializeField]
    private List<AudioClip> m_SFX_List = new List<AudioClip>();
    /// <summary> list of voice clips for the game </summary>
    [SerializeField]
    private List<AudioClip> m_Voice_List = new List<AudioClip>();
    /// <summary> list of system sounds </summary>
    [SerializeField]
    private List<AudioClip> m_System_List = new List<AudioClip>();
    /// <summary> List of sounds spawning in the level </summary>
    public List<AudioSource> DynamicSources = new List<AudioSource>();
    [Header("Audio Sources")]
    /// <summary> Music audio source </summary>
	public AudioSource Music_Source;
	/// <summary> Ambient audio source </summary>
	public AudioSource Ambient_Source;
    /// <summary> SFX audio source </summary>
	public AudioSource SFX_Source;
    /// <summary> Voice audio source </summary>
	public AudioSource Voice_Source;
    /// <summary> System audio source </summary>
	public AudioSource System_Source;
    /// <summary> Mute Boolean </summary>
    [HideInInspector]
	public bool Mute;
	/// <summary> Time it takes for music to fad in and out </summary>
	[SerializeField]
	private float m_FadeTime = 0.5f;
	/// <summary> The Timer used to account for the fade </summary>
	private float m_Timer;
	/// <summary> Gate used for transitioning in music </summary>
	private bool m_TransitionIn = false;
	/// <summary> Gate used for transitioning out music </summary>
	private bool m_TransitionOut = false;
    /// <summary> is music speeding up </summary>
    private bool m_SpeedUp = false;
    /// <summary> MAx pitch of music below the line </summary>
    [SerializeField]
    private float m_MaxPitch = 1.5f;
    /// <summary> Speed at which the music speeds up </summary>
    [SerializeField]
    private float m_SpeedUpSpeed = 0.1f;
    /// <summary> min volume level for the dynamic sounds </summary>
    private float m_MinDynamicVol = 0.1f;

    void Update()
	{
        HandleTransition();

        HandleMusicState();
    }

    /// <summary> Finds sound and plays it based on type and string identifier </summary>
    /// <param name="type"> Type of soundmixer to use </param>
    /// <param name="sound"> Name of sound </param>
    public void PlaySound (SoundType type, string sound)
    {
        switch (type)
        {
        case SoundType.Music:
            Music_Source.clip = FindSound(m_Music_List, sound);
			if (!Mute)
			{
				Music_Source.Play();
			}
			break;
		case SoundType.Ambient:
            SFX_Source.clip = FindSound(m_Ambient_List, sound);
			if (!Mute)
			{
                SFX_Source.Play();
			}
			break;
        case SoundType.SFX:
			if (!Mute)
			{
				SFX_Source.PlayOneShot(FindSound(m_SFX_List, sound));
			}	
			break;
        case SoundType.Voice:
			if (!Mute)
			{
				Voice_Source.PlayOneShot(FindSound(m_Voice_List, sound));
			}
			break;
        case SoundType.System:
			if (!Mute)
			{
            	System_Source.PlayOneShot(FindSound(m_System_List, sound));
			}
            break;
        }
	}

	/// <summary> Finds sound and plays it based on type and give sound file </summary>
	/// <param name="type"> Type of soundmixer to use </param>
	/// <param name="sound"> an audio clip to play </param>
	public void PlaySound (SoundType type, AudioClip sound)
	{
		switch (type)
		{
		case SoundType.Music:
			Music_Source.clip = sound;
			if (!Mute)
			{
				Music_Source.Play();
			}
			break;
		case SoundType.Ambient:
            Ambient_Source.clip = sound;
			if (!Mute)
			{
				Ambient_Source.Play();
			}
            break;
		case SoundType.SFX:
			if (!Mute)
			{
				SFX_Source.PlayOneShot(sound);
			}
			break;
		case SoundType.Voice:
			if (!Mute)
			{
				Voice_Source.PlayOneShot(sound);
			}
			break;
		case SoundType.System:
			if (!Mute)
			{
				System_Source.PlayOneShot(sound);
			}
			break;
		}
	}

    /// <summary> Finds sound and plays it based on type and string identifier </summary>
    /// <param name="type"> Type of soundmixer to use </param>
    /// <param name="sound"> Name of sound </param>
    /// <param name="pos"> Position of sound </param>>
    public void PlaySound(SoundType type, string sound, float volume)
    {
        switch (type)
        {
            case SoundType.Music:
                Music_Source.clip = FindSound(m_Music_List, sound);
                if (!Mute)
                {
                    Music_Source.Play();
                }
                break;
            case SoundType.Ambient:
                Ambient_Source.clip = FindSound(m_Ambient_List, sound);
                if (!Mute)
                {
                    Ambient_Source.Play();
                }
                break;
            case SoundType.SFX:
                if (!Mute)
                {
                    SFX_Source.PlayOneShot(FindSound(m_SFX_List, sound), volume);
                }
                break;
            case SoundType.Voice:
                if (!Mute)
                {
                    Voice_Source.PlayOneShot(FindSound(m_Voice_List, sound));
                }
                break;
            case SoundType.System:
                if (!Mute)
                {
                    System_Source.PlayOneShot(FindSound(m_System_List, sound));
                }
                break;
        }
    }

    /// <summary> Finds all sounds strting with the string and plays a random one </summary>
    /// <param name="type"> Type of sound we are playing </param>
    /// <param name="sound"> String that we will find all of </param>
    public float PlaySoundRandom (SoundType type, string sound)
	{
        float length = 0;
		if (!Mute)
		{
			List<AudioClip> clips = new List<AudioClip>();
			int rando;
			switch (type)
			{
			case SoundType.SFX:
				foreach(AudioClip clip in m_SFX_List)
				{
					bool match = true;
					if(!clip.name.Contains(sound))
					{
						match = false;
					}
					
					if(match)
					{
						clips.Add (clip);
					}
				}
				rando = Random.Range(0, clips.Count);
				SFX_Source.PlayOneShot(clips[rando]);
                    length = clips[rando].length;
                    break;
                case SoundType.Voice:
				foreach(AudioClip clip in m_Voice_List)
				{
					bool match = true;
					if(!clip.name.Contains(sound))
					{
						match = false;
					}
					
					if(match)
					{
						clips.Add (clip);
					}
				}
				rando = Random.Range(0, clips.Count);
				Voice_Source.PlayOneShot(clips[rando]);
                    length = clips[rando].length;
                    break;
			case SoundType.System:
				foreach(AudioClip clip in m_System_List)
				{
					bool match = true;
					if(!clip.name.Contains(sound))
					{
						match = false;
					}

					if(match)
					{
						clips.Add (clip);
					}
				}
				rando = Random.Range(0, clips.Count);
				System_Source.PlayOneShot(clips[rando]);
                    length = clips[rando].length;
                    break;
			}
		}
        return length;
	}

    /// <summary> Searches the lsits for a specific sound </summary>
    /// <param name="list"> List we are searching </param>
    /// <param name="sound"> Sound we are searching for </param>
    /// <returns>  </returns>
    private AudioClip FindSound(List<AudioClip> list, string sound)
    {
        AudioClip clip = new AudioClip();
        foreach(AudioClip aClip in list)
        {
			if(aClip.name == sound)
            {
				clip = aClip;
                break;
            }
        }
        return clip;
    }

	/// <summary> Set the state of the Music </summary>
	/// <param name="newState"> Accepts a Music State and changes current state to match </param>
	public void SetMusicState(MusicState newState)
	{
		m_NextMusicState = newState;
		OnMusicStateExit();
	}

	/// <summary> Transitions the current music into the new one </summary>
	/// <param name="newState">New state.</param>
	private void TransitionMusicState()
	{
		m_CurMusicState = m_NextMusicState;
		OnMusicStateEnter();
	}

    /// <summary> changes the theme for the music </summary>
    public void ChangeTheme()
    {
        m_ThemeChange = true;
        SetMusicState(MusicState.Game);
    }
	
	/// <summary> Is called every frame and handles per state calls </summary>
	private void HandleMusicState()
	{
		switch (m_CurMusicState)
		{
		    case MusicState.Menu:
			
			    break;
		    case MusicState.Shop:

			    break;
		    case MusicState.Game:
                if (Application.loadedLevelName == "Game" && InfectedZone.Instance.isGeorgeInfected)
                {
                    SetMusicState(MusicState.Infection);
                }
                break;
		    case MusicState.Infection:
                if (Music_Source.pitch < m_MaxPitch && GameManager.Instance._playerState.currentState != PlayerStateData.PlayerState.Dead)
                {
                    Music_Source.pitch = Mathf.Lerp(Music_Source.pitch, m_MaxPitch, m_SpeedUpSpeed * Time.deltaTime);
                }
                if (Application.loadedLevelName == "Game" && !InfectedZone.Instance.isGeorgeInfected)
                {
                    SetMusicState(MusicState.Game);
                }
                break;
		    case MusicState.Owie:
			
			    break;
			    // More down here
		}
	}
	
	/// <summary> Is called when state is entered </summary>
	private void OnMusicStateEnter()
	{
        LoadData();
		switch (m_CurMusicState)
		{
		    case MusicState.Menu:
			    if(Music_Source.clip != null)
			    {
				    if(Music_Source.clip.name != "title_theme")
				    {
					    PlaySound(SoundType.Music, "title_theme");
					    m_TransitionIn = true;
				    }
			    }
			    else if (Music_Source.clip == null)
			    {
				    PlaySound(SoundType.Music, "title_theme");
				    m_TransitionIn = true;
			    }
			    break;
		    case MusicState.Shop:
			    if(Music_Source.clip != null)
			    {
				    if(Music_Source.clip.name != "store_theme1")
				    {
					    PlaySound(SoundType.Music, "store_theme1");
					    m_TransitionIn = true;
				    }
			    }
			    else if (Music_Source.clip == null)
			    {
				    PlaySound(SoundType.Music, "store_theme1");
				    m_TransitionIn = true;
			    }
			    break;
		    case MusicState.Game:
			    theme = LevelThemes[(int)LevelManager.Instance.currentArt];

			    if(GameManager.Instance.timeMode == GameManager.TimeMode.Day)
			    {
				    if(Music_Source.clip != null)
				    {
					    if(Music_Source.clip.name != theme.MusicDay.name)
					    {
						    PlaySound(SoundType.Music, theme.MusicDay);
						    PlaySound(SoundType.Ambient, theme.AmbientDay);
						    m_TransitionIn = true;
					    }
				    }
				    else if(Music_Source.clip == null)
				    {
					    PlaySound(SoundType.Music, theme.MusicDay);
					    PlaySound(SoundType.Ambient, theme.AmbientDay);
					    m_TransitionIn = true;
				    }
			    }
			    else
			    {
				    if(Music_Source.clip != null)
				    {
					    if(Music_Source.clip.name != theme.MusicNight.name)
					    {
						    PlaySound(SoundType.Music, theme.MusicNight);
						    PlaySound(SoundType.Ambient, theme.AmbientNight);
						    m_TransitionIn = true;
					    }
				    }
				    else if(Music_Source.clip == null)
				    {
					    PlaySound(SoundType.Music, theme.MusicNight);
					    PlaySound(SoundType.Ambient, theme.AmbientNight);
					    m_TransitionIn = true;
				    }
			    }
			    break;
		    case MusicState.Infection:
                m_SpeedUp = true;
			    //if(Music_Source.clip != null)
			    //{
				   // if(Music_Source.clip.name != "infected_loop")
				   // {
					  //  PlaySound(SoundType.Music, "infected_loop");
					  //  m_TransitionIn = true;
				   // }
			    //}
			    //else if (Music_Source.clip == null)
			    //{
				   // PlaySound(SoundType.Music, "infected_loop");
				   // m_TransitionIn = true;
			    //}
			    break;
		    case MusicState.Owie:
			
			    break;
			    // More down here
		}
	}
	
	/// <summary> Is called when state is exited </summary>
	private void OnMusicStateExit()
	{
		switch (m_CurMusicState)
		{
		    case MusicState.Menu:
			    if(Music_Source.clip == null)
			    {
				    m_TransitionOut = true;
                }
			    else if(m_NextMusicState == MusicState.Menu)
			    {
				
			    }
			    else if(Music_Source.clip != null && Music_Source.clip.name == "title_theme")
			    {
				    m_TransitionOut = true;
                }
			    break;
		    case MusicState.Shop:
			    if(m_NextMusicState != MusicState.Shop)
			    {
				    m_TransitionOut = true;
			    }
			    break;
		    case MusicState.Game:
			    if(m_NextMusicState != MusicState.Game || m_ThemeChange)
			    {
                    Ambient_Source.Stop();
                    m_TransitionOut = true;
                    m_ThemeChange = false;
			    }
			    break;
		    case MusicState.Infection:
                m_SpeedUp = false;
                Music_Source.pitch = 1;
                m_TransitionOut = true;
			    break;
		    case MusicState.Owie:
			    m_TransitionOut = true;
			    break;
			    // More down here
		}
	}

    /// <summary> handles the transition between musics </summary>
    private void HandleTransition()
    {
        if (m_TransitionOut)
        {
            m_Timer -= Time.unscaledDeltaTime;
            Music_Source.volume = GameManager.saveData.MusicVolume * (m_Timer / m_FadeTime);
            if (m_Timer <= 0)
            {
                m_TransitionOut = false;
                m_Timer = m_FadeTime;
                TransitionMusicState();
            }
        }

        if (m_TransitionIn)
        {
            m_Timer -= Time.unscaledDeltaTime;
            Music_Source.volume = GameManager.saveData.MusicVolume - (GameManager.saveData.MusicVolume * (m_Timer / m_FadeTime));
            if (m_Timer <= 0)
            {
                m_TransitionIn = false;
                m_Timer = m_FadeTime;
            }
        }
    }

    /// <summary> Stops the music for when you toggle mute </summary>
    public void MuteSound()
	{
		Mute = !Mute;
		GameManager.saveData.muteSound = !Mute;
		if(Mute)
		{
			Music_Source.Pause();
			Ambient_Source.Pause();
            DynamicSourceMute(true);
        }
		else
		{
			Music_Source.Play();
			Ambient_Source.Play();
            DynamicSourceMute(false);
        }
        
        GameSaveSystem.Instance.SaveGame();
	}

	/// <summary> Saves the sound settings </summary>
	public void SaveSoundOptions()
	{
		GameManager.saveData.muteSound = Mute;
		GameManager.saveData.MusicVolume = Music_Source.volume;
		GameManager.saveData.SFXVolume = SFX_Source.volume;
        DynamicSourceVolume();
        GameSaveSystem.Instance.SaveGame();
	}

    /// <summary> Sets the Sound to proper amount from savedata </summary>
    public void LoadData()
    {
        Mute = GameManager.saveData.muteSound;
        Music_Source.volume = GameManager.saveData.MusicVolume;
		Ambient_Source.volume = GameManager.saveData.SFXVolume;
        SFX_Source.volume = GameManager.saveData.SFXVolume;
        Voice_Source.volume = GameManager.saveData.SFXVolume;
        System_Source.volume = GameManager.saveData.SFXVolume;
        DynamicSourceVolume();
        if (Mute)
		{
			Music_Source.Pause();
			Ambient_Source.Pause();
            DynamicSourceMute(true);
        }
		else
		{
			if(!Music_Source.isPlaying)
			{
				Music_Source.Play();
				Ambient_Source.Play();
                DynamicSourceMute(false);
            }
		}
    }

    /// <summary> Slows all audio sources </summary>
    public void SlowMoOn(float _Speed)
    {
        Music_Source.pitch = _Speed;
        Ambient_Source.pitch = _Speed;
        SFX_Source.pitch = _Speed;
        Voice_Source.pitch = _Speed;
    }

    /// <summary> turns all audio sources back to normal </summary>
    public void SlowMoOff(float _Speed)
    {
        Music_Source.pitch = _Speed;
        Ambient_Source.pitch = _Speed;
        SFX_Source.pitch = _Speed;
        Voice_Source.pitch = _Speed;
    }

    /// <summary> Changes the volume of the dynamic sounds </summary>
    /// <param name="volume"> volume to change to</param>
    public void DynamicSourceVolume()
    {
        List<List<AudioSource>> _Lists = new List<List<AudioSource>>();
        bool placed = false;
        foreach (AudioSource source in DynamicSources)
        {
            placed = false;
            foreach (List<AudioSource> list in _Lists)
            {
                if(list[0].clip.name == source.clip.name)
                {
                    list.Add(source);
                    placed = true;
                    break;
                }
            }

            if(!placed)
            {
                List<AudioSource> newList = new List<AudioSource>();
                newList.Add(source);
                _Lists.Add(newList);
            }
        }

        uint count = 0;
        foreach (List<AudioSource> list in _Lists)
        {
            foreach(AudioSource source in list)
            {
                count++;
            }

            foreach (AudioSource source in list)
            {
                source.volume = SFX_Source.volume - (1f - (1f / (float)count));
                if(source.volume < m_MinDynamicVol)
                {
                    source.volume = m_MinDynamicVol * SFX_Source.volume;
                }
            }
            
            count = 0;
        }
    }

    /// <summary> Mutes dynamic sounds </summary>
    public void DynamicSourceMute(bool mute)
    {
        foreach (AudioSource sound in DynamicSources)
        {
            if (sound.clip.name.Contains("slide"))
            {
                if (!mute)
                {
                    if (GameManager.Instance._playerState.GetComponent<PlayerData>().slideSoundOn)
                    {
                        sound.Play();
                    }
                }
                else
                {
                    if (!GameManager.Instance._playerState.GetComponent<PlayerData>().slideSoundOn)
                    {
                        sound.Pause();
                    }
                }
            }
            else
            {
                if (!mute)
                {
                    sound.Play();
                }
                else
                {
                    sound.Pause();
                }
            }
        }
    }

    /// <summary> Adds a sound to dynamic sounds and changes the volume based on the number of concurrent sounds </summary>
    /// <param name="newsource"></param>
    public void AddDynamicSound(AudioSource newsource)
    {
        DynamicSources.Add(newsource);
        uint count = 0;
        List<AudioSource> tempList = new List<AudioSource>();
        foreach(AudioSource source in DynamicSources)
        {
            if(source.clip.name == newsource.clip.name)
            {
                count++;
                tempList.Add(source);
            }
        }

        foreach (AudioSource source in tempList)
        {
            source.volume = SFX_Source.volume - (1f - (1f / (float)count));
            if (source.volume < m_MinDynamicVol)
            {
                source.volume = m_MinDynamicVol;
            }
        }
    }

    /// <summary> Finds the length of a voice sound by name </summary>
    public float VoiceSoundLength(string soundName)
    {
        return FindSound(m_Voice_List, soundName).length;
    }

    /// <summary> Finds the length of a SFX sound by name </summary>
    public float SFXSoundLength(string soundName)
    {
        return FindSound(m_SFX_List, soundName).length;
    }
}
