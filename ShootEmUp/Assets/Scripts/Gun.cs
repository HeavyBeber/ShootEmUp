using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum FireMode {AUTO, BURST, MULTIPLE};
    public FireMode fireMode;

    public Transform muzzle;
    public Projectile projectile;
    public float msBetweenShots = 100;
    public float muzzleVelocity = 35;

    public Transform shell;
    public Transform chamber;
    MuzzleFlash muzzleFlash;

    float nextShotTime;

    void Start() {
        muzzleFlash = GetComponent<MuzzleFlash> ();
    }

    public void Shoot() {
        if(Time.time > nextShotTime) {
            nextShotTime = Time.time + msBetweenShots / 1000;
            Projectile newProjectile = Instantiate(projectile, muzzle.position, muzzle.rotation) as Projectile;
            newProjectile.setSpeed(muzzleVelocity);     

            Instantiate(shell, chamber.position, chamber.rotation);
            muzzleFlash.Activate();
        }
    }


}
