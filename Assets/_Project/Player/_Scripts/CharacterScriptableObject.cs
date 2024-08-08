using UnityEngine;

[CreateAssetMenu(fileName = "Character_", menuName = "ScriptableObjects/Character")]
public class CharacterScriptableObject : ScriptableObject
{
    [field: SerializeField] public WeaponController startingWeapon { get; private set; }

    [field: Header("Stats")]
    [field: SerializeField] public float MaxHealth { get; private set; }
    [field: SerializeField] public float Recovery { get; private set; }
    [field: SerializeField] public float MoveSpeed { get; private set; }
    [field: SerializeField] public float Might { get; private set; }
    [field: SerializeField] public float ProjectileSpeedFactor { get; private set; }

}