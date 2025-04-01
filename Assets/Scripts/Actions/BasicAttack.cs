using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Attack/BasicAttack")]
public class BasicAttack : DamagingAction
{
    public override void PreformAction(EntityBattleData currentEntityBattleData, int actionStage)
    {
        Vector2[] shiftedEffectTiles = currentEntityBattleData.ArenaSide == ArenaSide.North ? EffectTiles.Select(x => -x).ToArray() : EffectTiles;
        foreach (EntityBattleData currentBattleData in BattleData.GetList)
        {
            if (currentBattleData == currentEntityBattleData) continue;
            if (currentBattleData.ArenaSide == currentEntityBattleData.ArenaSide) continue;
            Vector3 difference = currentBattleData.EntityManager.transform.position - currentEntityBattleData.EntityManager.transform.position;
            Vector2 currentPosition = new(Mathf.RoundToInt(difference.x), Mathf.RoundToInt(difference.z));
            
            if (!shiftedEffectTiles.Contains(currentPosition)) continue;
            currentBattleData.EntityManager.GetComponent<Health>().TakeDamage(Damage);
        }
    }
}
