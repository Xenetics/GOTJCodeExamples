using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Created 12/11/15 ->
/// Monitor for the achievements
/// </summary>
public class AchievementMonitor : MonoBehaviour
{
    private static AchievementMonitor instance = null;
    public static AchievementMonitor Instance { get { return instance; } }

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

    public enum Achieve {
                            Friends,        Powerups,       Level2,         Level3,         Level4,
                            Height1,        Height2,        Height3,        Height4,        Cosmetics,
                            TheFall,        OverPowered,    JustSayNo,      GetBonked,      Repellant,
                            BugCollector1,  BugCollector2,  BugCollector3,  Infected1,      Infected2,
                            Infected3,      Dodger1,        Dodger2,        Dodger3,        TotalHeight,
                            TotalFall,      TotalBugs,      CloseCall,      Share,          Magnolia,
                            Tookie,         Shep,           Ape,            You,            Challenge
                        }

    /// <summary> Parameter container for achievements </summary>
    public AchievementParams AParams;
    /// <summary> List of Achievements for editing in the inspector </summary>
    public List<Achievement> Achievements = new List<Achievement>();

    /// <summary> Will be called at a time when an achievement may occur </summary>
    public void UpdateAchievement(Achieve chievo)
    {
        if (!GameManager.Instance.isReplay())
        {
            if (!Achievements[(int)chievo].Achieved)
            {
                bool t_Notify = false;
                int count = 0;
                int count2 = 0;
                switch (chievo)
                {
                    case Achieve.Friends:
                        foreach (bool save in GameManager.saveData.FriendSaves)
                        {
                            if (save)
                            {
                                count++;
                            }
                            if (count == 4)
                            {
                                t_Notify = true;
                            }
                        }
                        break;
                    case Achieve.Powerups:
                        foreach (ShopItem item in PowerUpManager.Instance.ShopListings)
                        {
                            if (item.PowerUpType == ShopItem.PowerType.Perma && item.Owned)
                            {
                                count++;
                            }
                        }

                        foreach (ShopItem item in PowerUpManager.Instance.ShopListings)
                        {
                            if (item.PowerUpType == ShopItem.PowerType.Perma)
                            {
                                count2++;
                            }
                        }

                        if (count == count2)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.Level2:
                        if (LevelManager.Instance.LevelsUnlocked >= 2)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.Level3:
                        if (LevelManager.Instance.LevelsUnlocked >= 3)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.Level4:
                        if (LevelManager.Instance.LevelsUnlocked >= 4)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.Height1:
                        if (GameManager.Instance.heightAttained > 500)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.Height2:
                        if (GameManager.Instance.heightAttained > 1500)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.Height3:
                        if (GameManager.Instance.heightAttained > 4500)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.Height4:
                        if (GameManager.Instance.heightAttained > 9000)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.Cosmetics:
                        foreach (ShopItem item in PowerUpManager.Instance.ShopListings)
                        {
                            if (item.PowerUpType == ShopItem.PowerType.Vanity && item.Owned)
                            {
                                count++;
                            }
                        }

                        foreach (ShopItem item in PowerUpManager.Instance.ShopListings)
                        {
                            if (item.PowerUpType == ShopItem.PowerType.Vanity)
                            {
                                count2++;
                            }
                        }

                        if (count == count2)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.TheFall:
                        if (HUDManager.Instance.FallDistance > 5000)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.OverPowered:
                        if (AParams.PowersThisRun >= 5)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.JustSayNo:
                        if (GameManager.Instance.BestHieghtAttained > AParams.HeightWithoutPower)
                        {
                            AParams.HeightWithoutPower = GameManager.Instance.BestHieghtAttained;
                        }
                        if (AParams.PowersThisRun == 0 && AParams.HeightWithoutPower > 1000)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.BugCollector1:
                        if (AParams.TotalBugs >= 100)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.BugCollector2:
                        if (AParams.TotalBugs >= 1000)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.BugCollector3:
                        if (AParams.TotalBugs >= 10000)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.Infected1:
                        if (AParams.TotalInfection >= 30)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.Infected2:
                        if (AParams.TotalInfection >= 120)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.Infected3:
                        if (AParams.TotalInfection >= 240)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.Dodger1:
                        if (GameManager.Instance.heightAttained >= 500 && GameManager.Instance.bugsCollected <= 5)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.Dodger2:
                        if (GameManager.Instance.heightAttained >= 1500 && GameManager.Instance.bugsCollected <= 15)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.Dodger3:
                        if (GameManager.Instance.heightAttained >= 5000 && GameManager.Instance.bugsCollected <= 50)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.TotalHeight:
                        if (AParams.TotalHeight >= 50000)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.TotalFall:
                        if (AParams.TotalFall >= 10000)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.TotalBugs:
                        if (AParams.TotalBugs >= 50000)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.Challenge:
                        if( AParams.LevelCompletions[0] >= 15000
                            && AParams.LevelCompletions[1] >= 15000
                            && AParams.LevelCompletions[2] >= 15000
                            && AParams.LevelCompletions[3] >= 15000)
                        {
                            t_Notify = true;
                        }
                        break;
                    case Achieve.GetBonked:
                        t_Notify = true;
                        break;
                    case Achieve.Repellant:
                        t_Notify = true;
                        break;
                    case Achieve.CloseCall:
                        t_Notify = true;
                        break;
                    case Achieve.Share:
                        t_Notify = true;
                        break;
                    case Achieve.Magnolia:
                        t_Notify = true;
                        break;
                    case Achieve.Tookie:
                        t_Notify = true;
                        break;
                    case Achieve.Shep:
                        t_Notify = true;
                        break;
                    case Achieve.Ape:
                        t_Notify = true;
                        break;
                    case Achieve.You:
                        t_Notify = true;
                        break;
                }

                if (t_Notify)
                {

                    GameManager.Instance.googleAnalytics.LogEvent("Game", "Achievement Unlocked", Achievements[(int)chievo].Title, 1);
                    int n = (int)GameManager.Instance.TotalBugs;
                    string s = "";
                    if (n == 0)
                    {
                        s = "0";
                    }
                    else if (n <= 100)
                    {
                        s = "1-100";
                    }
                    else if (n <= 500)
                    {
                        s = "101-500";
                    }
                    else if (n <= 2000)
                    {
                        s = "501-2000";
                    }
                    else if (n <= 5000)
                    {
                        s = "2001-5000";
                    }
                    else if (n <= 10000)
                    {
                        s = "5001-10000";
                    }
                    else
                    {
                        s = "10000+";
                    }
                    GameManager.Instance.googleAnalytics.LogEvent(new EventHitBuilder().SetEventCategory("Game").SetEventAction("Bugs Collected").SetEventLabel("Achievement").SetEventValue((int)(Achievements[(int)chievo].Reward)).SetCustomDimension(6, GameManager.Instance.TotalBugs.ToString()).SetCustomDimension(7, s));
                    //GameManager.Instance.googleAnalytics.DispatchHits();
                    Achievements[(int)chievo].Achieved = true;
                    if (Application.loadedLevelName == "Game")
                    {
                        GameManager.Instance._playerState.GetComponent<PlayerData>().Smile();
                    }
                    if (GameManager.Instance.GetState() == GameManager.GameState.Menu)
                    {
                        StatusBarHandler.Instance.TopIndicatorUpdates();
                    }
                    SaveChievos();
                    GameManager.Instance.EarnBugs((int)(Achievements[(int)chievo].Reward));
                    NotificationManager.Instance.AddNotification(true, false, true, false, NotificationManager.Instance.AchievementImege, false, Achievements[(int)chievo].Title, Achievements[(int)chievo].Description, Achievements[(int)chievo].Reward.ToString());
                }

                if (chievo != Achieve.You)
                {
                    int totalAchieved = 0;
                    foreach (Achievement achieve in Achievements)
                    {
                        if (achieve.Achieved)
                        {
                            totalAchieved++;
                        }
                    }

                    if (totalAchieved == ((GameManager.Instance.WebVersion) ? (Achievements.Count - 2) : (Achievements.Count - 1)))
                    {
                        Achievements[(int)Achieve.You].Achieved = true;
                        UpdateAchievement(Achieve.You);
                    }
                }
            }
        }
    }

