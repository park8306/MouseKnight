using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode] // 플레이를 하지 않아도 실행됨
public class FixYPositionInEditMode : MonoBehaviour
{
    // Start is called before the first frame update
    public SpawnType spawnType;
    void Start()
    {
        if (Application.isPlaying)
            Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        var pos = transform.position;
        pos.y = 0;
        transform.position = pos;
    }
    private void OnDrawGizmos()
    {
        spawnType = GetComponent<SpawnPoint>().spawnType;
        string iconName;
        switch (spawnType)
        {
            case SpawnType.Player:
                iconName = "Player";
                break;
            case SpawnType.Goblin:
                iconName = "Goblin";
                break;
            case SpawnType.Skeleton:
                iconName = "Skeleton";
                break;
            case SpawnType.Boss:
                iconName = "Boss";
                break;
            default:
                iconName = "";
                break;
        }
        Gizmos.DrawIcon(transform.position, iconName + ".png", true);
    }
}
