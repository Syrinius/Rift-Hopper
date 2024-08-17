using System;
using System.Collections.Generic;
using UnityEngine;
using Direction = Utils.Direction;

public class InnerJunction : Junction {
    
    public InnerJunction(Vector2 position, Direction entryDirection, Corridor entryCorridor, Room tilemap) : base(position, entryDirection, entryCorridor, tilemap) {}

    public InnerJunction(Vector2 position, Room tilemap) : base(position, tilemap) {}

    public override Dictionary<Direction, Corridor> GetCorridorsForPlayer() {
        return corridors;
    }
    
    public override void AddCorridor(Direction direction, Corridor corridor) {
        corridors.Add(direction, corridor);
    }

    public override Dictionary<Direction, Corridor> GetCorridorsForEnemy() {
        return corridors;
    }
    
    public override void Populate(Room.TileLook[,] map) {
        foreach (Direction direction in Utils.GetNonEmptyDirections()) {
            if (!corridors.ContainsKey(direction)) {
                Vector2 offset = Utils.getDirectionVector2(direction);
                if (map[(int)Math.Round(position.x + offset.x), (int)Math.Round(position.y + offset.y)] == Room.TileLook.CORRIDOR) {
                    tilemap.CorridorWalker(direction, this, map);
                }
            }
        }
    }
}