using UnityEngine;

public class EnemyMovement : MonoBehaviour, IMover
{
    [SerializeField] private float moveSpeed;
    private Transform player;

    public Vector2 MoveDir { get; private set; }

    public Vector2 LastMoveDir { get; private set; }

    private void Start()
    {
        player = FindObjectOfType<PlayerMovement>().transform;
    }

    void Update()
    {
        MoveDir = (player.transform.position - transform.position).normalized;
        if (MoveDir.magnitude > 0)
        {
            LastMoveDir = MoveDir;
        }
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.deltaTime);
    }
}