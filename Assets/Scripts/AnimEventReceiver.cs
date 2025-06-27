using UnityEngine;

public class AnimEventReceiver : MonoBehaviour
{
    private Player player;
    private PlayerHitbox playerHitbox;

    private void Start()
    {
        player = Player.Instance;
        playerHitbox = GetComponentInChildren<PlayerHitbox>();
    }

    public void OnComboWindowOpen()
    {
        player?.AnimEvent_OnComboWindowOpen();
    }

    public void OnComboWindowClose()
    {
        player?.AnimEvent_OnComboWindowClose();
    }

    public void SetComboAttackDetails(int index)
    {
        playerHitbox?.SetComboAttackDetails(index);
    }

}