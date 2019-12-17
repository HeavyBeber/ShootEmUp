using UnityEngine;

public interface IDamageable
{
    void takeHit(float _damage, RaycastHit hit);
    
    void takeDamage(float _damage);
}
