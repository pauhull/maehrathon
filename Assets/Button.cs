using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    
    public GameController gameController;

    public int level;
    
    void Start () {
        Button btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick(){

        if (level == -2)
        {
            gameController.changeLevelPanel.SetActive(true);
            gameController.gameEndedPanel.SetActive(false);
        }
        else if (level == -1)
        {
            gameController.editChangeLevel(true);
            gameController.fixLevel = -1;
            gameController.gameEndedPanel.SetActive(true);
            gameController.changeLevelPanel.SetActive(false);
            gameController.scoreText.enabled = true;
        }
        else
        {
            gameController.editChangeLevel(false);
            gameController.RestartGame(level - 1);
            gameController.changeLevelPanel.SetActive(false);
            gameController.scoreText.enabled = false;
        }
    }
}
