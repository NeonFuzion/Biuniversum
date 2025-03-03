using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu]
public class MeleeEnemy : EnemyAI
{
    public override int ChooseAction(EntityObject[] entites, EntityObject currentEntity)
    {
        Debug.Log("Enemy preforming action");
        return Random.Range(0, currentEntity.Entity.Actions.Length);
    }

    public override Vector3[] ChooseMovement(EntityObject[] entites, EntityObject currentEntity)
    {
        Debug.Log("Enemy moving");
        float distance = 0;
        EntityObject closestEnemy = null;
        foreach (EntityObject entity in entites)
        {
            if (entity.Entity.EnemyAI) continue;
            float newDistance = Vector3.Distance(entity.transform.position, currentEntity.transform.position);

            if (newDistance < distance) continue;
            distance = newDistance;
            closestEnemy = entity;
        }

        Vector3[] directions = new Vector3[] { Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
        Vector3[] path = new Vector3[currentEntity.Entity.MoveTiles];
        for (int i = 0; i < closestEnemy.Entity.MoveTiles; i++)
        {
            float possibleDistance = int.MaxValue;
            int index = 0;
            for (int j = 0; j < directions.Length; j++)
            {
                Vector3 newPosition = currentEntity.transform.position + (i <= 0 ? Vector3.zero : path[i - 1]) + directions[j];
                float testDistance = Vector3.Distance(newPosition, closestEnemy.transform.position);

                if (testDistance > possibleDistance) continue;
                possibleDistance = testDistance;
                index = j;
            }
            path[i] = (i <= 0 ? Vector3.zero : path[i - 1]) + directions[index];
        }

        return path;
    }
}
