using UnityEngine;

public abstract class DamagingAction : Action
{
    [SerializeField] int damage;

    public int Damage { get => damage;}
}
