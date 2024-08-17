using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class StateMachine : MonoBehaviour {

    public enum StateEnum { SplashScreen, MainScreen, GameScreen, OptionsScreen, GameOverScreen }
    
    public struct State {
        public State(StateEnum newState, StateEnum oldState) {
            this.newState = newState;
            this.oldState = oldState;
        }
        public StateEnum newState;
        public StateEnum oldState;
    }
   
    public delegate void StateChangedHandler(State e);
    public event StateChangedHandler StateChanged;
    public event StateChangedHandler StateAboutToBeChanged;

    public StateEnum currentState = StateEnum.GameScreen;

    public void SetState(StateEnum newState) {
        State state = new State(newState, currentState);
        StateAboutToBeChanged?.Invoke(state);
        currentState = newState;
        StateChanged?.Invoke(state);
    }

    public void GoToMainMenu() {
        SetState(StateEnum.MainScreen);
    }
}
