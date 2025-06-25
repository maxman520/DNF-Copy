using UnityEngine;

public class AnimEventReceiver : MonoBehaviour
{
    // �θ� �ִ� Player ��ũ��Ʈ�� ������ ������ ����
    private Player player;

    private void Start()
    {
        // �� �θ� ������Ʈ�� �߿��� Player ������Ʈ�� ã�Ƽ� �����´�.
        player = Player.Instance;

        if (player == null )
        {
            Debug.Log("player�� Null");
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