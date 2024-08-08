using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeaponController : WeaponController
{

    [SerializeField] private MeleeWeaponScriptableObject meleeWeaponData;

    // state
    private float currentCooldown;
    private float currenDuration;

    private MeleeEffectBehaviour? meleeEffectBehaviour;

    private void Start()
    {
        ResetCooldown();
    }

    private void Update()
    {
        if (currenDuration > 0)
        {
            currenDuration -= Time.deltaTime;
            return;
        }

        if (currenDuration <= 0 && meleeEffectBehaviour != null)
        {
            Destroy(meleeEffectBehaviour.gameObject);
        }

        currentCooldown -= Time.deltaTime;

        if (currentCooldown <= 0f)
        {
            Attack();
        }
    }

    private void Attack()
    {
        ResetCooldown();
        currenDuration = meleeWeaponData.Duration;

        meleeEffectBehaviour = Instantiate(meleeWeaponData.EffectPrefab);
        meleeEffectBehaviour.Setup(meleeWeaponData);

        meleeEffectBehaviour.transform.position = transform.position;
        meleeEffectBehaviour.transform.parent = transform;
    }

    private void ResetCooldown()
    {
        currentCooldown = meleeWeaponData.CooldownDuration;
    }
}
