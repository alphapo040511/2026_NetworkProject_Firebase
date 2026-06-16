using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] Text scoreText;
    [SerializeField] GameObject gameOverPanel;

    int score;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        scoreText.text = "Score : 0";

        gameOverPanel.SetActive(false);
    }

    public void AddScore()
    {
        score++;

        scoreText.text =
            $"Score : {score}";
    }

    public void GameOver()
    {
        gameOverPanel.SetActive(true);

        SaveResult();
    }

    void SaveResult()
    {
        UserData userData =
            UserDataManager.Instance.CurrentUserData;

        int rewardCoin = score * 10;

        userData.Coin += rewardCoin;

        if (score > userData.Score)
        {
            userData.Score = score;
        }

        UserDataManager.Instance.SaveUserData();

        Debug.Log(
            $"점수:{score} / 보상:{rewardCoin}");
    }

    public void Restart()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void MainScene()
    {
        SceneManager.LoadScene("MainScene");
    }
}