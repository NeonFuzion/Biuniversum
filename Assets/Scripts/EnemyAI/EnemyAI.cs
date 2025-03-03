using UnityEngine;

public abstract class EnemyAI : ScriptableObject
{
    public abstract Vector3[] ChooseMovement(EntityObject[] entites, EntityObject currentEntity);
    public abstract int ChooseAction(EntityObject[] entites, EntityObject currentEntity);
}
