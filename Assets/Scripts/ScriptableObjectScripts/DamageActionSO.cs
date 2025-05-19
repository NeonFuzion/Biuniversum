using UnityEngine;

[System.Serializable]
public abstract class DamageActionSO : ActionSO
{
    [SerializeField] int damage;

    public int Damage { get => damage; }
}
