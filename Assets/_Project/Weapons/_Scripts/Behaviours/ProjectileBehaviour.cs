using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    protected Vector3 shootDirection;

    private IProjectileData projectileData;
    private int currentPierce;

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
        this.currentPierce = projectileData.Pierce;

        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && other.gameObject.TryGetComponent(out Health health))
        {
            health.TakeDamage(projectileData.Damage);
            ReducePierce();
        }
    }

    private void ReducePierce()
    {
        currentPierce -= 1;
        if (currentPierce <= 0)
        {
            Destroy(gameObject);
        }
    }
}