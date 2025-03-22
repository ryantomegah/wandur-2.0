using UnityEditor;
using UnityEngine;
using System.IO;
using System;

public class BuildScript
{
    // The name of the output APK file
    static string appName = "WandurApp";
    
    // Build the Android app
    [MenuItem("Build/Android")]
    public static void BuildAndroid()
    {
        // Get the path to save the build
        string buildPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), appName + ".apk");
        
        // Define the scenes to include in the build
        string[] scenes = GetEnabledScenes();
        
        // Configure player settings for Android
        PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.bundleVersion = "0.1.0";
        PlayerSettings.companyName = "Wandur";
        PlayerSettings.productName = "Wandur Shopper";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.wandur.shopperapp");
        
        // Set minimum Android version
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        
        // Build the APK
        BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.Android, BuildOptions.None);
        
        // Log success message
        Debug.Log("Android build completed: " + buildPath);
    }
    
    // Get all enabled scenes from the build settings
    static string[] GetEnabledScenes()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        
        // If no scenes are defined in build settings, use currently open scene
        if (scenes.Length == 0)
        {
            Debug.LogWarning("No scenes in build settings! Adding current scene.");
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            if (!string.IsNullOrEmpty(currentScene))
            {
                return new string[] { currentScene };
            }
            else
            {
                Debug.LogError("No scene is currently open!");
                return new string[0];
            }
        }
        
        // Get all enabled scenes
        string[] enabledScenes = new string[scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            enabledScenes[i] = scenes[i].path;
        }
        
        return enabledScenes;
    }
    
    // For command-line builds
    public static void PerformAndroidBuild()
    {
        Debug.Log("Starting Android build...");
        BuildAndroid();
    }
} 