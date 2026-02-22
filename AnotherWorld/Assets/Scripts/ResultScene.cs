using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultScene : MonoBehaviour
{
    public Text Title;
    public Text SubTitle;
    public Text MemoryScore;
    public Text MirrorScore;
    public Text KilledEnemyScore;
    public Text DamageScore;
    public Text returnText;
    public Material invertMAT;
    public void GoMain()
    {
        invertMAT.SetFloat("_Scroll", 4);
        SceneManager.LoadScene("StartScene");
    }

    public void Start()
    {
        SubTitle.text = "당신의 스코어 : " + GameManager.Instance.GetFinalScore().ToString();
        MemoryScore.text = "기억 점수 | " + GameManager.Instance.GetMemoryScore().ToString();
        MirrorScore.text = "거울 세계 점수 | " + GameManager.Instance.GetMirrorScore().ToString();
        KilledEnemyScore.text = "적 점수 | " + GameManager.Instance.GetKillScore().ToString();
        DamageScore.text = "데미지 점수 | " +GameManager.Instance.GetDamageScore().ToString();
        StartCoroutine(result());
    }

    private IEnumerator result()
    {
        Title.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        SubTitle.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        MemoryScore.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        MirrorScore.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        KilledEnemyScore.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        DamageScore.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        returnText.gameObject.SetActive(true);
    }
}
