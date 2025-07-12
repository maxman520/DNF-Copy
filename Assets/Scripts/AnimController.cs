using UnityEngine;
using UnityEngine.InputSystem.XInput;

public class AnimController
{

    private readonly int isGrounded = Animator.StringToHash("isGrounded");
    private readonly int isWalking = Animator.StringToHash("isWalking");
    private readonly int isRunning = Animator.StringToHash("isRunning");

    private readonly int jump = Animator.StringToHash("jump");
    private readonly int yVelocity = Animator.StringToHash("yVelocity");

    private readonly int attack0 = Animator.StringToHash("attack0");
    private readonly int attack1 = Animator.StringToHash("attack1");
    private readonly int attack2 = Animator.StringToHash("attack2");
    private readonly int attack3 = Animator.StringToHash("attack3");

    private readonly int hurt1 = Animator.StringToHash("hurt1");
    private readonly int hurt2 = Animator.StringToHash("hurt2");
    private readonly int airborne = Animator.StringToHash("airborne");


    private readonly Player player;

    public AnimController(Player player)
    {
        this.player = player;
    }
    public void UpdateAnimations()
    {
        player.Anim.SetBool(isGrounded, player.IsGrounded);
        player.Anim.SetBool(isWalking, player.IsMoving && !player.IsRunning);
        player.Anim.SetBool(isRunning, player.IsMoving && player.IsRunning);
            
    }

    public void ResetAnimations()
    {
        player.Anim.SetBool(isGrounded, true);
        player.Anim.SetBool(isWalking, false);
        player.Anim.SetBool(isRunning, false);
    }
    public void PlayHurt(int value)
    {
        switch (value)
        {
            case 1:
                player.Anim.SetTrigger(hurt1);
                break;
            case 2:
                player.Anim.SetTrigger(hurt2);
                break;
        }
    }

    public void PlayAirborne()
    {
        player.Anim.SetTrigger(airborne);
    }

    public void PlayAttack(int counter)
    {
        switch(counter)
        {
            case 0:
                player.Anim.SetTrigger(attack0);
                break;
            case 1:
                player.Anim.SetTrigger(attack1);
                break;
            case 2:
                player.Anim.SetTrigger(attack2);
                break;
            case 3:
                player.Anim.SetTrigger(attack3);
                break;
        }
    }

    public void PlayJump()
    {
        player.Anim.SetTrigger(jump);
    }

    public void UpdateJumpAnimation(float value) {
        player.Anim.SetFloat(yVelocity, value);
    }
    public void ResetJumpAttackTrigger() {
        player.Anim.ResetTrigger(attack0);
    }
}