    /// <summary> Saves the Achievements progress </summary>
    public void SaveChievos()
    {
        GameManager.saveData.AParams = AParams;
        GameManager.saveData.Achievements = new bool[Achievements.Count];
        for(int i = 0 ; i < GameManager.saveData.Achievements.Length ; ++i )
        {
            GameManager.saveData.Achievements[i] = Achievements[i].Achieved;
        }
        GameSaveSystem.Instance.SaveGame();
    }

    /// <summary> Returns the total achievements achieved </summary>
    public int TotalProgress()
    {
        int totalAchieved = 0;
        foreach (Achievement achieve in Achievements)
        {
            if (achieve.Achieved)
            {
                totalAchieved++;
            }
        }
        return totalAchieved;
    }

    /// <summary> Will be called at a time when an achievement may occur </summary>
    public float CheckProgress(Achieve chievo)
    {
        float progress = 0;
        int count = 0;
        int count2 = 0;
        float amount = 0;
        switch (chievo)
        {
            case Achieve.Friends:
                foreach (bool save in GameManager.saveData.FriendSaves)
                {
                    if (save)
                    {
                        count++;
                    }
                    progress = 0.25f * count;
                }
                break;
            case Achieve.Powerups:
                foreach (ShopItem item in PowerUpManager.Instance.ShopListings)
                {
                    if (item.PowerUpType == ShopItem.PowerType.Perma && item.Owned)
                    {
                        count++;
                    }
                }

                foreach (ShopItem item in PowerUpManager.Instance.ShopListings)
                {
                    if (item.PowerUpType == ShopItem.PowerType.Perma)
                    {
                        count2++;
                    }
                }

                progress = ((float)count / (float)count2);
                break;
            case Achieve.Level2:
                if (LevelManager.Instance.LevelsUnlocked > 1)
                {
                    progress = 1f;
                }
                break;
            case Achieve.Level3:
                if (LevelManager.Instance.LevelsUnlocked > 2)
                {
                    progress = 1f;
                }
                break;
            case Achieve.Level4:
                if (LevelManager.Instance.LevelsUnlocked > 3)
                {
                    progress = 1f;
                }
                break;
            case Achieve.Cosmetics:
                foreach (ShopItem item in PowerUpManager.Instance.ShopListings)
                {
                    if (item.PowerUpType == ShopItem.PowerType.Vanity && item.Owned)
                    {
                        count++;
                    }
                }

                foreach (ShopItem item in PowerUpManager.Instance.ShopListings)
                {
                    if (item.PowerUpType == ShopItem.PowerType.Vanity)
                    {
                        count2++;
                    }
                }

                progress = ((float)count / (float)count2);
                break;
            case Achieve.BugCollector1:
                progress = AParams.TotalBugs / 100;
                break;
            case Achieve.BugCollector2:
                progress = AParams.TotalBugs / 1000;
                break;
            case Achieve.BugCollector3:
                progress = AParams.TotalBugs / 10000;
                break;
            case Achieve.TotalHeight:
                progress = AParams.TotalHeight / 50000;
                break;
            case Achieve.TotalFall:
                progress = AParams.TotalFall / 10000;
                break;
            case Achieve.TotalBugs:
                progress = AParams.TotalBugs / 50000;
                break;
            case Achieve.Height1:
                foreach(LevelSaveData level in GameManager.saveData.LevelData)
                {
                    if(level.dHeight > amount)
                    {
                        amount = level.dHeight;
                    }

                    if (level.nHeight > amount)
                    {
                        amount = level.nHeight;
                    }
                }

                progress = amount / 500f;
                break;
            case Achieve.Height2:
                foreach (LevelSaveData level in GameManager.saveData.LevelData)
                {
                    if (level.dHeight > amount)
                    {
                        amount = level.dHeight;
                    }

                    if (level.nHeight > amount)
                    {
                        amount = level.nHeight;
                    }
                }

                progress = amount / 1500f;
                break;
            case Achieve.Height3:
                foreach (LevelSaveData level in GameManager.saveData.LevelData)
                {
                    if (level.dHeight > amount)
                    {
                        amount = level.dHeight;
                    }

                    if (level.nHeight > amount)
                    {
                        amount = level.nHeight;
                    }
                }

                progress = amount / 4500f;
                break;
            case Achieve.Height4:
                foreach (LevelSaveData level in GameManager.saveData.LevelData)
                {
                    if (level.dHeight > amount)
                    {
                        amount = level.dHeight;
                    }

                    if (level.nHeight > amount)
                    {
                        amount = level.nHeight;
                    }
                }

                progress = amount / 9000f;
                break;
            case Achieve.TheFall:
                progress = AParams.HighestFall / 5000f;
                break;
            case Achieve.OverPowered:
                if(AParams.PowersThisRun > AParams.BestPowersPerRun)
                {
                    AParams.BestPowersPerRun = AParams.PowersThisRun;
                }
                progress = AParams.BestPowersPerRun / 5f;
                break;
            case Achieve.JustSayNo:
                progress = AParams.HeightWithoutPower / 1000;
                break;
            case Achieve.GetBonked:
                if (Achievements[(int)chievo].Achieved)
                {
                    progress = 1f;
                }
                break;
            case Achieve.Repellant:
                if (Achievements[(int)chievo].Achieved)
                {
                    progress = 1f;
                }
                break;
            case Achieve.Infected1:
                if (Achievements[(int)chievo].Achieved)
                {
                    progress = 1f;
                }
                else
                {
                    progress = ( AParams.TotalInfection / 30f);
                    if (progress > 1f)
                    {
                        progress = 1f;
                    }
                }
                break;
            case Achieve.Infected2:
                if (Achievements[(int)chievo].Achieved)
                {
                    progress = 1f;
                }
                else
                {
                    progress = (AParams.TotalInfection / 120f);
                    if (progress > 1f)
                    {
                        progress = 1f;
                    }
                }
                break;
            case Achieve.Infected3:
                if (Achievements[(int)chievo].Achieved)
                {
                    progress = 1f;
                }
                else
                {
                    progress = (AParams.TotalInfection / 240f);
                    if (progress > 1f)
                    {
                        progress = 1f;
                    }
                }
                break;
            case Achieve.Challenge:
                foreach (float height in AParams.LevelCompletions)
                {
                    if (height >= 15000)
                    {
                        amount++;
                    }
                    progress = 0.25f * amount;
                }
                break;
            case Achieve.Dodger1:
                if (Achievements[(int)chievo].Achieved)
                {
                    progress = 1f;
                }
                break;
            case Achieve.Dodger2:
                if (Achievements[(int)chievo].Achieved)
                {
                    progress = 1f;
                }
                break;
            case Achieve.Dodger3:
                if (Achievements[(int)chievo].Achieved)
                {
                    progress = 1f;
                }
                break;
            case Achieve.CloseCall:
                if (Achievements[(int)chievo].Achieved)
                {
                    progress = 1f;
                }
                break;
            case Achieve.Share:
                if (Achievements[(int)chievo].Achieved)
                {
                    progress = 1f;
                }
                break;
            case Achieve.Magnolia:
                if (Achievements[(int)chievo].Achieved)
                {
                    progress = 1f;
                }
                break;
            case Achieve.Tookie:
                if (Achievements[(int)chievo].Achieved)
                {
                    progress = 1f;
                }
                break;
            case Achieve.Shep:
                if (Achievements[(int)chievo].Achieved)
                {
                    progress = 1f;
                }
                break;
            case Achieve.Ape:
                if (Achievements[(int)chievo].Achieved)
                {
                    progress = 1f;
                }
                break;
            case Achieve.You:
                foreach (Achievement achievement in Achievements)
                {
                    if (achievement.Achieved)
                    {
                        count++;
                    }
                }

                progress = ((float)count / (float)Achievements.Count);
                break;
        }
        return progress;
    }
}

/// <summary> Tracked parameters for achievements </summary>
[System.Serializable]
public class AchievementParams
{
    // Overall
    public float TotalHeight;
    public float TotalFall;
    public float TotalBugs;
    public float TotalInfection;
    public bool Shared = false;
    public float[] LevelCompletions = { 0, 0, 0, 0};

    // Pergame
    public int PowersThisRun;
    public int BestPowersPerRun;
    public float HighestFall;
    public float HeightWithoutPower;

    // Google Analytics
    public int noOfSessions;
    /// <summary> Did player do his first share? </summary>
    public bool firstShare = false;
    /// <summary> Did player make his first real money purchase? </summary>
    public bool firstPurchase = false;
    /// <summary> Did player make his first bug purchase after real money purchase? </summary>
    public bool realMoney = false;
    /// <summary> Total amount of play time </summary>
    public int totalMinutesPlayed;

    /// <summary> Resets the params that are needed only per game </summary>
    public void ResetPergame()
    {
        PowersThisRun = 0;
    }

}