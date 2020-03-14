using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum FireMode {AUTO, BURST, SINGLE};
    public FireMode fireMode;

    public Transform[] projetileSpawns;
    public Projectile projectile;
    public float msBetweenShots = 100;
    public float muzzleVelocity = 35;
    public int burstCount;

    public Transform shell;
    public Transform chamber;
    MuzzleFlash muzzleFlash;

    float nextShotTime;

    bool triggerReleasedSinceLastShot;
    int shotsRemainingInBurst;

    void Start() {
        muzzleFlash = GetComponent<MuzzleFlash> ();
        shotsRemainingInBurst = burstCount;
    }

    void Shoot() {
        if(Time.time > nextShotTime) {

            if (fireMode == FireMode.BURST) {
                if (shotsRemainingInBurst == 0) {
                    return;
                }
                shotsRemainingInBurst--;
            }
            else if (fireMode == FireMode.SINGLE) {
                if (!triggerReleasedSinceLastShot) {
                    return;
                }
            }

            for (int i = 0; i < projetileSpawns.Length; i++) {
                nextShotTime = Time.time + msBetweenShots / 1000;
                Projectile newProjectile = Instantiate(projectile, projetileSpawns[i].position, projetileSpawns[i].rotation) as Projectile;
                newProjectile.setSpeed(muzzleVelocity);     
            }
        
            Instantiate(shell, chamber.position, chamber.rotation);
            muzzleFlash.Activate();
        }
    }

    public void OnTriggerHold() {
        Shoot();
        triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerRelease() {
        triggerReleasedSinceLastShot = true;
        shotsRemainingInBurst = burstCount;
    }

}
