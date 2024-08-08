using UnityEngine;

[CreateAssetMenu(fileName = "MeleeWeapon_", menuName = "ScriptableObjects/Weapons/Melee")]
public class MeleeWeaponScriptableObject : WeaponScriptableObject, IMeleeEffectData
{
    [field: SerializeField] public float CooldownDuration { get; private set; }
    [field: SerializeField] public float Damage { get; private set; }
    [field: SerializeField] public float Duration { get; private set; }
    [field: SerializeField] public MeleeEffectBehaviour EffectPrefab { get; private set; }

}
