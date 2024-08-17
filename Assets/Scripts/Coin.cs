using UnityEngine;

public class Coin : MonoBehaviour
{
    public GameMaster gameMaster;
    private void OnTriggerEnter2D(Collider2D other) {
        gameMaster.PickUpCoin();
        Destroy(gameObject);
    }
}
