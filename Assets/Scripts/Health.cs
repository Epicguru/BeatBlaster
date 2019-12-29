using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public int HP { get; private set; } = 100;
    public int MaxHP { get; private set; } = 100;
    public bool IsDead { get { return HP == 0; } }

    public bool CanChangeHealthWhenDead = false;
    public UnityAction<Health, int, ProjectileHit> UponHealthChange;

    public void UponProjectileHit(ProjectileHit hit)
    {
        if (IsDead && !CanChangeHealthWhenDead)
            return;

        int old = HP;
        HP += hit.HealthChange;
        if (HP > MaxHP)
            HP = MaxHP;
        if (HP < 0)
            HP = 0;

        if(old != HP)
        {
            UponHealthChange?.Invoke(this, HP - old, hit);
        }
    }
}
