using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour
{
    // This would be linked to the "Start Game" button in the Unity Editor
    public Button startGameButton;

    void Start()
    {
        // Add a listener to the button to call the StartGame method when clicked
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(StartGame);
            startGameButton.interactable = false;
        }
        else
        {
            Debug.LogError("Start Game Button is not assigned in the inspector.");
        }
    }

    // This method will be called when a character is selected.
    // For now, we'll just enable the start button.
    public void SelectCharacter(/* CharacterData character */)
    {
        // In a real implementation, you would store the selected character data.
        // For example: GameManager.instance.SetSelectedCharacter(character);

        Debug.Log("Character selected!");
        if (startGameButton != null)
        {
            startGameButton.interactable = true;
        }
    }

    // This method loads the main game scene (e.g., "Town_Scene")
    public void StartGame()
    {
        // Here, you would load the next scene, for example, the town or the first level.
        // Make sure "Town_Scene" is added to the Build Settings.
        SceneManager.LoadScene("Town_Scene");
    }
}
