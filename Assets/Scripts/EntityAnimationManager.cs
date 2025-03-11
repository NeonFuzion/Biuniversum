using UnityEngine;
using UnityEngine.Events;

public class EntityAnimationManager : MonoBehaviour
{
    [SerializeField] UnityEvent onDealDamage, onFinishAnimation;

    Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Animate(Action action)
    {
        Debug.Log(action.AnimationName);
        animator.CrossFade(action.AnimationName, 0, 0);
    }

    public void OnDealDamage()
    {
        onDealDamage?.Invoke();
    }

    public void OnFinishAnimation()
    {
        onFinishAnimation?.Invoke();
    }
}
