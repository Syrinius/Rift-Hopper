using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class Enemy : MonoBehaviour {
    
    private Room room;
    private GameMaster gameMaster;
    private Junction currentTargetJunction;
    static private Vector2 startingPosition = new Vector2(17, 10);
    public float baseSpeed = 4.8f;
    public float leaveHiveSpeed = 2f;
    public float fleeSpeed = 1.5f;
    
    [ShowInInspector]
    private PathUser pathUser;
    private bool isFleeing = false;
    private bool isInsideHive = true;
    private Animator animator;

    public enum EnemyAIType { CHASE, FORWARD, RANDOM }
    public EnemyAIType aiType;

    private void Start() {
        moveMode = KinematicUpdate;
    }

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    public void EnterFleeMode() {
        if (isFleeing || isInsideHive) return;
        animator.SetBool("Power Up", true);
        pathUser.Config(fleeSpeed, GetCorridorFlee);
        pathUser.ReverseMovement();
        isFleeing = true;
    }
    
    public void LeaveFleeMode() {
        if (!isFleeing || isInsideHive) return;
        animator.SetBool("Power Up", false);
        pathUser.Config(baseSpeed, SetAI(aiType));
        pathUser.ReverseMovement();
        isFleeing = false;
    }

    bool GetCorridorFlee(PathUser user, Junction at, out Corridor corridor, out Utils.Direction entryDirection) {
        Corridor playerCorridor = gameMaster.GetPlayerCorridor();
        int corridor1Length = Utils.GetShortestPath(at, playerCorridor.junctionStart, out Corridor corridor1, out Utils.Direction entryDirection1);
        int corridor2Length = Utils.GetShortestPath(at, playerCorridor.junctionEnd, out Corridor corridor2, out Utils.Direction entryDirection2);
        if (corridor1Length == 0 || corridor2Length == 0) corridor = playerCorridor;
        else if (corridor1Length < corridor2Length) {
            corridor = corridor1;
            entryDirection = entryDirection1;
        }
        else {
            corridor = corridor2;
            entryDirection = entryDirection2;
        }
        corridor = GetFleeingPath(at, corridor, out entryDirection);
        return true;
    }

    bool GetCorridorChase(PathUser user, Junction at, out Corridor corridor, out Utils.Direction entryDirection) {
        Corridor playerCorridor = gameMaster.GetPlayerCorridor();
        int corridor1Length = Utils.GetShortestPath(at, playerCorridor.junctionStart, out Corridor corridor1, out Utils.Direction entryDirection1);
        int corridor2Length = Utils.GetShortestPath(at, playerCorridor.junctionEnd, out Corridor corridor2, out Utils.Direction entryDirection2);
        if (corridor1Length == 0 || corridor2Length == 0) {
            corridor = playerCorridor;
            entryDirection = at == playerCorridor.junctionStart ? playerCorridor.startDirection : playerCorridor.endDirection;
        }
        else if (corridor1Length < corridor2Length) {
            corridor = corridor1;
            entryDirection = entryDirection1;
        }
        else {
            corridor = corridor2;
            entryDirection = entryDirection2;
        }
        if (corridor == pathUser.corridor) {
            corridor = GetRandomPath(at, out entryDirection);
        }
        return true;
    }
    
    bool GetCorridorForward(PathUser user, Junction at, out Corridor corridor, out Utils.Direction entryDirection) {
        if (Utils.GetShortestPath(at, GetForwardJunction(), out corridor, out entryDirection) == 0 || corridor == pathUser.corridor) {
            corridor = GetRandomPath(at, out entryDirection);
        }
        return true;
    }
    
    bool GetCorridorRandom(PathUser user, Junction at, out Corridor corridor, out Utils.Direction entryDirection) {
        corridor = GetRandomPath(at, out entryDirection);
        return true;
    }

    Junction GetForwardJunction() {
        Junction junction = gameMaster.GetPlayerForwardJunction();
        int rng = Utils.RNG(junction.GetCorridorsForEnemy().Count - 2);
        foreach (var toCheck in junction.GetCorridorsForEnemy()) {
            if (toCheck.Value != gameMaster.GetPlayerCorridor() && rng-- == 0) {
                return toCheck.Value.GetOtherJunction(junction);
            }
        }
        throw new Exception("Couldn't find forward junction!");
    }

    Corridor GetRandomPath(Junction at, out Utils.Direction entryDirection) {
        int rng = Utils.RNG(at.GetCorridorsForEnemy().Count - 2);
        foreach (var toCheck in at.GetCorridorsForEnemy()) {
            if (toCheck.Value != pathUser.corridor && rng-- == 0) {
                entryDirection = toCheck.Key;
                return toCheck.Value;
            }
        }
        throw new Exception("Couldn't find random path!");
    }
    
    Corridor GetFleeingPath(Junction at, Corridor toAvoid, out Utils.Direction entryDirection) {
        if (pathUser.corridor == toAvoid || at.GetCorridorsForEnemy().Count == 2) {
            return GetRandomPath(at, out entryDirection);
        }
        int rng = Utils.RNG(at.GetCorridorsForEnemy().Count - 3);
        foreach (var toCheck in at.GetCorridorsForEnemy()) {
            if (toCheck.Value != pathUser.corridor && toCheck.Value != toAvoid && rng-- == 0) {
                entryDirection = toCheck.Key;
                return toCheck.Value;
            }
        }
        throw new Exception("Couldn't find fleeing path!");
    }

    private PathUser.GetCorridor SetAI(EnemyAIType aiType) {
        this.aiType = aiType;
        return aiType == EnemyAIType.CHASE ? GetCorridorChase : aiType == EnemyAIType.FORWARD ? GetCorridorForward : (PathUser.GetCorridor) GetCorridorRandom;
    }

    public Enemy SetupEnemy(GameMaster gameMaster, Room room) {
        pathUser = new PathUser(baseSpeed, SetAI(aiType));
        this.gameMaster = gameMaster;
        this.room = room;
        enabled = false;
        return this;
    }

    public void LeaveHive() {
        enabled = true;
        moveMode = KinematicUpdate;
        animator.SetTrigger("up");
        pathUser.currentMovementDirection = Utils.Direction.North;
        pathUser.corridor = room.enemyStartingSegment.corridor;
        if (Utils.RNG()) {
            pathUser.nodeIndex = room.enemyStartingSegment.node;
            pathUser.startToEnd = true;
        }
        else {
            pathUser.nodeIndex = room.enemyStartingSegment.node - 1;
            pathUser.startToEnd = false;
        }
    }

    private Action moveMode;

    private void KinematicUpdate() {
        animator.SetTrigger("up");
        float movePoints = Time.deltaTime * leaveHiveSpeed;
        var position = transform.position;
        transform.position = Vector2.Lerp(position, startingPosition, movePoints / (startingPosition.y - position.y));
        if (Math.Abs(transform.position.y - startingPosition.y) < 0.001) {
            moveMode = PathUpdate;
            isInsideHive = false;
        }
    }

    public void Die() {
        LeaveFleeMode();
        transform.position = room.enemyInHiveStartPosition + transform.parent.position;
        room.enemiesInHive.Add(this);
        enabled = false;
    }
    
    private void OnTriggerEnter2D(Collider2D other) {
        if (isFleeing) {
            gameMaster.AddScore(1000);
            Die();
        }
        else {
            gameMaster.KillPlayer();
        }
    }

    private void PathUpdate() {
        pathUser.Move(transform);
        animator.SetBool("Power Up", isFleeing);
        switch (pathUser.currentMovementDirection) {
            case Utils.Direction.West: animator.SetTrigger("left");
                break;
            case Utils.Direction.North: animator.SetTrigger("up");
                break;
            case Utils.Direction.South: animator.SetTrigger("down");
                break;
            case Utils.Direction.East: animator.SetTrigger("right");
                break;
        }
    }

    private void Update() {
        if (Time.timeScale== 0) return;
        moveMode.Invoke();
    }
}
