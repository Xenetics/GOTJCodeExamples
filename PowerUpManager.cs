using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// Date Created: 10/04/15 ->
/// Manages the powerup timers
/// </summary>
public class PowerUpManager : MonoBehaviour
{
    private static PowerUpManager instance = null;
    public static PowerUpManager Instance { get { return instance; } }

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

    /// <summary> List of Shop items for editing in the inspector </summary>
    public List<ShopItem> ShopListings;
    /// <summary> Wheather Items are on or not </summary>
    [HideInInspector]
    public List<bool> ShopItemSettings;
    /// <summary> List of All items to appear in the shop </summary>
    public List<BugShopItem> BugShopListings = new List<BugShopItem>();
    /// <summary> List of the Powerups that the player has unlocked permanently </summary>
    public List<string> PermPowerUps = new List<string>();
	/// <summary> List of toggles for the Perma powers </summary>
	public List<bool> PermPowerSettings = new List<bool>();
    /// <summary> List of the Powerups that the player has unlocked for next run </summary>
    public List<string> TempPowerUps = new List<string>();
    /// <summary> List of counts for the temp powerups </summary>
    public List<int> TempPowerCounts = new List<int>();
    /// <summary> List of counts for the temp powerups </summary>
    public List<Friend_Platform_Property.Buddy> ActiveTemps = new List<Friend_Platform_Property.Buddy>();
    /// <summary> List of the Vanity items a player has unlocked </summary>
    public List<string> VanityItems = new List<string>();
    /// <summary> List of toggles for the vanity items </summary>
    public List<bool> VanitySettings = new List<bool>();
    /// <summary> The items that george will wear </summary>
    public Inventory Skin;
    /// <summary> how many powers a player can have active </summary>
    public int TotalPowers = 3;
    /// <summary> how many powers are active </summary>
    public int CurrentUsedPowers = 0;
    /// <summary> how many minutes between freebies </summary>
    private int m_MinutesSinceLastFreebie = 5;

    /// <summary> Adds a Perma power to the list of activated Perma powers </summary>
    /// <param name="power"> PowerUp String Refference </param>
    public void AddPermPower(string power)
    {
        if (!PermPowerUps.Contains(power))
        {
            PermPowerUps.Add(power);
            CurrentUsedPowers++;
            AddToggle(ShopItem.PowerType.Perma);
        }
		SaveArray(ShopItem.PowerType.Perma);
    }

    /// <summary> Adds a Temp power to the list of activated temp powers </summary>
    /// <param name="power"> PowerUp String Refference </param>
    public void AddTempPower(string power)
    {
        if (!TempPowerUps.Contains(power))
        {
            TempPowerUps.Add(power);
        }
        TempPowerCounts[TempPowerUps.IndexOf(power)]++;
		SaveArray(ShopItem.PowerType.Temp);
    }

    /// <summary> Adds temp powers in the amount given </summary>
    /// <param name="t_amount"> How many to add </param>
    public void AddTempPower(int t_amount)
    {        
        TempPowerCounts[0] += t_amount;
        SaveArray(ShopItem.PowerType.Temp);
    }

    /// <summary>  Removes a Perma power to the list of activated Perma powers  </summary>
    /// <param name="power"> PowerUp String Refference </param>
    public void AddVanity(string power)
	{
		VanityItems.Add(power);
		AddToggle(ShopItem.PowerType.Vanity);
		SaveArray(ShopItem.PowerType.Vanity);
        AddToInventory(power);
        SaveSkin();
    }

    /// <summary> Removes a Perma power to the list of activated Perma powers </summary>
    /// <param name="power"> PowerUp String Refference </param>
    public void RemovePermaPower(string power)
    {
        RemoveToggle(ShopItem.PowerType.Perma, power);
        PermPowerUps.Remove(power);
        CurrentUsedPowers--;
        if(CurrentUsedPowers < 0)
        {
            CurrentUsedPowers = 0;
        }
        SaveArray(ShopItem.PowerType.Perma);
    }

    /// <summary> Removes reduces the total number of a hower there are </summary>
    /// <param name="power"> PowerUp String Refference </param>
    public void RemoveTempPower(string power)
    {
        TempPowerCounts[TempPowerUps.IndexOf(power)]--;
        if(TempPowerCounts[TempPowerUps.IndexOf(power)] < 0)
        {
            TempPowerCounts[TempPowerUps.IndexOf(power)] = 0;
        }
        ActiveTemps.Remove((Friend_Platform_Property.Buddy)Enum.Parse(typeof(Friend_Platform_Property.Buddy), power));
		SaveArray(ShopItem.PowerType.Temp);
    }

    /// <summary> Removes temp powers in the amount given </summary>
    /// <param name="t_amount"> How many to add </param>
    public void RemoveTempPower(int t_amount)
    {
        TempPowerCounts[0] -= t_amount;
        SaveArray(ShopItem.PowerType.Temp);
    }

    /// <summary> Removes a Vanity to the list of activated Vanities </summary>
    /// <param name="power"> Vanity String Refference </param>
    public void RemoveVanity(string vanity)
    {
		RemoveToggle(ShopItem.PowerType.Vanity, vanity);
        VanityItems.Remove(vanity);
        SaveArray(ShopItem.PowerType.Vanity);
    }

