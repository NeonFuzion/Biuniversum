using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu]
public class MeleeEnemy : EnemyAI
{
    public override int ChooseAction(EntityBattleData currentEntityBattleData)
    {
        Debug.Log("Enemy preforming action");
        Entity entity = currentEntityBattleData.EntityManager.Entity;
        return BattleData.GetRandomInt(0, entity.Actions.Length);
    }

    public override Vector2Int[] ChooseMovement(EntityBattleData currentEntityBattleData)
    {
        Debug.Log("Enemy moving");
        float distance = 0;
        EntityBattleData closestEnemy = null;
        foreach (EntityBattleData currentData in BattleData.Instance.GetList)
        {
            if (currentData.EntityManager.Entity.EnemyAI) continue;
            float newDistance = Vector2.Distance(currentData.Position, currentEntityBattleData.Position);

            if (newDistance < distance) continue;
            distance = newDistance;
            closestEnemy = currentData;
        }

        Vector2Int[] directions = new Vector2Int[] { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };
        Vector2Int[] path = new Vector2Int[currentEntityBattleData.EntityManager.Entity.MoveTiles];
        for (int i = 0; i < closestEnemy.EntityManager.Entity.MoveTiles; i++)
        {
            float possibleDistance = int.MaxValue;
            int index = 0;
            for (int j = 0; j < directions.Length; j++)
            {
                Vector2Int newPosition = currentEntityBattleData.Position + (i <= 0 ? new () : path[i - 1]) + directions[j];
                float testDistance = Vector2.Distance(newPosition, closestEnemy.Position);

                if (testDistance > possibleDistance) continue;
                possibleDistance = testDistance;
                index = j;
            }
            path[i] = (i <= 0 ? new () : path[i - 1]) + directions[index];
        }

        return path;
    }
}
