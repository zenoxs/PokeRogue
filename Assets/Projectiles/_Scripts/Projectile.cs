using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Stats")]
    [SerializeField] private float damage;
    [SerializeField] private float speed;
    [SerializeField] private float destroyAfterSeconds;

    protected Vector3 shootDirection;

    protected virtual void Start()
    {
        Destroy(gameObject, destroyAfterSeconds);
    }

    private void Update()
    {
        transform.position += speed * Time.deltaTime * shootDirection;
    }

    public void Setup(Vector3 shootDirection)
    {
        this.shootDirection = shootDirection;

        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}