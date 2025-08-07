using UnityEngine;
using Unity.Cinemachine;

public class VirtualCamera : Singleton<VirtualCamera>
{
    private CinemachineConfiner2D confiner;
    private CinemachineCamera virtualCam;


    protected override void Awake()
    {
        base.Awake();
        confiner = GetComponent<CinemachineConfiner2D>();
        virtualCam = GetComponent<CinemachineCamera>();
    }

    public void ChangeConfiner(Collider2D newBound)
    {
        if (newBound == null)
        {
            Debug.LogError("새로운 경계가 할당되지 않았습니다.");
            // 경계가 null일 경우, Confiner를 비활성화하여 카메라가 자유롭게 움직이도록 할 수 있음
            confiner.enabled = false; 
            return;
        }
        
        confiner.BoundingShape2D = newBound;
        // 경계가 유효하므로 Confiner를 활성화
        confiner.enabled = true;
    }

    public void SetFollowTarget(Transform target)  // Follow 설정 메서드
    {
        if (virtualCam != null)
        {
            virtualCam.Follow = target;
        }
    }
}
