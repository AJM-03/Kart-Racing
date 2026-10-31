using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class ImageScroller : MonoBehaviour
{
    private RawImage image;
    public Vector2 scrollSpeed;

    private void Start()
    {
        image = GetComponent<RawImage>();
    }

    void Update()
    {
        image.uvRect = new Rect(image.uvRect.position + scrollSpeed * Time.deltaTime, image.uvRect.size);
    }
}
