using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public float currentHealth { get; private set; }
    public float currentMana { get; private set; }

    private void Start()
    {
        // 게임 시작 시 스탯 초기화
        InitializePlayerState();
    }
    public void InitializePlayerState()
    {
        currentHealth = PlayerStats.Instance.MaxHP;
        currentMana = PlayerStats.Instance.MaxMP;

        // UI 매니저에게 초기 UI 업데이트 요청
        UIManager.Instance.UpdateHP(PlayerStats.Instance.MaxHP, currentHealth);
        UIManager.Instance.UpdateMP(PlayerStats.Instance.MaxMP, currentMana);
    }

    private void OnEnable()
    {
        // 씬이 로드될 때마다 SceneLoaded 함수를 호출하도록 이벤트에 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 오브젝트가 파괴될 때 이벤트 등록 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Player player = Player.Instance;
        if (player == null) return;


        if (scene.name == "Dungeon1_Scene")
        {
            Debug.Log("던전 씬 로드");
            player.ChangeState(player.dungeonState);
        }
        else if (scene.name == "Town_Scene")
        {
            Debug.Log("마을 씬 로드");
            player.ChangeState(player.townState);
            // 마을로 이동시 체력, 마나 회복
            currentHealth = PlayerStats.Instance.MaxHP;
            currentMana = PlayerStats.Instance.MaxMP;
        }

        // 플레이어를 시작 지점으로 이동
        GameObject startPoint = GameObject.FindWithTag("Respawn");
        if (startPoint != null)
        {
            player.transform.position = startPoint.transform.position;
        }

        // --- 카메라 경계, y 좌표 재설정 ---
        GameObject newBoundsObject = GameObject.FindWithTag("CameraBound");
        if (newBoundsObject != null)
        {
            Collider2D newBoundsCollider = newBoundsObject.GetComponent<Collider2D>();
            CinemachineCamera vcam = FindFirstObjectByType<CinemachineCamera>();
            if (vcam != null)
            {
                // 가상 카메라의 Confiner 2D에 새로운 경계를 연결해준다.
                CinemachineConfiner2D confiner = vcam.GetComponent<CinemachineConfiner2D>();
                if (confiner != null)
                {
                    confiner.BoundingShape2D = newBoundsCollider;
                    // 캐시를 무효화하여 새로운 경계를 즉시 적용
                    confiner.InvalidateBoundingShapeCache();
                    Debug.Log("카메라 경계 재설정");
                }
                vcam.OnTargetObjectWarped(player.transform, new Vector3(0, 0, -10));
                Debug.Log("카메라 Y 좌표 재설정");
            }
        }
    }


    // 데미지를 받는 공용 메서드
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        // UI 업데이트 요청
        UIManager.Instance.UpdateHP(PlayerStats.Instance.MaxHP, currentHealth);

        if (currentHealth <= 0)
        {
            // Die(); // 사망 처리 로직 (나중에 구현)
            Debug.Log("플레이어 사망!");
        }
    }

    // 마나를 사용하는 공용 메서드
    public void UseMana(float amount)
    {
        currentMana -= amount;
        if (currentMana < 0) currentMana = 0;

        // UI 업데이트 요청
        UIManager.Instance.UpdateMP(PlayerStats.Instance.MaxMP, currentMana);
    }
}