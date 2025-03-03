using UnityEngine;

[CreateAssetMenu]
public class DamagingAction : Action
{
    [SerializeField] int damage;

    public int Damage { get => damage;}
}
