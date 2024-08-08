using UnityEngine;

public class Enemy : MonoBehaviour
{
    [field: SerializeField] public EnemyStatScriptableObject Data { get; private set; }

    private void Awake()
    {
        GetComponent<Health>().SetMaxHealth(Data.MaxHealth);
    }
}