using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu]
public class MeleeEnemy : EnemyAI
{
    public override int ChooseAction(EntityBattleData[] battleData, EntityBattleData currentEntityBattleData)
    {
        Debug.Log("Enemy preforming action");
        Entity entity = currentEntityBattleData.EntityManager.Entity;
        return Random.Range(0, entity.Actions.Length);
    }

    public override Vector3[] ChooseMovement(EntityBattleData[] battleData, EntityBattleData currentEntityBattleData)
    {
        Debug.Log("Enemy moving");
        float distance = 0;
        EntityManager entityManager = currentEntityBattleData.EntityManager;
        EntityManager closestEnemy = null;
        foreach (EntityBattleData currentData in battleData)
        {
            EntityManager currentEntityManager = currentData.EntityManager;

            if (currentEntityManager.Entity.EnemyAI) continue;
            float newDistance = Vector3.Distance(currentEntityManager.transform.position, entityManager.transform.position);

            if (newDistance < distance) continue;
            distance = newDistance;
            closestEnemy = currentEntityManager;
        }

        Vector3[] directions = new Vector3[] { Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
        Vector3[] path = new Vector3[entityManager.Entity.MoveTiles];
        for (int i = 0; i < closestEnemy.Entity.MoveTiles; i++)
        {
            float possibleDistance = int.MaxValue;
            int index = 0;
            for (int j = 0; j < directions.Length; j++)
            {
                Vector3 newPosition = entityManager.transform.position + (i <= 0 ? Vector3.zero : path[i - 1]) + directions[j];
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
