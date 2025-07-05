using UnityEngine;

[System.Serializable] // 인스펙터 창에 노출
public struct AttackDetails
{
    public string attackName;
    public float damageRate;            // 이 공격의 데미지 배율. 기본 1
    public float knockbackForce;    // 수평 넉백은 Rigidbody 속도로 여전히 유효
    public float launchForce;    // 공중에 띄우는 힘 (띄우기 공격이 아니면 0)
    public float yOffset; // y축 범위 설정
    // 필요하다면 나중에 경직 시간, 속성 등도 추가 가능
}