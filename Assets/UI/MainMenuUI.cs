using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private string gameSceneName = "GameScene";

    private void Awake()
    {
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);
    }

    private void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(StartGame);
    }

    private void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}
