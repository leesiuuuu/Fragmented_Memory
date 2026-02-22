using UnityEngine;
using UnityEngine.UI;
public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 2f;      // 위로 올라가는 속도
    public float destroyTime = 0.5f;    // 사라지는 시간
    public Text damageText;

    void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // 위로 이동
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
    }

    public void SetText(string damage, bool isCritical)
    {
        string result = "";
        if (isCritical) result += "<size=48>크리티컬!</size>\n";
        result += damage;
        damageText.text = result;
    }

    public void SetTextPlayer(string damage)
    {
        string result = "!";
        result += damage + "!";
        damageText.text = result;
    }
}