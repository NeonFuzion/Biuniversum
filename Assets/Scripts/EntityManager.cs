using UnityEngine;

public class EntityManager : MonoBehaviour
{
    [SerializeField] Entity entity;
    [SerializeField] EntityObject entityObject;
    [SerializeField] EntityAnimationManager entityAnimationManager;
    [SerializeField] ActionVisual actionVisual;
    [SerializeField] Health health;
    [SerializeField] GameObject modelManager;

    public Entity Entity { get => entity; }
    public EntityObject EntityObject { get => entityObject; }
    public EntityAnimationManager EntityAnimationManager { get => entityAnimationManager; }
    public ActionVisual ActionVisual { get => actionVisual; }
    public GameObject ModelManager { get => modelManager; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize()
    {
        health.Initialize((int)entity.Health);
        actionVisual.Initialize();
    }
}
