public interface IProjectileData
{
    float Damage { get; }
    float Speed { get; }
    float DestroyAfterSeconds { get; }
    int Pierce { get; }
}