using TMPro;
using UnityEngine;

public class ItemInventoryPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text selectedItemText;
    [SerializeField] private ItemInventorySlotUI slotTemplate;
    [SerializeField] private Transform slotRoot;

    public GameObject PanelRoot => panelRoot;
    public TMP_Text SelectedItemText => selectedItemText;
    public ItemInventorySlotUI SlotTemplate => slotTemplate;
    public Transform SlotRoot => slotRoot;
}
