using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using VoxelBusters.NativePlugins;

/// <summary>
/// Date Created: 10/04/15 ->
/// Allows saving and loading of player game data
/// </summary>
public class GameSaveSystem : MonoBehaviour
{
    private static GameSaveSystem instance = null;
    public static GameSaveSystem Instance { get { return instance; } }

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
        //File.Delete(Application.persistentDataPath + "/SaveData.dat");
    }

    /// <summary> Creates save data and saves to a file </summary>
    public void SaveGame()
    {
        BinaryFormatter bf = new BinaryFormatter();
        //bf.Binder.
        FileStream file = File.Open(Application.persistentDataPath + "/SaveData.dat", FileMode.Create);
        if (GameManager.saveData.Contructed == false)
        {
            GameManager.saveData = Construct();
        }
        bf.Serialize(file, GameManager.saveData);
        file.Close();
    }

    /// <summary> Looks for a Save file and Losds that data into the current game </summary>
    public void LoadGame()
    {
        //NotificationManager.Instance.AddNotification(false, false, false, true, NotificationManager.Instance.AchievementImege, false, "File Exists", File.Exists(Application.persistentDataPath + "/SaveData.dat").ToString());
        if (File.Exists(Application.persistentDataPath + "/SaveData.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/SaveData.dat", FileMode.Open);
            GameManager.saveData = (UserData)bf.Deserialize(file);
            ReConstruct();
            file.Close();
        }
        else
        {
            SaveGame();
        }
    }

    /// <summary> Configures userdata to save </summary>
    public UserData Construct()
    {
        UserData returnData = new UserData();

        returnData.ShowAds = true;

        returnData.NoAdPlays = 0;

        returnData.LastFreebie = DateTime.MinValue;

        returnData.FirstPlayDay = new bool[(int)LevelManager.LevelArt.COUNT];
        for (int i = 0; i < returnData.FirstPlayDay.Length; ++i)
        {
            returnData.FirstPlayDay[i] = true;
        }

        returnData.FirstPlayNight = new bool[(int)LevelManager.LevelArt.COUNT];
        for (int i = 0; i < returnData.FirstPlayNight.Length; ++i)
        {
            returnData.FirstPlayNight[i] = true;
        }

        returnData.LevelsUnlocked = LevelManager.Instance.LevelsUnlocked;

        returnData.Currency = (GameManager.Instance.Testing) ? (100000) : (GameManager.Instance.TotalBugs);
        GameManager.Instance.TotalBugs = returnData.Currency;
        returnData.muteSound = SoundManager.Instance.Mute;
        returnData.MusicVolume = SoundManager.Instance.Music_Source.volume;
        returnData.SFXVolume = SoundManager.Instance.SFX_Source.volume;

        returnData.ItemsOwned = new bool[PowerUpManager.Instance.ShopListings.Count];
        for (int i = 0; i < returnData.ItemsOwned.Length; ++i)
        {
            returnData.ItemsOwned[i] = PowerUpManager.Instance.ShopListings[i].Owned;
        }

        returnData.ItemsOn = new bool[PowerUpManager.Instance.ShopListings.Count];
        for (int i = 0; i < returnData.ItemsOn.Length; ++i)
        {
            returnData.ItemsOn[i] = PowerUpManager.Instance.ShopListings[i].On;
        }

        returnData.Perma = new string[PowerUpManager.Instance.PermPowerUps.Count];
        for (int i = 0; i < returnData.Perma.Length; ++i)
        {
            returnData.Perma[i] = PowerUpManager.Instance.PermPowerUps[i];
        }

        returnData.PermaToggles = new bool[PowerUpManager.Instance.PermPowerSettings.Count];
        for (int i = 0; i < returnData.PermaToggles.Length; ++i)
        {
            returnData.PermaToggles[i] = PowerUpManager.Instance.PermPowerSettings[i];
        }

        returnData.PowersOn = PowerUpManager.Instance.CurrentUsedPowers;

        returnData.Temp = new string[PowerUpManager.Instance.TempPowerUps.Count];
        for (int i = 0; i < returnData.Temp.Length; ++i)
        {
            returnData.Temp[i] = PowerUpManager.Instance.TempPowerUps[i];
        }

        returnData.TempCount = new int[(int)Friend_Platform_Property.Buddy.COUNT];
        for (int i = 0; i < returnData.TempCount.Length; ++i)
        {
            returnData.TempCount[i] = 0;
        }

        returnData.Vanity = new string[PowerUpManager.Instance.VanityItems.Count];
        for (int i = 0; i < returnData.Vanity.Length; ++i)
        {
            returnData.Vanity[i] = PowerUpManager.Instance.VanityItems[i];
        }

        returnData.VanityToggles = new bool[PowerUpManager.Instance.VanitySettings.Count];
        for (int i = 0; i < returnData.VanityToggles.Length; ++i)
        {
            returnData.VanityToggles[i] = PowerUpManager.Instance.VanitySettings[i];
        }

        returnData.Skin = new Inventory();
        returnData.Skin.Head = "";
        returnData.Skin.Body = "";

        returnData.Achievements = new bool[AchievementMonitor.Instance.Achievements.Count];
        for (int i = 0; i < returnData.Achievements.Length; ++i)
        {
            returnData.Achievements[i] = AchievementMonitor.Instance.Achievements[i].Achieved;
        }

        returnData.AParams = new AchievementParams();

        returnData.LevelData = new LevelSaveData[(int)LevelManager.LevelArt.COUNT];
        for (int i = 0; i < returnData.LevelData.Length; ++i)
        {
            returnData.LevelData[i] = new LevelSaveData();
        }
        int j = 0;
        foreach (LevelManager.LevelArt level in Enum.GetValues(typeof(LevelManager.LevelArt)))
        {
            if (level == LevelManager.Instance.currentArt)
            {
                switch (GameManager.Instance.timeMode)
                {
                    case GameManager.TimeMode.Day:
                        returnData.LevelData[j].dHeight = GameManager.Instance.BestHieghtAttained;
                        if (GameManager.saveData.Contructed)
                        {
                            returnData.LevelData[j].nHeight = GameManager.saveData.LevelData[j].nHeight;
                        }
                        else
                        {
                            returnData.LevelData[j].nHeight = 0f;
                        }
                        break;
                    case GameManager.TimeMode.Night:
                        returnData.LevelData[j].nHeight = GameManager.Instance.BestHieghtAttained;
                        if (GameManager.saveData.Contructed)
                        {
                            returnData.LevelData[j].dHeight = GameManager.saveData.LevelData[j].dHeight;
                        }
                        else
                        {
                            returnData.LevelData[j].dHeight = 0f;
                        }
                        break;
                }
            }
            else
            {
                if (GameManager.saveData.Contructed)
                {
                    returnData.LevelData[j].dHeight = GameManager.saveData.LevelData[j].dHeight;
                }
                else
                {
                    returnData.LevelData[j].dHeight = 0f;
                }
                if (GameManager.saveData.Contructed)
                {
                    returnData.LevelData[j].nHeight = GameManager.saveData.LevelData[j].nHeight;
                }
                else
                {
                    returnData.LevelData[j].nHeight = 0f;
                }
            }
            j++;
            if (j == (int)LevelManager.LevelArt.COUNT)
            {
                break;
            }
        }

        returnData.FriendSaves = new bool[(int)Friend_Platform_Property.Buddy.COUNT];
        for (int i = 0; i < returnData.FriendSaves.Length; ++i)
        {
            returnData.FriendSaves[i] = false;
        }

        returnData.PowerUpChunks = new bool[(int)LevelPowerUpTrigger.PowerUps.COUNT];
        for (int i = 0; i < returnData.PowerUpChunks.Length; i++)
        {
            returnData.PowerUpChunks[i] = false;
        }

        returnData.Contructed = true;
        return returnData;
    }

    /// <summary> Places all data in its proper place on load </summary>
    private void ReConstruct()
    {
        GameManager.Instance.TotalBugs = GameManager.saveData.Currency;

        SoundManager.Instance.LoadData();

        GameManager.Instance.LoadAdStatus(GameManager.saveData.ShowAds);

        LevelManager.Instance.LevelsUnlocked = GameManager.saveData.LevelsUnlocked;

        for (int i = 0; i < GameManager.saveData.ItemsOwned.Length; ++i)
        {
            PowerUpManager.Instance.ShopListings[i].Owned = GameManager.saveData.ItemsOwned[i];
        }

        for (int i = 0; i < GameManager.saveData.ItemsOn.Length; ++i)
        {
            PowerUpManager.Instance.ShopListings[i].On = GameManager.saveData.ItemsOn[i];
        }

        for (int i = 0; i < GameManager.saveData.Perma.Length; ++i)
        {
            PowerUpManager.Instance.PermPowerUps.Add(GameManager.saveData.Perma[i]);
        }

        for (int i = 0; i < GameManager.saveData.PermaToggles.Length; ++i)
        {
            PowerUpManager.Instance.PermPowerSettings.Add(GameManager.saveData.PermaToggles[i]);
        }

        PowerUpManager.Instance.CurrentUsedPowers = GameManager.saveData.PowersOn;

        //for (int i = 0; i < GameManager.saveData.Temp.Length; ++i)
        //{
        //    PowerUpManager.Instance.TempPowerUps.Add(GameManager.saveData.Temp[i]);
        //}

        for (int i = 0; i < GameManager.saveData.TempCount.Length; ++i)
        {
            PowerUpManager.Instance.TempPowerCounts[i] = GameManager.saveData.TempCount[i];
        }

        for (int i = 0; i < GameManager.saveData.Vanity.Length; ++i)
        {
            PowerUpManager.Instance.VanityItems.Add(GameManager.saveData.Vanity[i]);
        }

        for (int i = 0; i < GameManager.saveData.VanityToggles.Length; ++i)
        {
            PowerUpManager.Instance.VanitySettings.Add(GameManager.saveData.VanityToggles[i]);
        }

        for (int i = 0; i < GameManager.saveData.Achievements.Length; ++i)
        {
            AchievementMonitor.Instance.Achievements[i].Achieved = GameManager.saveData.Achievements[i];
        }

        AchievementMonitor.Instance.AParams = GameManager.saveData.AParams;

        PowerUpManager.Instance.Skin = GameManager.saveData.Skin;
    }
}