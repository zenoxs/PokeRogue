using UnityEngine;

public class PlayerMovement : MonoBehaviour, IMover
{
    [SerializeField] private PlayerControls playerControls;

    // References
    private Rigidbody2D rb;
    private Player player;

    public Vector2 MoveDir { get; private set; }
    public Vector2 LastMoveDir { get; private set; }

    void Awake()
    {
        playerControls = new();
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();

    }

    void Start()
    {
        LastMoveDir = new Vector2(0, -1f);

        playerControls.PlayerInput.Enable();
    }

    void Update()
    {
        InputManagement();
    }

    void FixedUpdate()
    {
        Move();
    }

    void OnDestroy()
    {
        playerControls.PlayerInput.Disable();
    }

    void InputManagement()
    {
        MoveDir = playerControls.PlayerInput.Move.ReadValue<Vector2>().normalized;

        if (MoveDir.magnitude > 0)
        {
            LastMoveDir = MoveDir;
        }
    }

    void Move()
    {
        rb.velocity = new Vector2(MoveDir.x * player.CharacterData.MoveSpeed, MoveDir.y * player.CharacterData.MoveSpeed);
    }

}
