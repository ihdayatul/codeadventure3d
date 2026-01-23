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
        }
    }

    public void RestartLevel()
    {
        isProgramFinished = false;
        isPlayerAtGoal = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
