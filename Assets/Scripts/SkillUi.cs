using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class SkillUi : MonoBehaviour
{
    public Key skillKey;

    public float cooldownTime = 3f;
    public TMP_Text cooldownText;
    public Image skillImage;

    float cooldown = 0f;

    void Start()
    {
        cooldownText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current[skillKey].wasPressedThisFrame && cooldown <= 0)
        {
            cooldown = cooldownTime;

            skillImage.color = Color.gray;
            cooldownText.gameObject.SetActive(true);
        }

        if (cooldown > 0)
        {
            cooldown -= Time.deltaTime;

            cooldownText.text = Mathf.Ceil(cooldown).ToString();

            if (cooldown <= 0)
            {
                cooldown = 0;

                cooldownText.text = "";
                cooldownText.gameObject.SetActive(false);

                skillImage.color = Color.white;
            }
        }
    }
}