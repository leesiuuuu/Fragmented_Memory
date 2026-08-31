public enum EffectId
{
    EnemyHit,
    EnemyDeath,
    PlayerHit,
    PlayerDeath,
    Dash,
    Strike,
    BasicAttack,
    Jump,
    Poke,

    // 뒤에 붙여야 한다 — 중간에 끼우면 씬에 직렬화된 기존 값이 전부 밀린다.
    Parry
}
