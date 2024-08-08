using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileThrowerScriptableObject", menuName = "ScriptableObjects/Weapons/ProjectileThrower")]
public class ProjectileThrowerScriptableObject : WeaponScriptableObject, IProjectileData
{
    [field: SerializeField] public float CooldownDuration { get; private set; }

    [field: Header("Projectile Stats")]
    [field: SerializeField] public float Damage { get; private set; }
    [field: SerializeField] public float Speed { get; private set; }
    [field: SerializeField] public float DestroyAfterSeconds { get; private set; }
    [field: SerializeField] public Projectile ProjectilePrefab { get; private set; }


}
