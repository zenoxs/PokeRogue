using UnityEngine;

public interface IMover
{
    Vector2 MoveDir { get; }
    Vector2 LastMoveDir { get; }
}