﻿using UnityEngine;

public class EnemyHit : MonoBehaviour
{
    [Tooltip("Amount of damage dealt to players (used by main collider).")] public long damage = 1;
    [SerializeField] private bool instakill = false;
    [SerializeField] private GameObject explosion = null;

    private bool hit = false;

    void Start()
    {
        if (PlayerPrefs.GetInt("Difficulty") <= 1) //Easy
        {
            damage = (long)(damage * 0.75);
        } else if (PlayerPrefs.GetInt("Difficulty") == 3) //Hard
        {
            damage = (long)(damage * 1.15);
        } else if (PlayerPrefs.GetInt("Difficulty") >= 4) //Nightmare
        {
            damage = (long)(damage * 1.3);
        }
    }

    void Update()
    {
        if (damage < 1) damage = 1; //Checks if damage is less than 1 
    }

    void OnTriggerStay(Collider other)
    {
        if (!hit && other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController && !playerController.invulnerable)
            {
                if (!instakill)
                {
                    playerController.takeDamage(damage);
                } else
                {
                    playerController.health = 0;
                    playerController.lives = 0;
                }
                if (explosion) Instantiate(explosion, transform.position, transform.rotation);
                hit = true;
                Destroy(gameObject);
            }
        }
    }
}