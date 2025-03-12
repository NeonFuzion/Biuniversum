using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Attack/BasicAttack")]
public class BasicAttack : DamagingAction
{
    public override void PreformAction(List<EntityBattleData> entityBattleData, EntityBattleData currentEntityBattleData, int actionStage)
    {
        foreach (EntityBattleData currentBattleData in entityBattleData)
        {
            if (currentBattleData == currentEntityBattleData) continue;
            if (currentBattleData.ArenaSide == currentEntityBattleData.ArenaSide) continue;
            Vector3 difference = currentBattleData.EntityManager.transform.position - currentEntityBattleData.EntityManager.transform.position;
            Vector2 currentPosition = new(difference.x, difference.z);
            
            if (!EffectTiles.Contains(currentPosition)) continue;
            currentBattleData.EntityManager.GetComponent<Health>().TakeDamage(Damage);
        }
    }
}
