using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoadAttribute]
public static class StartupSceneLoader
{
    private const string LoadOnStartupKey = "LoadOnStartup"; 

    [MenuItem("Custom/Enable Startup Scene Loading")]
    private static void EnableStartupSceneLoading()
    {
        EditorPrefs.SetBool(LoadOnStartupKey, true); 
    }

    [MenuItem("Custom/Disable Startup Scene Loading")]
    private static void DisableStartupSceneLoading()
    {
        EditorPrefs.SetBool(LoadOnStartupKey, false); 
    }

    static StartupSceneLoader()
    {
        Debug.Log($"Startup Scene Load state = {EditorPrefs.GetBool(LoadOnStartupKey)}");
        if (EditorPrefs.GetBool(LoadOnStartupKey, true)) 
        {
            EditorApplication.playModeStateChanged += LoadStartupScene;
        }
    }

    static void LoadStartupScene(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }

        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            if (EditorSceneManager.GetActiveScene().buildIndex != 0)
            {
                EditorSceneManager.LoadScene(0);
            }
        }
    }
}