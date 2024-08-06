using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileThrower : Weapon
{

    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float cooldownDuration;

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
        var projectileInstance = Instantiate(projectilePrefab);

        projectileInstance.transform.position = transform.position;
        projectileInstance.Setup(mover.LastMoveDir);
    }

    private void ResetCooldown()
    {
        currentCooldown = cooldownDuration;
    }
}
