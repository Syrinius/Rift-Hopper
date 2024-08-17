using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{

    public GameMaster gameMaster;
    private void OnTriggerEnter2D(Collider2D other) {
        gameMaster.ActivatePowerUp();
        Destroy(gameObject);
    }
}
