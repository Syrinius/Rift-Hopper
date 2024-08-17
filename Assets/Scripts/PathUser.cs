using UnityEngine;

public class PathUser {
    
    public float speed;
    public Corridor corridor;
    public Utils.Direction lastInputDirection = Utils.Direction.Empty;
    public Utils.Direction currentMovementDirection = Utils.Direction.Empty;
    public int nodeIndex;
    public bool startToEnd;
    private GetCorridor getCorridor;

    public void Config(float speed, GetCorridor getCorridor) {
        this.speed = speed;
        this.getCorridor = getCorridor;
    }

    public void ReverseMovement() {
        if (startToEnd) {
            nodeIndex--;
            startToEnd = false;
        }
        else {
            nodeIndex++;
            startToEnd = true;
        }
    }

    public delegate bool GetCorridor(PathUser user, Junction at, out Corridor corridor, out Utils.Direction entryDirection);
    
    public PathUser(float speed, GetCorridor getCorridor) {
        this.speed = speed;
        this.getCorridor = getCorridor;
    }

    public void SetCorridor(Corridor corridor, int nodeIndex, bool startToEnd) {
        this.corridor = corridor;
        this.nodeIndex = nodeIndex;
        this.startToEnd = startToEnd;
    }

    public static float DirectionAndMagnitude(Vector2 from, Vector2 to, out Utils.Direction direction) {
        Vector2 difference = to - from;
        direction = 
            difference.x > 0 ? Utils.Direction.East :
            difference.x < 0 ? Utils.Direction.West :
            difference.y > 0 ? Utils.Direction.North :
            difference.y < 0 ? Utils.Direction.South : Utils.Direction.Empty;
        return difference.magnitude;
    }

    public void Move(Transform transform) {
        float remainingMove = Time.deltaTime * speed;
        while (remainingMove > 0) {
            Vector2 nextNode = corridor.nodes[nodeIndex];
            Vector2 thisPosition = transform.position;
            float distanceToNode = DirectionAndMagnitude(thisPosition, nextNode, out currentMovementDirection);
            Vector2 newPosition;
            float alpha;
            if (distanceToNode == 0) {
                alpha = 1;
                newPosition = thisPosition;
            }
            else {
                newPosition = Vector2.Lerp(thisPosition, nextNode, alpha = remainingMove / distanceToNode);
            }

            if (alpha >= 1) {
                Junction junction;
                bool endReached;
                if (startToEnd) {
                    junction = corridor.junctionEnd;
                    endReached = nodeIndex + 1 == corridor.nodes.Count;
                    if (!endReached) {
                        nodeIndex++; //possibly not valid anymore
                    }
                    else nodeIndex = corridor.nodes.Count - 1;
                }
                else {
                    junction = corridor.junctionStart;
                    endReached = nodeIndex == 0;
                    if (!endReached) {
                        nodeIndex--; //possibly not valid anymore
                    }
                    else nodeIndex = 0;
                }

                //Junctions
                if (endReached) {
                    if (getCorridor.Invoke(this, junction, out Corridor nextCorridor, out currentMovementDirection)) {
                        corridor = nextCorridor;
                        if (nextCorridor.junctionStart.position == newPosition && currentMovementDirection == nextCorridor.startDirection) {
                            nodeIndex = 1;
                            startToEnd = true;
                        }
                        else {
                            nodeIndex = nextCorridor.nodes.Count - 2;
                            startToEnd = false;
                        }
                    }
                    else {
                        transform.position = newPosition;
                        return;
                    }
                }

                lastInputDirection = Utils.Direction.Empty;
            }

            if (newPosition != thisPosition) {
                remainingMove -= (1 / alpha) * remainingMove;
            }

            if (remainingMove < 0.001) remainingMove = 0;
            transform.position = newPosition;
        }
    }
}