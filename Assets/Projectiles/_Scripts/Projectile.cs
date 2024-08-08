using UnityEngine;

public class Projectile : MonoBehaviour
{
    protected Vector3 shootDirection;

    private IProjectileData projectileData;

    protected virtual void Start()
    {
        Destroy(gameObject, projectileData.DestroyAfterSeconds);
    }

    private void Update()
    {
        transform.position += projectileData.Speed * Time.deltaTime * shootDirection;
    }

    public void Setup(Vector3 shootDirection, IProjectileData projectileData)
    {
        this.projectileData = projectileData;
        this.shootDirection = shootDirection;

        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}