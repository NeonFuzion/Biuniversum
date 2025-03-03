using UnityEngine;

[CreateAssetMenu]
public class Entity : ScriptableObject
{
    [SerializeField] float speed, health, attack;
    [SerializeField] string entityName;
    [SerializeField] int moveTiles;

    [SerializeField] EnemyAI enemyAI;
    [SerializeField] Action[] actions;

    public float Speed { get => speed; }
    public float Health { get => health; }
    public float Attack { get => attack; }
    public string Name { get => entityName; }
    public int MoveTiles { get => moveTiles; }

    public EnemyAI EnemyAI { get => enemyAI; }
    public Action[] Actions { get => actions; }
}