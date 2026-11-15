using UnityEngine;
using UnityEngine.SceneManagement;

/* Is present in scenes in order to properly start them up with a runner whilst testing the game. 
 * it will kick them back to the menu and quickly reload the scene in singleplayer by saving the 
 * scene reference in TestSceneLoader */

public class TestInitialiser : MonoBehaviour
{
    void Awake()
    {
        if (FusionManager.Instance != null) return;
        Debug.Log("Loading scene " + SceneManager.GetActiveScene().buildIndex + " for testing");

        TestSceneLoader.SetScene(SceneManager.GetActiveScene().buildIndex);
        SceneManager.LoadScene(0);  // Load the menu
    }
}
