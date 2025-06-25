using UnityEngine;

public class AnimEventReceiver : MonoBehaviour
{
    // 부모에 있는 Player 스크립트의 참조를 저장할 변수
    private Player player;

    private void Start()
    {
        // 내 부모 오브젝트들 중에서 Player 컴포넌트를 찾아서 가져온다.
        player = Player.Instance;

        if (player == null )
        {
            Debug.Log("player가 Null");
        }
    }

    public void OnAttackStart()
    {
        player?.AnimEvent_OnAttackStart();
    }

    public void OnComboWindowOpen()
    {
        player?.AnimEvent_OnComboWindowOpen();
    }

    public void OnComboWindowClose()
    {
        player?.AnimEvent_OnComboWindowClose();
    }

    public void OnAttackEnd()
    {
        player?.AnimEvent_OnAttackEnd();
    }

    public void OnHurtEnd()
    {
        player?.AnimEvent_OnHurtEnd();
    }
}