using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;
using System.Linq;
using Unity.Collections;

public class BattleManager : NetworkBehaviour
{
    [SerializeField] Transform[] northEntities, southEntities;

    public static BattleManager Instance { get; private set; }

    public event EventHandler<TurnDataArgs> OnSendTurnData;
    public class TurnDataArgs : EventArgs
    {
        public TurnData[] TurnData;
    }

    public event EventHandler<EntityArgs> OnSendEntities;
    public class EntityArgs : EventArgs
    {
        public string[] PrefabNames;
        public ArenaSide ArenaSide;
    }

    public event EventHandler<CharacterIndexArgs> OnCharacterSelected;
    public class CharacterIndexArgs : EventArgs
    {
        public int Index;
    }

    public event EventHandler<MovementArgs> OnMovement;
    public class MovementArgs : EventArgs
    {
        public Vector2Int Step;
        public bool CanAdd;
        public int Index;
    }

    public event EventHandler<ActionArgs> OnAction;
    public class ActionArgs : EventArgs
    {
        public int ActionIndex, EntityIndex;
    }

    public event EventHandler OnTurnEnded;

    List<TurnData> turnDataList;
    List<BattleData> battleDataList;
    CycleState cycleState;
    ArenaSide arenaSide;

    int entityIndex, remainingSteps;

    public List<TurnData> TurnDataList { get => turnDataList; }
    public List<BattleData> BattleDataList { get => battleDataList; }
    public ArenaSide ArenaSide { get => arenaSide; }

    public int EntityIndex { get; }

    void Awake()
    {
        if (Instance && Instance != this) Destroy(this);
        else Instance = this;

        battleDataList = new();
        turnDataList = new();
        arenaSide = ArenaSide.North;

        for (int i = 0; i < 8; i++)
        {
            turnDataList.Add(new());
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnSendEntities += AddEntities;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void OnNetworkSpawn()
    {
        arenaSide = (ArenaSide)(NetworkManager.Singleton.LocalClientId + 1);

        if (arenaSide != ArenaSide.South) return;
        SendEntitiesRpc();
    }

    void SetupMovement()
    {
        ChangeEntity(0);
    }

    void StartBattle()
    {
        StartBattleRpc(UnityEngine.Random.Range(0, 1000));
    }

    public void Movement(Vector2 inputVector)
    {
        if (cycleState != CycleState.Selecting) return;

        if (Mathf.Abs(inputVector.x) > 0.05f && Mathf.Abs(inputVector.y) > 0.05f) return;
        Vector2Int movement = new((int)inputVector.x, (int)inputVector.y);
        bool canAdd = remainingSteps-- > 0;
        OnMovement?.Invoke(this, new() { Step = movement, CanAdd = canAdd, Index = entityIndex });

        if (!canAdd) return;
        turnDataList[entityIndex].Movement.Add(movement);
    }

    public void Action(int actionIndex)
    {
        OnAction?.Invoke(this, new() { ActionIndex = actionIndex, EntityIndex = entityIndex });
    }

    public void ChangeEntity(int index = -1)
    {
        cycleState = CycleState.Selecting;
        entityIndex = index == -1 ? (entityIndex + 1) % 4 : index;
        remainingSteps = battleDataList[entityIndex].EntitySO.Step;
        OnCharacterSelected?.Invoke(this, new() { Index = entityIndex });
    }

    public void AddEntities(object sender, EntityArgs args)
    {
        Debug.Log("Recieving data");
        AddEntitiesRpc(string.Join("|", args.PrefabNames), args.ArenaSide);
    }

    public void EndTurn()
    {
        OnTurnEnded?.Invoke(this, new());
        OnSendTurnData?.Invoke(this, new() { TurnData = turnDataList.ToArray() });
    }

    [Rpc(SendTo.Everyone)]
    public void SendEntitiesRpc()
    {
        Debug.Log("Sending data");
        OnSendEntities?.Invoke(this, new()
        {
            ArenaSide = arenaSide,
            PrefabNames = (arenaSide == ArenaSide.North ? northEntities : southEntities).Select(x => x.gameObject.name).ToArray()
        });
    }

    [Rpc(SendTo.Everyone)]
    public void AddEntitiesRpc(string paths, ArenaSide arenaSide)
    {
        //Debug.Log("Spawning objects");
        int x = -2;
        foreach (string path in paths.Split("|"))
        {
            battleDataList.Add(new BattleData()
            {
                EntitySO = Resources.Load($"EntityData/{path}") as EntitySO,
                ArenaSide = arenaSide
            }
            );

            //if (this.arenaSide == ArenaSide.South) continue;
            Vector3 spawnPosition = new(x++, 0, arenaSide == ArenaSide.North ? 3 : -3);
            GameObject entityGameObject = Instantiate(Resources.Load($"Entities/{path}") as GameObject, spawnPosition, Quaternion.identity);
            //entityGameObject.GetComponent<NetworkObject>().Spawn(true);
            BattleData data = battleDataList[battleDataList.Count - 1];
            data.Transform = entityGameObject.transform;
            data.Position = new((int)spawnPosition.x, (int)spawnPosition.z);

            if (battleDataList.Count < 8) continue;
            StartBattle();
        }
    }

    [Rpc(SendTo.Everyone)]
    public void StartBattleRpc(int seed)
    {
        //Debug.Log($"Full send {battleDataList.Count}");
        SetupMovement();

        if (NetworkManager.Singleton.LocalClientId != 1) return;
        UnityEngine.Random.InitState(seed);
    }
}

public class TurnData
{
    public List<Vector2Int> Movement = new();
    public int ActionIndex = -1;
}

public class BattleData
{
    public EntitySO EntitySO;
    public Transform Transform;
    public ArenaSide ArenaSide;
    public Vector2Int Position;
    public int Attack, Health, Speed;
}

public class BattleDataList
{
    public BattleData[] List;
}

public enum CycleState { None, Selecting, Cycling }
public enum ArenaSide { None, North, South }