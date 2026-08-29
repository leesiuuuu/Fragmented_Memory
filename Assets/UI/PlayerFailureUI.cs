using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerFailureUI : MonoBehaviour
{
    private const string InputLockId = "PlayerFailureUI";

    [SerializeField] private PlayerFailurePanel panelPrefab;
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";
    [SerializeField] private float deathAnimationDuration = 0.5f;

    private PlayerHP playerHP;
    private PlayerFailurePanel panel;

    private void Awake()
    {
        playerHP = GetComponent<PlayerHP>();

        if (panelPrefab != null)
            panel = Instantiate(panelPrefab);

        if (panel != null)
        {
            panel.PanelRoot.SetActive(false);
            panel.ReturnButton.onClick.AddListener(ReturnToMainMenu);
        }
    }

    private void OnEnable()
    {
        if (playerHP != null)
            playerHP.Died += OnPlayerDied;
    }

    private void OnDisable()
    {
        GameplayInputLock.SetLocked(InputLockId, false);

        if (playerHP != null)
            playerHP.Died -= OnPlayerDied;
    }

    private void OnDestroy()
    {
        GameplayInputLock.SetLocked(InputLockId, false);

        if (panel != null)
        {
            panel.ReturnButton.onClick.RemoveListener(ReturnToMainMenu);
            Destroy(panel.gameObject);
        }
    }

    private void OnPlayerDied()
    {
        GameplayInputLock.SetLocked(InputLockId, true);
        StartCoroutine(ShowAfterDeathAnimation());
    }

    private IEnumerator ShowAfterDeathAnimation()
    {
        yield return new WaitForSecondsRealtime(deathAnimationDuration);

        if (panel != null)
            panel.PanelRoot.SetActive(true);

        Time.timeScale = 0f;
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        GameplayInputLock.SetLocked(InputLockId, false);
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
