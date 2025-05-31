using UnityEngine;

public abstract class ActionSO : ScriptableObject
{
    [SerializeField] string actionName;
    [SerializeField] Vector2Int[] effectedPositions;

    public string ActionName { get => actionName; }
    public Vector2Int[] EffectedPositions { get => effectedPositions; }

    public abstract void PreformAction();
}
