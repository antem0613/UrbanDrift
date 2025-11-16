using UnityEngine;
using Dreamteck.Forever;
using UrbanDrift;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;
using Unity.Cinemachine;
using System.Collections;

public class GameManagerHelper : MonoBehaviour
{
    public TMP_Text TMP_Score, TMP_TuttorialCount, TMP_TutorialPage,TMP_HighScore;
    public GameManager gameManager;
    public GameObject GO_Score;
    InputAction IA_Enter, IA_Escape, IA_Pause, IA_Back;
    UrbanDriftInput inputs;
    public TMP_Dropdown dropdownQuality, dropdownMode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (PlayerPrefs.HasKey("highScore"))
        {
            GameManager.instance.sliderBGM.value = PlayerPrefs.GetFloat("BGM");
            GameManager.instance.BGM.volume = GameManager.instance.sliderBGM.value * 0.036f;
            GameManager.instance.sliderSE.value = PlayerPrefs.GetFloat("SE");
            GameManager.instance.SE.volume = GameManager.instance.sliderSE.value / 10f;
            GameManager.instance.CarSound.volume = GameManager.instance.sliderSE.value * 0.025f;
            QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("Quality"), false);
            dropdownQuality.value = PlayerPrefs.GetInt("Quality");
            int mode = PlayerPrefs.GetInt("ScreenMode");

            if(mode == 0)
            {
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            }
            else
            {
                Screen.fullScreenMode = FullScreenMode.Windowed;
            }
            dropdownMode.value = mode;
        }
        else
        {
            GameManager.instance.sliderBGM.value = 10;
            GameManager.instance.BGM.volume = 0.36f;
            GameManager.instance.sliderSE.value = 10;
            GameManager.instance.SE.volume = 10f;
            GameManager.instance.CarSound.volume = 0.25f;
            QualitySettings.SetQualityLevel(1, false);
            dropdownQuality.value = 1;
            dropdownMode.value = 0;

            PlayerPrefs.SetInt("highScore", 0);
            PlayerPrefs.SetFloat("BGM", 10f);
            PlayerPrefs.SetFloat("SE", 10f);
            PlayerPrefs.SetInt("Quality", 1);
            PlayerPrefs.SetInt("ScreenMode", 0);
            PlayerPrefs.Save();
        }

        gameManager.highScore = PlayerPrefs.GetInt("highScore");
        inputs = new UrbanDriftInput();
        IA_Enter = InputSystem.actions.FindAction("Enter");
        IA_Escape = InputSystem.actions.FindAction("Escape");
        IA_Pause = InputSystem.actions.FindAction("Pause");
        IA_Back = InputSystem.actions.FindAction("Back");
        GameManager.instance.EnablePlayerInput += OnEnablePlayer;
        GameManager.instance.EnableUIInput += OnEnableUI;
        OnEnableUI(this, EventArgs.Empty);
    }

    // Update is called once per frame
    void Update()
    {
        GO_Score.SetActive(gameManager.endTutorial);
        TMP_Score.text = Mathf.CeilToInt(gameManager.score).ToString();
        TMP_HighScore.text = gameManager.highScore.ToString();

        if (!GameManager.instance.endTutorial)
        {
            TMP_TutorialPage.text = GameManager.instance.currentPage.ToString();
            TMP_TuttorialCount.text = (GameManager.instance.TutorialPanels.Length - 1).ToString();
        }

        if (IA_Enter.triggered)
        {
            GameManager.instance.OnEnter();
        }

        if (IA_Escape.triggered)
        {
            GameManager.instance.OnEscape();
        }

        if (IA_Pause.triggered && GameManager.instance.IsGaming)
        {
            GameManager.instance.OnEscape();
        }

        if(IA_Back.triggered)
        {
            if (GameManager.instance.isPaused)
            {
                GameManager.instance.OnEscape();
            }

            if (GameManager.instance.SettingPanel.activeSelf)
            {
                CloseSetting();
            }
        }
    }

    void OnEnableUI(object obj,EventArgs e)
    {
        inputs.UI.Enable();
        inputs.Player.Disable();
    }

    void OnEnablePlayer(object obj,EventArgs e)
    {
        inputs.Player.Enable();
        inputs.UI.Disable();
    }

    public void OpenSetting()
    {
       StartCoroutine(OnOpenSetting());
    }

    IEnumerator OnOpenSetting()
    {
        CinemachineCamera camera = GameManager.instance.SettingCamera.GetComponent<CinemachineCamera>();
        camera.Lens.FieldOfView = 40f;

        GameManager.instance.InitialCamera.SetActive(false);
        GameManager.instance.SettingCamera.SetActive(true);
        GameManager.instance.eventSystem?.SetSelectedGameObject(GameManager.instance.defaultSelect);
        GameManager.instance.TitlePanel.SetActive(false);

        yield return new WaitForSeconds(1.5f);

        float val = 0;

        while(val < 1f)
        {
            val += 0.02f;
            camera.Lens.FieldOfView = Mathf.Lerp(40f, 13f, val);
            yield return new WaitForSeconds(0.001f);
        }

        GameManager.instance.SettingPanel.SetActive(true);
    }

    public void CloseSetting()
    {
       StartCoroutine (OnCloseSetting());
    }

    IEnumerator OnCloseSetting()
    {
        GameManager.instance.SettingPanel.SetActive(false);
        GameManager.instance.SettingCamera.SetActive(false);
        GameManager.instance.InitialCamera.SetActive(true);
        GameManager.instance.eventSystem?.SetSelectedGameObject(GameManager.instance.defaultSelect);

        yield return new WaitForSeconds(1);

        GameManager.instance.TitlePanel.SetActive(true);
    }

    public void SetQuality()
    {
        QualitySettings.SetQualityLevel(dropdownQuality.value, false);
        PlayerPrefs.SetInt("Quality", dropdownQuality.value);
        PlayerPrefs.Save();
    }

    public void SetScreenMode()
    {
        if (dropdownMode.value == 0)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }

        PlayerPrefs.SetInt("ScreenMode", dropdownQuality.value);
        PlayerPrefs.Save();
    }
}
