using UnityEngine;

public abstract class EnemyAI : ScriptableObject
{
    public abstract Vector2Int[] ChooseMovement(EntityBattleData[] battleData, EntityBattleData currentEntityBattleData);
    public abstract int ChooseAction(EntityBattleData[] battleData, EntityBattleData currentEntityBattleData);
}
