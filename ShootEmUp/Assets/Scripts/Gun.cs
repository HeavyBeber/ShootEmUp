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
    public int projectilePerMag;
    public float reloadTime = .5f;

    [Header("Recoil")]
    public Vector2 kickMinMax = new Vector2(.2f,.5f);
    public Vector2 recoilAngleMinMax = new Vector2(5,10);
    public float recoilMoveReturnTime = .1f;
    public float recoilAngleReturnTime = .05f;

    [Header("Effect")]
    public Transform shell;
    public Transform chamber;
    MuzzleFlash muzzleFlash;

    float nextShotTime;

    bool triggerReleasedSinceLastShot;
    int shotsRemainingInBurst;
    int projectileRemainingInMag;
    bool isReloading;

    Vector3 recoilSmoothDampVelocity;
    float recoilAngleSmoothDampVelocity;
    float recoilAngle;

    void Start() {
        muzzleFlash = GetComponent<MuzzleFlash> ();
        shotsRemainingInBurst = burstCount;
        projectileRemainingInMag = projectilePerMag;
    }

    private void LateUpdate() {
        //animate recoil
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, recoilMoveReturnTime);
        if (!isReloading) {
            recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilAngleSmoothDampVelocity, recoilAngleReturnTime);
            transform.localEulerAngles = transform.localEulerAngles + Vector3.left * recoilAngle;
        }

        if (!isReloading && projectileRemainingInMag == 0) {
            Reload();
        }
    }

    void Shoot() {
        if(!isReloading && Time.time > nextShotTime && projectileRemainingInMag > 0) {

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
                if(projectileRemainingInMag == 0) {
                    break;
                }
                projectileRemainingInMag --;
                nextShotTime = Time.time + msBetweenShots / 1000;
                Projectile newProjectile = Instantiate(projectile, projetileSpawns[i].position, projetileSpawns[i].rotation) as Projectile;
                newProjectile.transform.localEulerAngles = newProjectile.transform.localEulerAngles + Vector3.up * Random.Range(-recoilAngle, recoilAngle);
                newProjectile.setSpeed(muzzleVelocity);     
            }
        
            Instantiate(shell, chamber.position, chamber.rotation);
            muzzleFlash.Activate();
            transform.localPosition -= Vector3.forward * Random.Range(kickMinMax.x,kickMinMax.y);
            recoilAngle += Random.Range(recoilAngleMinMax.x,recoilAngleMinMax.y);
            recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);
        }
    }

    public void Aim(Vector3 aimPoint) {
        if (!isReloading) {
            transform.LookAt(aimPoint);
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

    IEnumerator AnimateReload() {
        yield return new WaitForSeconds(.2f);

        float percent = 0;
        float reloadSpeed = 1 / reloadTime;
        Vector3 initialRotation = transform.localEulerAngles;
        float maxReloadAngle = 60;

        while (percent <1) {
            percent += Time.deltaTime * reloadSpeed;
            float interpolation = (-percent*percent + percent) * 4;
            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
            transform.localEulerAngles = initialRotation + Vector3.right * reloadAngle;
            yield return null;
        }

        isReloading = false;
        projectileRemainingInMag = projectilePerMag;
    }

    public void Reload() {
        if (!isReloading && projectileRemainingInMag != projectilePerMag) {
            isReloading = true;
            StartCoroutine(AnimateReload());
        }
    }

}
