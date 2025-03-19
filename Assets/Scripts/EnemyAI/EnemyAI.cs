using UnityEngine;

public abstract class EnemyAI : ScriptableObject
{
    public abstract Vector2Int[] ChooseMovement(EntityBattleData currentEntityBattleData);
    public abstract int ChooseAction(EntityBattleData currentEntityBattleData);
}
