using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[System.Serializable]
public class StageInfo
{
    public int stageID;  //stage 번호
    public string titleString;  // stage의 이름
    public int rewradXP;
}
public class GameData : MonoBehaviour
{
    public static GameData instance;
    [SerializeField]
    private List<StageInfo> stageInfos;
    public static Dictionary<int, StageInfo> stageInfoMap 
        = new Dictionary<int, StageInfo>();
    private void Awake()
    {

        instance = this;
        DontDestroyOnLoad(gameObject);
        stageInfoMap = stageInfos.ToDictionary(x => x.stageID);
    }
}
