using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Direction = Utils.Direction;

public abstract class Junction {
        
    public override string ToString() {
            
        return $"Junction at {position}, directions {string.Join(",", corridors.Keys.Select(x => x.ToString()).ToArray())}";
    }

    public Vector2 position;
    internal Dictionary<Direction, Corridor> corridors;
    internal Room tilemap;

    public abstract void AddCorridor(Direction direction, Corridor corridor);

    public abstract Dictionary<Direction, Corridor> GetCorridorsForPlayer();
    
    public abstract Dictionary<Direction, Corridor> GetCorridorsForEnemy();

    private int accessToken;
    private bool isFullyVisited = false;
    private int shortestPathToStart = Int32.MaxValue;

    public void SetShortestPathToStart(int accessToken, int shortestPathToStart) {
        if (accessToken != this.accessToken) {
            isFullyVisited = false;
            this.accessToken = accessToken;
        }

        this.shortestPathToStart = shortestPathToStart;
    }
        
    public int GetShortestPathToStart(int accessToken) {
        if (accessToken != this.accessToken) {
            isFullyVisited = false;
            this.accessToken = accessToken;
            shortestPathToStart = Int32.MaxValue;
        }
        return shortestPathToStart;
    }
        
    public void SetIsFullyVisited(int accessToken) {
        if (accessToken != this.accessToken) {
            shortestPathToStart = Int32.MaxValue;
            this.accessToken = accessToken;
        }
        isFullyVisited = true;
    }
        
    public bool GetIsFullyVisited(int accessToken) {
        if (accessToken != this.accessToken) {
            isFullyVisited = false;
            this.accessToken = accessToken;
            shortestPathToStart = Int32.MaxValue;
        }
        return isFullyVisited;
    }
        
    public Junction(Vector2 position, Direction entryDirection, Corridor entryCorridor, Room tilemap) : this(position, tilemap) {
        AddCorridor(entryDirection, entryCorridor);
    }
        
    public Junction(Vector2 position, Room tilemap) {
        this.position = position;
        this.tilemap = tilemap;
        corridors = new Dictionary<Direction, Corridor>(4);
        tilemap.junctions.Add(position, this);
    }

    public abstract void Populate(Room.TileLook[,] map);
}