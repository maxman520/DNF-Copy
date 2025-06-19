using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [SerializeField] private string destinationSceneName;

    private void OnTriggerEnter2D(Collider2D other)
    {

        Debug.Log(other.name + " ������Ʈ�� " + other.GetType().Name + " �ݶ��̴��� ��Ż�� ����!");

        if (other.transform.CompareTag("Player"))
        {
            Debug.Log("�÷��̾ ��Ż�� ��ҽ��ϴ�. " + destinationSceneName + " (��)�� �̵��մϴ�.");

            SceneManager.LoadScene(destinationSceneName);
        }
    }
}