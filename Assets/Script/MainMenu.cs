using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(1);

    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        SceneManager.LoadSceneAsync(2);
    }

    public void NextLavel(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }

}
