using System;
using System.Linq;
using UnityEngine;

public class Transitioner {
    
    private readonly Room from;
    private readonly Room to;
    private readonly Player player;
    private readonly Transform transientTransform;
    private readonly Vector3 transientTarget;
    private readonly Vector3 playerTarget;
    private readonly Vector3 transientOrigin;
    private readonly Vector3 playerOrigin;
    private readonly float duration = 0.5f;
    private readonly Action<Room> done;
    
    private float activeAlpha;
    
    public Transitioner(Room from, Utils.Direction direction, Player player, Transform transientTransform, Action<Room> done) {
        this.from = from;
        this.player = player;
        this.done = done;
        this.transientTransform = transientTransform;
        to = from.neighbors[direction];
        to.gameObject.SetActive(true);

        transientOrigin = transientTarget = transientTransform.position;
        playerOrigin = player.transform.position;
        
        if (direction == Utils.Direction.North || direction == Utils.Direction.South ) {
            transientTarget -= (Vector3) Utils.getDirectionVector2(direction) * 18;
            playerTarget = new Vector3(player.transform.position.x, direction == Utils.Direction.North ? 1 : 16, player.transform.position.z);
        }
        else {
            transientTarget -= (Vector3) Utils.getDirectionVector2(direction) * 34;
            playerTarget = new Vector3(direction == Utils.Direction.East ? 1 : 32, player.transform.position.y,  player.transform.position.z);
        }
    }

    public void TransitionFinished() {
        from.gameObject.SetActive(false);
        Junction junction = to.junctions[player.transform.position];
        Corridor corridor = junction.corridors.Values.First();
        bool startToEnd = corridor.junctionStart == junction; 
        player.SetCorridor(corridor, startToEnd ? 1 : corridor.nodes.Count - 2, startToEnd);
        player.SetStartingCorridor(to.GetStartPosition());
        done.Invoke(to);
    }

    public void Update() {
        activeAlpha += Time.unscaledDeltaTime / duration;
        transientTransform.position = Vector3.Lerp(transientOrigin, transientTarget, activeAlpha);
        player.transform.position = Vector3.Lerp(playerOrigin, playerTarget, activeAlpha);
        if (activeAlpha >= 1) {
            TransitionFinished();
        }
    }
}