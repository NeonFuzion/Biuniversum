using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;
using System;
using UnityEditor;

public class BattleData : NetworkBehaviour
{
    public static BattleData Instance { get; private set; }

    public event EventHandler<InitializeBattleDataEventArgs> OnBattleDataFilled;
    public event EventHandler<TurnDataEventArgs> OnTurnDataFilled;

    int northClamp, eastClamp, southClamp, westClamp;

    List<EntityBattleData> battleData;
    List<Entity> entities;
    List<ArenaSide> arenaSides;
    List<EntityTurnData> turnDataList;

    public IList<EntityBattleData> GetList { get => battleData.AsReadOnlyList(); }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        battleData = new ();
        northClamp = 3;
        eastClamp = 2;
        southClamp = -3;
        westClamp = -2;
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.LocalClientId != 1) return;
        InitializeRandomRpc(UnityEngine.Random.value);
    }

    public void AddBattleData(EntityBattleData data)
    {
        if (battleData.Contains(data)) return;
        battleData.Add(data);

        if (battleData.Count == 4) InitializeBattleDataRpc();
    }

    public void AddBattleData(List<EntityBattleData> data)
    {
        foreach (EntityBattleData dataItem in data)
        {
            AddBattleData(dataItem);
        }
    }

    public void UpdatePosition(int index, Vector2Int position)
    {
        Debug.Log("position index: " + index);
        if (index >= battleData.Count) return;
        Debug.Log("updating position:");
        battleData[index].Position += position;
    }

    public void UpdatePosition(int index, Vector2Int[] positions)
    {
        UpdatePosition(index, positions[positions.Length - 1]);
    }

    public void SendTurnData(List<EntityTurnData> turnData)
    {
        turnDataList = turnData;
        SendTurnDataRpc();
    }

    public int[] GetSpeedBracket()
    {
        List<EntityBattleData> output = new ();

        int lastMaxSpeed = int.MaxValue;
        while (output.Count < battleData.Count)
        {
            int maxSpeed = (int)battleData.Max(data => data.EntityManager.Entity.Speed * (data.EntityManager.Entity.Speed >= lastMaxSpeed ? 0 : 1));
            var fastest = battleData.Where(data => (int)data.EntityManager.Entity.Speed == maxSpeed).ToList();
            lastMaxSpeed = maxSpeed;

            var count = fastest.Count();
            var last = count - 1;
            for (var i = 0; i < last; ++i) {
                var r = GetRandomInt(i, count);//UnityEngine.Random.Range(i, count);
                var tmp = fastest[i];
                fastest[i] = fastest[r];
                fastest[r] = tmp;
            }

            output.AddRange(fastest);
        }
        Debug.Log(string.Join(", ", output.Select(x => x.EntityManager.gameObject.name)));
        Debug.Log(string.Join(", ", output.Select(data => battleData.IndexOf(data))));
        return output.Select(data => battleData.IndexOf(data)).ToArray();
    }

    public bool IsPositionEmpty(Vector2Int position)
    {
        return !battleData.Select(data => data.Position).Contains(position);
    }

    public bool IsPositionClamped(Vector2Int position)
    {
        if (position.x > eastClamp) return false;
        if (position.x < westClamp) return false;
        if (position.y > northClamp) return false;
        if (position.y < southClamp) return false;
        return true;
    }

    public bool IsPositionValid(Vector2Int position)
    {
        return IsPositionClamped(position) && IsPositionEmpty(position);
    }

    public static int GetRandomInt(int min, int max)
    {
        return Mathf.RoundToInt(UnityEngine.Random.value * (max - min)) + min;
    }

    [Rpc(SendTo.Everyone)]
    public void InitializeBattleDataRpc()
    {
        OnBattleDataFilled?.Invoke(this, new () { entities = entities, arenaSides = arenaSides });
    }

    [Rpc(SendTo.Everyone)]
    public void InitializeRandomRpc(float randomValue)
    {
        UnityEngine.Random.InitState((int)(randomValue * 100));
    }

    [Rpc(SendTo.Server)]
    public void SendTurnDataRpc()
    {
        OnTurnDataFilled?.Invoke(this, new () { turnDataList = turnDataList });
    }
}

public class InitializeBattleDataEventArgs : EventArgs
{
    public List<Entity> entities;
    public List<ArenaSide> arenaSides;
}

public class TurnDataEventArgs : EventArgs
{
    public List<EntityTurnData> turnDataList;
}