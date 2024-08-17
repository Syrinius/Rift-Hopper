using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public static class Utils {
    [Flags]
    public enum Direction {
        Empty = 0,
        North = 1,
        East = 2,
        South = 4,
        West = 8
    }

    public static Vector2 getDirectionVector2(Direction direction) {
        switch (direction) {
            case Direction.North:
                return new Vector2(0, 1);
            case Direction.East:
                return new Vector2(1, 0);
            case Direction.South:
                return new Vector2(0, -1);
            case Direction.West:
                return new Vector2(-1, 0);
        }
        throw new Exception("Can't get direction vector for empty direction!");
    }

    private static Random random = new Random();

    public static bool RNG() {
        return random.Next() == 1;
    }
    
    public static int RNG(int maxValue) {
        return random.Next(maxValue);
    }
    
    public static double RNGDouble() {
        return random.NextDouble();
    }
    
    public static int RNG(int minValue, int maxValue) {
        return random.Next(minValue, maxValue);
    }

    public static T RNG<T>(ICollection<T> collection) {
        return collection.ElementAt(RNG(collection.Count));
    }

    public static Direction Flip(this Direction toFlip) {
        return FlipDirection(toFlip);
    }

    public static Direction FlipDirection(Direction toFlip) {
        switch (toFlip) {
            case Direction.North:
                return Direction.South;
            case Direction.East:
                return Direction.West;
            case Direction.South:
                return Direction.North;
            case Direction.West:
                return Direction.East;
        }
        return Direction.Empty;
    }

    public static T MinElement<T>(this IEnumerable<T> collection, Func<T, int> predicate) {
        int minimumValue = Int32.MaxValue;
        T toReturn = collection.First();
        foreach (T element in collection) {
            int currentValue = predicate.Invoke(element);
            if (minimumValue > currentValue) {
                minimumValue = currentValue;
                toReturn = element;
            }
        }
        return toReturn;
    }

    public static HashSet<Direction> GetNonEmptyDirections() {
        return nonEmptyDirections;
    }

    private static HashSet<Direction> nonEmptyDirections = new HashSet<Direction>
        {Direction.East, Direction.North, Direction.South, Direction.West};

    private static int token = 0;
    
    /**
     * Only to be used by enemies, since it uses a corridor list that excludes room exits
     */
    public static int GetShortestPath(Junction at, Junction target, out Corridor instruction, out Direction entryDirection) {
        token++;
        instruction = null;
        entryDirection = Direction.Empty;
        if (at == target) {
            return 0;
        }
        HashSet<Junction> toCheck = new HashSet<Junction>();
        target.SetShortestPathToStart(token, 0);
        toCheck.Add(target);
        do {
            Junction beingChecked = toCheck.MinElement(junction => junction.GetShortestPathToStart(token));
            toCheck.Remove(beingChecked);
            foreach (Corridor corridor in beingChecked.GetCorridorsForEnemy().Values) {
                Junction connectedJunction = corridor.GetOtherJunction(beingChecked);
                if (connectedJunction.GetIsFullyVisited(token)) {
                    continue;
                }
                if (beingChecked.GetShortestPathToStart(token) + corridor.length < connectedJunction.GetShortestPathToStart(token)) {
                    connectedJunction.SetShortestPathToStart(token, beingChecked.GetShortestPathToStart(token) + corridor.length);
                    if (connectedJunction == at) {
                        instruction = corridor;
                        entryDirection = at == corridor.junctionStart ? corridor.startDirection : corridor.endDirection;
                    }
                    toCheck.Add(connectedJunction);
                }
            }
            beingChecked.SetIsFullyVisited(token);
            if (beingChecked == at) {
                if (instruction == null || entryDirection == Direction.Empty) {
                    throw new Exception("Failed to set instruction");
                }
                return at.GetShortestPathToStart(token);
            }
        } while (toCheck.Any());
        throw new Exception($"Failed to find path from {at} to {target}");
    }
    
    
}