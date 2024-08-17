using UnityEngine;
using Direction = Utils.Direction;

public class Player : MonoBehaviour {

    public float deadZone = 0.25f;
    
    private GameMaster gameMaster;
    
    private Utils.Direction inputDirection;

    private Corridor startingCorridor;

    public static float baseSpeed = 5f;
    
    private Animator animator;
    
    private void Awake() {
        animator = GetComponent<Animator>();
    }

    public void SetStartingCorridor(Corridor corridor) {
        startingCorridor = corridor;
    }

    private PathUser pathUser = new PathUser(baseSpeed, 
        (PathUser user, Junction at, out Corridor corridor, out Utils.Direction entryDirection) => 
            GetNextCorridor(user.currentMovementDirection, user.lastInputDirection, at, out corridor, out entryDirection));
    
    public static bool GetNextCorridor(Direction towards, Direction desired, Junction at, out Corridor corridor, out Direction enter) {
        if (at.GetCorridorsForPlayer().ContainsKey(desired & ~towards)) {
            corridor = at.GetCorridorsForPlayer()[desired & ~towards];
            enter = desired & ~towards;
            return true;
        }
        if (at.GetCorridorsForPlayer().ContainsKey(towards)) {
            corridor = at.GetCorridorsForPlayer()[towards];
            enter = towards;
            return true;
        }

        corridor = null;
        enter = Direction.Empty;
        return false;
    }

    public void Die() {
        transform.position = startingCorridor.junctionStart.position;
        pathUser.SetCorridor(startingCorridor, 1, true);
        gameMaster.Pause();
    }
    
    public Corridor GetCorridor() {
        return pathUser.corridor;
    }

    public void SetCorridor(Corridor corridor, int nodeIndex, bool startToEnd) {
        pathUser.SetCorridor(corridor, nodeIndex, startToEnd);
    }

    public Junction GetForwardJunction() {
        return pathUser.startToEnd ? pathUser.corridor.junctionEnd : pathUser.corridor.junctionStart;
    }
    void Update() {
        if (Time.timeScale == 0) return;
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        inputDirection = horizontal >= deadZone ? Utils.Direction.East : horizontal <= -deadZone ? Utils.Direction.West : Utils.Direction.Empty;
        inputDirection |= vertical >= deadZone ? Utils.Direction.North : vertical <= -deadZone ? Utils.Direction.South : Utils.Direction.Empty;

        if (inputDirection != Utils.Direction.Empty) {
            pathUser.lastInputDirection = inputDirection;
        }
        
        //TODO stop input

        if (pathUser.currentMovementDirection != Utils.Direction.Empty && pathUser.lastInputDirection.HasFlag(Utils.FlipDirection(pathUser.currentMovementDirection))) {
            pathUser.ReverseMovement();
        }
        pathUser.Move(transform);
        if (pathUser.corridor.GetType() == typeof(ExitCorridor)) {
            gameMaster.RoomTransition(pathUser.currentMovementDirection);
        }
        switch (pathUser.currentMovementDirection) {
            case Direction.West: animator.SetTrigger("left");
                break;
            case Direction.North: animator.SetTrigger("up");
                break;
            case Direction.South: animator.SetTrigger("down");
                break;
            case Direction.East: animator.SetTrigger("right");
                break;
        }
    }

    public void SetStart(Corridor corridor, GameMaster gameMaster) {
        this.gameMaster = gameMaster;
        startingCorridor = corridor;
        pathUser.SetCorridor(corridor, 1, true);
    }
}