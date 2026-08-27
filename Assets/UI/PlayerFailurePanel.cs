using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFailurePanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button returnButton;

    public GameObject PanelRoot => panelRoot;
    public TMP_Text MessageText => messageText;
    public Button ReturnButton => returnButton;
}
