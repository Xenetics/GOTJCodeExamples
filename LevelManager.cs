using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class LevelManager : MonoBehaviour
{
    private static LevelManager instance = null;
    public static LevelManager Instance { get { return instance; } }

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

    public enum LevelArt { Jungle = 0, Waterfall = 1, Thicket = 2, Mystery = 3, COUNT }
    /// <summary> Costs of the levels. Same order as enum </summary>
    public List<int> LevelCosts = new List<int>();
    public LevelArt currentArt;
    public int LevelsUnlocked = 1;
        
    public List<SpriteContainer> sprites = new List<SpriteContainer>();
}
