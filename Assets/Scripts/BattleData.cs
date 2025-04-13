using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BattleData : MonoBehaviour
{
    static BattleData instance;
    public static BattleData Instance { get => instance; }

    static int northClamp, eastClamp, southClamp, westClamp;

    static List<EntityBattleData> battleData;

    public static IList<EntityBattleData> GetList { get => battleData.AsReadOnlyList(); }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public static void Initialize()
    {
        battleData = new ();
        northClamp = 3;
        eastClamp = 2;
        southClamp = -3;
        westClamp = -2;
    }

    static public void AddBattleData(EntityBattleData data)
    {
        if (battleData.Contains(data)) return;
        battleData.Add(data);
    }

    static public void AddBattleData(List<EntityBattleData> data)
    {
        foreach (EntityBattleData dataItem in data)
        {
            if (battleData.Contains(dataItem)) continue;
            battleData.Add(dataItem);
        }
    }

    static public void UpdatePosition(int index, Vector2Int position)
    {
        Debug.Log("position index: " + index);
        if (index >= battleData.Count) return;
        Debug.Log("updating position:");
        battleData[index].Position += position;
    }

    static public void UpdatePosition(int index, Vector2Int[] positions)
    {
        UpdatePosition(index, positions[positions.Length - 1]);
    }

    static public void SortBySpeed()
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
                var r = Random.Range(i, count);
                var tmp = fastest[i];
                fastest[i] = fastest[r];
                fastest[r] = tmp;
            }

            output.AddRange(fastest);
        }
        battleData = output;
        //Debug.Log(string.Join(", ", battleData.Select(x => x.EntityManager.gameObject.name)));
    }

    public static bool IsPositionEmpty(Vector2Int position)
    {
        return !battleData.Select(data => data.Position).Contains(position);
    }

    public static bool IsPositionClamped(Vector2Int position)
    {
        if (position.x > eastClamp) return false;
        if (position.x < westClamp) return false;
        if (position.y > northClamp) return false;
        if (position.y < southClamp) return false;
        return true;
    }

    public static bool IsPositionValid(Vector2Int position)
    {
        return IsPositionClamped(position) && IsPositionEmpty(position);
    }
}