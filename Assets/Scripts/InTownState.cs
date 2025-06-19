using UnityEngine;

public class InTownState : PlayerStateInterface
{
    public void Enter(Player player)
    {
        Debug.Log("마을 상태에 진입");
        player.anim.Play("Idle_Town");
    }

    public void Update(Player player)
    {
        player.moveInput = player.inputActions.Player.Move.ReadValue<Vector2>();

        if (player.moveInput.magnitude > 0)
        {
            player.anim.SetBool("isWalking", true);
        }
        else
        {
            player.anim.SetBool("isWalking", false);
        }

        // 캐릭터 방향 전환
        if (player.moveInput.x != 0)
        {
            player.transform.localScale = new Vector3(Mathf.Sign(player.moveInput.x), 1f, 1f);
        }
    }

    public void FixedUpdate(Player player)
    {
        // 플레이어 걷기
        player.rb.linearVelocity = player.moveInput.normalized * player.walkSpeed;
    }

    public void Exit(Player player)
    {
        Debug.Log("마을 상태를 벗어납니다.");
        // 다음 상태로 가기 전, 걷기 애니메이션 상태를 초기화
        player.anim.SetBool("isWalking", false);
    }
}