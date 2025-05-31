using UnityEngine;

[CreateAssetMenu(menuName = "Entity/EntityOS" )]
public class EntitySO : ScriptableObject
{
    [SerializeField] int health, speed, steps;
    [SerializeField] string entityName;
    [SerializeField] ActionSO[] actions;

    public int Health { get => health; }
    public int Speed { get => speed; }
    public int Step { get => steps; }
    public string EntityName { get => entityName; }
    public ActionSO[] Actions { get => actions; }
}
