using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGameUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject endPanel;         // endUI GameObject

    [Header("Buttons")]
    [SerializeField] private Button retryButton;          // RetryButton
    [SerializeField] private Button homeButton;           // Home

    void Start()
    {
        // Ensure panel is hidden at start
        endPanel.SetActive(false);

        // Buttons
        retryButton.onClick.AddListener(Retry);
        homeButton.onClick.AddListener(GoHome);
    }

    void Update()
    {
        // Show EndGameUI when player reaches max level and full experience
        if (PlayerCharacter.Instance != null
            && PlayerCharacter.Instance.isGameEnd
            && !endPanel.activeSelf)
        {
            ShowEndGameUI();
        }
    }

    public void ShowEndGameUI()
    {
        endPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(1);
    }

    public void GoHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
