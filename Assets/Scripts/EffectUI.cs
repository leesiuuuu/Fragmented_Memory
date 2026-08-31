using UnityEngine;

public class SpecialEffectUI : MonoBehaviour
{
    public GameObject effectText;

    void Start()
    {
        effectText.SetActive(false);
    }

    public void ShowEffect()
    {
        effectText.SetActive(true);
    }

    public void HideEffect()
    {
        effectText.SetActive(false);
    }
}