using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Linq;
using Unity.VisualScripting;

public class BattleManager : NetworkBehaviour
{
    [SerializeField] GameObject actionVisual;
    [SerializeField] GameObject[] northEntityObjects, southEntityObjects;

    List<TurnData> turnDataList;
    CycleState cycleState;
    ArenaSide arenaSide;

    int entityIndex;

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
        cycleState = CycleState.Movement;
        arenaSide = (ArenaSide)((int)NetworkManager.Singleton.LocalClientId + 1);
        turnDataList = new();

        GameObject[] transfer = arenaSide == ArenaSide.North ? northEntityObjects : southEntityObjects;
        List<EntityInitializationData> dataList = transfer.Select(x => new EntityInitializationData() { ArenaSide = arenaSide, EntityGameObject = x }).ToList();
        SpawnEntities(this, new() { EntitySidePairs = JsonUtility.ToJson(dataList) });

        BattleData.Instance.OnSendGameObjects += SpawnEntities;

        if (NetworkManager.Singleton.LocalClientId != 1) return;
        BattleData.Instance.SendEntityGameObjects(arenaSide, transfer.ToList());
    }

    void SetupMovement()
    {
        entityIndex = 0;
    }

    public void Movement(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (cycleState != CycleState.Movement) return;

        Vector2 inputVector = context.action.ReadValue<Vector2>();
        if (Mathf.Abs(inputVector.x) > 0.05f && Mathf.Abs(inputVector.y) > 0.05f) return;
        Vector2Int movement = new(Mathf.RoundToInt(inputVector.x), Mathf.RoundToInt(inputVector.y));
        turnDataList[entityIndex].Movement.Add(movement);
    }

    public void SpawnEntities(object sender, EntityGameObjectListArgs args)
    {
        //Debug.Log("Spawning entities");
        int northX = -2;
        int southX = -2;
        Debug.Log(args.EntitySidePairs);
        EntityInitializationDataList dataList = JsonUtility.FromJson<EntityInitializationDataList>(args.EntitySidePairs);
        Debug.Log(string.Join(", ", dataList));
        foreach (EntityInitializationData data in dataList.DataList)
        {
            bool isNorth = data.ArenaSide == ArenaSide.North;
            Vector3 spawnPosition = new(isNorth ? northX++ : southX++, 2, isNorth ? 3 : -3);
            GameObject entityGameObject = Instantiate(data.EntityGameObject, spawnPosition, Quaternion.identity);
        }

        if (NetworkManager.Singleton.LocalClientId != 0) return;
        GameObject[] transfer = arenaSide == ArenaSide.North ? northEntityObjects : southEntityObjects;
        BattleData.Instance.SendEntityGameObjects(arenaSide, transfer.ToList());
    }
}

[System.Serializable]
public class TurnData
{
    public List<Vector2Int> Movement = new ();
    public int ActionIndex;
}

public enum CycleState { None, Movement, Action, Cycling }
public enum ArenaSide { None, North, South }