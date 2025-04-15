using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class InputManager : NetworkBehaviour
{
    [SerializeField] List<EntityManager> battleData;

    ArenaSide side;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        if (BattleData.GetList.Count == 0)
        {
            side = (ArenaSide)Random.Range(0, 2);
        }
        else
        {
            side = BattleData.GetList[0].ArenaSide == ArenaSide.North ? ArenaSide.South : ArenaSide.North;
        }

        for (int i = 0; i < battleData.Count; i++)
        {
            Vector2Int position = new Vector2Int(i + (i > battleData.Count / 2 ? 1 : 0), side == ArenaSide.North ? 3 : -3);
            EntityBattleData data = new EntityBattleData(battleData[i], side, position);
            BattleData.AddBattleData(data);
        }
    }
}
