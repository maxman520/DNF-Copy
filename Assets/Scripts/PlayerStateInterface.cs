using UnityEngine;

public interface PlayerStateInterface
{
    // 이 상태에 진입했을 때 호출될 함수
    void Enter(Player player);

    // 이 상태에서 매 프레임 실행될 함수 (입력 처리, 로직 업데이트 등)
    void Update(Player player);

    // 이 상태에서 고정 프레임마다 실행될 함수 (물리 처리)
    void FixedUpdate(Player player);

    // 이 상태를 빠져나갈 때 호출될 함수
    void Exit(Player player);
}