using Cinemachine;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public enum Ground
{
    Carpet,
    Just
}

public enum Theme
{
    Tree,
    Ram,
    Art
}

public enum Effect
{
    Ground,
    BG,
    Nothing
}

public class MirrorWorldRandomizer : MonoBehaviour
{
    // 최소 거리와 최대 거리 사이에서 랜덤하게 시작
    // 시자ㄱ 위치를 기준으로 일정 거리마다 몬스터 스포너 설치
    // 몬스터 스포너는 몬스터를 일정량 이상 잡았을 때 스폰 안되게 함
    [Header("Values")]
    [SerializeField] private float minRange;
    [SerializeField] private float maxRange;
    [SerializeField] private float spawnerGap;

    [Header("References")]
    [SerializeField] private Material invertMAT;
    [SerializeField] private Transform player;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("World Object")]
    [SerializeField] private GameObject MirrorWorldObject;
    [SerializeField] private GameObject WorldObject;

    [Header("Map Parts")]
    [SerializeField] private GameObject carpet;
    [SerializeField] private GameObject justGround;
    [Space(10)]
    [SerializeField] private GameObject tree;
    [SerializeField] private GameObject ram;
    [SerializeField] private GameObject art;
    [Space(10)]
    [SerializeField] private GameObject groundParticle;
    [SerializeField] private GameObject bgParticle;

    [Header("NPC Parts")]
    [SerializeField] private float minX;
    [SerializeField] private float maxX;
    [SerializeField] private GameObject npcPrefab;

    private Ground groundState;
    private Theme themeState;
    private Effect effectState;

    private Vector3 basicPos;

    public void GameStart()
    {
        WalkNPC[] npcs = FindObjectsOfType<WalkNPC>();
    
        for(int i = 0; i < npcs.Length; i++)
        {
            Destroy(npcs[i].gameObject);
        }

        GameManager.Instance.killedEnemy_InGame = 0;

        basicPos = transform.position;
        float startXPos = Random.Range(minRange, maxRange);

        Vector3 pos = player.position;
        Vector3 playerPos = new Vector3(startXPos, 
            pos.y,
            pos.z);
        Vector3 cameraPos = new Vector3(startXPos,
            pos.y + 0.9f,
            pos.z);

        virtualCamera.ForceCameraPosition(cameraPos, Quaternion.identity);

        player.position = playerPos;

        bool isLeft = basicPos.x - startXPos > 0 ? true : false;
        float distance = Mathf.Abs(basicPos.x - startXPos);

        distance *= isLeft ? -1 : 1;

        transform.position = new Vector3(basicPos.x + distance, basicPos.y, basicPos.z);
        activateMapRand();

        StartCoroutine(invert());

        MirrorWorldObject.SetActive(true);
        WorldObject.SetActive(false);

        enemySpawner.StartSpawn(GameManager.Instance.SpawnEnemy);
        player.GetComponent<Animator>().SetBool("Enter", true);

        StartCoroutine(disappear());
    }

    public void ReturnGame()
    {
        ConvertEnemiesToNPCs();

        GameManager.Instance.MirrorValue++;
        GameManager.Instance.SpawnEnemy += 5;

        transform.position = basicPos;
        transform.localScale = Vector3.one;
        GetComponentInChildren<GameStart>().OnEnable();
        StartCoroutine(invert());
        MirrorWorldObject.SetActive(false);
        WorldObject.SetActive(true);
    }

    private void ConvertEnemiesToNPCs()
    {
        MirrorEnemy[] enemies = FindObjectsOfType<MirrorEnemy>();

        foreach (MirrorEnemy enemy in enemies)
        {
            Vector2 pos = enemy.transform.position;

            // 2. 박스 범위 안에 있는지 직접 체크
            bool isInside = (pos.x >= minX && pos.x <= maxX);

            Debug.Log(isInside);

            if (!isInside)
            {
                Destroy(enemy.gameObject);
                continue;
            }

            if (!enemy.IsDeath)
            {
                Destroy(enemy.gameObject);
                continue;
            }

            // 3. NPC 생성
            if (npcPrefab != null)
            {
                Instantiate(npcPrefab, enemy.transform.position, Quaternion.identity);
            }

            // 4. Enemy 삭제
            Destroy(enemy.gameObject);
        }
    }

    private void ResetMapObjects()
    {
        carpet.SetActive(false);
        justGround.SetActive(false);

        tree.SetActive(false);
        ram.SetActive(false);
        art.SetActive(false);

        groundParticle.SetActive(false);
        bgParticle.SetActive(false);
    }

    private void activateMapRand()
    {
        ResetMapObjects();

        // 1. Ground 할당
        int groundRand = Random.Range(0, 2);
        groundState = (Ground)groundRand; // enum에 직접 캐스팅 할당

        carpet.SetActive(groundState == Ground.Carpet);
        justGround.SetActive(groundState == Ground.Just);

        // 2. Theme 할당
        int themeRand = Random.Range(0, 3);
        themeState = (Theme)themeRand;

        tree.SetActive(themeState == Theme.Tree);
        ram.SetActive(themeState == Theme.Ram);
        art.SetActive(themeState == Theme.Art);

        // 3. Effect 할당
        int effectRand = Random.Range(0, 3);
        effectState = (Effect)effectRand;

        if (effectState == Effect.Ground) groundParticle.SetActive(true);
        else if (effectState == Effect.BG) bgParticle.SetActive(true);
        // Nothing일 경우 아무것도 활성화되지 않음
    }
    public IEnumerator disappear()
    {
        yield return new WaitForSeconds(0.2f);
        player.GetComponent<Animator>().SetBool("Enter", false);
        yield return new WaitForSeconds(0.5f);
        float duration = 0.3f;
        float elapsedTime = 0f;

        while(elapsedTime <= duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            float v = Mathf.Lerp(1f, 0f, t);
            transform.localScale = new Vector3(1f, v, 1f);
            yield return null;
        }

        yield break;
    }
    private IEnumerator invert()
    {
        float duration = 0.5f;
        float elapsedTime = 0f;

        // 현재 값 기준으로 시작/끝점 명확히 잡기
        float startValue = invertMAT.GetFloat("_Scroll");
        float endValue = startValue > 2f ? 0f : 4f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // 핵심: t를 0~1 사이의 부드러운 곡선으로 변환 (SmoothStep 공식)
            float smoothT = t * t * (3f - 2f * t);

            float value = Mathf.Lerp(startValue, endValue, smoothT);
            invertMAT.SetFloat("_Scroll", value);
            yield return null;
        }
        invertMAT.SetFloat("_Scroll", endValue);
    }

}
