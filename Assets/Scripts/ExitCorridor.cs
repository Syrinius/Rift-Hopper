public class ExitCorridor : Corridor {

    public ExitCorridor(Junction junctionStart, Utils.Direction startDirection) : base(junctionStart, startDirection) {
        nodes.Add(2 * Utils.getDirectionVector2(startDirection) + junctionStart.position);
    }
}