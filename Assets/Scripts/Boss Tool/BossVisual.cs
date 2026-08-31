using System.Collections;
using UnityEngine;

[System.Serializable]
public class BossMotion
{
    public int patternID;
    public Sprite[] sprites;
    public float frameTime = 0.08f;
}

[RequireComponent(typeof(SpriteRenderer))]
public class BossVisual : MonoBehaviour
{
    [Header("Idle")]
    [SerializeField] private Sprite[] idleSprites;
    [SerializeField] private float idleFrameTime = 0.1f;

    [Header("Skill Motions")]
    [SerializeField] private BossMotion[] motions;

    [Header("Die")]
    [SerializeField] private Sprite[] dieSprites;
    [SerializeField] private float dieFrameTime = 0.1f;

    private BossControl boss;
    private SpriteRenderer spriteRenderer;

    private Coroutine idleCoroutine;
    private Coroutine motionCoroutine;
    private Coroutine dieCoroutine;

    private bool isDead;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        boss = GetComponent<BossControl>();

        if (boss != null)
        {
            boss.OnPatternAttack += PlaySkillMotion;
            boss.OnDeath += PlayDie;
        }

        StartIdle();
    }

    private void OnDestroy()
    {
        if (boss != null)
        {
            boss.OnPatternAttack -= PlaySkillMotion;
            boss.OnDeath -= PlayDie;
        }
    }

    private void PlaySkillMotion(int patternID)
    {
        if (isDead)
            return;

        for (int i = 0; i < motions.Length; i++)
        {
            if (motions[i].patternID == patternID)
            {
                if (motionCoroutine != null)
                    StopCoroutine(motionCoroutine);

                StopIdle();

                motionCoroutine =
                    StartCoroutine(
                        PlayMotion(motions[i])
                    );

                break;
            }
        }
    }

    private IEnumerator PlayMotion(BossMotion motion)
    {
        if (motion.sprites == null ||
            motion.sprites.Length == 0)
        {
            StartIdle();
            motionCoroutine = null;
            yield break;
        }

        for (int i = 0; i < motion.sprites.Length; i++)
        {
            if (motion.sprites[i] != null)
                spriteRenderer.sprite =
                    motion.sprites[i];

            yield return new WaitForSeconds(
                motion.frameTime
            );
        }

        motionCoroutine = null;

        if (!isDead)
            StartIdle();
    }

    private void StartIdle()
    {
        if (isDead)
            return;

        if (idleCoroutine != null)
            StopCoroutine(idleCoroutine);

        idleCoroutine =
            StartCoroutine(PlayIdle());
    }

    private void StopIdle()
    {
        if (idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
            idleCoroutine = null;
        }
    }

    private IEnumerator PlayIdle()
    {
        if (idleSprites == null ||
            idleSprites.Length == 0)
            yield break;

        int index = 0;

        while (!isDead)
        {
            if (idleSprites[index] != null)
                spriteRenderer.sprite =
                    idleSprites[index];

            index++;

            if (index >= idleSprites.Length)
                index = 0;

            yield return new WaitForSeconds(
                idleFrameTime
            );
        }
    }

    private void PlayDie()
    {
        if (isDead)
            return;

        isDead = true;

        StopIdle();

        if (motionCoroutine != null)
        {
            StopCoroutine(motionCoroutine);
            motionCoroutine = null;
        }

        if (dieCoroutine != null)
            StopCoroutine(dieCoroutine);

        dieCoroutine =
            StartCoroutine(PlayDieMotion());
    }

    private IEnumerator PlayDieMotion()
    {
        if (dieSprites == null ||
            dieSprites.Length == 0)
            yield break;

        for (int i = 0; i < dieSprites.Length; i++)
        {
            if (dieSprites[i] != null)
                spriteRenderer.sprite =
                    dieSprites[i];

            yield return new WaitForSeconds(
                dieFrameTime
            );
        }

        dieCoroutine = null;
    }
}