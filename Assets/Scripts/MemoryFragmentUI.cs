using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MemoryFragmentUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text grade;
    public GameObject effect;

    public string gradeNumber = "1";
    public string effectDescription = "";

    TMP_Text effectText;

    void Start()
    {
        grade.text = gradeNumber;

        effectText = effect.GetComponentInChildren<TMP_Text>();

        effect.SetActive(false);

        effectText.text = effectDescription;
        effectText.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        effect.SetActive(true);
        effectText.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        effect.SetActive(false);
        effectText.gameObject.SetActive(false);
    }
}