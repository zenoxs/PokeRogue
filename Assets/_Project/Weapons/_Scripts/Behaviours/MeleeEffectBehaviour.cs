using System.Collections.Generic;
using UnityEngine;

public class MeleeEffectBehaviour : MonoBehaviour
{
    private IMeleeEffectData meleeEffectData;

    List<GameObject> markedEnemies;

    private void Awake()
    {
        markedEnemies = new List<GameObject>();
    }

    public void Setup(IMeleeEffectData meleeEffectData)
    {
        this.meleeEffectData = meleeEffectData;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && !markedEnemies.Contains(other.gameObject) && other.gameObject.TryGetComponent(out Health health))
        {
            health.TakeDamage(meleeEffectData.Damage);
            markedEnemies.Add(other.gameObject);
        }
    }
}