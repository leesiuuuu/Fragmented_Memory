using UnityEngine;
using UnityEngine.SceneManagement;

// Esc 입력으로 일시정지 패널을 전환한다. 게임 시간과 오디오와 입력 잠금도 함께 전환한다.
// 메인 화면 이동 전에는 정지 상태를 먼저 해제해 다음 씬에 남지 않게 한다.
public class PauseUI : MonoBehaviour
{
    private const string InputLockId = "PauseUI";

    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private PausePanel panelPrefab;
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    private PlayerHP playerHP;
    private PausePanel panel;
    private bool isPaused;

    private void Awake()
    {
        playerHP = GetComponent<PlayerHP>();

        if (panelPrefab != null)
            panel = Instantiate(panelPrefab);

        if (panel != null)
        {
            panel.SetPaused(false);
            panel.ResumeButton.onClick.AddListener(Resume);
            panel.MainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
    }

    private void Update()
    {
        if (!Input.GetKeyDown(pauseKey) || (playerHP != null && playerHP.IsDead))
            return;

        TogglePause();
    }

    private void OnDisable()
    {
        if (isPaused)
            Resume();
    }

    private void OnDestroy()
    {
        GameplayInputLock.SetLocked(InputLockId, false);

        if (panel != null)
        {
            panel.ResumeButton.onClick.RemoveListener(Resume);
            panel.MainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
            Destroy(panel.gameObject);
        }
    }

    private void TogglePause()
    {
        if (playerHP != null && playerHP.IsDead)
            return;

        if (isPaused)
            return;

        if (!GameplayInputLock.IsLocked)
            Pause();
    }

    private void Pause()
    {
        isPaused = true;
        panel.SetPaused(true);
        GameplayInputLock.SetLocked(InputLockId, true);
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    private void Resume()
    {
        isPaused = false;

        if (panel != null)
            panel.SetPaused(false);

        Time.timeScale = 1f;
        AudioListener.pause = false;
        GameplayInputLock.SetLocked(InputLockId, false);
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        GameplayInputLock.SetLocked(InputLockId, false);
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
