using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 일시정지 화면의 표시 요소와 버튼 참조만 보관한다.
// 시간 정지와 씬 이동 같은 실제 동작은 PauseUI가 담당한다.
public class PausePanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;

    public GameObject PanelRoot => panelRoot;
    public Button ResumeButton => resumeButton;
    public Button MainMenuButton => mainMenuButton;

    public void SetPaused(bool paused)
    {
        panelRoot.SetActive(paused);

        if (paused && titleText != null)
            titleText.text = "메인 화면으로 돌아가시겠습니까?";
    }
}
