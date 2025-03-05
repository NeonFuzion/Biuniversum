using UnityEngine;

public abstract class EnemyAI : ScriptableObject
{
    public abstract Vector3[] ChooseMovement(EntityBattleData[] battleData, EntityBattleData currentEntityBattleData);
    public abstract int ChooseAction(EntityBattleData[] battleData, EntityBattleData currentEntityBattleData);
}
