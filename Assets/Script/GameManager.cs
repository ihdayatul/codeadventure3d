using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [Header("Game State")]
    public bool isProgramFinished = false;
    public bool isPlayerAtGoal = false;

    [Header("UI")]
    public GameObject CompleteLevelUI;

    [Header("restartLevel")]
    public GameObject RestartBatton;

    [Header("Fail State")]
    public bool isPlayerFallen = false;


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void CheckWinCondition()
    {
        if (isProgramFinished && isPlayerAtGoal)
        {
            CompleteLevelUI.SetActive(true);
            RestartBatton.SetActive(false);

        }
    }

    public void RestartLevel()
    {
        isPlayerFallen = false;
        isProgramFinished = false;
        isPlayerAtGoal = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void PlayerFall()
    {
        if (isPlayerFallen) return;

        isPlayerFallen = true;
    }

    public void ResetData()
    {
        isPlayerFallen = false;
        isProgramFinished = false;
        isPlayerAtGoal = false;
    }
}
