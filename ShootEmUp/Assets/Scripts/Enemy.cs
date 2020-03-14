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

    public ParticleSystem deathEffect;

    Color originalColor;

    float attackDistanceThreshold = .5f;
    float timeBewteenAttacks = 1;
    float nextAttackTime;
    float damage = 1;

    float myCollisionRadius;
    float targetCollisionRadius;

    bool hasTarget;

    void Awake() {
        pathFinder = GetComponent<UnityEngine.AI.NavMeshAgent>();    

        if (GameObject.FindGameObjectWithTag("Player") != null) {
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
        }
    }

    protected override void Start() {
        base.Start();

        if (hasTarget) {
            currentState = State.CHASING;
            targetEntity.OnDeath += OnTargetDeath;

            StartCoroutine(UpdatePath());
        }

    }

    public override void TakeHit (float damage, Vector3 hitPoint, Vector3 hitDirection) {
        if (damage >= health) {
            Destroy(
                Instantiate(
                    deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)
                ) as GameObject, 
                deathEffect.startLifetime);
        }
        base.TakeHit(damage,hitPoint,hitDirection);
    }

    void OnTargetDeath() {
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

    public void SetCharacteristic(float moveSpeed, int hitsToKillPlayer, float enemyHealth, Color skinColour) {
        pathFinder.speed = moveSpeed;
        if (hasTarget) {
            damage = Mathf.Ceil(targetEntity.startingHealth / hitsToKillPlayer);
        }
        startingHealth = enemyHealth;
        skinMaterial = GetComponent<Renderer>().material;
        skinMaterial.color = skinColour;
        originalColor = skinMaterial.color;

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
                targetEntity.TakeDamage(damage);
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
