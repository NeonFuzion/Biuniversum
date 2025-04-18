using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] int health, maxHealth;

    [SerializeField] UnityEvent onDeath;
    [SerializeField] UnityEvent<int, bool> onHit;
    [SerializeField] UnityEvent<int, int> onInitialize;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        onHit?.Invoke(damage, false);

        if (health > 0) return;
        onDeath?.Invoke();
    }

    public void Initialize(int _maxHealth, int _health = -1)
    {
        maxHealth = _maxHealth;
        health = _health < 0 ? _maxHealth : _health;

        onInitialize?.Invoke(maxHealth, health);
    }
}
