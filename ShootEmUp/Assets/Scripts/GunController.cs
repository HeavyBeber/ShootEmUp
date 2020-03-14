using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public Transform weaponHold;
    public Gun startingGun;
    Gun equippedGun;

    public void Start() {
        if (startingGun != null) {
            EquipGun(startingGun);
        }
    }

    public void EquipGun(Gun _gunToEquip) {
        if (equippedGun != null) {
            Destroy(equippedGun.gameObject);
        }
        equippedGun = Instantiate(_gunToEquip, weaponHold.position, weaponHold.rotation) as Gun;
        equippedGun.transform.parent = weaponHold;
    }

    public void OnTriggerHold() {
        if (equippedGun != null) {
            equippedGun.OnTriggerHold();
        }
    }

    public void OnTriggerRelease() {
        if (equippedGun != null) {
            equippedGun.OnTriggerRelease();
        }
    }
}
