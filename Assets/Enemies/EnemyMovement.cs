using UnityEngine;

public class EnemyMovement : MonoBehaviour, IMover
{
    private Transform player;
    private Enemy enemy;

    public Vector2 MoveDir { get; private set; }

    public Vector2 LastMoveDir { get; private set; }

    private void Start()
    {
        player = FindObjectOfType<Player>().transform;
        enemy = GetComponent<Enemy>();
    }

    void Update()
    {
        MoveDir = (player.transform.position - transform.position).normalized;
        if (MoveDir.magnitude > 0)
        {
            LastMoveDir = MoveDir;
        }
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, enemy.Data.MoveSpeed * Time.deltaTime);
    }
}