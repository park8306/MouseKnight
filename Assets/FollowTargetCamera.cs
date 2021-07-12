﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTargetCamera : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform target;
    public Vector3 offset;

    public BoxCollider moveableArea;
    public float minX, maxX, minZ, maxZ;
    void Start()
    {
        var camera = GetComponent<Camera>();

        float height = 2f * camera.orthographicSize;
        float width = height * camera.aspect;

        offset = target.position - transform.position;
        minX = width / 2 + moveableArea.transform.position.x + moveableArea.center.x - moveableArea.size.x / 2;
        maxX = -width / 2 + moveableArea.transform.position.x + moveableArea.center.x + moveableArea.size.x / 2;
        minZ = height / 2 + moveableArea.transform.position.z + moveableArea.center.z - moveableArea.size.z / 2;
        maxZ = -height / 2 + moveableArea.transform.position.z + moveableArea.center.z + moveableArea.size.z / 2;
    }

    // Update is called once per frame
    void Update()
    {
        var newPos = target.position - offset;
        newPos.x = Mathf.Min(newPos.x, maxX);
        newPos.x = Mathf.Max(newPos.x, minX);

        newPos.z = Mathf.Min(newPos.z, minZ);
        newPos.z = Mathf.Max(newPos.z, minZ);
        transform.position = newPos;
    }
}
