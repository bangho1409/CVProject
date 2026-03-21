using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeadUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject deadPanel;        // DeadUI GameObject

    [Header("Buttons")]
    [SerializeField] private Button retryButton;          // RetryButton
    [SerializeField] private Button homeButton;           // Home

    void Start()
    {
        // Ensure panel is hidden at start
        deadPanel.SetActive(false);

        // Buttons
        retryButton.onClick.AddListener(Retry);
        homeButton.onClick.AddListener(GoHome);
    }

    void Update()
    {
        // Show DeadUI when player dies
        if (PlayerCharacter.Instance != null
            && PlayerCharacter.Instance.isDead
            && !deadPanel.activeSelf)
        {
            ShowDeadUI();
        }
    }

    public void ShowDeadUI()
    {
        deadPanel.SetActive(true);
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
