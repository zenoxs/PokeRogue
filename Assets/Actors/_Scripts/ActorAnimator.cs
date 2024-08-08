using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActorAnimator : MonoBehaviour
{
  private static readonly AnimationKey Idle =
    new("Idle");
  private static readonly AnimationKey Walk =
     new("Walk");

  [SerializeField] private SpriteRenderer spriteRenderer = null!;
  [SerializeField] private Animator animator = null!;

  private IMover mover;

  // state
  private int animationCurrentState;
  private ActorOrientation facingOrientation = ActorOrientation.South;
  private AnimatorState animatorState = AnimatorState.Walk;

  private Vector2 lastNonZeroMoveDir = Vector2.zero;

  private void Awake()
  {
    mover = GetComponent<IMover>();
  }

  private void LateUpdate()
  {
    if (mover.MoveDir.x != 0 || mover.MoveDir.y != 0)
    {
      // walk
      ChangeAnimationState(Walk, GetOrientation(mover.MoveDir));
      return;
    }

    // idle
    ChangeAnimationState(Idle, facingOrientation);
    return;
  }

  public void OnFinished(string animName)
  {
    Debug.Log("Anim finished " + animName);
  }

  public ActorOrientation GetOrientation() => facingOrientation;

  public void SetAnimatorController(RuntimeAnimatorController runtimeAnimatorController)
  {
    animator.runtimeAnimatorController = runtimeAnimatorController;
  }

  private void ChangeAnimationState(AnimationKey animationKey, ActorOrientation orientation)
  {
    int newAnimationState = animationKey.GetAnimationByOrientation(orientation);

    if (animationCurrentState == newAnimationState) return;

    animator.Play(newAnimationState);
    facingOrientation = orientation;

    animationCurrentState = newAnimationState;
  }

  private ActorOrientation GetOrientation(Vector2 movDir)
  {
    float angle = Mathf.Atan2(movDir.y, movDir.x) * Mathf.Rad2Deg;

    var orientation = angle switch
    {
      >= 120 and <= 150 => ActorOrientation.NorthWest,
      >= 60 and <= 120 => ActorOrientation.North,
      >= 30 and <= 60 => ActorOrientation.NorthEast,
      >= -30 and <= 30 => ActorOrientation.East,
      >= -60 and <= -30 => ActorOrientation.SouthEast,
      >= -120 and <= -60 => ActorOrientation.South,
      >= -150 and <= -120 => ActorOrientation.SouthWest,
      (>= 150 and <= 180) or (>= -150) => ActorOrientation.West,
      _ => ActorOrientation.North
    };

    // Debug.Log(angle + ":" + orientation);

    return orientation;
  }

  private enum AnimatorState
  {
    Idle,
    Walk,
  }

  private class AnimationKey
  {
    private readonly Dictionary<ActorOrientation, int> animationKeys = new();
    private readonly string key;


    public AnimationKey(string key)
    {
      this.key = key;
      var actorOrientations = Enum.GetValues(typeof(ActorOrientation)).Cast<ActorOrientation>();
      foreach (var actorOrientation in actorOrientations)
      {
        animationKeys.Add(actorOrientation, GetHash(actorOrientation));
      }
    }

    public int GetAnimationByOrientation(ActorOrientation actorOrientation)
    {
      return animationKeys[actorOrientation];
    }

    private int GetHash(ActorOrientation actorOrientation)
    {
      return Animator.StringToHash(key + "_" + actorOrientation.ToString());
    }
  }
}

