using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // --- ╫л╠шео фпео ---
    public static PlayerStats Instance { get; private set; }

    [Header("Player Stats")]
    public int atk = 10;
    public int def = 10;
    public float walkSpeed = 3f;
    public float runSpeed = 6f;

    public float MaxHealth = 100f;
    public float MaxMana = 100f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}