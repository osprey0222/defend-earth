﻿using UnityEngine;

public class EnemyGun : MonoBehaviour
{
    [Header("Settings")]
    public long damage = 10;
    [SerializeField] private float spreadDegree = 0;
    public float RPM = 50;

    [Header("Default Skin")]
    [Tooltip("Used only if SkinPicker is in this GameObject.")] [SerializeField] private Texture defaultAlbedo = null;
    [Tooltip("Used only if SkinPicker is in this GameObject.")] [SerializeField] private Texture greenAlbedo = null;
    [Tooltip("Used only if SkinPicker is in this GameObject.")] [SerializeField] private Texture whiteAlbedo = null;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip fireSound = null;

    [Header("Setup")]
    [SerializeField] private GameObject bullet = null;
    [Tooltip("Nightmare only (used only if the value is set to a GameObject).")] [SerializeField] private GameObject nightmareBullet = null;

    private AudioSource audioSource;
    private float nextShot = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        nextShot = Time.time + 60 / RPM;
        if (GameController.instance.isCampaignLevel)
        {
            if (PlayerPrefs.GetInt("Difficulty") <= 1) //Easy
            {
                damage = (long)(damage * 0.8);
                RPM *= 0.85f;
            } else if (PlayerPrefs.GetInt("Difficulty") == 3) //Hard
            {
                damage = (long)(damage * 1.15);
                RPM *= 1.05f;
            } else if (PlayerPrefs.GetInt("Difficulty") >= 4) //Nightmare
            {
                damage = (long)(damage * 1.3);
                RPM *= 1.1f;
            }
        } else
        {
            if (GameController.instance.wavesCleared > 0)
            {
                float multiplier = 1;
                for (long i = 0; i < GameController.instance.wavesCleared; i++) multiplier += 0.05f;
                if (multiplier > 1.5f) multiplier = 1.5f;
                damage = (long)(damage * multiplier);
            }
        }
    }

    void Update()
    {
        if (!GameController.instance.gameOver && !GameController.instance.won && !GameController.instance.paused && Time.time >= nextShot)
        {
            SkinPicker skinPicker = GetComponent<SkinPicker>();
            bool foundBulletSpawns = false;
            nextShot = Time.time + 60 / RPM;
            foreach (Transform bulletSpawn in transform)
            {
                if (bulletSpawn.CompareTag("BulletSpawn") && bulletSpawn.gameObject.activeSelf)
                {
                    GameObject newBullet;
                    if (PlayerPrefs.GetInt("Difficulty") < 4) //Easy, Normal, Hard
                    {
                        newBullet = Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation);
                    } else
                    {
                        if (nightmareBullet)
                        {
                            newBullet = Instantiate(nightmareBullet, bulletSpawn.position, bulletSpawn.rotation);
                        } else
                        {
                            newBullet = Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation);
                        }
                    }
                    newBullet.transform.position = new Vector3(newBullet.transform.position.x, newBullet.transform.position.y, 0);
                    if (turnToPlayer && GameObject.FindWithTag("Player")) newBullet.transform.LookAt(GameObject.FindWithTag("Player").transform);
                    if (spreadDegree != 0) newBullet.transform.Rotate(0, Random.Range(-spreadDegree, spreadDegree), 0);
                    newBullet.GetComponent<EnemyHit>().damage = damage;
                    if (skinPicker)
                    {
                        if (skinPicker.skin <= 1) //Default
                        {
                            if (defaultAlbedo) newBullet.GetComponent<Renderer>().material.SetTexture("_MainTex", defaultAlbedo);
                        } else if (skinPicker.skin == 2) //Green
                        {
                            if (greenAlbedo) newBullet.GetComponent<Renderer>().material.SetTexture("_MainTex", greenAlbedo);
                        } else if (skinPicker.skin >= 3) //White
                        {
                            if (whiteAlbedo) newBullet.GetComponent<Renderer>().material.SetTexture("_MainTex", whiteAlbedo);
                        }
                    }
                    foundBulletSpawns = true;
                }
            }
            if (!foundBulletSpawns)
            {
                if (PlayerPrefs.GetInt("Difficulty") < 4) //Easy, Normal, Hard
                {
                    newBullet = Instantiate(bullet, transform.position - new Vector3(0, 1, 0), transform.rotation);
                } else
                {
                    if (nightmareBullet)
                    {
                        newBullet = Instantiate(nightmareBullet, transform.position - new Vector3(0, 1, 0), transform.rotation);
                    } else
                    {
                        newBullet = Instantiate(bullet, transform.position - new Vector3(0, 1, 0), transform.rotation);
                    }
                }
                newBullet.transform.position = new Vector3(newBullet.transform.position.x, newBullet.transform.position.y, 0);
                if (newBullet.transform.rotation.x != 90) newBullet.transform.rotation = Quaternion.Euler(90, 0, 0);
                if (turnToPlayer && GameObject.FindWithTag("Player")) newBullet.transform.LookAt(GameObject.FindWithTag("Player").transform);
                if (spreadDegree != 0) newBullet.transform.Rotate(0, Random.Range(-spreadDegree, spreadDegree), 0);
                newBullet.GetComponent<EnemyHit>().damage = damage;
                if (skinPicker)
                {
                    if (skinPicker.skin <= 1) //Default
                    {
                        if (defaultAlbedo) newBullet.GetComponent<Renderer>().material.SetTexture("_MainTex", defaultAlbedo);
                    } else if (skinPicker.skin == 2) //Green
                    {
                        if (greenAlbedo) newBullet.GetComponent<Renderer>().material.SetTexture("_MainTex", greenAlbedo);
                    } else if (skinPicker.skin >= 3) //White
                    {
                        if (whiteAlbedo) newBullet.GetComponent<Renderer>().material.SetTexture("_MainTex", whiteAlbedo);
                    }
                }
                foundBulletSpawns = true;
            }
            if (audioSource && foundBulletSpawns)
            {
                if (fireSound)
                {
                    audioSource.PlayOneShot(fireSound);
                } else
                {
                    audioSource.Play();
                }
            }
        }
        if (damage < 1) damage = 1; //Checks if damage is less than 1
    }
}