using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 2;
    private int currentHealth;
    
    public UnityEvent OnDeath;
    
    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    public void Heal(int healAmount)
    {
        currentHealth += healAmount;

        if (currentHealth >= maxHealth)
        {
            currentHealth = maxHealth; 
        }
    }
}
