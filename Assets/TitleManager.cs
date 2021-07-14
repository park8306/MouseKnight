using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    CanvasGroup blackScreen;
    // Start is called before the first frame update
    void Start()
    {
        // 검은 화면에서 밝게 한다.
        blackScreen = GameObject.Find("PersistCanvas").transform.Find("BlackScreen")
            .GetComponent<CanvasGroup>();
        blackScreen.gameObject.SetActive(true);
        blackScreen.alpha = 1;
        blackScreen.DOFade(0, 1.7f).OnComplete(()=> blackScreen.gameObject.SetActive(false));
        // 뉴게임 누르면 스테이지 1로드
        // 어두워졌을 때 스테이지1로드
        // 밝아지자

        Button button = GameObject.Find("TitleCanvas").transform.Find("Button").GetComponent<Button>();
        button.AddListener(this, OnClickNewGame);
    }

    public void OnClickNewGame()
    {
        blackScreen.gameObject.SetActive(true);
        blackScreen.DOFade(1, 1.7f)
            .OnComplete(()=>
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Stage1");
            });
    }
}
