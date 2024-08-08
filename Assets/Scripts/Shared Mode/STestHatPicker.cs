using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STestHatPicker : MonoBehaviour
{
    public void OnMouseOver()
    {
        Debug.Log(this.gameObject.name);
        transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
    }

    public void OnMouseExit()
    {
        transform.localScale = Vector3.one;
    }

    public void OnMouseDown()
    {
        int hatIndex = STestHats.hats.IndexOf(this.gameObject);

        STestPlayerStats.Instance.hatIndex = hatIndex;
    }
}
