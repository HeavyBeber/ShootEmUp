﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable
{

    public float startingHealth;
    protected float health;
    protected bool dead;

    public event System.Action OnDeath;

    protected virtual void Start() {
        health = startingHealth;
    }

    public void takeHit (float damage, RaycastHit hit) {
        takeDamage(damage);
    }

    public void takeDamage(float damage) {
        health -= damage;
        if (health <= 0 && !dead) {
            Die();
        }
    }

    protected void Die() {
        dead = true;
        if (OnDeath != null) {
            OnDeath();
        }
        GameObject.Destroy(gameObject);
    }
}
