using UnityEngine;

public class PlayerMovement : MonoBehaviour, IMover
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private PlayerControls playerControls;

    new Rigidbody2D rigidbody;

    public Vector2 MoveDir { get; private set; }

    void Awake()
    {
        playerControls = new();
        rigidbody = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
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
    }

    void Move()
    {
        rigidbody.velocity = new Vector2(MoveDir.x * moveSpeed, MoveDir.y * moveSpeed);
    }

}
