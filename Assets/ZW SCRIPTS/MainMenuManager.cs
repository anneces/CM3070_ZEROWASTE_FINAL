using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    public string targetGameScene = "MainGame";

    public void OnClickStartGame()
    {
        StartCoroutine(LoadSceneAsync(targetGameScene));
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}