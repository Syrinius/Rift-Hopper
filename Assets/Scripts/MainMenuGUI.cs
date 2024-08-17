using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuGUI : MonoBehaviour {

    public GUISkin skin;
     public Texture2D bgImage; 
    public StateMachine gameState;
    private void OnGUI() {
        GUI.skin = skin;
        GUILayout.BeginArea(new Rect (0, 0, Screen.width, Screen.height));
        GUILayout.BeginVertical("box");

        GUI.BeginGroup (new Rect (0, 0, Screen.width, Screen.height));
        GUI.Box (new Rect (0,0,Screen.width,Screen.height), bgImage);
        if (GUI.Button (new Rect (Screen.width/23*6, Screen.height/50*24, Screen.width/6, Screen.height/7), "")) 
        gameState.SetState(StateMachine.StateEnum.GameScreen); //play the game
        if (GUI.Button (new Rect (Screen.width/23*6,Screen.height/50*31, Screen.width/6, Screen.height/7), "", "optionsbutton"))
        gameState.SetState(StateMachine.StateEnum.OptionsScreen); //settings
        if (GUI.Button (new Rect (Screen.width/23*6,Screen.height/50*38, Screen.width/6, Screen.height/7), "", "quitbutton"))
        Application.Quit(); //quit
        GUI.EndGroup ();

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
