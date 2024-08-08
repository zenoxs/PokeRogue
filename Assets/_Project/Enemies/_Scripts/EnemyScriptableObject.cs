using UnityEngine;

[CreateAssetMenu(fileName = "Enemy_", menuName = "ScriptableObjects/Enemy", order = 0)]
public class EnemyStatScriptableObject : ScriptableObject
{
    [field: Header("Stats")]
    [field: SerializeField] public float MaxHealth { get; private set; }
    [field: SerializeField] public float MoveSpeed { get; private set; }
    [field: SerializeField] public float Damage { get; private set; }
}