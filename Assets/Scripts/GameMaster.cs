using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class GameMaster : MonoBehaviour {

    public LevelManager levelManager;
    public Player playerPrefab;
    private Player playerInstance;
    public TextMeshProUGUI Score;
    public TextMeshProUGUI GameOverScore;
    public TextMeshProUGUI Lives;
    public float enemySendOutCooldown = 10;
    int TotalScore;
    int LivesCounter = 3;
    public List<MapData> MapDatas;
    private MapData currentMapData;
    private Room currentRoom;
    public int coinCounter = 0;
    public AudioSource coinPickup;
    public AudioSource deathSound;
    public AudioSource powerupSound;
    private float activePowerUpDuration;
    public float basePowerUpDuration = 8;
    private bool isPowerUpActive;
    public float levelCompletedDifficultyModifier = 0.9f;
    public StateMachine stateMachine;
    
    private Action updateMode;
    
    public void AddScore(int amount) {
        TotalScore += amount;
        Score.text = "Score: " +TotalScore;
        GameOverScore.text = "Score: " +TotalScore;
    }

    public void AddLife() {
        LivesCounter++;
        Lives.text = "Lives: " +LivesCounter;
    }

    public void PickUpCoin() {
        AddScore(100);
        coinPickup.Play();
        if (++currentRoom.coinsPickedUp == 120) {
            AddLife();
        }
        if (--coinCounter == 0) {
            LevelCompleted();
        }
    }

    public Corridor GetPlayerCorridor() {
        return playerInstance.GetCorridor();
    }

    public Junction GetPlayerForwardJunction() {
        return playerInstance.GetForwardJunction();
    }
    
    public void KillPlayer() {
        LivesCounter--;
        if (LivesCounter == 0) {
            stateMachine.SetState(StateMachine.StateEnum.GameOverScreen);
            return;
        }
        deathSound.Play();
        Lives.text = "Lives: " +LivesCounter;
        foreach (var enemyInstance in currentRoom.enemyInstances) {
            enemyInstance.Die();
        }
        isPowerUpActive = false;
        activePowerUpDuration = 0;
        playerInstance.Die();
        currentRoom.activeEnemySendOutCooldown = 0;
    }

    /*private void Awake() {
        updateMode = VoidUpdate;
    }*/

    // Start is called before the first frame update
    void OnEnable() {
        coinCounter = 0;
        TotalScore = 0;
        LivesCounter = 3;
        enemySendOutCooldown = 10;
        basePowerUpDuration = 8;
        
        #if DEBUG
        if (Input.GetKey(KeyCode.Alpha1)) {
            currentRoom = levelManager.GenerateLevel(this, currentMapData = MapDatas[0]);
        }
            else if (Input.GetKey(KeyCode.Alpha2)) {
            currentRoom = levelManager.GenerateLevel(this, currentMapData = MapDatas[1]);
        }
            else if (Input.GetKey(KeyCode.Alpha3)) {
            currentRoom = levelManager.GenerateLevel(this, currentMapData = MapDatas[2]);
        }
        else {
            currentRoom = levelManager.GenerateLevel(this, currentMapData = Utils.RNG(MapDatas));
        }
        #else
            currentRoom = levelManager.GenerateLevel(this, currentMapData = Utils.RNG(MapDatas));
        #endif
        
        currentRoom.gameObject.SetActive(true);
        Corridor corridor = currentRoom.GetStartPosition();
        if (playerInstance != null) Destroy(playerInstance.gameObject);
        playerInstance = Instantiate(playerPrefab, corridor.junctionStart.position, Quaternion.identity, transform);
        playerInstance.SetStart(corridor, this);
        Pause();
    }

    public void Pause() {
        if (updateMode != null && updateMode != RegularUpdate) return;
        updateMode = PauseUpdate;
        Time.timeScale = 0;
    }

    public void UnPause(bool force = false) {
        if (!force && updateMode != null && updateMode != PauseUpdate) return;
        updateMode = RegularUpdate;
        Time.timeScale = 1;
    }

    public bool IsPowerUpActive {
        get => isPowerUpActive;
    }

    public void ActivatePowerUp() {
        powerupSound.Play();
        activePowerUpDuration += basePowerUpDuration;
        MakeEnemiesEnterFleeMode();
        isPowerUpActive = true;
    }

    public void LevelCompleted() {
        coinCounter = 0;
        enemySendOutCooldown *= levelCompletedDifficultyModifier;
        basePowerUpDuration *= levelCompletedDifficultyModifier;
        currentRoom = levelManager.GenerateLevel(this, currentMapData = Utils.RNG(MapDatas));
        currentRoom.gameObject.SetActive(true);
        Corridor corridor = currentRoom.GetStartPosition();
        playerInstance.transform.position = corridor.junctionStart.position;
        playerInstance.SetStart(currentRoom.GetStartPosition(), this);
        isPowerUpActive = false;
        activePowerUpDuration = 0;
    }

    public void RoomTransition(Utils.Direction direction) {
        Transitioner transitioner = new Transitioner(currentRoom, direction, playerInstance,
            levelManager.TempObjs.transform, (Room to) => {
                UnPause(true);
                currentRoom = to;
                if (isPowerUpActive) {
                    MakeEnemiesEnterFleeMode();
                }
                else {
                    MakeEnemiesLeaveFleeMode();
                }
            });
        updateMode = transitioner.Update;
        Time.timeScale = 0;
    }

    private void Update() {
        updateMode.Invoke();
    }

    private void VoidUpdate() {
        
    }

    private void PauseUpdate() {
        if (Input.anyKey) {
            UnPause();
        }
    }

    private void MakeEnemiesEnterFleeMode() {
        foreach (var enemyInstance in currentRoom.enemyInstances) {
            enemyInstance.EnterFleeMode();
        }
    }
    
    private void MakeEnemiesLeaveFleeMode() {
        foreach (var enemyInstance in currentRoom.enemyInstances) {
            enemyInstance.LeaveFleeMode();
        }
    }
    
    private void RegularUpdate() {
        if (currentRoom.enemiesInHive.Any()) {
            if (currentRoom.activeEnemySendOutCooldown > 0)
                currentRoom.activeEnemySendOutCooldown -= Time.deltaTime;
            else {
                Enemy leaver = currentRoom.enemiesInHive.First();
                leaver.LeaveHive();
                currentRoom.enemiesInHive.Remove(leaver);
                currentRoom.activeEnemySendOutCooldown = enemySendOutCooldown;
            }
        } else currentRoom.activeEnemySendOutCooldown = enemySendOutCooldown;
        
        if (activePowerUpDuration > 0) {
            activePowerUpDuration -= Time.deltaTime;
        }
        else if(isPowerUpActive) {
            MakeEnemiesLeaveFleeMode();
            activePowerUpDuration = 0;
            isPowerUpActive = false;
        }
    }
}
