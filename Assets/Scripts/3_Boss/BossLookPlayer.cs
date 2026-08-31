using UnityEngine;

public class BossLookPlayer : MonoBehaviour
{
    private BossControl boss;
    private SpriteRenderer spriteRenderer;

    public Transform effect;
    public Transform effect2;

    private SpriteRenderer effectRenderer;
    private SpriteRenderer effectRenderer2;

    private Vector3 effectStartPosition;
    private Vector3 effect2StartPosition;

    void Start()
    {
        boss = GetComponent<BossControl>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (effect != null)
        {
            effectStartPosition = effect.localPosition;
            effectRenderer = effect.GetComponent<SpriteRenderer>();
        }

        if (effect2 != null)
        {
            effect2StartPosition = effect2.localPosition;
            effectRenderer2 = effect2.GetComponent<SpriteRenderer>();
        }
    }

    void Update()
    {
        if (boss == null || boss.Player == null)
            return;

        bool lookLeft =
            boss.Player.position.x < transform.position.x;

        spriteRenderer.flipX = lookLeft;

        if (effect != null)
        {
            Vector3 position = effectStartPosition;

            position.x =
                Mathf.Abs(effectStartPosition.x) *
                (lookLeft ? -1 : 1);

            effect.localPosition = position;

            if (effectRenderer != null)
                effectRenderer.flipX = lookLeft;
        }

        if (effect2 != null)
        {
            Vector3 position = effect2StartPosition;

            position.x =
                Mathf.Abs(effect2StartPosition.x) *
                (lookLeft ? -1 : 1);

            effect2.localPosition = position;

            if (effectRenderer2 != null)
                effectRenderer2.flipX = lookLeft;
        }
    }
}