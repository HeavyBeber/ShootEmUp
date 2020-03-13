using UnityEngine;

public interface IDamageable
{
    void TakeHit(float _damage, Vector3 hitPoint, Vector3 hitDirection);
    
    void TakeDamage(float _damage);
}
