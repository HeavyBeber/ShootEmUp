using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (UnityEngine.AI.NavMeshAgent))]
public class Enemy : LivingEntity
{

    public enum State {IDLE, CHASING, ATTACKING}

    State currentState;
    UnityEngine.AI.NavMeshAgent pathFinder;
    Transform target;
    LivingEntity targetEntity;
    Material skinMaterial;

    Color originalColor;

    float attackDistanceThreshold = .5f;
    float timeBewteenAttacks = 1;
    float nextAttackTime;
    float damage = 1;

    float myCollisionRadius;
    float targetCollisionRadius;

    bool hasTarget;

    protected override void Start()
    {
        base.Start();
        pathFinder = GetComponent<UnityEngine.AI.NavMeshAgent>();    
        skinMaterial = GetComponent<Renderer>().material;
        originalColor = skinMaterial.color;

        if (GameObject.FindGameObjectWithTag("Player") != null) {
            currentState = State.CHASING;
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();
            targetEntity.OnDeath += onTargetDeath;

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;

            StartCoroutine(UpdatePath());
        }

    }

    void onTargetDeath() {
        hasTarget = false;
        currentState = State.IDLE;
    }

    void Update()
    {
        if (hasTarget) {
            if (Time.time > nextAttackTime) {
                float sqrDistanceToTarget = (target.position - transform.position).sqrMagnitude;
                if (sqrDistanceToTarget < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2)) {
                    nextAttackTime = Time.time + timeBewteenAttacks;
                    StartCoroutine(Attack());
                }
            }
        }
    }


    IEnumerator Attack() {

        currentState = State.ATTACKING;
        pathFinder.enabled = false;
        skinMaterial.color = Color.green;

        Vector3 originalPostion = transform.position;
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Vector3 targetPosition = target.position - directionToTarget * myCollisionRadius;

        float attackSpeed = 3;
        float percent = 0;
        bool hasAppliedDmg = false;

        while(percent <= 1) {

            if (percent > .5f && !hasAppliedDmg) {
                hasAppliedDmg = true;
                targetEntity.takeDamage(damage);
            }

            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-percent*percent + percent) * 4;

            transform.position = Vector3.Lerp(originalPostion, targetPosition, interpolation);

            yield return null;
        }
        
        pathFinder.enabled = true;
        currentState = State.CHASING;
        skinMaterial.color = originalColor;
    }

    IEnumerator UpdatePath() {
        float refreshRate = .25f;
        while (hasTarget) {
            if (currentState == State.CHASING) {
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - directionToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold/2);
                if (!dead) {
                    pathFinder.SetDestination(targetPosition);
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
