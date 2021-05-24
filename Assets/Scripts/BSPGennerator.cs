using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using Random = UnityEngine.Random;

public class BSPGennerator : RandomWalkGenerator
{
    [SerializeField]
    private bool randomWalkRooms = false;


    private Vector2Int farest;
    public Transform Enemy;
    private List<Vector2Int> roomCenters = new List<Vector2Int>();
    private List<BoundsInt> roomList;
    public int counter;
    private BoundsInt firstRoom;
    public Vector3 currRoom;
    private int TotalEnemyCount = 8;
    public Dictionary<Vector3, List<int>>enemyCounters = new Dictionary<Vector3, List<int>>();
    private bool FirstWaveKilled = false;
    public GameObject Trap;
    private void Awake()
    {
        Clear(); 
        CreateRooms();
    }

    void Start()
    {
        Enemy.GetComponent<EnemyFollowing>().target = Player.transform;
        foreach (var center in roomList
            .Select(x => x.center)
            .ToList())
        {
            enemyCounters[new Vector3(0f, 0f, 0f)] = new List<int>(2){0, 0};
            if (!enemyCounters.ContainsKey(center))
            {
                enemyCounters[center] =new List<int>(2){0, 0};
            }
        }
    }

    private void FixedUpdate()
    {
        var spawnRoom = roomList
            .Where(x=> x!= firstRoom)
            .FirstOrDefault(y =>
            {
                var min = Math.Min(_mapSettings.minRoomHeight, _mapSettings.minRoomWidth) / 2;
                
                return Vector3.Distance(Player.transform.position, y.center) <=
                       min;
            });
      
        var spawnCenter = spawnRoom.center;
        if (spawnCenter != Vector3.zero)
        {
            currRoom = spawnCenter;
        }
        
        if (spawnCenter != Vector3.zero && enemyCounters[spawnCenter][0] == 0  && new Vector3(playerPos.x, playerPos.y, 0f)!=spawnCenter)
        {
            
            
            if (enemyCounters[spawnCenter][1]<3)
            {
                if (enemyCounters[spawnCenter][1] == 0)
                {
                    
                   
                    foreach (var dir in Direction.directions)
                    {

                        var trapPos = spawnCenter +
                                      new Vector3(dir.x, dir.y,
                                          0f) * (Mathf.Min(spawnRoom.size.x / 2.5f, spawnRoom.size.y / 2.5f) - _mapSettings.offset);
                        if (Vector2.Distance(trapPos, Player.position) <=
                            Math.Min(_mapSettings.minRoomHeight, _mapSettings.minRoomWidth) / 2)
                            continue;
                        
                        Instantiate(Trap,trapPos , Quaternion.identity );
                    }
                    
                }
                
                
                foreach (var random2d in Direction.directionsDiag)
                {

                    var enemyPos = spawnCenter + new Vector3(random2d.x, random2d.y, 0f).normalized*(Mathf.Min(spawnRoom.size.x/2.5f, spawnRoom.size.y/2.5f)-_mapSettings.offset);
                    Instantiate(Enemy,enemyPos , Quaternion.identity );

                    enemyCounters[spawnCenter][0]++;
                    currRoom = spawnCenter;
                }
                enemyCounters[spawnCenter][1]++;
            }

            
        }

    }

    public void CreateRooms()
    {
        roomList = RandomGenerationAlgs.BinarySpacePartitioning(new BoundsInt((Vector3Int)startPos, new Vector3Int(_mapSettings.dungeonWidth, _mapSettings.dungeonHeight, 0)), _mapSettings.minRoomWidth, _mapSettings.minRoomHeight);
        firstRoom = roomList.First();
        var floor = CreateSimpleRooms(roomList);
        foreach (var room in roomList)
        {
               roomCenters.Add((Vector2Int) Vector3Int.RoundToInt(room.center)); 
        }
        
        var corridors = ConnectRooms(floor, roomCenters);
        
        floor.UnionWith(corridors);

        visualizer.PaintFloor(floor);
        
        // visualizer.PaintKey(keyPos);
        visualizer.SpawnPlayer(Player, playerPos);
        
        visualizer.SpawnKey(Key, keyPos);
        visualizer.SpawnExit(Exit, exitPos);
        
        WallGenerator.CreateWalls(floor, visualizer);
        
    }



