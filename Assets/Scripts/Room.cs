using System;
using System.Collections.Generic;
using System.Linq;
using TileType;
using UnityEngine;
using UnityEngine.Tilemaps;
using Tile = TileType.Tile;

public class Room : MonoBehaviour {
    
    private Tilemap tilemap;
    public HashSet<Enemy> enemyInstances = new HashSet<Enemy>();
    public Vector3 enemyInHiveStartPosition = new Vector3(17, 7, -10); //TODO do the encapsulation correctly
    public HashSet<Enemy> enemiesInHive = new HashSet<Enemy>();
    public float activeEnemySendOutCooldown = 0; //TODO revamp timer system
    public int coinsPickedUp;
    
    public Dictionary<Utils.Direction, Room> neighbors =
        new Dictionary<Utils.Direction, Room>(4);
    
    public enum TileLook {
        WALL, CORRIDOR, HIVE_ENTRANCE, HIVE_INSIDE
    }

    public TileLook[,] FirstPass(GameMaster gameMaster, MapData mapData) {
        TileLook[,] data = GenerateMap();
        
        tilemap = GetComponent<Tilemap>();
        tilemap.ClearAllTiles();
        return data;
    }

    public void SecondPass(TileLook[,] data, GameMaster gameMaster, MapData mapData) {
        for (int y = 0; y < data.GetLength(1); y++) {
            for (int x = 0; x < data.GetLength(0); x++) {
                if (data[x,y] == TileLook.WALL) {
                    tilemap.SetTile(new Vector3Int(x,y,0), mapData.wallTile);
                }
                else if (data[x,y] == TileLook.CORRIDOR || data[x,y] == TileLook.HIVE_INSIDE) {
                    tilemap.SetTile(new Vector3Int(x,y,0), mapData.pathTile);
                    if ((x == 1 || x == 17 || x == data.GetLength(0) - 2) && (y == 1 || y == data.GetLength(1) - 2)) {
                        PowerUp powerUpInstance = Instantiate(mapData.powerUpPrefab, new Vector3(x, y, -1) + transform.position, Quaternion.identity, transform);
                        powerUpInstance.name = $"PowerUp at {x}:{y}";
                        powerUpInstance.gameMaster = gameMaster;
                    } else if (!(x <= 20 && x >= 13 && y <= 10 && y >= 4) && x > 0 && x < 33 && y > 0 && y < 17) {
                        Coin instance = Instantiate(mapData.coinPrefab, new Vector3(x, y, -1) + transform.position, Quaternion.identity, transform);
                        instance.name = $"Coin at {x}:{y}";
                        gameMaster.coinCounter++;
                        instance.gameMaster = gameMaster;
                    }
                } else if (data[x, y] == TileLook.HIVE_ENTRANCE) {
                    tilemap.SetTile(new Vector3Int(x, y, 0), mapData.gateTile);
                }
            }
        }
        
        GenerateJunctionsAndCorridors(data);
        GenerateEnemies(gameMaster, mapData.enemyPrefabs);
    }

    private void GenerateEnemies(GameMaster gameMaster, List<Enemy> enemyPrefabs) {
        enemyInstances.Add(Instantiate(enemyPrefabs[0], enemyInHiveStartPosition + transform.position, Quaternion.identity, transform).SetupEnemy(gameMaster, this));
        enemyInstances.Add(Instantiate(enemyPrefabs[1], enemyInHiveStartPosition + transform.position, Quaternion.identity, transform).SetupEnemy(gameMaster, this));
        enemyInstances.Add(Instantiate(enemyPrefabs[1], enemyInHiveStartPosition + transform.position, Quaternion.identity, transform).SetupEnemy(gameMaster, this));
        enemyInstances.Add(Instantiate(enemyPrefabs[2], enemyInHiveStartPosition + transform.position, Quaternion.identity, transform).SetupEnemy(gameMaster, this));
        enemiesInHive.UnionWith(enemyInstances);
    }

