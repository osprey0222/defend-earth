﻿using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    [Header("Game Settings")]
    [SerializeField] private long maxWaves = 1;
    [SerializeField] private Vector2 enemySpawnTime = new Vector2(3, 4);
    [SerializeField] private Vector2 asteroidSpawnTime = new Vector2(7, 8);
    [SerializeField] private int maxAliensReached = 10;
    [SerializeField] private GameObject[] enemies = new GameObject[0];
    [SerializeField] private GameObject[] asteroids = new GameObject[0];
    [Tooltip("Leave blank to not have a boss in the last wave.")] [SerializeField] private GameObject boss = null;

    [Header("UI")]
    [SerializeField] private Canvas gameHUD = null;
    [SerializeField] private Canvas gamePausedMenu = null;
    [SerializeField] private Canvas gameOverMenu = null;
    [SerializeField] private Canvas levelCompletedMenu = null;
    [SerializeField] private Canvas settingsMenu = null;
    [SerializeField] private Canvas graphicsQualityMenu = null;
    [SerializeField] private Canvas soundMenu = null;
    [SerializeField] private Canvas quitGameMenu = null;
    [SerializeField] private Canvas restartPrompt = null;
    [SerializeField] private Text levelCount = null;
    [SerializeField] private Text waveCount = null;
    [SerializeField] private Text moneyCount = null;
    [SerializeField] private Text bossName = null;
    [SerializeField] private Slider bossHealthBar = null;
    [SerializeField] private Text bossHealthText = null;
    [SerializeField] private Slider soundSlider = null;
    [SerializeField] private Slider musicSlider = null;
    [SerializeField] private Text deathMessage = null;
    [SerializeField] private RectTransform controllerShootIcon = null;
    [SerializeField] private GameObject loadingText = null;
    [SerializeField] private Slider loadingSlider = null;
    [SerializeField] private Text loadingPercentage = null;

    [Header("Miscellanous")]
    public int enemiesLeft = 8;
    public int aliensReached = 0;
    public GameObject currentBoss;
    public string deathMessageToShow = "";
    public bool gameOver = false;
    public bool won = false;
    public bool paused = false;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip buttonClick = null;

    [Header("Setup")]
    [SerializeField] private GameObject[] playerShips = new GameObject[0];

    private AudioSource audioSource;
    private long wave = 1;
    private int enemyAmount = 0; //Stores the amount of enemies
    private bool reachedNextWave = false; //Checks if the player just reached the next wave, preventing wave skyrocketing
    private bool canWin = false; //Checks if the player is on the last wave, thus allowing the player to win
    private long bossMaxHealth = 0; //If the value is above 0, the boss health bar's max value is not updated
    private int clickSource = 1; //1 is game paused menu, 2 is game over menu, 3 is level completed menu
    private bool loading = false;

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
        if (audioSource) audioSource.ignoreListenerPause = true;
        gameOver = false;
        won = false;
        paused = false;
        currentBoss = null;
        bossMaxHealth = 0;
        if (enemiesLeft <= 0) enemiesLeft = 5; //Checks if the amount of enemies left is 0 or less
        enemyAmount = enemiesLeft;
        aliensReached = 0;
        if (maxAliensReached < 5) maxAliensReached = 5; //Checks if maximum aliens reached is below 5
        deathMessageToShow = "";
        Time.timeScale = 1;
        AudioListener.pause = false;

        //Destroy all player, enemy and projectile objects in the scene
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player) Destroy(player);
        }
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (enemy) Destroy(enemy);
        }
        foreach (GameObject projectile in GameObject.FindGameObjectsWithTag("Projectile"))
        {
            if (projectile) Destroy(projectile);
        }

        if (!PlayerPrefs.HasKey("Difficulty")) //Sets the difficulty to Normal if no difficulty key is found
        {
            PlayerPrefs.SetInt("Difficulty", 2);
            PlayerPrefs.Save();
        }
        if (PlayerPrefs.GetInt("Difficulty") <= 1) //Easy
        {
            maxAliensReached += 2;
        } else if (PlayerPrefs.GetInt("Difficulty") == 3) //Hard
        {
            asteroidSpawnTime *= 0.95f;
            maxAliensReached -= 1;
        } else if (PlayerPrefs.GetInt("Difficulty") >= 4) //Nightmare
        {
            asteroidSpawnTime *= 0.85f;
            enemyAmount += 1;
            maxAliensReached -= 2;
        }
        if (!PlayerPrefs.HasKey("SoundVolume"))
        {
            PlayerPrefs.SetFloat("SoundVolume", 1);
            PlayerPrefs.Save();
        } else
        {
            soundSlider.value = PlayerPrefs.GetFloat("SoundVolume");
        }
        if (!PlayerPrefs.HasKey("MusicVolume"))
        {
            PlayerPrefs.SetFloat("MusicVolume", 1);
            PlayerPrefs.Save();
        } else
        {
            if (Camera.main.GetComponent<AudioSource>()) Camera.main.GetComponent<AudioSource>().volume = getVolumeData(false);
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        }
        if (Camera.main.GetComponent<AudioSource>())
        {
            Camera.main.GetComponent<AudioSource>().volume = getVolumeData(false);
            Camera.main.GetComponent<AudioSource>().Play();
        }
        if (PlayerPrefs.GetString("Spaceship") == "SpaceFighter")
        {
            Instantiate(playerShips[0], new Vector3(0, -7, 0), Quaternion.Euler(-90, 0, 0));
        } else if (PlayerPrefs.GetString("Spaceship") == "AlienMower")
        {
            Instantiate(playerShips[1], new Vector3(0, -6.5f, 0), Quaternion.Euler(-90, 0, 0));
        } else if (PlayerPrefs.GetString("Spaceship") == "BlazingRocket")
        {
            Instantiate(playerShips[2], new Vector3(0, -6.75f, 0), Quaternion.Euler(-90, 0, 0));
        } else if (PlayerPrefs.GetString("Spaceship") == "QuadShooter")
        {
            Instantiate(playerShips[3], new Vector3(0, -6.75f, 0), Quaternion.Euler(-90, 0, 0));
        } else if (PlayerPrefs.GetString("Spaceship") == "PointVoidBreaker")
        {
            Instantiate(playerShips[4], new Vector3(0, -6.5f, 0), Quaternion.Euler(-90, 0, 0));
        } else if (PlayerPrefs.GetString("Spaceship") == "Annihilator")
        {
            Instantiate(playerShips[5], new Vector3(0, -6.5f, 0), Quaternion.Euler(-90, 0, 0));
        } else //Instantiates Space Fighter if the currently used spaceship is invalid
        {
            PlayerPrefs.SetString("Spaceship", "SpaceFighter");
            PlayerPrefs.Save();
            Instantiate(playerShips[0], new Vector3(0, -7, 0), Quaternion.Euler(-90, 0, 0));
        }
        gameHUD.enabled = true;
        gamePausedMenu.enabled = false;
        gameOverMenu.enabled = false;
        levelCompletedMenu.enabled = false;
        settingsMenu.enabled = false;
        graphicsQualityMenu.enabled = false;
        soundMenu.enabled = false;
        quitGameMenu.enabled = false;
        restartPrompt.enabled = false;
        StartCoroutine(spawnWaves());
        StartCoroutine(spawnAsteroids());
    }

    void Update()
    {
        if (Camera.main.GetComponent<AudioSource>()) Camera.main.GetComponent<AudioSource>().volume = getVolumeData(false);
        if (Input.GetKeyDown(KeyCode.F11)) Screen.fullScreen = !Screen.fullScreen;
        if (Input.GetKeyDown(KeyCode.Escape)) pause();
        if (paused && Input.GetKeyDown(KeyCode.Escape) || paused && Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            if (settingsMenu.enabled)
            {
                settingsMenu.enabled = false;
                gamePausedMenu.enabled = true;
            } else if (graphicsQualityMenu.enabled)
            {
                graphicsQualityMenu.enabled = false;
                settingsMenu.enabled = true;
            } else if (soundMenu.enabled)
            {
                soundMenu.enabled = false;
                settingsMenu.enabled = true;
            } else if (quitGameMenu.enabled)
            {
                quitGameMenu.enabled = false;
                if (clickSource <= 1)
                {
                    gamePausedMenu.enabled = true;
                } else if (clickSource == 2)
                {
                    gameOverMenu.enabled = true;
                } else if (clickSource >= 3)
                {
                    levelCompletedMenu.enabled = true;
                }
            } else if (restartPrompt.enabled)
            {
                restartPrompt.enabled = false;
                if (clickSource <= 1)
                {
                    gamePausedMenu.enabled = true;
                } else if (clickSource == 2)
                {
                    gameOverMenu.enabled = true;
                } else if (clickSource >= 3)
                {
                    levelCompletedMenu.enabled = true;
                }
            }
        }
        if (restartPrompt.enabled)
        {
            if (Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                restart();
            } else if (Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                openCanvasFromClickSource(restartPrompt);
            }
        }
        if (!gameOver && !won)
        {
            if (wave < maxWaves && enemiesLeft <= 0 && !canWin)
            {
                if (!reachedNextWave)
                {
                    reachedNextWave = true;
                    if (wave < maxWaves + 1) ++wave;
                }
            } else if (wave >= maxWaves && enemiesLeft <= 0 && canWin)
            {
                won = true;
                clickSource = 3;
                if (PlayerPrefs.GetInt("Level") < PlayerPrefs.GetInt("MaxLevels"))
                {
                    if (!loading && !quitGameMenu.enabled) levelCompletedMenu.enabled = true;
                } else
                {
                    PlayerPrefs.SetInt("Level", 1);
                    PlayerPrefs.Save();
                    StartCoroutine(loadScene("Ending"));
                }
                StopCoroutine(spawnWaves());
                StopCoroutine(spawnAsteroids());
                if (Camera.main.GetComponent<AudioSource>()) Camera.main.GetComponent<AudioSource>().Stop();
            }
        }
        if (!gameOver && !won && aliensReached >= maxAliensReached)
        {
            gameOver = true;
            deathMessageToShow = "You failed to protect the Earth!";
        }
        if (gameOver)
        {
            clickSource = 2;
            if (!loading && !quitGameMenu.enabled) gameOverMenu.enabled = true;
            if (Camera.main.GetComponent<AudioSource>()) Camera.main.GetComponent<AudioSource>().Stop();
            StopCoroutine(spawnWaves());
            StopCoroutine(spawnAsteroids());
        }

        //Updates volume data to match the slider values
        PlayerPrefs.SetFloat("SoundVolume", soundSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        PlayerPrefs.Save();

        if (PlayerPrefs.GetInt("Level") > 0)
        {
            levelCount.text = PlayerPrefs.GetInt("Level").ToString();
        } else
        {
            levelCount.text = "1";
        }
        waveCount.text = wave + "/" + maxWaves;
        if (PlayerPrefs.GetString("Money") != "")
        {
            moneyCount.text = "$" + PlayerPrefs.GetString("Money");
        } else
        {
            moneyCount.text = "$0";
        }
        if (!currentBoss)
        {
            currentBoss = null;
            if (controllerShootIcon) controllerShootIcon.anchoredPosition = new Vector2(-20, 20);
            bossMaxHealth = 0;
            bossName.gameObject.SetActive(false);
            bossName.text = "Boss Name";
            bossHealthBar.value = 100;
            bossHealthBar.maxValue = 100;
            bossHealthText.text = "100 / 100";
        } else
        {
            if (bossMaxHealth <= 0) bossMaxHealth = currentBoss.GetComponent<EnemyHealth>().health;
            if (controllerShootIcon) controllerShootIcon.anchoredPosition = new Vector2(-20, 70);
            bossName.gameObject.SetActive(true);
            bossName.text = currentBoss.name;
            bossHealthBar.value = currentBoss.GetComponent<EnemyHealth>().health;
            bossHealthBar.maxValue = bossMaxHealth;
            bossHealthText.text = bossHealthBar.value + " / " + bossHealthBar.maxValue;
        }
        deathMessage.text = deathMessageToShow;
        if (!loading)
        {
            GameObject[] backgrounds = GameObject.FindGameObjectsWithTag("Background");
            Camera.main.transform.position = new Vector3(0, 0, -10);
            foreach (GameObject background in backgrounds)
            {
                if (background)
                {
                    background.transform.position = new Vector3(0, background.transform.position.y, 5);
                    background.GetComponent<BackgroundScroll>().enabled = true;
                }
            }
            loadingText.SetActive(false);
        } else
        {
            GameObject[] backgrounds = GameObject.FindGameObjectsWithTag("Background");
            Camera.main.transform.position = new Vector3(500, 0, -10);
            foreach (GameObject background in backgrounds)
            {
                if (background)
                {
                    background.transform.position = new Vector3(500, background.transform.position.y, 5);
                    background.GetComponent<BackgroundScroll>().enabled = false;
                }
            }
            loadingText.SetActive(true);
        }
        if (wave > maxWaves) wave = maxWaves; //Checks if current wave is above max waves
        if (maxAliensReached < 5) maxAliensReached = 5; //Checks if maximum aliens reached is below 5
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey("Difficulty");
    }

    IEnumerator spawnWaves()
    {
        while (!gameOver && !won && wave < maxWaves + 1)
        {
            if (!gameOver && !won)
            {
                Vector3 left = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
                Vector3 right = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));
                if (enemiesLeft > 0)
                {
                    yield return new WaitForSeconds(Random.Range(enemySpawnTime.x, enemySpawnTime.y));
                    Instantiate(enemies[Random.Range(0, enemies.Length)], new Vector3(Random.Range(left.x, right.x), 16, 0), Quaternion.Euler(90, 180, 0));
                } else
                {
                    if (!boss)
                    {
                        yield return new WaitForSeconds(3);
                        if (wave >= maxWaves) canWin = true;
                        enemiesLeft = enemyAmount;
                        reachedNextWave = false;
                    } else
                    {
                        if (wave < maxWaves)
                        {
                            yield return new WaitForSeconds(3);
                            if (wave >= maxWaves) canWin = true;
                            enemiesLeft = enemyAmount;
                            reachedNextWave = false;
                        } else
                        {
                            yield return new WaitForSeconds(3);
                            GameObject enemy = Instantiate(boss, new Vector3(0, 16, 0), Quaternion.Euler(0, 180, 0));
                            enemy.name = boss.name;
                            currentBoss = enemy;
                            StartCoroutine(scrollEnemy(enemy, 4.5f));
                            if (PlayerPrefs.GetInt("Difficulty") <= 2) //Easy and Normal
                            {
                                asteroidSpawnTime *= 2;
                            } else if (PlayerPrefs.GetInt("Difficulty") == 3) //Hard
                            {
                                asteroidSpawnTime *= 1.75f;
                            } else if (PlayerPrefs.GetInt("Difficulty") >= 4) //Nightmare
                            {
                                asteroidSpawnTime *= 1.5f;
                            }
                            enemiesLeft = 1;
                            reachedNextWave = false;
                            if (wave >= maxWaves) canWin = true;
                            yield break;
                        }
                    }
                }
            } else
            {
                yield break;
            }
        }
    }

    IEnumerator spawnAsteroids()
    {
        while (!gameOver && !won)
        {
            if (!gameOver && !won)
            {
                yield return new WaitForSeconds(Random.Range(asteroidSpawnTime.x, asteroidSpawnTime.y));
                Vector3 left = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
                Vector3 right = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));
                if (!gameOver && !won && !paused)
                {
                    Instantiate(asteroids[Random.Range(0, asteroids.Length)], new Vector3(Random.Range(left.x, right.x), 9.5f, 0), Quaternion.Euler(0, 0, 0));
                }
            } else
            {
                yield break;
            }
        }
    }

    IEnumerator scrollEnemy(GameObject enemy, float y)
    {
        if (enemy && enemy.CompareTag("Enemy") && enemy.GetComponent<Mover>() && y > 0)
        {
            while (enemy && enemy.transform.position.y > y)
            {
                enemy.GetComponent<Mover>().enabled = true;
                if (enemy.GetComponent<HorizontalOnlyMover>()) enemy.GetComponent<HorizontalOnlyMover>().enabled = false;
                yield return new WaitForEndOfFrame();
            }
            if (enemy)
            {
                enemy.GetComponent<Mover>().enabled = false;
                if (enemy.GetComponent<HorizontalOnlyMover>()) enemy.GetComponent<HorizontalOnlyMover>().enabled = true;
            }
        }
    }

    void pause()
    {
        if (!gameOver && !won && !gameOverMenu.enabled && !levelCompletedMenu.enabled)
        {
            if (!paused) //Pauses the game
            {
                clickSource = 1;
                paused = true;
                Time.timeScale = 0;
                AudioListener.pause = true;
                gamePausedMenu.enabled = true;
            } else //Unpauses the game
            {
                if (!settingsMenu.enabled && !graphicsQualityMenu.enabled && !soundMenu.enabled && !quitGameMenu.enabled && !restartPrompt.enabled)
                {
                    paused = false;
                    Time.timeScale = 1;
                    AudioListener.pause = false;
                    gamePausedMenu.enabled = false;
                }
            }
        }
    }

    public void resumeGame()
    {
        if (!settingsMenu.enabled && !graphicsQualityMenu.enabled && !soundMenu.enabled && !quitGameMenu.enabled && !restartPrompt.enabled)
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
            paused = false;
            Time.timeScale = 1;
            AudioListener.pause = false;
            gamePausedMenu.enabled = false;
        }
    }

    public void restart()
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
        StartCoroutine(loadScene(SceneManager.GetActiveScene().name));
    }

    public void exitGame()
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
        Application.Quit();
    }

    public void exitToMainMenu()
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
        StartCoroutine(loadScene("Main Menu"));
    }

    public void toNextLevel()
    {
        if (won && levelCompletedMenu.enabled)
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
            if (PlayerPrefs.GetInt("Level") < PlayerPrefs.GetInt("MaxLevels"))
            {
                PlayerPrefs.SetInt("Level", PlayerPrefs.GetInt("Level") + 1);
                StartCoroutine(loadScene("Level " + PlayerPrefs.GetInt("Level")));
            } else
            {
                StartCoroutine(loadScene("Ending"));
            }
            PlayerPrefs.Save();
        }
    }

    public void clickSettings()
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
        if (!settingsMenu.enabled)
        {
            settingsMenu.enabled = true;
            gamePausedMenu.enabled = false;
        } else
        {
            settingsMenu.enabled = false;
            gamePausedMenu.enabled = true;
        }
    }

    public void clickGraphicsQuality()
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
        if (!graphicsQualityMenu.enabled)
        {
            graphicsQualityMenu.enabled = true;
            settingsMenu.enabled = false;
        } else
        {
            graphicsQualityMenu.enabled = false;
            settingsMenu.enabled = true;
        }
    }

    public void clickSoundMenu()
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
        if (!soundMenu.enabled)
        {
            soundMenu.enabled = true;
            settingsMenu.enabled = false;
        } else
        {
            soundMenu.enabled = false;
            settingsMenu.enabled = true;
        }
    }

    public void openCanvasFromClickSource(Canvas canvas)
    {
        if (canvas)
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
            if (!canvas.enabled)
            {
                canvas.enabled = true;
                if (clickSource <= 1)
                {
                    gamePausedMenu.enabled = false;
                } else if (clickSource == 2)
                {
                    gameOverMenu.enabled = false;
                } else if (clickSource >= 3)
                {
                    levelCompletedMenu.enabled = false;
                }
            } else
            {
                canvas.enabled = false;
                if (clickSource <= 1)
                {
                    gamePausedMenu.enabled = true;
                } else if (clickSource == 2)
                {
                    gameOverMenu.enabled = true;
                } else if (clickSource >= 3)
                {
                    levelCompletedMenu.enabled = true;
                }
            }
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

    IEnumerator loadScene(string scene)
    {
        if (!loading)
        {
            loading = true;
            AsyncOperation load = SceneManager.LoadSceneAsync(scene);
            while (!load.isDone)
            {
                loadingSlider.value = load.progress;
                loadingPercentage.text = Mathf.Floor(load.progress * 100) + "%";
                gameHUD.enabled = false;
                gamePausedMenu.enabled = false;
                gameOverMenu.enabled = false;
                levelCompletedMenu.enabled = false;
                settingsMenu.enabled = false;
                graphicsQualityMenu.enabled = false;
                soundMenu.enabled = false;
                quitGameMenu.enabled = false;
                restartPrompt.enabled = false;
                yield return null;
            }
            loading = false;
            loadingSlider.value = 0;
            loadingPercentage.text = "0%";
        } else
        {
            StopCoroutine(loadScene(scene));
        }
    }
}