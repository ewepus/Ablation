using System.Collections.Generic;
using System.Xml.Serialization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour {
    enum gridSpace { empty, floor, wall };
    gridSpace[,] grid;
    int roomHeight, roomWidth;
    [SerializeField] Vector2 roomSizeWorldUnits = new Vector2(30, 30);
    float worldUnitsInOneGridCell = 1;
    struct walker {
        public Vector2 dir;
        public Vector2 pos;
    }
    List<walker> walkers;
    [SerializeField] float chanceWalkerChangeDir = 0.5f, chanceWalkerSpawn = 0.05f;
    [SerializeField] float chanceWalkerDestroy = 0.05f;
    [SerializeField] int amountOfStartingWalkers = 1;
    [SerializeField] int maxWalkers = 10;
    [SerializeField] float percentToFill = 0.2f;
    [SerializeField] Tile wallTile;
    [SerializeField] RuleTile floorRuleTile;
    [SerializeField] Tilemap tilemapFloor;
    [SerializeField] Tilemap tilemapWalls;


    private void Start() {
        Setup();
        CreateFloors();
        CreateWalls();
        //RemoveSingleWalls();
        SpawnLevel();
    }

    private void Setup() {
        //find grid size
        roomHeight = Mathf.RoundToInt(roomSizeWorldUnits.x / worldUnitsInOneGridCell);
        roomWidth = Mathf.RoundToInt(roomSizeWorldUnits.y / worldUnitsInOneGridCell);

        //create grid and set it's size
        grid = new gridSpace[roomWidth, roomHeight];

        //set grid's default state
        for (int x = 0; x < roomWidth - 1; x++) {
            for (int y = 0; y < roomHeight - 1; y++) {
                //make every cell in the grid 'empty'
                grid[x, y] = gridSpace.empty;
            }
        }

        //set first walker
        //init list
        walkers = new List<walker>();

        for (int i = 0; i < amountOfStartingWalkers; i++) {
            //create a walker
            walker walker = new walker();
            walker.dir = RandomDirection();

            //find center of the grid
            Vector2 spawnPosition = new Vector2(Mathf.RoundToInt(roomWidth / 2f),
                                                Mathf.RoundToInt(roomHeight / 2f));
            walker.pos = spawnPosition;
            walkers.Add(walker);
        }
    }

    private Vector2 RandomDirection() {
        int choice = Mathf.FloorToInt(Random.value * 3.99f);
        switch (choice) {
            case 0:
                return Vector2.down;
            case 1:
                return Vector2.left;
            case 2:
                return Vector2.up;
            default:
                return Vector2.right;
        }
    }

    private void CreateFloors() {
        int iterations = 0;
        do {
            //create floor at position of every walker
            foreach (walker walker in walkers) {
                grid[(int)walker.pos.x, (int)walker.pos.y] = gridSpace.floor;
            }

            //move walkers
            for (int i = 0; i < walkers.Count; i++) {
                walker walker = walkers[i];
                walker.pos += walker.dir;
                walkers[i] = walker;
            }

            //chance: spawn a new walker at the position of an existing walker
            int numberChecks = walkers.Count;
            for (int i = 0; i < numberChecks; i++) {
                //only if number of walkers doesn't exceed the limit and at a low chance
                if (Random.value < chanceWalkerSpawn && walkers.Count < maxWalkers) {
                    //create a walker
                    walker walker = new walker();
                    walker.dir = RandomDirection();
                    walker.pos = walkers[i].pos;
                    walkers.Add(walker);
                }
            }

            //chance: destroy walker
            numberChecks = walkers.Count; //number of walkers will change
            for (int i = 0; i < numberChecks; i++) {
                //destroy if it's not the only walker and at a low chance
                if (Random.value < chanceWalkerDestroy && walkers.Count > 1) {
                    walkers.RemoveAt(i);
                    break; //destroy only one per iteration
                }
            }

            //chance: walker will pick new direction
            for (int i = 0; i < walkers.Count; i++) {
                if (Random.value < chanceWalkerChangeDir) {
                    walker walker = walkers[i];
                    walker.dir = RandomDirection();
                    walkers[i] = walker;
                }
            }

            //avoid border of the grid
            for (int i = 0; i < walkers.Count; i++) {
                walker walker = walkers[i];

                //clamp x,y to leave a 1 space border: leave room for walls
                walker.pos.x = Mathf.Clamp(walker.pos.x, 1, roomWidth - 2);
                walker.pos.y = Mathf.Clamp(walker.pos.y, 1, roomHeight - 2);
                walkers[i] = walker;
            }

            //check to exit the loop
            if ((float)NumberOfFloors() / (float)grid.Length > percentToFill) {
                break;
            }

            iterations++;

        } while (iterations < 100000); //100K
    }

    private int NumberOfFloors() {
        int count = 0;

        foreach (gridSpace space in grid) {
            if (space == gridSpace.floor) {
                count++;
            }
        }

        return count;
    }

    private void SpawnLevel() {
        for (int x = 0; x < roomWidth; x++) {
            for (int y = 0; y < roomHeight; y++) {
                switch (grid[x, y]) {
                    case gridSpace.empty:
                        break;
                    case gridSpace.floor:
                        SpawnRuleTile(x, y, floorRuleTile, tilemapFloor);
                        break;
                    case gridSpace.wall:
                        SpawnTile(x, y, wallTile, tilemapWalls);
                        break;
                }
            }
        }
    }

    private void SpawnTile(int x, int y, Tile tile, Tilemap tilemap) {
        //find the position to spawn
        //offset lets center the grid in the scene
        Vector2 offset = roomSizeWorldUnits / 2.0f;
        Vector2 spawnPosition = new Vector2(x, y) * worldUnitsInOneGridCell - offset;
        Vector3Int vector3Int = new Vector3Int((int)spawnPosition.x, (int)spawnPosition.y);

        //spawn an object!
        tilemap.SetTile(vector3Int, tile);
    }
    private void SpawnRuleTile(int x, int y, RuleTile ruleTile, Tilemap tilemap) {
        Vector2 offset = roomSizeWorldUnits / 2.0f;
        Vector2 spawnPosition = new Vector2(x, y) * worldUnitsInOneGridCell - offset;
        Vector3Int vector3Int = new Vector3Int((int)spawnPosition.x, (int)spawnPosition.y);

        tilemap.SetTile(vector3Int, ruleTile);
    }

    private void CreateWalls() {
        //loop through every grid space
        for (int x = 0; x < roomWidth - 1; x++) {
            for (int y = 0; y < roomHeight - 1; y++) {
                //if the space is a floor, check surrounding spaces
                if (grid[x, y] == gridSpace.floor) {
                    //if a surrounding space is empty, make it a wall
                    if (grid[x, y + 1] == gridSpace.empty) {
                        grid[x, y + 1] = gridSpace.wall;
                    }
                    if (grid[x, y - 1] == gridSpace.empty) {
                        grid[x, y - 1] = gridSpace.wall;
                    }
                    if (grid[x + 1, y] == gridSpace.empty) {
                        grid[x + 1, y] = gridSpace.wall;
                    }
                    if (grid[x - 1, y] == gridSpace.empty) {
                        grid[x - 1, y] = gridSpace.wall;
                    }
                }
            }
        }
    }
}