    TileLook[,] GenerateMap(int firstColumn = 0, int lastColumn = 16, int firstRow = 0, int lastRow = 17, int hiveBottomRow = 5, int hiveTopRow = 9) {
        TileGenerationData[,] generationData = new TileGenerationData[lastColumn - firstColumn + 1, lastRow - firstRow + 1];
        
        //first column iteration
        
        for (int y = 0; y <= lastRow; y++) {
            generationData[0, y] = new TileGenerationData(2, Tile.wall);
        }
        
        //first row and last row iteration
        
        for (int x = 0; x <= lastColumn; x++) {
            generationData[x, 0] = new TileGenerationData(5, Tile.wall);
            generationData[x, generationData.GetLength(1) - 1] = new TileGenerationData(5, Tile.wall);
        }
        
       
        //other columns iteration
        
        int tileContinuationCount = 0;
        ITileBase currentContinuationType = Tile.wall;
        ITileBase currentTiletype = Tile.wall;
        int tileWeight;
        int columnWeight = 2;

        for (int x = 1; x <= lastColumn; x++) {
            
            //decisions for first row

            tileWeight = 2;
            generationData[x - 1, 1].tileType.Decision(generationData, tileWeight, x - 1, 1, generationData.GetLength(1) - 1, generationData.GetLength(0) - 1, out currentContinuationType, out tileContinuationCount);
            tileWeight = (currentContinuationType == Tile.wall) ? tileWeight + 1 : 0;
            currentTiletype = currentContinuationType;
            

            for (int y = 1; y <= lastRow - 2; y++) {
                
                //main magic is done here

                if (tileContinuationCount != 0) {
                    tileContinuationCount--;
                    if (tileWeight == 0) {
                        currentContinuationType = (generationData[x - 1, y + 1].weight == 0) ? (ITileBase) Tile.noWallConnection : Tile.horizontalWallConnection;
                    }
                }
                else {
                    generationData[x - 1, y + 1].tileType.Decision(generationData, tileWeight, x - 1, y + 1, generationData.GetLength(1) - 1, generationData.GetLength(0) - 1, out currentContinuationType, out tileContinuationCount);
                }
                
                //increment weight counters

                columnWeight = (currentTiletype == Tile.wall) ? generationData[x - 1, y].weight + 1 : 0;
                tileWeight = (currentContinuationType == Tile.wall) ? tileWeight + 1 : 0;

                //this is where a new generation data is finalized and created
                
                generationData[x, y] = new TileGenerationData(columnWeight, currentTiletype.Finalize(tileWeight));
                currentTiletype = currentContinuationType;
                
            }
            
            //finalization of last row since decision must be skipped
            tileContinuationCount = 0;
            columnWeight = (currentContinuationType == Tile.wall) ? generationData[x - 1, generationData.GetLength(1) - 2].weight + 1 : 0;
            generationData[x, lastRow - firstRow - 1] = new TileGenerationData(columnWeight, currentTiletype.Finalize(2));
        }
        
        //hive iteration

        for (int y = hiveBottomRow; y <= hiveTopRow; y++) {
            generationData[lastColumn - 1, y] = new TileGenerationData(8, Tile.wall);
        }
        
        generationData[lastColumn, hiveBottomRow] = new TileGenerationData(8, Tile.wall);
        
        for (int y = hiveBottomRow + 1; y <= hiveTopRow - 1; y++) {
            generationData[lastColumn, y] = new TileGenerationData(8, Tile.hiveTile);
        }
        
        generationData[lastColumn, hiveTopRow] = new TileGenerationData(8, Tile.hiveEntrance);
        
        for (int x = lastColumn - 2; x <= lastColumn; x++) {
            generationData[x, hiveBottomRow - 1] = new TileGenerationData(0, Tile.doubleVerticalWallConnection);
            generationData[x, hiveTopRow + 1] = new TileGenerationData(0, Tile.doubleVerticalWallConnection);
        }
        
        generationData[lastColumn - 2, hiveBottomRow + 2] = new TileGenerationData(8, Tile.wall);

        if (generationData[lastColumn - 3, hiveBottomRow].isWall) {
            for (int y = hiveBottomRow - 1; y <= hiveBottomRow + 2; y++) {
                generationData[lastColumn - 2, y] = new TileGenerationData(0, Tile.horizontalWallConnection);
            }
        }
        
        if (generationData[lastColumn - 3, hiveTopRow].isWall) {
            for (int y = hiveTopRow - 2; y <= hiveTopRow + 1; y++) {
                generationData[lastColumn - 2, y] = new TileGenerationData(0, Tile.horizontalWallConnection);
            }
        }

        int width = generationData.GetLength(0) * 2;
        TileLook[,] toReturn = new TileLook[width, generationData.GetLength(1)];
        for (int x = 0; x < generationData.GetLength(0); x++) {
            for (int y = 0; y < generationData.GetLength(1); y++) {
                ITileBase type = generationData[x, y].tileType;
                if (type == Tile.wall) {
                    toReturn[x, y] = TileLook.WALL;
                    toReturn[width - x - 1, y] = TileLook.WALL;
                } else if (type == Tile.hiveEntrance) {
                    toReturn[x, y] = TileLook.HIVE_ENTRANCE;
                    toReturn[width - x - 1, y] = TileLook.HIVE_ENTRANCE;
                } else if (type == Tile.hiveTile) {
                    toReturn[x, y] = TileLook.HIVE_INSIDE;
                    toReturn[width - x - 1, y] = TileLook.HIVE_INSIDE;
                } else {
                    toReturn[x, y] = TileLook.CORRIDOR;
                    toReturn[width - x - 1, y] = TileLook.CORRIDOR;
                }
            }
        }
        
        return toReturn;
    }

