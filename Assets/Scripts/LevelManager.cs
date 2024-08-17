using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    
    public Room roomPrefab;
    public GameObject TempObjs;

    public Room GenerateLevel(GameMaster gameMaster, MapData mapData) {
        if (TempObjs != null) Destroy(TempObjs);
        TempObjs = new GameObject("Transient GameObjects");
        TempObjs.transform.parent = transform.parent;

        Dictionary<Vector2, Room> LevelLayoutGenerationData = new Dictionary<Vector2, Room>(); //current method only scales up to 5
        
        Room CreateRoom(Vector2 at) {
            Room toReturn = Instantiate(roomPrefab, roomPrefab.transform.position + new Vector3(at.x * 34, at.y * 18, 0), Quaternion.identity, TempObjs.transform);
            LevelLayoutGenerationData.Add(at, toReturn);
            toReturn.name = $"Room at {at}";
            return toReturn;
        }

        CreateRoom(Vector2.zero);

        List<(Vector2, Vector2, Utils.Direction)> connections = new List<(Vector2, Vector2, Utils.Direction)>();
        HashSet<Utils.Direction> directions = Utils.GetNonEmptyDirections();
        for (int roomIndex = 1; roomIndex < 4; roomIndex++) {
            Vector2 fromIndex = Utils.RNG(LevelLayoutGenerationData.Keys);
            Room from = LevelLayoutGenerationData[fromIndex];
            int toPick = Utils.RNG(3 - from.neighbors.Count);
            for (int i = 0; i < 4; i++) {
                if (from.neighbors.ContainsKey(directions.ElementAt(i))) continue;
                if (toPick-- == 0) {
                    Vector2 location = fromIndex + Utils.getDirectionVector2(directions.ElementAt(i));
                    Room to = CreateRoom(location);
                    foreach (Utils.Direction direction in directions) {
                        if (LevelLayoutGenerationData.TryGetValue(location + Utils.getDirectionVector2(direction), out Room neighbor)) {
                            neighbor.neighbors.Add(Utils.FlipDirection(direction), to);
                            to.neighbors.Add(direction, neighbor);
                            connections.Add((fromIndex, location, direction));
                        }
                    }
                }
            }
        }

        Dictionary<Vector2, TileData> datas = new Dictionary<Vector2, TileData>();
        
        foreach (var room in LevelLayoutGenerationData) {
            datas[room.Key] = new TileData {data = room.Value.FirstPass(gameMaster, mapData)};
            room.Value.gameObject.SetActive(false);
        }

        void GetConnection(Room.TileLook[,] room1Data, Room.TileLook[,] room2Data, Utils.Direction direction) {
            int width = room1Data.GetLength(0);
            int height = room1Data.GetLength(1);
            List<int> possibleConnections = new List<int>();
            int position;
            switch (direction) {
                case Utils.Direction.North:
                    for (int i = 2; i < width / 2 - 3; i++) {
                        if (room1Data[i, 1] == Room.TileLook.CORRIDOR && room2Data[i, height - 2] == Room.TileLook.CORRIDOR) {
                            possibleConnections.Add(i);
                        }
                    } 
                    position = Utils.RNG(possibleConnections);
                    room1Data[position, 0] = Room.TileLook.CORRIDOR;
                    room1Data[width - 1 - position, 0] = Room.TileLook.CORRIDOR;
                    room2Data[position, height - 1] = Room.TileLook.CORRIDOR;
                    room2Data[width - 1 - position, height - 1] = Room.TileLook.CORRIDOR;
                    break;
                case Utils.Direction.South:
                    for (int i = 2; i < width / 2 - 3; i++) {
                        if (room2Data[i, 1] == Room.TileLook.CORRIDOR && room1Data[i, height - 2] == Room.TileLook.CORRIDOR) {
                            possibleConnections.Add(i);
                        }
                    }  
                    position = Utils.RNG(possibleConnections);
                    room2Data[position, 0] = Room.TileLook.CORRIDOR;
                    room2Data[width - 1 - position, 0] = Room.TileLook.CORRIDOR;
                    room1Data[position, height - 1] = Room.TileLook.CORRIDOR;
                    room1Data[width - 1 - position, height - 1] = Room.TileLook.CORRIDOR;
                    break;
                case Utils.Direction.East:
                    for (int i = 2; i < height - 3; i++) {
                        if (room1Data[1, i] == Room.TileLook.CORRIDOR && room2Data[width - 2, i] == Room.TileLook.CORRIDOR) {
                            possibleConnections.Add(i);
                        }
                    } 
                    position = Utils.RNG(possibleConnections);
                    room1Data[0, position] = Room.TileLook.CORRIDOR;
                    room2Data[width - 1, position] = Room.TileLook.CORRIDOR;
                    break;
                case Utils.Direction.West:
                    for (int i = 2; i < height - 3; i++) {
                        if (room2Data[1, i] == Room.TileLook.CORRIDOR && room1Data[width - 2, i] == Room.TileLook.CORRIDOR) {
                            possibleConnections.Add(i);
                        }
                    }  
                    position = Utils.RNG(possibleConnections);
                    room2Data[0, position] = Room.TileLook.CORRIDOR;
                    room1Data[width - 1, position] = Room.TileLook.CORRIDOR;
                    break;
            }
        }
        

        foreach ((Vector2 room1, Vector2 room2, Utils.Direction direction) in connections) {
            GetConnection(datas[room1].data, datas[room2].data, direction);
        }
        
        foreach (var room in datas) {
            LevelLayoutGenerationData[room.Key].SecondPass(room.Value.data, gameMaster, mapData);
        }

        return LevelLayoutGenerationData[Vector2.zero];
    }
}
