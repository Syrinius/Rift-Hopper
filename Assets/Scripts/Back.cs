using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Back : MonoBehaviour
{
    public StateMachine gameState;
    public void BackToMenu() {
        gameState.SetState(StateMachine.StateEnum.MainScreen); //back to menu
    }
}