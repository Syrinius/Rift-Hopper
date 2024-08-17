using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AspectRatio : MonoBehaviour {
    
    public float desiredAspectRatio = 16 / 9f;
    private int width;
    private int height;
    private Camera myCamera;
    public float screenWidth = 16f;
    public float screenHeight = 9f;
    
    private void Start() {
        myCamera = GetComponent<Camera>();
        myCamera.projectionMatrix = Matrix4x4.Ortho(
            -screenWidth/2,
            screenWidth/2,
            -screenHeight/2,
            screenHeight/2, .3f, 1000);
    }
    
    private void Update() {
        if (Screen.width == width && Screen.height == height) return;
        width = Screen.width;
        height = Screen.height;
        float currentAspectRatio = width / (float)height;
        if (desiredAspectRatio < currentAspectRatio) {
            myCamera.rect = new Rect((1 - (desiredAspectRatio/currentAspectRatio))/2,0,desiredAspectRatio/currentAspectRatio,1);
        } else if (desiredAspectRatio > currentAspectRatio) {
            myCamera.rect = new Rect(0,(1 - (currentAspectRatio/desiredAspectRatio))/2,1,currentAspectRatio/desiredAspectRatio);
        } else {
            myCamera.rect = new Rect(0,0,1,1);
        }
    }

    private void OnDrawGizmosSelected() {
        Vector3 myPosition = transform.position;
        Gizmos.DrawLine(new Vector3(myPosition.x - screenWidth/2, myPosition.y - screenHeight/2), new Vector3(myPosition.x  + screenWidth/2, myPosition.y - screenHeight/2));
        Gizmos.DrawLine(new Vector3(myPosition.x + screenWidth/2, myPosition.y - screenHeight/2), new Vector3(myPosition.x  + screenWidth/2, myPosition.y + screenHeight/2));
        Gizmos.DrawLine(new Vector3(myPosition.x + screenWidth/2, myPosition.y + screenHeight/2), new Vector3(myPosition.x - screenWidth/2, myPosition.y + screenHeight/2));
        Gizmos.DrawLine(new Vector3(myPosition.x - screenWidth/2, myPosition.y + screenHeight/2), new Vector3(myPosition.x - screenWidth/2, myPosition.y - screenHeight/2));
    }
}
