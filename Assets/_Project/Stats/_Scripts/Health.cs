using UnityEngine;

public class Health : MonoBehaviour
{
    private float currentHealth;
    private float maxHealth;

    public void SetMaxHealth(float maxHealth)
    {
        currentHealth = maxHealth;
        this.maxHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Kill();
        }
    }

    private void Kill()
    {
        Destroy(gameObject);
    }
}