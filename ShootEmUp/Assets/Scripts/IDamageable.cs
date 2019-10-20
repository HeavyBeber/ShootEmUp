using UnityEngine;

public interface IDamageable
{
    void TakeHit(float _damage, RaycastHit hit);
}
