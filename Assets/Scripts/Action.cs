using UnityEngine;

[CreateAssetMenu]
public class Action : ScriptableObject
{
    [SerializeField] bool requiresCharge;
    [SerializeField] string actionName, animationName;
    
    [SerializeField] Vector2[] effectTiles;

    public bool RequiresCharge { get => requiresCharge; }
    public string ActionName { get => actionName;}
    public string AnimationName { get => animationName; }

    public Vector2[] EffectTiles { get => effectTiles; }
}