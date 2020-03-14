using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public bool devMode;

    public Wave[] waves;
    public Enemy enemy;

    LivingEntity playerEntity;
    Transform playerTransform;

    Wave currentWave;
    int currentWaveNumber;
    int enemiesRemainingToSpawn;
    int enemiesRemainingAlive;
    float nextSpawnTime;

    MapGenerator map;

    float timeBewteenCampingChecks = 2;
    float campThresholdDistance = 1.5f;
    float nextCampingCheckTime;
    Vector3 campingPosition;
    bool isCamping;

    bool isDisabled;

    public event System.Action<int> OnNewWave;

    void Start() {
        map = FindObjectOfType<MapGenerator>();
        playerEntity = FindObjectOfType<Player>();
        playerTransform = playerEntity.transform;

        playerEntity.OnDeath += OnPlayerDeath;

        nextCampingCheckTime = timeBewteenCampingChecks + Time.time;
        campingPosition = playerTransform.position;

        NextWave();
    }

    void Update() {
        if (!isDisabled) {
            if (Time.time > nextCampingCheckTime) {
                nextCampingCheckTime = Time.time + timeBewteenCampingChecks;
                
                isCamping = Vector3.Distance(campingPosition,playerTransform.position) < campThresholdDistance;
                campingPosition = playerTransform.position;
            }

            if ((enemiesRemainingToSpawn > 0 || currentWave.infinite) && Time.time > nextSpawnTime) {
                enemiesRemainingToSpawn --;
                nextSpawnTime = Time.time + currentWave.timeBetweenSpanws;
                
                StartCoroutine("SpawnEnemy");        
            }
        }

        if (devMode) {
            if (Input.GetKeyDown(KeyCode.Return)) {
                StopCoroutine("SpawnEnemy");
                foreach (Enemy enemy in FindObjectsOfType<Enemy>()) {
                    GameObject.Destroy(enemy.gameObject);
                }
               NextWave();
            }
        }
    }

    IEnumerator SpawnEnemy() {
        float spawnDelay = 1;
        float tileFlashSpeed = 4;

        Transform spawnTile = map.getRandomOpenTile();
        if(isCamping) {
            spawnTile = map.getTileFromPosition(playerTransform.position);
        }

        Material tileMat= spawnTile.GetComponent<Renderer>().material;
        Color initialColor = tileMat.color;
        Color flashColor = Color.red;
        float spawnTimer = 0;

        while(spawnTimer < spawnDelay) {
            tileMat.color = Color.Lerp(initialColor,flashColor,Mathf.PingPong(spawnTimer*tileFlashSpeed,1));
            
            spawnTimer += Time.deltaTime;
            yield return null;
        }


        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up,Quaternion.identity) as Enemy;
        spawnedEnemy.OnDeath += OnEnemyDeath;

        spawnedEnemy.SetCharacteristic(currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth, currentWave.skinColour);

    }

    void OnEnemyDeath() {
        enemiesRemainingAlive --;

        if (enemiesRemainingAlive == 0) {
            NextWave();
        }
    }

    void OnPlayerDeath() {
        isDisabled = true;
    }

    void ResetPlayerPosition() {
        playerTransform.position = map.getTileFromPosition(Vector3.zero).position + Vector3.up*3;
    }

    void NextWave() {
        currentWaveNumber ++;
        if (currentWaveNumber - 1 < waves.Length) {
            currentWave = waves[currentWaveNumber - 1];
            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesRemainingAlive = enemiesRemainingToSpawn;

            if (OnNewWave != null)  {
                OnNewWave(currentWaveNumber);
            }
        }
        ResetPlayerPosition();
    }

    [System.Serializable]
    public class Wave {
        public bool infinite;
        public int enemyCount;
        public float timeBetweenSpanws;

        public float moveSpeed;
        public int hitsToKillPlayer;
        public float enemyHealth;
        public Color skinColour;
    }

}
