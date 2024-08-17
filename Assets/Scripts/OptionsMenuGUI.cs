using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenuGUI : MonoBehaviour
{
    public GUISkin optionsskin;
    
    public StateMachine gameState;
    private bool Sounds = true;
    float SoundLevel = 0.5f;
    public AudioSource coinPickup;
    public AudioSource deathSound;
    public AudioSource powerupSound;
    private void OnGUI() {
        GUI.skin = optionsskin;
        GUILayout.BeginArea(new Rect(Screen.width/25*11, Screen.height/25*11, Screen.width/4, Screen.height/3));
        GUILayout.BeginVertical("box");
        Sounds = GUILayout.Toggle(Sounds, " Sounds ON/OFF");
        if (Sounds == false)
        {
            coinPickup.mute = !coinPickup.mute;
            deathSound.mute = !deathSound.mute;
            powerupSound.mute = !powerupSound.mute;
        }
        
        GUILayout.Label("Volume                            "+ SoundLevel.ToString("F1"));
        SoundLevel = GUILayout.HorizontalSlider(SoundLevel , 0.0f, 1f);
        coinPickup.volume = SoundLevel;
        deathSound.volume = SoundLevel;
        powerupSound.volume = SoundLevel;
        if (GUILayout.Button("Back")) gameState.SetState(StateMachine.StateEnum.MainScreen);
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    
}
