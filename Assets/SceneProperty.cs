using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneProperty : MonoBehaviour
{
    public static SceneProperty instance;
    private void Awake()
    {
        instance = this;
        // 해당 오브젝트가 없다면 만들어라
        if (PersistCanvas.instance == null)
        {
            Instantiate(Resources.Load("PersistCanvas"));
        }
        if (GameData.instance == null)
        {
            Instantiate(Resources.Load("PersistCanvas"));
        }
    }

    private void OnDestroy()
    {
        instance = null;
    }
    public int StageID = -1;
    public enum SceneType
    {
        Stage,
        Title,
    }

    public SceneType sceneType = SceneType.Stage;
}
