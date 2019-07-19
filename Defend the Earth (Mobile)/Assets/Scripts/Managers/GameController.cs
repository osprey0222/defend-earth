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
    [SerializeField] private float timeBetweenWaves = 5;
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
    [SerializeField] private Text levelCount = null;
    [SerializeField] private Text waveCount = null;
    [SerializeField] private Text moneyCount = null;
    [SerializeField] private Text bossName = null;
    [SerializeField] private Slider bossHealthBar = null;
    [SerializeField] private Text bossHealthText = null;
    [SerializeField] private Slider soundSlider = null;
    [SerializeField] private Slider musicSlider = null;
    [SerializeField] private Text deathMessage = null;
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

    [Header("Setup")]
    [SerializeField] private GameObject[] playerShips = new GameObject[0];

    private GameObject[] backgrounds;
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
        backgrounds = GameObject.FindGameObjectsWithTag("Background");
        if (enemiesLeft <= 0) enemiesLeft = 1;
        currentBoss = null;
        bossMaxHealth = 0;
        aliensReached = 0;
        deathMessageToShow = "";
        gameOver = false;
        won = false;
        paused = false;
        enemyAmount = enemiesLeft;

        //Destroy all player and enemy objects in the scene 
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] aliens = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject player in players)
        {
            if (player) Destroy(player);
        }
        foreach (GameObject enemy in aliens)
        {
            if (enemy) Destroy(enemy);
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
            asteroidSpawnTime -= new Vector2(0.5f, 0);
            maxAliensReached -= 1;
        } else if (PlayerPrefs.GetInt("Difficulty") >= 4) //Nightmare
        {
            asteroidSpawnTime -= new Vector2(1.25f, 1);
            enemyAmount += 1;
            maxAliensReached -= 2;
        }
        if (maxAliensReached < 5) maxAliensReached = 5; //Checks if maximum aliens reached is below 5
        Time.timeScale = 1;
        AudioListener.pause = false;
        paused = false;
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
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        }
        if (Camera.main.GetComponent<AudioSource>())
        {
            Camera.main.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("MusicVolume");
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
        StartCoroutine(spawnWaves());
        StartCoroutine(spawnAsteroids());
    }

    void Update()
    {
        if (Camera.main.GetComponent<AudioSource>()) Camera.main.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("MusicVolume");
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1)) pause();
        if (Input.GetKeyDown(KeyCode.F11)) Screen.fullScreen = !Screen.fullScreen;
        if (!gameOver && aliensReached >= maxAliensReached)
        {
            gameOver = true;
            deathMessageToShow = "You failed to protect the Earth!";
        }
        if (gameOver)
        {
            clickSource = 2;
            if (!quitGameMenu.enabled && !loading) gameOverMenu.enabled = true;
            StopCoroutine(spawnWaves());
            StopCoroutine(spawnAsteroids());
            if (Camera.main.GetComponent<AudioSource>()) Camera.main.GetComponent<AudioSource>().Stop();
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
                if (!quitGameMenu.enabled && !loading) levelCompletedMenu.enabled = true;
                StopCoroutine(spawnWaves());
                StopCoroutine(spawnAsteroids());
                if (Camera.main.GetComponent<AudioSource>()) Camera.main.GetComponent<AudioSource>().Stop();
            }
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
            bossMaxHealth = 0;
            bossName.gameObject.SetActive(false);
            bossName.text = "Boss Name";
            bossHealthBar.value = 100;
            bossHealthBar.maxValue = 100;
            bossHealthText.text = "100 / 100";
        } else
        {
            if (bossMaxHealth <= 0) bossMaxHealth = currentBoss.GetComponent<EnemyHealth>().health;
            bossName.gameObject.SetActive(true);
            bossName.text = currentBoss.name;
            bossHealthBar.value = currentBoss.GetComponent<EnemyHealth>().health;
            bossHealthBar.maxValue = bossMaxHealth;
            bossHealthText.text = bossHealthBar.value + " / " + bossHealthBar.maxValue;
        }
        deathMessage.text = deathMessageToShow;
        if (!loading)
        {
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
                        yield return new WaitForSeconds(timeBetweenWaves);
                        if (wave >= maxWaves) canWin = true;
                        enemiesLeft = enemyAmount;
                        reachedNextWave = false;
                    } else
                    {
                        if (wave < maxWaves)
                        {
                            yield return new WaitForSeconds(timeBetweenWaves);
                            if (wave >= maxWaves) canWin = true;
                            enemiesLeft = enemyAmount;
                            reachedNextWave = false;
                        } else
                        {
                            yield return new WaitForSeconds(timeBetweenWaves);
                            if (wave >= maxWaves) canWin = true;
                            GameObject enemy = Instantiate(boss, new Vector3(0, 16, 0), Quaternion.Euler(0, 180, 0));
                            enemy.name = boss.name;
                            currentBoss = enemy;
                            StartCoroutine(scrollEnemy(enemy, 4.5f));
                            enemiesLeft = 1;
                            reachedNextWave = false;
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
                if (!gameOver && !won) Instantiate(asteroids[Random.Range(0, asteroids.Length)], new Vector3(Random.Range(-11, 11), 9.5f, 0), Quaternion.Euler(0, 0, 0));
            } else
            {
                break;
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
                if (enemy.GetComponent<SideMover>()) enemy.GetComponent<SideMover>().enabled = false;
                yield return new WaitForEndOfFrame();
            }
            if (enemy)
            {
                enemy.GetComponent<Mover>().enabled = false;
                if (enemy.GetComponent<SideMover>()) enemy.GetComponent<SideMover>().enabled = true;
            }
        }
    }

    void pause()
    {
        if (!gameOver && !won)
        {
            clickSource = 1;
            if (!paused)
            {
                Time.timeScale = 0;
                paused = true;
                gamePausedMenu.enabled = true;
                AudioListener.pause = true;
            } else
            {
                Time.timeScale = 1;
                paused = false;
                gamePausedMenu.enabled = false;
                AudioListener.pause = false;
            }
        }
    }

    public void resumeGame()
    {
        Time.timeScale = 1;
        paused = false;
        gamePausedMenu.enabled = false;
        AudioListener.pause = false;
    }

    public void restart()
    {
        StartCoroutine(loadScene(SceneManager.GetActiveScene().name));
    }

    public void exitGame()
    {
        Application.Quit();
    }

    public void exitToMainMenu()
    {
        StartCoroutine(loadScene("Main Menu"));
    }

    public void toNextLevel()
    {
        if (PlayerPrefs.GetInt("Level") < PlayerPrefs.GetInt("MaxLevels"))
        {
            PlayerPrefs.SetInt("Level", PlayerPrefs.GetInt("Level") + 1);
            StartCoroutine(loadScene("Level " + PlayerPrefs.GetInt("Level")));
        } else
        {
            PlayerPrefs.SetInt("Level", 1);
            StartCoroutine(loadScene("Level 1"));
        }
        PlayerPrefs.Save();
    }

    public void clickSettings()
    {
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

    public void clickQuitGame()
    {
        if (!quitGameMenu.enabled)
        {
            quitGameMenu.enabled = true;
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
        }
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
