using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class ExtensionMethods
{
    // float ---------------------------------------------------------------------------------------------------- float

    // Remaps a value between two points to a value between two other points
    // Eg: Remapping 0.5 from a scale of 0-1 to a scale of 0-2 would give you 1
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }


    // Rounds a float
    public static float Round(this float f, int decimalPlaces = 2)
    {
        float multiplier = 1;
        for (int i = 0; i < decimalPlaces; i++)
        {
            multiplier *= 10f;
        }
        return Mathf.Round(f * multiplier) / multiplier;
    }


    // Converts angles from 0–360 to -180–180
    public static float NormalizeAngle(this float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }



    // Vector 3 ---------------------------------------------------------------------------------------------------- Vector 3

    // Sets a value inside a vector3
    public static Vector3 SetX(this Vector3 vector, float x) { return new Vector3(x, vector.y, vector.z); }
    public static Vector3 SetY(this Vector3 vector, float y) { return new Vector3(vector.x, y, vector.z); }
    public static Vector3 SetZ(this Vector3 vector, float z) { return new Vector3(vector.x, vector.y, z); }



    // Adds a value inside a vector3
    public static Vector3 AddX(this Vector3 vector, float x) { return new Vector3(vector.x + x, vector.y, vector.z); }
    public static Vector3 AddY(this Vector3 vector, float y) { return new Vector3(vector.x, vector.y + y, vector.z); }
    public static Vector3 AddZ(this Vector3 vector, float z) { return new Vector3(vector.x, vector.y, vector.z + z); }



    // Rounds a Vector 3
    public static Vector3 Round(this Vector3 vector3, int decimalPlaces = 2)
    {
        float multiplier = 1;
        for (int i = 0; i < decimalPlaces; i++)
        {
            multiplier *= 10f;
        }
        return new Vector3(
            Mathf.Round(vector3.x * multiplier) / multiplier,
            Mathf.Round(vector3.y * multiplier) / multiplier,
            Mathf.Round(vector3.z * multiplier) / multiplier);
    }



    // Returns the closest vector from an array
    public static Vector3 GetClosestVector3From(this Vector3 vector, Vector3[] otherVectors)
    {
        if (otherVectors.Length == 0) throw new Exception("The list of other vectors is empty");
        var minDistance = Vector3.Distance(vector, otherVectors[0]);
        var minVector = otherVectors[0];
        for (var i = otherVectors.Length - 1; i > 0; i--)
        {
            var newDistance = Vector3.Distance(vector, otherVectors[i]);
            if (newDistance < minDistance)
            {
                minDistance = newDistance;
                minVector = otherVectors[i];
            }
        }
        return minVector;
    }



    // Transform ---------------------------------------------------------------------------------------------------- Transform

    // Resets a transform
    public static void ResetTransform(this Transform trans)
    {
        trans.position = Vector3.zero; // Reset position
        trans.localRotation = Quaternion.identity; // Reset rotation
        trans.localScale = Vector3.one; // Reset scale
    }



    // Copy another transform
    public static void CopyFrom(this Transform transform, Transform other)
    {
        transform.position = other.position;
        transform.rotation = other.rotation;
        transform.localScale = other.localScale;
    }



    // Get an array of all direct children
    public static Transform[] GetChildren(this Transform transform)
    {
        Transform[] children = new Transform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            children[i] = transform.GetChild(i);
        }

        return children;
    }



    // Destroy all children in a transform
    public static void DestroyChildren(this Transform transform)
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);
        }
    }



    // Get the full heirarchy directory path for the transform
    public static string GetPath(this Transform transform, string delimiter = "/")
    {
        if (!transform.parent)
            return transform.name;

        return transform.parent.GetPath(delimiter) + delimiter + transform.name;
    }



    // GameObject ---------------------------------------------------------------------------------------------------- GameObject

    // Creates a copy of this gameobject
    public static GameObject Clone(this GameObject gameObject)
    {
        return UnityEngine.Object.Instantiate(gameObject);
    }



    // Checks if a gameobject has a component
    public static bool HasComponent<T>(this GameObject gameObject) where T : Component
    {
        return gameObject.GetComponent<T>() != null;
    }



    // Gets or adds the specified component to this GameObject.
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        if (gameObject.TryGetComponent<T>(out var component))
            return component;

        return gameObject.AddComponent<T>();
    }



    // Remove a component from a gameobject
    public static bool DestroyComponent<T>(this GameObject gameObject) where T : Component
    {
        if (!gameObject.TryGetComponent<T>(out var component))
            return false;

        UnityEngine.Object.Destroy(component);
        return true;
    }



    // Returns a list of components in the direct children
    public static List<T> GetComponentsInDirectChildren<T>(this GameObject gameObject) where T : Component
    {
        List<T> components = new List<T>();

        foreach (Transform child in gameObject.transform)
        {
            if (child.TryGetComponent<T>(out var component))
                components.Add(component);
        }

        return components;
    }



    // Sets the layer in all children of a gameobject
    public static void SetLayerRecursively(this GameObject gameObject, int layer)
    {
        gameObject.layer = layer;
        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.SetLayerRecursively(layer);
        }
    }



    // Get the full path of the gameobject in the scene heirarchy
    public static string GetPath(this GameObject gameObject, string delimiter = "/")
    {
        return gameObject.transform.GetPath(delimiter);
    }



    // Rigidbody ---------------------------------------------------------------------------------------------------- Rigidbody

    // Change the current direction of a rigidbody while keeping it's direction
    public static void ChangeDirection(this Rigidbody rb, Vector3 direction)
    {
        rb.velocity = direction.normalized * rb.velocity.magnitude;
    }



    // Stop a rigidbody from moving
    public static void Stop(this Rigidbody rb)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }



    // Move towards a point at a set speed
    public static void MoveTowards(this Rigidbody rb, Vector3 targetPosition, float speed)
    {
        Vector3 direction = (targetPosition - rb.position).normalized;
        rb.velocity = direction * speed;
    }



    // List ---------------------------------------------------------------------------------------------------- List

    // Get a random item from a list
    public static T GetRandomItem<T>(this IList<T> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }



    // Shuffle a list
    public static void Shuffle<T>(this IList<T> list)
    {
        for (var i = list.Count - 1; i > 1; i--)
        {
            var j = UnityEngine.Random.Range(0, i + 1);
            var value = list[j];
            list[j] = list[i];
            list[i] = value;
        }
    }



    // Layer Mask ---------------------------------------------------------------------------------------------------- Layer Mask

    // Check if layer mask contains a certain layer
    public static bool Contains(this LayerMask mask, int layer)
    {
        return (mask & (1 << layer)) != 0;
    }

    // Check if layer mask contains an object's layer
    public static bool Contains(this LayerMask mask, GameObject obj)
    {
        return (mask & (1 << obj.layer)) != 0;
    }



    // Rect Transform ---------------------------------------------------------------------------------------------------- Rect Transform

    // Returns the world space bounding box of this RectTransform
    public static Rect GetWorldRect(this RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        // Get the bottom left corner.
        Vector3 position = corners[0];

        var rect = rectTransform.rect;
        var lossyScale = rectTransform.lossyScale;

        Vector2 size = new Vector2(
            lossyScale.x * rect.size.x,
            lossyScale.y * rect.size.y);

        return new Rect(position, size);
    }



    // Checks if the world space bounding box of this RectTransform contains the world space bounding box of another RectTransform.
    public static bool Contains(this RectTransform rectTransform, RectTransform other)
    {
        Rect wr = rectTransform.GetWorldRect();
        Rect owr = other.GetWorldRect();
        return wr.xMin <= owr.xMin && wr.yMin <= owr.yMin && wr.xMax >= owr.xMax && wr.yMax >= owr.yMax;
    }



    // Audio Source ---------------------------------------------------------------------------------------------------- Audio Source

    // Fades in the AudioSource to the target volume over a specified duration.
    public static IEnumerator FadeIn(this AudioSource audioSource, float duration, float targetVolume, Action onComplete = null)
    {
        if (audioSource.volume >= targetVolume)
            yield break;

        float currentTime = 0;
        float endVolume = Mathf.Clamp(targetVolume, 0f, 1f);

        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(0, endVolume, currentTime / duration);
            yield return null;
        }

        audioSource.volume = endVolume;
        onComplete?.Invoke();
    }



    // Fades out the AudioSource from its current volume to silence over a specified duration.
    public static IEnumerator FadeOut(this AudioSource audioSource, float duration, Action onComplete = null)
    {
        float currentTime = 0;
        float startingVolume = audioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startingVolume, 0, currentTime / duration);
            yield return null;
        }

        audioSource.volume = 0;
        onComplete?.Invoke();
    }



#if CSHARP_7_3_OR_NEWER
    // Asynchronously fades in the AudioSource to a specified target volume over a specified duration.
    public static async void FadeInAsync(this AudioSource audioSource, float duration, float targetVolume, Action onComplete = null)
    {
        if (audioSource.volume >= targetVolume)
            return;

        float currentTime = 0;
        float endVolume = Mathf.Clamp(targetVolume, 0f, 1f);

        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(0, endVolume, currentTime / duration);
            await Task.Yield();
        }

        audioSource.volume = endVolume;
        onComplete?.Invoke();
    }



    // Asynchronously fades out the volume of the AudioSource over a specified duration.
    public static async void FadeOutAsync(this AudioSource audioSource, float duration, Action onComplete = null)
    {
        float currentTime = 0;
        float startingVolume = audioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startingVolume, 0, currentTime / duration);
            await Task.Yield();
        }

        audioSource.volume = 0;
        onComplete?.Invoke();
    }
#endif



    // Camera ---------------------------------------------------------------------------------------------------- Camera

    // Retrieves the mouse ray from the camera based on the current mouse position.
    public static Ray GetMouseRay(this Camera camera)
    {
        return camera.ScreenPointToRay(Input.mousePosition);
    }
}