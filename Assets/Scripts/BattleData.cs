using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;
using System.Linq;

public class BattleData : NetworkBehaviour
{
    public static BattleData Instance;

    public event EventHandler<EntityGameObjectListArgs> OnSendGameObjects;

    List<EntityData> entityDataList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        entityDataList = new ();
    }

    public void SendEntityGameObjects(ArenaSide arenaSide, List<GameObject> entityGameObjects)
    {
        EntityInitializationDataList dataList = new()
        {
            DataList = entityGameObjects.Select(x => new EntityInitializationData()
            {
                ArenaSide = arenaSide,
                EntityGameObject = x
            }).ToArray()
        };
        string json = JsonUtility.ToJson(dataList, true);
        Debug.Log(json);

        SendEntityGameObjectsRpc(json);
    }

    [Rpc(SendTo.NotMe)]
    public void SendEntityGameObjectsRpc(string json)
    {
        OnSendGameObjects?.Invoke(this, new () { EntitySidePairs = json });
    }
}

[System.Serializable]
public class EntityData
{
    public int Health;
    public int MaxHealth;
    public int Speed;
    public Vector2Int Position;
}

public class EntityGameObjectListArgs : EventArgs
{
    public string EntitySidePairs;
}

[System.Serializable]
public class EntityInitializationData
{
    public ArenaSide ArenaSide;
    public GameObject EntityGameObject;
}

[Serializable]
public class EntityInitializationDataList
{
    public EntityInitializationData[] DataList;
}