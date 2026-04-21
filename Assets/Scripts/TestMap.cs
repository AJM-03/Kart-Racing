using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMap : MonoBehaviour
{
    public GameObject car;
    public Transform[] teleportPoints;
    public GameObject[] maps;



    void Update()
    {
        if (Input.GetKey(KeyCode.Alpha1)) Teleport(1);
        if (Input.GetKey(KeyCode.Alpha2)) Teleport(2);
        if (Input.GetKey(KeyCode.Alpha3)) Teleport(3);
        if (Input.GetKey(KeyCode.Alpha4)) Teleport(4);
        if (Input.GetKey(KeyCode.Alpha5)) Teleport(5);
        if (Input.GetKey(KeyCode.Alpha6)) Teleport(6);
        if (Input.GetKey(KeyCode.Alpha7)) Teleport(7);
        if (Input.GetKey(KeyCode.Alpha8)) Teleport(8);
        if (Input.GetKey(KeyCode.Alpha9)) Teleport(9);
        if (Input.GetKey(KeyCode.Alpha0)) Teleport(0);
    }

    private void Teleport(int i)
    {
        car.transform.position = teleportPoints[i].transform.position;
        car.transform.rotation = teleportPoints[i].transform.rotation;

        foreach (GameObject m in maps)
        {
            m.SetActive(false);
        }

        maps[i].SetActive(true);
    }
}
