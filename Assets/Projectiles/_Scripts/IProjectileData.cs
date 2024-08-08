public interface IProjectileData
{
    float Damage { get; }
    float Speed { get; }
    float DestroyAfterSeconds { get; }
    Projectile ProjectilePrefab { get; }
}