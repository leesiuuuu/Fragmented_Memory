using TMPro;
using UnityEngine;

public class PlayerStatusPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text statusText;

    public GameObject PanelRoot => panelRoot;
    public TMP_Text StatusText => statusText;
}
