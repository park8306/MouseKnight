using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;

public class EditorSceneLoad
{
    [MenuItem("Window/1 Title Scene Load")]
    private static void TitleSceneLoad()
    {
        LoadScene("TitleScene");
    }


    [MenuItem("Window/2 Stage1 Scene Load")]
    private static void Stage1SceneLoad()
    {
        LoadScene("Stage1");
    }
    private static void LoadScene(string loadSceneName)
    {
        EditorSceneManager.OpenScene($"Asset/Scenes/{ loadSceneName}.unity");
    }
}
