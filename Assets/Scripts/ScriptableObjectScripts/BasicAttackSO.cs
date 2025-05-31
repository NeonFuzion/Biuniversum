using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/BasicAttack")]
public class BasicAttackSO : DamageActionSO
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void PreformAction()
    {
        List<BattleData> list = BattleManager.Instance.BattleDataList;
        Vector2Int currentPosition = list[BattleManager.Instance.EntityIndex].Position;
        foreach (BattleData data in list)
        {
            Vector2Int relativePosition = data.Position - currentPosition;
            if (!EffectedPositions.Contains(relativePosition)) continue;
            data.Health -= Damage;
        }
    }
}
