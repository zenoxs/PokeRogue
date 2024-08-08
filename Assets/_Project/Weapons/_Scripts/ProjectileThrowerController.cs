using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileThrowerController : WeaponController
{

    [SerializeField] private ProjectileThrowerScriptableObject projectileThrowerData;

    IMover mover;

    // state
    private float currentCooldown;

    private void Start()
    {
        mover = GetComponentInParent<IMover>();
        ResetCooldown();
    }

    private void Update()
    {
        currentCooldown -= Time.deltaTime;

        if (currentCooldown <= 0f)
        {
            ThrowProjectile();
        }
    }

    private void ThrowProjectile()
    {
        ResetCooldown();
        var projectileInstance = Instantiate(projectileThrowerData.ProjectilePrefab);

        projectileInstance.transform.position = transform.position;
        projectileInstance.Setup(mover.LastMoveDir, projectileThrowerData);
    }

    private void ResetCooldown()
    {
        currentCooldown = projectileThrowerData.CooldownDuration;
    }
}
