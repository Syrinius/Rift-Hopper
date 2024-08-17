 using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScreen : MonoBehaviour
{
    public StateMachine gameState;
    public GameObject Container;
    public StateMachine.StateEnum activeState;

    private void Awake() {
        gameState.StateChanged += GameStateOnStateChanged;
        Container.SetActive(gameState.currentState == activeState);
    }

    private void GameStateOnStateChanged(StateMachine.State e) {
        Container.SetActive(e.newState == activeState);
    }
}