    public struct TileGenerationData {
        public readonly int weight;
        
        public readonly ITileBase tileType;

        public TileGenerationData(int weight, ITileBase tileType) {
            //if (tileType == Tile.wall && weight == 0) Debug.Log($"weight: {weight}, type: {tileType}");
            //else if (tileType != Tile.wall && weight != 0) Debug.Log($"weight: {weight}, type: {tileType}");
            this.weight = weight;
            this.tileType = tileType;
        }

        public bool isWall => tileType == Tile.wall;
    }
    
    List<Corridor> corridors;
    public Dictionary<Vector2, Junction> junctions;
    public Segment enemyStartingSegment;

    public Corridor GetStartPosition() {
        return corridors.First();
    }

    Vector2 FindJunctionPosition(TileLook[,] map, Vector2 position, Utils.Direction entryDirection, out Utils.Direction lastDirection, out Utils.Direction exits) {
        while (LookAhead(map, position, entryDirection, out exits) == 1) {
            if (exits.HasFlag(Utils.Direction.North)) {
                position += Vector2.up;
            }
            else if (exits.HasFlag(Utils.Direction.East)) {
                position += Vector2.right;
            }
            else if (exits.HasFlag(Utils.Direction.South)) {
                position += Vector2.down;
            }
            else if (exits.HasFlag(Utils.Direction.West)) {
                position += Vector2.left;
            }
            entryDirection = Utils.FlipDirection(exits);
        }
        lastDirection = entryDirection;
        return position;
    }
    
    public struct Segment {
        public int node;
        public Corridor corridor;
        
        public Segment(int node, Corridor corridor) {
            this.node = node;
            this.corridor = corridor;
        }
    }

    void GenerateJunctionsAndCorridors(TileLook[,] map) {
        junctions = new Dictionary<Vector2, Junction>();
        corridors = new List<Corridor>();
        Vector2 position = FindJunctionPosition(map, new Vector2(1, 2), Utils.Direction.South, out _, out Utils.Direction exits);
        if ((int)Math.Round(position.x) == 1 && exits.HasFlag(Utils.Direction.West)) {
            new ExitJunction(position,  this, Utils.Direction.West).Populate(map);
        } else if ((int)Math.Round(position.x) == map.GetLength(0) - 2 && exits.HasFlag(Utils.Direction.East)) {
            new ExitJunction(position,  this, Utils.Direction.East).Populate(map);
        } else if ((int)Math.Round(position.y) == 1 && exits.HasFlag(Utils.Direction.South)) {
            new ExitJunction(position,  this, Utils.Direction.South).Populate(map);
        } else if ((int)Math.Round(position.y) == map.GetLength(1) - 2 && exits.HasFlag(Utils.Direction.North)) {
            new ExitJunction(position,  this, Utils.Direction.North).Populate(map);
        }else {
            new InnerJunction(position, this).Populate(map);
        }

        GenerateEnemyStartingPosition(map);
        /*foreach (Corridor corridor in corridors) {
            Debug.Log(corridor);
        }

        foreach (KeyValuePair<Vector2,Junction> junction in junctions) {
            Debug.Log(junction.Value);
        }*/
    }

    void GenerateEnemyStartingPosition(TileLook[,] map) {
        Utils.Direction lastDirection;
        Vector2 enemyStartPosition = new Vector2(17, 10);
        Corridor enemyStartCorridor = junctions[FindJunctionPosition(map, enemyStartPosition, Utils.Direction.East, out lastDirection, out _)].GetCorridorsForEnemy()[lastDirection];
        for (int index = 0; index < enemyStartCorridor.nodes.Count; index++) {
            if (index == enemyStartCorridor.nodes.Count - 1)
                throw new Exception("Unable to find starting position for enemies");
            if (enemyStartCorridor.nodes[index].x <= enemyStartPosition.x &&
                enemyStartCorridor.nodes[index + 1].x >= enemyStartPosition.x || 
                enemyStartCorridor.nodes[index].x >= enemyStartPosition.x &&
                enemyStartCorridor.nodes[index + 1].x <= enemyStartPosition.x) {
                enemyStartingSegment = new Segment(index + 1, enemyStartCorridor);
                break;
            }
        }
    }

