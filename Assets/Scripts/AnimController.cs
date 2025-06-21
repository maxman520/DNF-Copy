using UnityEngine;
using UnityEngine.InputSystem.XInput;

public class AnimController
{
    private readonly Player player;
    private readonly InputHandler inputHandler;
    private readonly AnimHashes animHashes;

    public AnimController(Player player, InputHandler inputHandler)
    {
        this.player = player;
        this.inputHandler = inputHandler;
        this.animHashes = new AnimHashes();
    }

    public void UpdateAnimations()
    {
        bool isMoving = inputHandler.MoveInput.magnitude > 0;

        player.Anim.SetBool(animHashes.IsWalking, isMoving && !player.IsRunning);
        player.Anim.SetBool(animHashes.IsRunning, isMoving && player.IsRunning);
    }

    public void ResetAnimations()
    {
        player.Anim.SetBool(animHashes.IsWalking, false);
        player.Anim.SetBool(animHashes.IsRunning, false);
    }
}