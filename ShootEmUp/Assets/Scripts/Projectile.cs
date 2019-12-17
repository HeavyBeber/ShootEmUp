using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask collisionMask;
    
    float damage = 1;
    float speed = 10;
    float lifeTime = 3; 
    //Used when enemy is moving on the same time that the bullet :
    //it could be that the bullet is in the enemy when the raycast starts
    float skinWidth = .1f;

    void Start() {
        Destroy(gameObject,lifeTime);

        Collider[] initialCollisions = Physics.OverlapSphere(transform.position,.1f,collisionMask);

        if(initialCollisions.Length > 0) {
            OnHitObject(initialCollisions[0]);
        }
    }
    public void setSpeed(float _newSpeed) {
        speed = _newSpeed;
    }

    void Update()
    {
        float moveDistance = speed * Time.deltaTime;
        CheckCollisions(moveDistance);
        transform.Translate(Vector3.forward * moveDistance);
    }

    void CheckCollisions(float moveDistance) {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide)) {
            OnHitObject(hit);
        }
    }

    void OnHitObject(RaycastHit hit) {
        IDamageable damageableObject = hit.collider.GetComponent<IDamageable>();
        if (damageableObject != null) {
            damageableObject.takeHit(damage, hit);
        }
        GameObject.Destroy(gameObject);
    }
    void OnHitObject(Collider c) {
        IDamageable damageableObject = c.GetComponent<IDamageable>();
        if (damageableObject != null) {
            damageableObject.takeDamage(damage);
        }
        GameObject.Destroy(gameObject);
    }
}
