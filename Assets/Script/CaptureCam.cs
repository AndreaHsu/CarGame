using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class CaptureCam : MonoBehaviour
{
    // Reference to the camera you want to capture from
    public Camera captureCamera;
    // File name for the saved PNG image
    public string fileName = "screenshot";
    public string folderName = "images";
    public bool autoCapture = true;
    public int autoInterval = 3; // it should be set up if autoCapture == true
    private int num = 0;

    public void Update(){
        if(!autoCapture && Input.GetKeyDown("p")){
            CaptureAndSave();
        }
        if(autoCapture && Mathf.FloorToInt(Time.time) % autoInterval == 0){
            CaptureAndSave();
        }
    }

    public void CaptureAndSave()
    {
        // Ensure the camera is not null
        if (captureCamera == null)
        {
            Debug.LogError("Capture camera is not assigned!");
            return;
        }

        // Create a render texture to capture the camera's output
        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        captureCamera.targetTexture = renderTexture;

        // Activate the camera and render the scene
        captureCamera.Render();

        // Read the pixels from the render texture
        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        RenderTexture.active = renderTexture;
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();

        // Reset the camera's target texture to null
        captureCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        // Encode the screenshot as PNG
        byte[] bytes = screenshot.EncodeToPNG();

        // Save the PNG to a file
        string parentPath = Directory.GetParent(Application.dataPath).FullName;
        string folderPath = Path.Combine(parentPath, folderName);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        string filePath = Path.Combine(folderPath, fileName+num.ToString()+".png");
        File.WriteAllBytes(filePath, bytes);

        // Debug.Log("Screenshot saved to: " + filePath);
        num++;
    }
}
