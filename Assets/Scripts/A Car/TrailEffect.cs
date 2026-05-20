using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailEffect : MonoBehaviour
{
    public float teleportDistance = 5;
    private Vector3 lastPosition;
    private TrailRenderer rend;

    private void Start()
    {
        rend = GetComponent<TrailRenderer>();
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, lastPosition) > teleportDistance)
            rend.Clear();

        lastPosition = transform.position;
    }
}