    private HashSet<Vector2Int> ConnectRooms(HashSet<Vector2Int> floor, List<Vector2Int> roomCenters)
    {
        var corridors = new HashSet<Vector2Int>();
        var currentCenter = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentCenter);

        while (roomCenters.Count > 0)
        {
            var closest = FindClosestRoom(currentCenter, roomCenters);
            roomCenters.Remove(closest);
            var newCorridor = new HashSet<Vector2Int>();
            newCorridor = CreateConnection(currentCenter , closest, floor);
            currentCenter = closest;
            corridors.UnionWith(newCorridor);
            var corridorCenter = newCorridor.ToList()[newCorridor.Count/2-1];


        }

        return corridors;
    }

    private HashSet<Vector2Int> CreateConnection(Vector2Int currentCenter, Vector2Int closest, HashSet<Vector2Int> floor)
    {
        var corridor = new HashSet<Vector2Int>();
        var position = currentCenter;
        var pos2Up = Vector2Int.zero;
        var pos2Down = Vector2Int.zero;
        corridor.Add(position);
        while (position.y != closest.y)
        {
            if(closest.y > position.y)
            {
                position += Vector2Int.up;
                
            }
            else if(closest.y < position.y)
            {
                position += Vector2Int.down;
            }
            pos2Up = position + Vector2Int.left;
            if(!floor.Contains(pos2Up))corridor.Add(pos2Up);
            pos2Up = position + Vector2Int.right;
            if(!floor.Contains(pos2Up))corridor.Add(pos2Up);

            if(!floor.Contains(position))corridor.Add(position);
            

        }
        
        while (position.x != closest.x)
        {

                if (closest.x > position.x)
                {
                    position += Vector2Int.right;
                }else if(closest.x < position.x)
                {
                    position += Vector2Int.left;
                }

                pos2Down = position + Vector2Int.up;
                if(!floor.Contains(pos2Down))corridor.Add(pos2Down);
                pos2Down = position + Vector2Int.down;
                if(!floor.Contains(pos2Down))corridor.Add(pos2Down);
                if(!floor.Contains(position))corridor.Add(position);
                
        }
        
        return corridor;
    }

    private Vector2Int FindClosestRoom(Vector2Int currentCenter, List<Vector2Int> roomCenters)
    {
        var closest = Vector2Int.zero;
        float minDistance = float.MaxValue;

        foreach (var pos in roomCenters)
        {
            var currentDistance = Vector2.Distance(pos, currentCenter);
            if (currentDistance<minDistance)
            {
                minDistance = currentDistance;
                closest = pos; 
            }
        }

        return closest;
        
    }

    private HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        foreach (var room in roomsList)
        {
            for (int col = _mapSettings.offset; col < room.size.x - _mapSettings.offset; col++)
            {
                for (int row = _mapSettings.offset; row < room.size.y - _mapSettings.offset; row++)
                {
                    Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
                    floor.Add(position);
                }
            }
        }

        var center = roomsList.First().center;
        playerPos = (Vector2Int) Vector3Int.RoundToInt(center);
        keyPos = KeySpawner.GetKeyPlace(playerPos, roomsList.Select(x => (Vector2Int) Vector3Int.RoundToInt(x.center)).ToList());
        exitPos =  (Vector2Int)Vector3Int.RoundToInt(new Vector3(center.x - 3, center.y));   
        return floor;
    }
    

}

[CustomEditor(typeof(BSPGennerator))]
public class LevelGeneratorEditor2 : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        //Reference to our script
        BSPGennerator levelGen = (BSPGennerator) target;

        //Only show the mapsettings UI if we have a reference set up in the editor
        if (levelGen._mapSettings != null)
        {
            Editor mapSettingEditor = CreateEditor(levelGen._mapSettings);
            mapSettingEditor.OnInspectorGUI();

            if (GUILayout.Button("Generate"))
            {
                levelGen.Clear();
                levelGen.CreateRooms();
            }

            if (GUILayout.Button("Clear"))
            {
                levelGen.Clear();
            }
        }
    }
}