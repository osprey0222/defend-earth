﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class DataInitializer : MonoBehaviour
{
    [SerializeField] private int level = 1;
    [SerializeField] private int maxLevels = 20;
    [SerializeField] private bool setLevel = true;

    void Awake()
    {
        if (!PlayerPrefs.HasKey("Spaceship")) PlayerPrefs.SetString("Spaceship", "SpaceFighter");

        //Set up owned spaceship data
        if (!PlayerPrefs.HasKey("HasSpaceFighter")) PlayerPrefs.SetInt("HasSpaceFighter", 1);
        if (!PlayerPrefs.HasKey("HasAlienMower")) PlayerPrefs.SetInt("HasAlienMower", 0);
        if (!PlayerPrefs.HasKey("HasBlazingRocket")) PlayerPrefs.SetInt("HasBlazingRocket", 0);
        if (!PlayerPrefs.HasKey("HasQuadShooter")) PlayerPrefs.SetInt("HasQuadShooter", 0);
        if (!PlayerPrefs.HasKey("HasPointVoidBreaker")) PlayerPrefs.SetInt("HasPointVoidBreaker", 0);
        if (!PlayerPrefs.HasKey("HasAnnihilator")) PlayerPrefs.SetInt("HasAnnihilator", 0);

        //Set up level data
        string sceneName = SceneManager.GetActiveScene().name;
        if (setLevel)
        {
            if (!PlayerPrefs.HasKey("Level"))
            {
                if (sceneName.ToLower().Contains("level"))
                {
                    PlayerPrefs.SetInt("IngameLevel", level);
                    if (!PlayerPrefs.HasKey("Restarted")) PlayerPrefs.SetInt("Level", level);
                } else
                {
                    PlayerPrefs.SetInt("Level", 1);
                    PlayerPrefs.SetInt("IngameLevel", 1);
                }
            } else
            {
                if (sceneName.ToLower().Contains("level"))
                {
                    PlayerPrefs.SetInt("IngameLevel", level);
                    if (!PlayerPrefs.HasKey("Restarted")) PlayerPrefs.SetInt("Level", level);
                }
            }
        }
        if (maxLevels > 0)
        {
            PlayerPrefs.SetInt("MaxLevels", maxLevels);
        } else
        {
            PlayerPrefs.SetInt("MaxLevels", 1);
        }

        /*
        //Set up player upgrade data
        if (!PlayerPrefs.HasKey("DamageMultiplier")) PlayerPrefs.SetFloat("DamageMultiplier", 1);
        if (!PlayerPrefs.HasKey("SpeedMultiplier")) PlayerPrefs.SetFloat("SpeedMultiplier", 1);
        if (!PlayerPrefs.HasKey("HealthMultiplier")) PlayerPrefs.SetFloat("HealthMultiplier", 1);
        if (!PlayerPrefs.HasKey("MoneyMultiplier")) PlayerPrefs.SetFloat("MoneyMultiplier", 1);

        //Set up player upgrade price data
        if (!PlayerPrefs.HasKey("DamagePrice")) PlayerPrefs.SetInt("DamagePrice", 8);
        if (!PlayerPrefs.HasKey("SpeedPrice")) PlayerPrefs.SetInt("SpeedPrice", 5);
        if (!PlayerPrefs.HasKey("HealthPrice")) PlayerPrefs.SetInt("HealthPrice", 7);
        if (!PlayerPrefs.HasKey("MoneyPrice")) PlayerPrefs.SetInt("MoneyPrice", 4);
        */

        //Set up money data
        if (!PlayerPrefs.HasKey("Money")) PlayerPrefs.SetString("Money", "0");

        PlayerPrefs.Save();
        Destroy(gameObject);
    }
}