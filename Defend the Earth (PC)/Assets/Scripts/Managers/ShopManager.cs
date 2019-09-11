﻿using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct Spaceship
{
    [Tooltip("Spaceship data key.")] public string key;
    [Tooltip("Spaceship price.")] public int price;
    [Tooltip("Spaceship info panel color.")] public Color32 panelColor;
    [Tooltip("Spaceship preview object.")] public GameObject preview;
    [Tooltip("Spaceship name.")] public string name;
    [Tooltip("Spaceship health in info panel.")] public long health;
    [Tooltip("Spaceship damage in info panel.")] public string damage;
    [Tooltip("Spaceship fire rate in info panel.")] public float fireRate;
}

[System.Serializable]
public struct Upgrade
{
    [Tooltip("Upgrade name.")] public string name;
    [Tooltip("Maximum upgrade value.")] public int maxValue;
    [Tooltip("Upgrade price.")] public int price;
    [Tooltip("Upgrade price multiplier.")] public float priceMultiplier;
    [Tooltip("Amount added to the upgrade multiplier.")] public float multiplierIncrease;
    [Tooltip("Amount added to the upgrade percentage visuals.")] public int percentageIncrease;
}

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    [SerializeField] private Spaceship[] spaceships = new Spaceship[0];
    [SerializeField] private Upgrade[] upgrades = new Upgrade[0];

    [Header("UI")]
    [SerializeField] private Text spaceshipName = null;
    [SerializeField] private Text spaceshipPrice = null;
    [SerializeField] private Text spaceshipBuyText = null;
    [SerializeField] private Text spaceshipInfo = null;
    [SerializeField] private Image spaceshipPanel = null;
    [SerializeField] private GameObject leftButton = null;
    [SerializeField] private GameObject rightButton = null;
    [SerializeField] private Text controllerXText = null;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip buttonClick = null;
    [SerializeField] private AudioClip cannotAfford = null;

    [Header("Miscellanous")]
    public int page = 1;

    private AudioSource audioSource;
    private bool pressedBumper = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else if (instance != this)
        {
            Destroy(gameObject);
        }
        audioSource = GetComponent<AudioSource>();
        if (PlayerPrefs.GetString("Spaceship") == "SpaceFighter")
        {
            page = 1;
        } else if (PlayerPrefs.GetString("Spaceship") == "AlienMower")
        {
            page = 2;
        } else if (PlayerPrefs.GetString("Spaceship") == "BlazingRocket")
        {
            page = 3;
        } else if (PlayerPrefs.GetString("Spaceship") == "QuadShooter")
        {
            page = 4;
        } else if (PlayerPrefs.GetString("Spaceship") == "PointVoidBreaker")
        {
            page = 5;
        } else if (PlayerPrefs.GetString("Spaceship") == "Annihilator")
        {
            page = 6;
        } else
        {
            page = 1;
        }
        for (int i = 0; i < upgrades.Length; i++)
        {
            if (!PlayerPrefs.HasKey(upgrades[i].name + "Multiplier")) PlayerPrefs.SetFloat(upgrades[i].name + "Multiplier", 1);
            if (!PlayerPrefs.HasKey(upgrades[i].name + "Price")) PlayerPrefs.SetInt(upgrades[i].name + "Price", upgrades[i].price);
        }
        PlayerPrefs.Save();
    }

    void Update()
    {
        Spaceship spaceship = spaceships[page - 1];
        if (Input.GetKeyDown(KeyCode.JoystickButton4)) // LB/L1 (Xbox/PS Controller)
        {
            pressedBumper = true;
            changeSpaceshipsPage(false);
            pressedBumper = false;
        } else if (Input.GetKeyDown(KeyCode.JoystickButton5)) // RB/R1 (Xbox/PS Controller)
        {
            pressedBumper = true;
            changeSpaceshipsPage(true);
            pressedBumper = false;
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton2)) // X/Square (Xbox/PS Controller)
        {
            pressedBumper = true;
            buySpaceship();
            equipSpaceship();
            pressedBumper = false;
        }
        if (page < 1)
        {
            page = 1;
        } else if (page > spaceships.Length)
        {
            page = spaceships.Length;
        }
        spaceshipName.text = spaceship.name;
        if (PlayerPrefs.GetInt("Has" + spaceship.key) >= 1)
        {
            spaceshipPrice.text = "";
            if (PlayerPrefs.GetString("Spaceship") == spaceship.key)
            {
                spaceshipBuyText.text = "Using";
                spaceshipBuyText.alignment = TextAnchor.MiddleCenter;
            } else
            {
                spaceshipBuyText.text = "Use";
                spaceshipBuyText.alignment = TextAnchor.MiddleCenter;
            }
            if (controllerXText) controllerXText.text = "Use";
        } else
        {
            if (spaceship.price > 0)
            {
                spaceshipPrice.text = "$" + spaceship.price;
            } else
            {
                spaceshipPrice.text = "Free";
            }
            spaceshipBuyText.text = "Buy";
            spaceshipBuyText.alignment = TextAnchor.MiddleLeft;
            if (controllerXText) controllerXText.text = "Buy";
        }
        spaceshipInfo.text = "Health: " + spaceship.health + "\nDamage: " + spaceship.damage + "\nFire Rate: " + spaceship.fireRate;
        spaceshipPanel.color = new Color32(spaceship.panelColor.r, spaceship.panelColor.g, spaceship.panelColor.b, 150);
        if (page <= 1)
        {
            leftButton.SetActive(false);
            rightButton.SetActive(true);
        } else if (page >= spaceships.Length)
        {
            leftButton.SetActive(true);
            rightButton.SetActive(false);
        } else
        {
            leftButton.SetActive(true);
            rightButton.SetActive(true);
        }
        foreach (GameObject shipPreview in GameObject.FindGameObjectsWithTag("Player")) shipPreview.SetActive(false);
        if (spaceship.preview) spaceship.preview.SetActive(true);
    }

    //Main Functions
    public void equipSpaceship()
    {
        if (PlayerPrefs.GetInt("Has" + spaceships[page - 1].key) >= 1)
        {
            if (!pressedBumper && audioSource)
            {
                if (buttonClick)
                {
                    audioSource.PlayOneShot(buttonClick, getVolumeData(true));
                } else
                {
                    audioSource.volume = getVolumeData(true);
                    audioSource.Play();
                }
            }
            PlayerPrefs.SetString("Spaceship", spaceships[page - 1].key);
            PlayerPrefs.Save();
        }
    }

    public void changeSpaceshipsPage(bool next)
    {
        if (!pressedBumper && audioSource)
        {
            if (buttonClick)
            {
                audioSource.PlayOneShot(buttonClick, getVolumeData(true));
            } else
            {
                audioSource.volume = getVolumeData(true);
                audioSource.Play();
            }
        }
        if (next && page < spaceships.Length)
        {
            ++page;
        } else if (!next && page > 1)
        {
            --page;
        }
    }

    float getVolumeData(bool isSound)
    {
        float volume = 1;
        if (isSound)
        {
            if (PlayerPrefs.HasKey("SoundVolume")) volume = PlayerPrefs.GetFloat("SoundVolume");
        } else
        {
            if (PlayerPrefs.HasKey("MusicVolume")) volume = PlayerPrefs.GetFloat("MusicVolume");
        }
        return volume;
    }

    //Buy Functions
    public void buySpaceship()
    {
        if (PlayerPrefs.GetInt("Has" + spaceships[page - 1].key) <= 0)
        {
            if (spaceships[page - 1].price > 0)
            {
                long money = long.Parse(PlayerPrefs.GetString("Money"));
                if (money >= spaceships[page - 1].price)
                {
                    money -= spaceships[page - 1].price;
                    PlayerPrefs.SetString("Money", money.ToString());
                    PlayerPrefs.SetInt("Has" + spaceships[page - 1].key, 1);
                    PlayerPrefs.Save();
                } else
                {
                    if (audioSource)
                    {
                        if (cannotAfford)
                        {
                            audioSource.PlayOneShot(cannotAfford, getVolumeData(true));
                        } else
                        {
                            audioSource.volume = getVolumeData(true);
                            audioSource.Play();
                        }
                    }
                }
            } else
            {
                PlayerPrefs.SetInt("Has" + spaceships[page - 1].key, 1);
            }
            PlayerPrefs.Save();
        }
    }

    public void upgrade(int index)
    {
        int maxValue = upgrades[index].maxValue;
        int c = (int)(PlayerPrefs.GetInt(upgrades[index].name + "Price") * upgrades[index].priceMultiplier);
        float m = PlayerPrefs.GetFloat(upgrades[index].name + "Multiplier") + upgrades[index].multiplierIncrease;
        int p = PlayerPrefs.GetInt(upgrades[index].name + "Percentage") + upgrades[index].percentageIncrease;
        if (PlayerPrefs.GetInt(upgrades[index].name + "Percentage") < maxValue)
        {
            if (upgrades[index].price > 0)
            {
                long money = long.Parse(PlayerPrefs.GetString("Money"));
                if (money >= PlayerPrefs.GetInt(upgrades[index].name + "Price"))
                {
                    if (audioSource)
                    {
                        if (buttonClick)
                        {
                            audioSource.PlayOneShot(buttonClick, getVolumeData(true));
                        } else
                        {
                            audioSource.volume = getVolumeData(true);
                            audioSource.Play();
                        }
                    }
                    money -= PlayerPrefs.GetInt(upgrades[index].name + "Price");
                    PlayerPrefs.SetString("Money", money.ToString());
                    PlayerPrefs.SetInt(upgrades[index].name + "Price", c);
                    PlayerPrefs.SetFloat(upgrades[index].name + "Multiplier", m);
                    PlayerPrefs.SetInt(upgrades[index].name + "Percentage", p);
                } else
                {
                    if (audioSource)
                    {
                        if (cannotAfford)
                        {
                            audioSource.PlayOneShot(cannotAfford, getVolumeData(true));
                        } else
                        {
                            audioSource.volume = getVolumeData(true);
                            audioSource.Play();
                        }
                    }
                }
            } else
            {
                PlayerPrefs.SetInt(upgrades[index].name + "Price", c);
                PlayerPrefs.SetFloat(upgrades[index].name + "Multiplier", m);
                PlayerPrefs.SetInt(upgrades[index].name + "Percentage", p);
            }
            PlayerPrefs.Save();
        }
    }
}