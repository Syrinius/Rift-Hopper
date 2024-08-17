using System;
using System.Collections.Generic;
using UnityEngine;
using Direction = Utils.Direction;

public class ExitJunction : Junction {
    private Direction exitDirection;

    public ExitJunction(Vector2 position, Direction entryDirection, Corridor entryCorridor, Room tilemap, Direction exitDirection) : base(position, entryDirection, entryCorridor, tilemap) {
        this.exitDirection = exitDirection;
        playerCorridors[exitDirection] = new ExitCorridor(this, exitDirection);
    }
    
    public override void AddCorridor(Direction direction, Corridor corridor) {
        corridors.Add(direction, corridor);
        playerCorridors.Add(direction, corridor);
    }

    public ExitJunction(Vector2 position, Room tilemap, Direction exitDirection) : base(position, tilemap) {
        this.exitDirection = exitDirection;
        playerCorridors[exitDirection] = new ExitCorridor(this, exitDirection);
    }
    
    private Dictionary<Direction, Corridor> playerCorridors = new Dictionary<Direction, Corridor>();
    
    public override Dictionary<Direction, Corridor> GetCorridorsForPlayer() {
        return playerCorridors;
    }
    
    public override Dictionary<Direction, Corridor> GetCorridorsForEnemy() {
        return corridors;
    }
    
    public override void Populate(Room.TileLook[,] map) {
        foreach (Direction direction in Utils.GetNonEmptyDirections()) {
            if (exitDirection != direction && !corridors.ContainsKey(direction)) {
                Vector2 offset = Utils.getDirectionVector2(direction);
                if (map[(int)Math.Round(position.x + offset.x), (int)Math.Round(position.y + offset.y)] == Room.TileLook.CORRIDOR) {
                    tilemap.CorridorWalker(direction, this, map);
                }
            }
        }
    }
}