    internal void CorridorWalker(Utils.Direction entryDirection, Junction origin, TileLook[,] map) {
        Corridor corridor = new Corridor(origin, entryDirection);
        origin.AddCorridor(entryDirection, corridor);
        Utils.Direction exits;
        Vector2 position = origin.position;
        //Debug.Log($"1 position: {position}, entryDirection: {entryDirection}");
        switch (entryDirection) {
            case Utils.Direction.North:
                position += Vector2.up;
                break;
            case Utils.Direction.East:
                position += Vector2.right;
                break;
            case Utils.Direction.South:
                position += Vector2.down;
                break;
            case Utils.Direction.West:
                position += Vector2.left;
                break;
        }
        entryDirection = Utils.FlipDirection(entryDirection);
        //Debug.Log($"2 position: {position}, entryDirection: {entryDirection}");
        while (LookAhead(map, position, entryDirection, out exits) == 1) {
            if (exits.HasFlag(Utils.Direction.North)) {
                if (entryDirection != Utils.Direction.South ) {
                    corridor.nodes.Add(position);
                }
                position += Vector2.up;
            } else if (exits.HasFlag(Utils.Direction.East)) {
                if (entryDirection != Utils.Direction.West ) {
                    corridor.nodes.Add(position);
                }
                position += Vector2.right;
            } else if (exits.HasFlag(Utils.Direction.South)) {
                if (entryDirection != Utils.Direction.North ) {
                    corridor.nodes.Add(position);
                }
                position += Vector2.down;
            } else if (exits.HasFlag(Utils.Direction.West)) {
                if (entryDirection != Utils.Direction.East ) {
                    corridor.nodes.Add(position);
                }
                position += Vector2.left;
            }
            corridor.length++;
            entryDirection = Utils.FlipDirection(exits);
            //Debug.Log($"3 position: {position}, entryDirection: {entryDirection}");
        }

        if (!junctions.TryGetValue(position, out Junction end)) {
            //Debug.Log("bundaa");
            if ((int)Math.Round(position.x) == 1 && exits.HasFlag(Utils.Direction.West)) {
                Junction junction = new ExitJunction(position, entryDirection, corridor, this, Utils.Direction.West);
                corridor.End(junction, entryDirection);
            } else if ((int)Math.Round(position.x) == map.GetLength(0) - 2 && exits.HasFlag(Utils.Direction.East)) {
                Junction junction = new ExitJunction(position, entryDirection, corridor, this, Utils.Direction.East);
                corridor.End(junction, entryDirection);
            } else if ((int)Math.Round(position.y) == 1 && exits.HasFlag(Utils.Direction.South)) {
                Junction junction = new ExitJunction(position, entryDirection, corridor, this, Utils.Direction.South);
                corridor.End(junction, entryDirection);
            } else if ((int)Math.Round(position.y) == map.GetLength(1) - 2 && exits.HasFlag(Utils.Direction.North)) {
                Junction junction = new ExitJunction(position, entryDirection, corridor, this, Utils.Direction.North);
                corridor.End(junction, entryDirection);
            } else {
                Junction junction = new InnerJunction(position, entryDirection, corridor, this);
                corridor.End(junction, entryDirection);
            }
        } else {
            corridor.End(end, entryDirection);
            corridor.junctionEnd.AddCorridor(entryDirection, corridor);
        }
        corridors.Add(corridor);
        corridor.junctionEnd.Populate(map);
    }

    byte LookAhead(TileLook[,] map, Vector2 position, Utils.Direction entryDirection, out Utils.Direction exits) {
        exits = Utils.Direction.Empty;
        byte toReturn = 0;
        if ((~entryDirection).HasFlag(Utils.Direction.North)) {
            if (map[(int)position.x, (int)position.y + 1] == TileLook.CORRIDOR) {
                exits |= Utils.Direction.North;
                toReturn += 1;
            }
        }
        if ((~entryDirection).HasFlag(Utils.Direction.East)) {
            if (map[(int)position.x + 1, (int)position.y] == TileLook.CORRIDOR) {
                exits |= Utils.Direction.East; 
                toReturn += 1;
            }  
        }
        if ((~entryDirection).HasFlag(Utils.Direction.South)) {
            if (map[(int)position.x, (int)position.y - 1] == TileLook.CORRIDOR) {
                exits |= Utils.Direction.South; 
                toReturn += 1;
            }
        }
        if ((~entryDirection).HasFlag(Utils.Direction.West)) {
            if (map[(int)position.x - 1, (int)position.y] == TileLook.CORRIDOR) {
                exits |= Utils.Direction.West; 
                toReturn += 1;
            }
        }
        //Debug.Log($"10 position: {position}, entryDirection: {entryDirection}, exits: {exits}, toReturn: {toReturn}");
        return toReturn;
    }
}