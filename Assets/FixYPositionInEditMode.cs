using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode] // 플레이를 하지 않아도 실행됨
public class FixYPositionInEditMode : MonoBehaviour
{
    // Start is called before the first frame update
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
}
