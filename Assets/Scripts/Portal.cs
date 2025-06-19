using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [SerializeField] private string destinationSceneName;

    private void OnTriggerEnter2D(Collider2D other)
    {

        Debug.Log(other.name + " 오브젝트의 " + other.GetType().Name + " 콜라이더가 포탈에 닿음!");

        if (other.transform.CompareTag("Player"))
        {
            Debug.Log("플레이어가 포탈에 닿았습니다. " + destinationSceneName + " (으)로 이동합니다.");

            SceneManager.LoadScene(destinationSceneName);
        }
    }
}