	/// <summary> Adds a new toggle to a list of toggles of choice </summary>
	/// <param name="type"> Type of power were adding a toggle for </param>
	private void AddToggle(ShopItem.PowerType type)
	{
		switch(type)
		{
			case ShopItem.PowerType.Perma:
				PermPowerSettings.Add(true);
				break;
			case ShopItem.PowerType.Vanity:
				VanitySettings.Add(true);
				break;
		}
    }

	/// <summary> Removes the proper toggle based on type of togger and name </summary>
	/// <param name="type"> Type of power were removing a toggle for </param>
	/// <param name="name"> Name of the power were removing the toggle for </param>
	private void RemoveToggle(ShopItem.PowerType type, string name)
	{
        int i = 0;
        switch (type)
		{
		case ShopItem.PowerType.Perma:
			foreach(string item in PermPowerUps)
			{
				if(name == item)
				{
					PermPowerSettings.RemoveAt(i);
					break;
				}
				i++;
			}
			break;
		case ShopItem.PowerType.Vanity:
			foreach(string item in VanityItems)
			{
				if(name == item)
				{
					VanitySettings.RemoveAt(i);
					break;
				}
				i++;
			}
			break;
		}
    }

	/// <summary> Saves just the arrays for powerup manader </summary>
	/// <param name="type"> Type of arrays were saving </param>
	private void SaveArray(ShopItem.PowerType type)
	{
		switch(type)
		{
			case ShopItem.PowerType.Perma:
				GameManager.saveData.Perma = new string[PermPowerUps.Count];
				GameManager.saveData.PermaToggles = new bool[PermPowerUps.Count];
				for (int i = 0; i < GameManager.saveData.Perma.Length; ++i)
				{
					GameManager.saveData.Perma[i] = PermPowerUps[i];
					GameManager.saveData.PermaToggles[i] = PermPowerSettings[i];
				}
			break;
			case ShopItem.PowerType.Temp:
				GameManager.saveData.Temp = new string[TempPowerUps.Count];
				for (int i = 0; i < GameManager.saveData.Temp.Length; ++i)
				{
					GameManager.saveData.Temp[i] = TempPowerUps[i];
				}

                GameManager.saveData.TempCount = new int[TempPowerCounts.Count];
                for (int i = 0; i < GameManager.saveData.TempCount.Length; ++i)
                {
                    GameManager.saveData.TempCount[i] = TempPowerCounts[i];
                }
                break;
			case ShopItem.PowerType.Vanity:
				GameManager.saveData.Vanity = new string[VanityItems.Count];
				GameManager.saveData.VanityToggles = new bool[VanityItems.Count];
				for (int i = 0; i < GameManager.saveData.Vanity.Length; ++i)
				{
					GameManager.saveData.Vanity[i] = VanityItems[i];
					GameManager.saveData.VanityToggles[i] = VanitySettings[i];
				}
			break;
		}
        GameManager.saveData.PowersOn = CurrentUsedPowers;
		GameSaveSystem.Instance.SaveGame();
	}

    /// <summary> Adds an iten to inventory </summary>
    /// <param name="item"> Item string </param>
    public void AddToInventory(string item)
    {
        if (Application.loadedLevelName == "Menu")
        {
            if (ShopMenuHandler.Instance.SelectedItem.Slot == ShopItem.ItemSlot.Head)
            {
                Skin.Head = item;
            }
            else if (ShopMenuHandler.Instance.SelectedItem.Slot == ShopItem.ItemSlot.Body)
            {
                Skin.Body = item;
            }
        }
        else if (Application.loadedLevelName == "Game")
        {
            if (PreGameShopHandler.Instance.SelectedItem.Slot == ShopItem.ItemSlot.Head)
            {
                Skin.Head = item;
            }
            else if (PreGameShopHandler.Instance.SelectedItem.Slot == ShopItem.ItemSlot.Body)
            {
                Skin.Body = item;
            }
        }
    }

    /// <summary> Clears active temps </summary>
    public void ClearActiveTemps()
    {
        ActiveTemps.Clear();
    }

    /// <summary> Saves the current skinning of George </summary>
    public void SaveSkin()
    {
        GameManager.saveData.Skin = Skin;
        GameSaveSystem.Instance.SaveGame();
    }

    /// <summary> returns true if the freebie can be used </summary>
    public bool FreebieAvailable()
    {
        bool available = false;
        if(DateTime.Now > (GameManager.saveData.LastFreebie.Add(new TimeSpan(0, m_MinutesSinceLastFreebie, 0))))
        {
            available = true;
        }
        return available;
    }

    /// <summary>  returns time since last freebie in string form </summary>
    public string TimeSinceLastFreebie()
    {
        TimeSpan timeSince = new TimeSpan(0, m_MinutesSinceLastFreebie, 0) - DateTime.Now.Subtract(GameManager.saveData.LastFreebie);
        string min = (timeSince.Minutes > 0) ? (timeSince.Minutes.ToString() + ":") : ("00:");
        string second = (timeSince.Seconds < 10) ? ("0" + timeSince.Seconds.ToString()) : (timeSince.Seconds.ToString());
        return min + second;
    }
}

[Serializable]
public class Inventory
{
    public string Head;
    public string Body;
}

    