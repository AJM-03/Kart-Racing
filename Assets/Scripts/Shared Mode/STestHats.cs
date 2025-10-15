using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STestHats : MonoBehaviour
{
    public static List<GameObject> hats = new List<GameObject>();

    private void Awake()
    {
        foreach(Transform a in transform)
        {
            hats.Add(a.gameObject);
            a.gameObject.AddComponent<STestHatPicker>();
        }
    }
}
