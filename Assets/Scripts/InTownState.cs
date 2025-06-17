using UnityEngine;

public class InTownState : PlayerStateInterface
{
    public void Enter(Player player)
    {
        Debug.Log("마을 상태에 진입합니다.");
        player.Animator.Play("Idle_Town");
    }

    public void Update(Player player)
    {
        // 이동 입력을 받아와서 플레이어의 moveInput 변수에 저장
        player.moveInput = player.inputActions.Player.Move.ReadValue<Vector2>();

        // 이동 벡터의 크기가 0보다 크면 걷기 애니메이션, 아니면 마을 Idle 애니메이션
        if (player.moveInput.magnitude > 0)
        {
            player.Animator.SetBool("IsWalking", true);
        }
        else
        {
            player.Animator.SetBool("IsWalking", false);
        }

        // 캐릭터 방향 전환
        if (player.moveInput.x != 0)
        {
            player.transform.localScale = new Vector3(Mathf.Sign(player.moveInput.x), 1f, 1f);
        }
    }

    public void FixedUpdate(Player player)
    {
        // 플레이어의 속도를 걷는 속도로 설정하고 이동
        player.Rigidbody.linearVelocity = player.moveInput.normalized * player.walkSpeed;
    }

    public void Exit(Player player)
    {
        Debug.Log("마을 상태를 벗어납니다.");
        // 다음 상태로 가기 전, 걷기 애니메이션 상태를 초기화
        player.Animator.SetBool("IsWalking", false);
    }
}