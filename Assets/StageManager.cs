using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// Start is called before the first frame update
/// <summary>
/// 스테이지에 발생하는 모든 이벤트 관리
/// 
/// 
/// 1. 플레이어 로드게임 시작시까지 플레이어 대기시킴
/// 2. 스테이지 시작시 화면 밝아지게함
/// 3. 몬스터 로드
/// </summary>
public class StageManager : BaseUI<StageManager>
{
    public GameStateType gameState = GameStateType.Ready;

    public int sumMonsterCount;
    public int enemiesKilledCount;
    public int damageTakenPoint;

    new private void Awake()
    {
        base.Awake();
        gameState = GameStateType.Ready;

        List<SpawnPoint> allSpawnPoints = new List<SpawnPoint>( FindObjectsOfType<SpawnPoint>());

        //Linq를 리스트형에서 SpawnType.Player가 아닌 SpawnPoint의 수를 넣음
        sumMonsterCount = allSpawnPoints.Where(x => x.spawnType != SpawnType.Player).Count();
    }
    public Ease inEaseType = Ease.InElastic;
    public Ease outEaseType = Ease.OutBounce;
    [Button]
    IEnumerator Start()
    {
        //화면 어두운 상태로 만들고 2초동안 밝아지게 하자.
        CanvasGroup blackScreen = PersistCanvas.instance.blackScreen;
        blackScreen.gameObject.SetActive(true);
        blackScreen.alpha = 1;
        blackScreen.DOFade(0, 1.7f);
        yield return new WaitForSeconds(1.7f);

        // 스테이지 이름 표시하자.
        StageInfo stageInfo = GameData.stageInfoMap[SceneProperty.instance.StageID];
        string stageName = stageInfo.titleString;
        // "Stage1"
        // 2초 쉬었다가
        StageCanvas.instance.stageNameText.transform.localPosition = new Vector3(-1000, 0, 0);
        StageCanvas.instance.stageNameText.transform.DOLocalMoveX(0, 0.5f).SetEase(inEaseType);
        StageCanvas.instance.stageNameText.text = stageName;
        // 플레이어를 움직일 수 있게 하자.
        yield return new WaitForSeconds(2f);
        StageCanvas.instance.stageNameText.transform.DOLocalMoveX(-1000, 0.5f).SetEase(outEaseType);
        gameState = GameStateType.Playing;
    }
    
}
public enum GameStateType
{
    Ready,
    Playing,
    StageEnd,
}
