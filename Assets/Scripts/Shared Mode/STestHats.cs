using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STestHats : MonoBehaviour
{
    public static List<GameObject> hats = new List<GameObject>();

    private void Awake()
    {
        foreach(Transform child in transform)
        {
            hats.Add(child.gameObject);
        }

        foreach(GameObject hat in hats)
        {
            hat.AddComponent<STestHatPicker>();
        }
    }
}
