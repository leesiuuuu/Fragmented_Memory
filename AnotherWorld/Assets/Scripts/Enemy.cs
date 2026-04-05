using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public virtual int MaxHP { get; set; } = 100;
    public virtual int HP { get; set; }
    public virtual int AD { get; set; } = 10;
    public virtual int Defence { get; set; } = 5;
    public virtual bool IsDeath { get; set; } = false;

    public virtual void Start()
    {
        HP = MaxHP;
    }


    public virtual void Hitted(int value)
    {
        if (IsDeath) return;

        int finalDamage = Mathf.Max(0, value - Defence);
        HP -= finalDamage;

        if(HP <= 0)
        {
            HP = 0;
            Death();
        }
        else
        {
            Groggy();
        }
    }

    protected virtual void Groggy()
    {

    }

    protected virtual void Death()
    {
        Debug.Log("쥬금");
        IsDeath = true;
        GameManager.Instance.KilledEnemy++;
        GameManager.Instance.killedEnemy_InGame++;
    }
}
