using UnityEngine;

[System.Serializable]
public class EntitySO : MonoBehaviour
{
    [SerializeField] int health, speed;
    [SerializeField] string entityName;
    [SerializeField] ActionSO[] actions;

    public int Health { get => health; }
    public int Speed { get => speed; }
    public string EntityName { get => entityName; }
    public ActionSO[] Actions { get => actions; }
}
