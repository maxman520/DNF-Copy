using UnityEngine;

public class InTownState : IPlayerState
{
    private readonly Player player;
    public InTownState(Player player)
    {
        this.player = player;
        if (player == null)
        {
            Debug.Log("Player를 찾을 수 없음");
            return;
        }
    }
    public void Enter()
    {
        Debug.Log("마을 상태에 진입");
        player.Anim.Play("Idle_Town");
    }

    public void Update()
    {
        player.MoveInput = player.InputActions.Player.Move.ReadValue<Vector2>();

        if (player.MoveInput.magnitude > 0)
        {
            player.Anim.SetBool("isWalking", true);
        }
        else
        {
            player.Anim.SetBool("isWalking", false);
        }

        // 캐릭터 방향 전환
        if (player.MoveInput.x != 0)
        {
            player.transform.localScale = new Vector3(Mathf.Sign(player.MoveInput.x), 1f, 1f);
        }
    }

    public void FixedUpdate()
    {
        // 플레이어 걷기
        player.Rb.linearVelocity = player.MoveInput.normalized * player.WalkSpeed;
    }

    public void Exit()
    {
        Debug.Log("마을 상태를 벗어납니다.");
        // 다음 상태로 가기 전, 걷기 애니메이션 상태를 초기화
        player.Anim.SetBool("isWalking", false);
    }
}