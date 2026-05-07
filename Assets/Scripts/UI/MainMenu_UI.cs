using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class MainMenu_UI : MonoBehaviour
{
    #region 组件变量
    private Transform mainMenuUI_Canvas;
    private Button newGameButton;
    private Button continueButton;
    private Button quitButton;
    private PlayableDirector director;
    #endregion

    void Awake()
    {
        mainMenuUI_Canvas = GameObject.Find("Menu Canvas").transform;
        newGameButton = mainMenuUI_Canvas.Find("New Game Btn").GetComponent<Button>();
        continueButton = mainMenuUI_Canvas.Find("Continue Btn").GetComponent<Button>();
        quitButton = mainMenuUI_Canvas.Find("Quit Btn").GetComponent<Button>();
        director = FindObjectOfType<PlayableDirector>();

        newGameButton.onClick.AddListener(PlayTimeline);
        continueButton.onClick.AddListener(ContinueGame);
        quitButton.onClick.AddListener(QuitGame);

        director.stopped += NewGame;
    }

    private void NewGame(PlayableDirector obj)
    {
        PlayerPrefs.DeleteKey("PlayerData");
        SceneController.Instance.TransitToFirstLevel();
    }

    private void ContinueGame()
    {
        SceneController.Instance.TransitToFirstLevel();
    }

    private void QuitGame()
    {
        Debug.Log("Quit game.");
        Application.Quit();
    }

    private void PlayTimeline()
    {
        director.Play();
    }
}
