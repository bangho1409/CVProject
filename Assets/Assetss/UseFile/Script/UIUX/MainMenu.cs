using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject settingPanel;     // SETTINGPannel GameObject

    [Header("Menu Buttons")]
    [SerializeField] private Button playButton;           // PlayButton
    [SerializeField] private Button settingButton;        // SettingButton
    [SerializeField] private Button quitButton;           // QuitButton

    [Header("Setting Buttons")]
    [SerializeField] private Button returnButton;         // ReturnButton
    [SerializeField] private Button soundButton;          // SoundButton

    [Header("Sound Toggle Icons")]
    [SerializeField] private GameObject soundOn;          // On icon inside SoundButton
    [SerializeField] private GameObject soundOff;         // Off icon inside SoundButton

    private bool isSoundOn = true;

    void Start()
    {
        // Ensure panels are correct at start
        settingPanel.SetActive(false);

        // Menu buttons
        playButton.onClick.AddListener(PlayGame);
        settingButton.onClick.AddListener(OpenSetting);
        quitButton.onClick.AddListener(QuitGame);

        // Setting buttons
        returnButton.onClick.AddListener(ReturnToMenu);
        soundButton.onClick.AddListener(ToggleSound);

        // Initialize sound icons
        soundOn.SetActive(true);
        soundOff.SetActive(false);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void OpenSetting()
    {
        settingPanel.SetActive(true);
    }

    public void ReturnToMenu()
    {
        settingPanel.SetActive(false);
    }

    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        AudioListener.pause = !isSoundOn;
        soundOn.SetActive(isSoundOn);
        soundOff.SetActive(!isSoundOn);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
