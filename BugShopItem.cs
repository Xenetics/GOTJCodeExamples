using UnityEngine;
/// <summary>
/// Date Created: 01/26/16 ->
/// The structure of a Shop Item
/// </summary>
[System.Serializable]
public class BugShopItem
{
    /// <summary> The items name </summary>
    public string Title;
    /// <summary> What the item is or is used for </summary>
    public string Description;
    /// <summary> The image that represents the item </summary>
    public Sprite Art;
    /// <summary> Cost of the item in real life dollars </summary>
    public float Cost;
    /// <summary> Amount of bugs rewarded </summary>
    public int BugAmount;
    /// <summary> if this is a restore item </summary>
    public bool Restore;
    /// <summary> If is active in store </summary>
    //public bool Active;
}
