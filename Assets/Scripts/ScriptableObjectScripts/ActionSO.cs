using UnityEngine;

[System.Serializable]
public abstract class ActionSO : ScriptableObject
{
    [SerializeField] string actionName;
    [SerializeField] Vector2Int[] effectedPositions;

    public string ActionName { get => actionName; }
    public Vector2Int[] EffectedPosition { get => effectedPositions; }

    public abstract void DoAction();
}
