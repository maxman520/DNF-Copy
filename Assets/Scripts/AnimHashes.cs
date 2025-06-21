using UnityEngine;

public class AnimHashes
{
    public readonly int IsWalking = Animator.StringToHash("isWalking");
    public readonly int IsRunning = Animator.StringToHash("isRunning");
    public readonly int IsGrounded = Animator.StringToHash("isGrounded");
    public readonly int Jump = Animator.StringToHash("jump");
    public readonly int JumpAttack = Animator.StringToHash("jumpAttack");
    public readonly int YVelocity = Animator.StringToHash("yVelocity");
    public readonly int Attack = Animator.StringToHash("attack");

}