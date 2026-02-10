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

    [Header("Programming")]
    [SerializeField]
    private ProgrammingArea programmingArea;

    [Header("Fail State")]
    public bool isPlayerFallen = false;

    AudioManager audioManager;

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }


    public void CheckWinCondition()
    {
        if (isProgramFinished && isPlayerAtGoal)
        {
            audioManager.PlaySFX(audioManager.Walk);
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

    public int GetMaxBlocks()
    {
        return programmingArea.maxBlocks;
    }

    public int GetCurrentBlockCount()
    {
        return programmingArea.BlocksCount;
    }

    public void RefreshList()
    {
        programmingArea.RefreshList();
    }

    public void ShowValidDropFeedback()
    {
        programmingArea.DoShowValidDropFeedback();
    }

    public void ShowInvalidDropFeedback()
    {
        programmingArea.DoShowInvalidDropFeedback();
    }

}
