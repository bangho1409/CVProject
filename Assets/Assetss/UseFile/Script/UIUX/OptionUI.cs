using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject optionPanel;      // OPTION GameObject
    [SerializeField] private GameObject settingPanel;     // SettingUI GameObject

    [Header("Option Buttons")]
    [SerializeField] private Button escButton;            // ESCbutton
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button exitButton;

    [Header("Setting Buttons")]
    [SerializeField] private Button returnButton;
    [SerializeField] private Button soundButton;

    [Header("Sound Toggle Icons")]
    [SerializeField] private GameObject soundOn;          // On icon inside SoundButton
    [SerializeField] private GameObject soundOff;         // Off icon inside SoundButton

    private bool isPaused = false;
    private bool isSoundOn = true;

    void Start()
    {
        // Ensure panels are hidden at start
        optionPanel.SetActive(false);
        settingPanel.SetActive(false);

        // ESC button toggles pause
        escButton.onClick.AddListener(TogglePause);

        // Option buttons
        resumeButton.onClick.AddListener(ResumeGame);
        settingButton.onClick.AddListener(OpenSetting);
        exitButton.onClick.AddListener(ExitToMenu);

        // Setting buttons
        returnButton.onClick.AddListener(ReturnToOption);
        soundButton.onClick.AddListener(ToggleSound);

        // Initialize sound icons
        soundOn.SetActive(true);
        soundOff.SetActive(false);
    }

    void TogglePause()
    {
        if (settingPanel.activeSelf)
        {
            ReturnToOption();
        }
        else if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    void PauseGame()
    {
        isPaused = true;
        optionPanel.SetActive(true);
        settingPanel.SetActive(false);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        optionPanel.SetActive(false);
        settingPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OpenSetting()
    {
        optionPanel.SetActive(false);
        settingPanel.SetActive(true);
    }

    public void ReturnToOption()
    {
        settingPanel.SetActive(false);
        optionPanel.SetActive(true);
    }

    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        AudioListener.pause = !isSoundOn;
        soundOn.SetActive(isSoundOn);
        soundOff.SetActive(!isSoundOn);
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}