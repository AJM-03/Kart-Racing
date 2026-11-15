using UnityEngine;

/* Holds a reference to a scene that is used for quickly starting the game during testing. 
 * The scene is set in TestInitialiser and will be used to reload the scene in singleplayer mode */

public class TestSceneLoader
{
    private static int sceneToLoad = -1;

    public static bool HasScene()
    {
        return sceneToLoad >= 0;
    }

    public static int GetScene()
    {
        int s = sceneToLoad;
        sceneToLoad = -1;
        return s;
    }

    public static void SetScene(int s)
    { 
        sceneToLoad = s; 
    }
}
