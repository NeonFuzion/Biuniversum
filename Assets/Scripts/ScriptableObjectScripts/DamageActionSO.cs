using UnityEngine;

public abstract class DamageActionSO : ActionSO
{
    [SerializeField] int damage;

    public int Damage { get => damage; }
}
