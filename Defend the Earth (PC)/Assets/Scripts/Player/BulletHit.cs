﻿using UnityEngine;

public class BulletHit : MonoBehaviour
{
    [Tooltip("Amount of damage dealt to enemies.")] public long damage = 5;
    [SerializeField] private float doubleDamageMultiplier = 1.5f;
    [SerializeField] private LayerMask doubleDamageLayer = -1;
    [SerializeField] private GameObject explosion = null;

    private bool hit = false;

    void Update()
    {
        if (damage < 1) damage = 1; //Checks if damage is less than 1
    }

    void OnTriggerStay(Collider other)
    {
        if (!hit && other.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth)
            {
                if (other.gameObject.layer != doubleDamageLayer)
                {
                    enemyHealth.takeDamage((long)(damage * enemyHealth.defense));
                } else
                {
                    long dealtDamage = (long)(damage * doubleDamageMultiplier);
                    dealtDamage = (long)(dealtDamage * enemyHealth.defense);
                    enemyHealth.takeDamage(dealtDamage);
                }
                if (explosion) Instantiate(explosion, transform.position, transform.rotation);
                hit = true;
                Destroy(gameObject);
            }
        }
    }
}