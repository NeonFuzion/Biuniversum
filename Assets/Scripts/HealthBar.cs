using UnityEngine;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textMeshPro;

    int maxHealth, health;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateHealthBar()
    {
        textMeshPro.SetText(health + "/" + maxHealth);
    }

    public void Initialize(int maxHealth, int health)
    {
        this.maxHealth = maxHealth;
        this.health = health;

        UpdateHealthBar();
    }

    public void TakeDamage(int damage, bool boolean)
    {
        health -= damage;
        
        UpdateHealthBar();
    }
}
