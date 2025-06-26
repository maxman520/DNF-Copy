using UnityEngine;
using UnityEngine.InputSystem.XInput;

public class AnimController
{
    private readonly Player player;
    private readonly AnimHashes animHashes;

    public AnimController(Player player)
    {
        this.player = player;
        this.animHashes = new AnimHashes();
    }

    public void UpdateAnimations()
    {
        player.Anim.SetBool(animHashes.IsGrounded, player.IsGrounded);
        player.Anim.SetBool(animHashes.IsWalking, player.IsMoving && !player.IsRunning);
        player.Anim.SetBool(animHashes.IsRunning, player.IsMoving && player.IsRunning);
    }

    public void ResetAnimations()
    {
        player.Anim.SetBool(animHashes.IsGrounded, true);
        player.Anim.SetBool(animHashes.IsWalking, false);
        player.Anim.SetBool(animHashes.IsRunning, false);
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
        player.Anim.SetTrigger(animHashes.Jump);
    }

    public void UpdateJumpAnimation(float yVelocity) {
        player.Anim.SetFloat(animHashes.YVelocity, yVelocity);
    }
    public void ResetJumpAttackTrigger() {
        player.Anim.ResetTrigger("jumpAttack");
    }
}