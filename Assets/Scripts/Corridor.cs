using System;
using System.Collections.Generic;
using UnityEngine;

public class Corridor {
    public Corridor(Junction junctionStart, Utils.Direction startDirection) {
        this.junctionStart = junctionStart;
        this.startDirection = startDirection;
        nodes.Add(junctionStart.position);
    }
        
    public Junction junctionStart;
    public Junction junctionEnd;

    public Utils.Direction startDirection;
    public Utils.Direction endDirection;

    public Junction GetOtherJunction(Junction junction) {
        return junction == junctionStart ? junctionEnd : junction == junctionEnd ? junctionStart : throw new Exception($"Corridor does not contain {junction}");
    }

    public void End(Junction endingJunction, Utils.Direction endDirection) {
        junctionEnd = endingJunction;
        this.endDirection = endDirection;
        nodes.Add(endingJunction.position);
    }
    public List<Vector2> nodes = new List<Vector2>();
    public int length = 1;
    public override string ToString() {
        return $"Corridor starting at {junctionStart}, ending at {junctionEnd}, length {length}";
    }
}