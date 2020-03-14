using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    public Map[] maps;
    public int mapIndex;

    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform mapFloor;
    public Transform navmeshFloor;
    public Transform navmeshMask;
    public Vector2 maxMapSize;
    public float tileSize = 1;

    [Range(0,1)]
    public float outlinePercent;

    List<Coordinate> tilesCoord;
    Queue<Coordinate> shuffledCoord;
    Queue<Coordinate> shuffledOpenTilesCoord;
    Transform[,] tileMap;

    public int seed = 10;

    Map currentMap;

    void Start() {
        FindObjectOfType<Spawner> ().OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumber) {
        mapIndex = waveNumber - 1;
        GenerateMap();
    }

    public void GenerateMap() {
        currentMap = maps[mapIndex];
        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];
        System.Random rng = new System.Random(currentMap.seed);

        //Prepare the map
        tilesCoord = new List<Coordinate> ();
        string holderName = "Generated Map";
        if (transform.Find(holderName)) {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }
        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        //Generate tiles 
        for (int x = 0; x < currentMap.mapSize.x; x ++) { 
            for (int y = 0; y < currentMap.mapSize.y; y ++) {
                Vector3 tilePosition = coordToPosition(x,y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right*90)) as Transform;
                newTile.localScale = Vector3.one * (1-outlinePercent) * tileSize;
                newTile.parent = mapHolder;
                tilesCoord.Add(new Coordinate(x,y));
                tileMap[x,y] = newTile;
            }
        }

        //Generate obstacles
        shuffledCoord = new Queue<Coordinate> (Utils.shuffleArray(tilesCoord.ToArray(),currentMap.seed));
        bool[,] obstacleMap = new bool[currentMap.mapSize.x,(int) currentMap.mapSize.y];

        int obstacleCount = (int) (currentMap.mapSize.x*currentMap.mapSize.y*currentMap.obstaclePercent);
        int currentObstacleCount = 0;
        List<Coordinate> allOpenCoords = new List<Coordinate>(tilesCoord);

        for (int i = 0 ; i < obstacleCount ; i ++) {
            Coordinate randomCoord = getRandomCoord();
            obstacleMap[randomCoord.x,randomCoord.y] = true;
            currentObstacleCount ++;

            if (randomCoord != currentMap.mapCenter && mapIsFullyAccessible(obstacleMap, currentObstacleCount)) {
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight,currentMap.maxObstacleHeight,(float)rng.NextDouble());
                Vector3 obstaclePosition = coordToPosition(randomCoord.x,randomCoord.y);

                Transform newObstacle = Instantiate(obstaclePrefab,obstaclePosition + Vector3.up*obstacleHeight/2,Quaternion.identity) as Transform;
                newObstacle.localScale = new Vector3((1-outlinePercent)*tileSize, obstacleHeight, (1-outlinePercent)*tileSize);
                newObstacle.parent = mapHolder;

                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                float colorPercent = (float) randomCoord.y / currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColor,currentMap.backgroundColor,colorPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                allOpenCoords.Remove(randomCoord);
            } else {
                obstacleMap[randomCoord.x,randomCoord.y] = false;
                currentObstacleCount --;
            }
        }
              
        shuffledOpenTilesCoord = new Queue<Coordinate> (Utils.shuffleArray(allOpenCoords.ToArray(),currentMap.seed));

        //Generate navMesh mask
        Transform maskLeft = Instantiate(navmeshMask,Vector3.left*(currentMap.mapSize.x + maxMapSize.x)*tileSize/4f,Quaternion.identity) as Transform;
        maskLeft.localScale = new Vector3((maxMapSize.x-currentMap.mapSize.x)/2f,1,currentMap.mapSize.y)*tileSize;
        maskLeft.parent = mapHolder;
        Transform maskRight = Instantiate(navmeshMask,Vector3.right*(currentMap.mapSize.x + maxMapSize.x)*tileSize/4f,Quaternion.identity) as Transform;
        maskRight.localScale = new Vector3((maxMapSize.x-currentMap.mapSize.x)/2f,1,currentMap.mapSize.y)*tileSize;
        maskRight.parent = mapHolder;
        Transform maskTop = Instantiate(navmeshMask,Vector3.forward*(currentMap.mapSize.y + maxMapSize.y)*tileSize/4f,Quaternion.identity) as Transform;
        maskTop.localScale = new Vector3(maxMapSize.x,1,(maxMapSize.y-currentMap.mapSize.y)/2f)*tileSize;
        maskTop.parent = mapHolder;
        Transform maskBottom = Instantiate(navmeshMask,Vector3.back*(currentMap.mapSize.y + maxMapSize.y)*tileSize/4f,Quaternion.identity) as Transform;
        maskBottom.localScale = new Vector3(maxMapSize.x,1,(maxMapSize.y-currentMap.mapSize.y)/2f)*tileSize;
        maskBottom.parent = mapHolder;
        navmeshFloor.localScale = new Vector3(maxMapSize.x,maxMapSize.y) * tileSize;
        mapFloor.localScale = new Vector3(currentMap.mapSize.x*tileSize,currentMap.mapSize.y*tileSize);
    }

    bool mapIsFullyAccessible(bool[,] _obstacleMap, int _obstacleCount) {
        bool[,] mapFlags = new bool[_obstacleMap.GetLength(0),_obstacleMap.GetLength(1)];
        Queue<Coordinate> queue = new Queue<Coordinate>();

        queue.Enqueue(currentMap.mapCenter);
        mapFlags[currentMap.mapCenter.x,currentMap.mapCenter.y] = true;

        int accessibleTileCount = 1;

        while (queue.Count > 0) {
            Coordinate tile = queue.Dequeue();
            
            for (int x = -1; x <= 1 ; x ++) {
                for (int y = -1; y <= 1 ; y ++) {
                    int neighBourX = tile.x + x;
                    int neighBourY = tile.y + y;

                    if (x == 0 || y == 0) {
                        if (neighBourX >= 0 && neighBourX < _obstacleMap.GetLength(0) && neighBourY >= 0 && neighBourY < _obstacleMap.GetLength(1)) {
                            if (!mapFlags[neighBourX,neighBourY] && !_obstacleMap[neighBourX,neighBourY]) {
                                mapFlags[neighBourX,neighBourY] = true;
                                queue.Enqueue(new Coordinate(neighBourX,neighBourY));
                                accessibleTileCount ++;
                            }
                        }
                    }
                }
            }
        }
        
        int targetAccessibleTileCount = currentMap.mapSize.x*currentMap.mapSize.y - _obstacleCount;

        return targetAccessibleTileCount == accessibleTileCount;
    }

    Vector3 coordToPosition(int x, int y) {
        return new Vector3(-currentMap.mapSize.x/2f + 0.5f + x, 0, -currentMap.mapSize.y/2f + 0.5f + y) * tileSize;
    }

    public Transform getTileFromPosition(Vector3 position) {
        int x = Mathf.RoundToInt(position.x/tileSize + (currentMap.mapSize.x - 1)/2f); 
        int y = Mathf.RoundToInt(position.z/tileSize + (currentMap.mapSize.y - 1)/2f); 
        x = Mathf.Clamp(x,0,tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y,0,tileMap.GetLength(1) - 1);

        return tileMap[x,y];
    }

    public Coordinate getRandomCoord() {
        Coordinate randomCoord = shuffledCoord.Dequeue();
        shuffledCoord.Enqueue(randomCoord);
        return randomCoord;
    }

    public Transform getRandomOpenTile() {
        Coordinate randomCoord = shuffledOpenTilesCoord.Dequeue();
        shuffledOpenTilesCoord.Enqueue(randomCoord);
        return tileMap[randomCoord.x,randomCoord.y];
    }

    [System.Serializable]
    public struct Coordinate {
        public int x;
        public int y;
        public Coordinate(int _x, int _y) {
            x = _x;
            y = _y;
        }

        public static bool operator ==(Coordinate c1, Coordinate c2) {
            return c1.x == c2.y && c1.y == c2.y;
        }
        public static bool operator !=(Coordinate c1, Coordinate c2) {
            return !(c1==c2);
        }
    }

    [System.Serializable]
    public class Map {
        public Coordinate mapSize;
        [Range(0,1)]
        public float obstaclePercent;
        public int seed;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foregroundColor;
        public Color backgroundColor;

        
        public Coordinate mapCenter {
            get {
                return new Coordinate(mapSize.x/2,mapSize.y/2);
            }
        }
    }

}
