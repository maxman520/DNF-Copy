using UnityEngine;
using UnityEngine.InputSystem.XInput;

public class AnimController
{
    private readonly Player player;

    public AnimController(Player player)
    {
        this.player = player;
    }

    public void UpdateAnimations()
    {
        player.Anim.SetBool("isGrounded", player.IsGrounded);
        player.Anim.SetBool("isWalking", player.IsMoving && !player.IsRunning);
        player.Anim.SetBool("isRunning", player.IsMoving && player.IsRunning);
    }

    public void ResetAnimations()
    {
        player.Anim.SetBool("isGrounded", true);
        player.Anim.SetBool("isWalking", false);
        player.Anim.SetBool("isRunning", false);
    }

    public void PlayAttack(int counter)
    {
        player.Anim.SetTrigger("attack" + counter);
    }

    public void PlayJumpAttack()
    {
        player.Anim.SetTrigger("jumpAttack");
    }

    public void PlayJump()
    {
        player.Anim.SetTrigger("jump");
    }

    public void UpdateJumpAnimation(float yVelocity) {
        player.Anim.SetFloat("yVelocity", yVelocity);
    }
    public void ResetJumpAttackTrigger() {
        player.Anim.ResetTrigger("jumpAttack");
    }
}